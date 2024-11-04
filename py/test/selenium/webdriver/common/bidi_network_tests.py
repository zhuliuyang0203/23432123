# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.
import pytest
import trio

from selenium.webdriver.common.bidi.cdp import open_cdp
from selenium.webdriver.common.bidi.network import UrlPatternString


@pytest.mark.xfail_firefox
@pytest.mark.xfail_safari
@pytest.mark.xfail_edge
async def test_request_handler(driver, pages):

    url1 = pages.url("simpleTest.html")
    url2 = pages.url("clicks.html")
    url3 = pages.url("formPage.html")

    pattern1 = [UrlPatternString(url1)]
    pattern2 = [UrlPatternString(url2)]

    def request_handler(params):
        request = params["request"]
        json = {"request": request, "url": url3}
        return json

    ws_url = driver.caps.get("webSocketUrl")
    async with open_cdp(ws_url) as conn:
        async with trio.open_nursery() as nursery:
            # Multiple intercepts
            intercept1 = await nursery.start(driver.network.add_request_handler, request_handler, pattern1, conn)
            intercept2 = await nursery.start(driver.network.add_request_handler, request_handler, pattern2, conn)
            await driver.network.get(url1, conn)
            assert driver.title == "We Leave From Here"
            await driver.network.get(url2, conn)
            assert driver.title == "We Leave From Here"

            # Removal of a single intercept
            await driver.network.remove_intercept(intercept2)
            await driver.network.get(url2, conn)
            assert driver.title == "clicks"
            await driver.network.get(url1, conn)
            assert driver.title == "We Leave From Here"

            await driver.network.remove_intercept(intercept1)
            assert driver.title == "We Leave From Here"
