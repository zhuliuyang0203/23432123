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

'use strict'

const assert = require('node:assert')
const { Browser } = require('selenium-webdriver')
const { Pages, suite } = require('../../lib/test')
const until = require('selenium-webdriver/lib/until')
const { By } = require('selenium-webdriver')
const { Request, Response } = require('selenium-webdriver/http')
const { Network } = require('selenium-webdriver/bidi/network')

suite(
  function (env) {
    let driver

    beforeEach(async function () {
      driver = await env.builder().build()
    })

    afterEach(async function () {
      await driver.quit()
    })

    describe('script()', function () {
      it('can add authentication handler', async function () {
        await driver.network().addAuthenticationHandler('genie', 'bottle')
        await driver.get(Pages.basicAuth)

        await driver.wait(until.elementLocated(By.css('pre')))
        let source = await driver.getPageSource()
        assert.equal(source.includes('Access granted'), true)
      })

      it('can add authentication handler with filter', async function () {
        await driver.network().addAuthenticationHandler('genie', 'bottle', 'basicAuth')
        await driver.get(Pages.basicAuth)

        await driver.wait(until.elementLocated(By.css('pre')))
        let source = await driver.getPageSource()
        assert.equal(source.includes('Access granted'), true)
      })

      it('can add multiple authentication handlers with filter', async function () {
        await driver.network().addAuthenticationHandler('genie', 'bottle', 'basicAuth')
        await driver.network().addAuthenticationHandler('test', 'test', 'test')
        await driver.get(Pages.basicAuth)

        await driver.wait(until.elementLocated(By.css('pre')))
        let source = await driver.getPageSource()
        assert.equal(source.includes('Access granted'), true)
      })

      it('can add multiple authentication handlers with the same filter', async function () {
        await driver.network().addAuthenticationHandler('genie', 'bottle', 'basicAuth')
        await driver.network().addAuthenticationHandler('genie', 'bottle', 'basicAuth')
        await driver.get(Pages.basicAuth)

        await driver.wait(until.elementLocated(By.css('pre')))
        let source = await driver.getPageSource()
        assert.equal(source.includes('Access granted'), true)
      })

      it('can remove authentication handler', async function () {
        const id = await driver.network().addAuthenticationHandler('genie', 'bottle')

        await driver.network().removeAuthenticationHandler(id)

        try {
          await driver.get(Pages.basicAuth)
          await driver.wait(until.elementLocated(By.css('pre')))
          assert.fail('Page should not be loaded')
        } catch (e) {
          assert.strictEqual(e.name, 'UnexpectedAlertOpenError')
        }
      })

      it('throws an error when remove authentication handler that does not exist', async function () {
        try {
          await driver.network().removeAuthenticationHandler(10)
          assert.fail('Expected error not thrown. Non-existent handler cannot be removed')
        } catch (e) {
          assert.strictEqual(e.message, 'Callback with id 10 not found')
        }
      })

      it('can clear authentication handlers', async function () {
        await driver.network().addAuthenticationHandler('genie', 'bottle', 'basicAuth')

        await driver.network().addAuthenticationHandler('bottle', 'genie')

        await driver.network().clearAuthenticationHandlers()

        try {
          await driver.get(Pages.basicAuth)
          await driver.wait(until.elementLocated(By.css('pre')))
          assert.fail('Page should not be loaded')
        } catch (e) {
          assert.strictEqual(e.name, 'UnexpectedAlertOpenError')
        }
      })

      it('can add request handler to modify method', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Request('HEAD', Pages.logEntryAdded, null)

        await driver.network().addRequestHandler(filter, handler)

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('log entry added events'), false)
      })

      it('can add request handler to modify uri', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Request('GET', Pages.blankPage, null)

        await driver.network().addRequestHandler(filter, handler)

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('blank'), true)
      })

      it('can add request handler to modify body', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Request('POST', Pages.addRequestBody, 'hello world!')

        await driver.network().addRequestHandler(filter, handler)

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('hello world'), true)
      })

      it('can add multiple request handlers', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Request('GET', Pages.blankPage, null)

        await driver.network().addRequestHandler(filter, handler)

        await driver.network().addRequestHandler(
          (req) => req.path.includes('hello.html'),
          () => new Request('GET', Pages.logEntryAdded, null),
        )

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('blank'), true)
      })

      it('can add multiple request handlers with same filter', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Request('GET', Pages.blankPage, null)

        await driver.network().addRequestHandler(filter, handler)

        await driver.network().addRequestHandler(filter, handler)

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('blank'), true)
      })

      it('can remove request handler', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Request('GET', Pages.blankPage, null)

        const id = await driver.network().addRequestHandler(filter, handler)

        await driver.network().removeRequestHandler(id)

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('entry added'), true)
      })

      it('throws an error when removing request handler that does not exist', async function () {
        try {
          await driver.network().removeRequestHandler(10)
          assert.fail('Expected error not thrown. Non-existent handler cannot be removed')
        } catch (e) {
          assert.strictEqual(e.message, 'Callback with id 10 not found')
        }
      })

      it('can clear request handlers', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Request('GET', Pages.blankPage, null)

        await driver.network().addRequestHandler(filter, handler)

        await driver.network().addRequestHandler(
          (req) => req.path.includes('hello.html'),
          () => new Request('GET', Pages.logEntryAdded, null),
        )

        await driver.network().clearRequestHandlers()

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('entry added'), true)
      })

      it('can add response handler to mock complete response', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Response(500, { test: 'header-value' }, 'Internal server error')

        const network = await Network(driver)

        let onResponseCompleted = null

        await network.responseStarted(function (event) {
          if (event.response.url.includes('logEntryAdded')) {
            onResponseCompleted = event.response
          }
        })

        await driver.network().addResponseHandler(filter, handler)

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('Internal server error'), true)

        assert.strictEqual(onResponseCompleted.status, 500)
        assert.strictEqual(onResponseCompleted.headers[0].name, 'test')
        assert.strictEqual(onResponseCompleted.headers[0].value.value, 'header-value')
      })

      it('can add multiple response handler with same filter', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Response(500, { test: 'header-value' }, 'Internal server error')

        const network = await Network(driver)

        let onResponseCompleted = null

        await network.responseStarted(function (event) {
          if (event.response.url.includes('logEntryAdded')) {
            onResponseCompleted = event.response
          }
        })

        await driver.network().addResponseHandler(filter, handler)
        await driver.network().addResponseHandler(filter, handler)

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('Internal server error'), true)

        assert.strictEqual(onResponseCompleted.status, 500)
        assert.strictEqual(onResponseCompleted.headers[0].name, 'test')
        assert.strictEqual(onResponseCompleted.headers[0].value.value, 'header-value')
      })

      it('throws an error when removing response handler that does not exist', async function () {
        try {
          await driver.network().removeResponseHandler(10)
          assert.fail('Expected error not thrown. Non-existent handler cannot be removed')
        } catch (e) {
          assert.strictEqual(e.message, 'Callback with id 10 not found')
        }
      })

      it('can clear response handlers', async function () {
        const filter = (req) => req.path.includes('bidi/logEntryAdded.html')
        const handler = () => new Response(200, { test: 'header' }, 'Hello!')

        await driver.network().addResponseHandler(filter, handler)

        await driver.network().addResponseHandler(
          (req) => req.path.includes('hello.html'),
          () => new Response(401, { test: 'header' }, 'Not found!'),
        )

        await driver.network().clearResponseHandlers()

        await driver.get(Pages.logEntryAdded)

        const pageSource = await driver.getPageSource()

        assert.strictEqual(pageSource.includes('entry added'), true)
      })
    })
  },
  { browsers: [Browser.FIREFOX] },
)
