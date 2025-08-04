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
const ie = require('selenium-webdriver/ie')
const test = require('../../lib/test')
const { getBinaryPaths } = require('selenium-webdriver/common/driverFinder')

test.suite(
  function (_env) {
    describe('iedriver', function () {
      let service

      afterEach(function () {
        if (service) {
          return service.kill()
        }
      })

      it('can start iedriver', async function () {
        // Skip on non-Windows platforms
        if (process.platform !== 'win32') {
          this.skip()
          return
        }

        service = new ie.ServiceBuilder().build()
        service.setExecutable(getBinaryPaths(new ie.Options()).driverPath)
        let url = await service.start()
        assert(/127\.0\.0\.1/.test(url), `unexpected url: ${url}`)
      })

      describe('environment variable support', function () {
        let originalEnvValue

        beforeEach(function () {
          originalEnvValue = process.env.SE_IEDRIVER
        })

        afterEach(function () {
          if (originalEnvValue) {
            process.env.SE_IEDRIVER = originalEnvValue
          } else {
            delete process.env.SE_IEDRIVER
          }
        })

        it('uses SE_IEDRIVER environment variable when set', function () {
          const testPath = '/custom/path/to/iedriver'
          process.env.SE_IEDRIVER = testPath

          const serviceBuilder = new ie.ServiceBuilder()
          assert.strictEqual(serviceBuilder.getExecutable(), testPath)
        })

        it('explicit path overrides environment variable', function () {
          const envPath = '/env/path/to/iedriver'
          const explicitPath = '/explicit/path/to/iedriver'

          process.env.SE_IEDRIVER = envPath
          const serviceBuilder = new ie.ServiceBuilder(explicitPath)

          assert.strictEqual(serviceBuilder.getExecutable(), explicitPath)
        })

        it('falls back to default behavior when environment variable is not set', function () {
          delete process.env.SE_IEDRIVER

          const serviceBuilder = new ie.ServiceBuilder()
          // Should be null/undefined when no explicit path and no env var
          assert.ok(!serviceBuilder.getExecutable())
        })
      })
    })
  },
  { browsers: ['ie'] },
)
