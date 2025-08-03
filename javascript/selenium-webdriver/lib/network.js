// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

const { Network: getNetwork } = require('../bidi/network')
const { InterceptPhase } = require('../bidi/interceptPhase')
const { AddInterceptParameters } = require('../bidi/addInterceptParameters')
const { ContinueRequestParameters } = require('../bidi/continueRequestParameters')
const { ProvideResponseParameters } = require('../bidi/provideResponseParameters')
const { Request } = require('./http')
const { BytesValue, Header } = require('../bidi/networkTypes')

class Network {
  #callbackId = 0
  #driver
  #network
  #authHandlers = new Map()
  #requestHandlers = new Map()
  #responseHandlers = new Map()

  constructor(driver) {
    this.#driver = driver
  }

  // This should be done in the constructor.
  // But since it needs to call async methods we cannot do that in the constructor.
  // We can have a separate async method that initialises the Network instance.
  // However, that pattern does not allow chaining the methods as we would like the user to use it.
  // Since it involves awaiting to get the instance and then another await to call the method.
  // Using this allows the user to do this "await driver.network.addAuthenticationHandler(callback)"
  async #init() {
    if (this.#network !== undefined) {
      return
    }
    this.#network = await getNetwork(this.#driver)

    await this.#network.addIntercept(new AddInterceptParameters(InterceptPhase.AUTH_REQUIRED))

    await this.#network.addIntercept(new AddInterceptParameters(InterceptPhase.BEFORE_REQUEST_SENT))

    await this.#network.authRequired(async (event) => {
      const requestId = event.request.request
      const uri = event.request.url
      const credentials = this.getAuthCredentials(uri)
      if (credentials !== null) {
        await this.#network.continueWithAuth(requestId, credentials.username, credentials.password)
        return
      }

      await this.#network.continueWithAuthNoCredentials(requestId)
    })

    await this.#network.beforeRequestSent(async (event) => {
      const requestId = event.request.request
      const requestData = event.request

      // Build the original request from the intercepted request details.
      const originalRequest = new Request(requestData.method, requestData.url, null, new Map(requestData.headers))

      let requestHandler = this.getRequestHandler(originalRequest)
      let responseHandler = this.getResponseHandler(originalRequest)

      // Populate the headers of the original request.
      // Body is not available as part of WebDriver Spec, hence we cannot add that or use that.

      const continueRequestParams = new ContinueRequestParameters(requestId)

      // If a response handler exists, we mock the response instead of modifying the outgoing request
      if (responseHandler !== null) {
        const modifiedResponse = await responseHandler()

        const provideResponseParams = new ProvideResponseParameters(requestId)
        provideResponseParams.statusCode(modifiedResponse.status)

        // Convert headers
        if (modifiedResponse.headers.size > 0) {
          const headers = []

          modifiedResponse.headers.forEach((value, key) => {
            headers.push(new Header(key, new BytesValue('string', value)))
          })
          provideResponseParams.headers(headers)
        }

        // Convert body if available
        if (modifiedResponse.body && modifiedResponse.body.length > 0) {
          provideResponseParams.body(new BytesValue('string', modifiedResponse.body))
        }

        await this.#network.provideResponse(provideResponseParams)
        return
      }

      // If request handler exists, modify the request
      if (requestHandler !== null) {
        const modifiedRequest = requestHandler(originalRequest)

        continueRequestParams.method(modifiedRequest.method)

        if (originalRequest.path !== modifiedRequest.path) {
          continueRequestParams.url(modifiedRequest.path)
        }

        // Convert headers
        if (modifiedRequest.headers.size > 0) {
          const headers = []

          modifiedRequest.headers.forEach((value, key) => {
            headers.push(new Header(key, new BytesValue('string', value)))
          })
          continueRequestParams.headers(headers)
        }

        if (modifiedRequest.data && modifiedRequest.data.length > 0) {
          continueRequestParams.body(new BytesValue('string', modifiedRequest.data))
        }
      }

      // Continue with the modified or original request
      await this.#network.continueRequest(continueRequestParams)
    })
  }

  getAuthCredentials(uri) {
    for (let [, value] of this.#authHandlers) {
      if (uri.match(value.uri)) {
        return value
      }
    }
    return null
  }

  getRequestHandler(req) {
    for (let [, value] of this.#requestHandlers) {
      const filter = value.filter
      if (filter(req)) {
        return value.handler
      }
    }
    return null
  }

  getResponseHandler(req) {
    for (let [, value] of this.#responseHandlers) {
      const filter = value.filter
      if (filter(req)) {
        return value.handler
      }
    }
    return null
  }

  async addAuthenticationHandler(username, password, uri = '//') {
    await this.#init()

    const id = this.#callbackId++

    this.#authHandlers.set(id, { username, password, uri })
    return id
  }

  async removeAuthenticationHandler(id) {
    await this.#init()

    if (this.#authHandlers.has(id)) {
      this.#authHandlers.delete(id)
    } else {
      throw Error(`Callback with id ${id} not found`)
    }
  }

  async clearAuthenticationHandlers() {
    this.#authHandlers.clear()
  }

  /**
   * Adds a request handler that filters requests based on a predicate function.
   * @param {Function} filter - A function that takes an HTTP request and returns true or false.
   * @param {Function} handler - A function that takes an HTTP request and returns a modified request.
   * @returns {number} - A unique handler ID.
   * @throws {Error} - If filter is not a function or handler does not return a request.
   */
  async addRequestHandler(filter, handler) {
    if (typeof filter !== 'function') {
      throw new Error('Filter must be a function.')
    }

    if (typeof handler !== 'function') {
      throw new Error('Handler must be a function.')
    }

    await this.#init()

    const id = this.#callbackId++

    this.#requestHandlers.set(id, { filter, handler })
    return id
  }

  async removeRequestHandler(id) {
    await this.#init()

    if (this.#requestHandlers.has(id)) {
      this.#requestHandlers.delete(id)
    } else {
      throw Error(`Callback with id ${id} not found`)
    }
  }

  async clearRequestHandlers() {
    this.#requestHandlers.clear()
  }

  /**
   * Adds a response handler that mocks responses.
   * @param {Function} filter - A function that takes an HTTP request, returning a boolean.
   * @param {Function} handler - A function that returns a mocked HTTP response.
   * @returns {number} - A unique handler ID.
   * @throws {Error} - If filter is not a function or handler is not an async function.
   */
  async addResponseHandler(filter, handler) {
    if (typeof filter !== 'function') {
      throw new Error('Filter must be a function.')
    }

    if (typeof handler !== 'function') {
      throw new Error('Handler must be a function.')
    }

    await this.#init()

    const id = this.#callbackId++

    this.#responseHandlers.set(id, { filter, handler })
    return id
  }

  async removeResponseHandler(id) {
    await this.#init()

    if (this.#responseHandlers.has(id)) {
      this.#responseHandlers.delete(id)
    } else {
      throw Error(`Callback with id ${id} not found`)
    }
  }

  async clearResponseHandlers() {
    this.#responseHandlers.clear()
  }
}

module.exports = Network
