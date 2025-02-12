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

from selenium.webdriver.common.by import By


@pytest.mark.xfail_safari
def test_network_initialized(driver):
    assert driver.network is not None


@pytest.mark.xfail_safari
def test_add_response_handler(driver, pages):
    passed = [False]

    def callback(response):
        passed[0] = True
        response.continue_response()

    driver.network.add_response_handler(callback)
    pages.load("basicAuth")
    assert passed[0], "Callback was NOT successful"


@pytest.mark.xfail_safari
def test_remove_response_handler(driver, pages):
    passed = [False]

    def callback(response):
        passed[0] = True
        response.continue_response()

    test_response_id = driver.network.add_response_handler(callback)
    driver.network.remove_response_handler(response_id=test_response_id)
    pages.load("basicAuth")
    assert not passed[0], "Callback should NOT be successful"


@pytest.mark.xfail_safari
def test_add_request_handler(driver, pages):
    passed = [False]

    def callback(request):
        passed[0] = True
        request.continue_request()

    driver.network.add_request_handler(callback)
    pages.load("basicAuth")
    assert passed[0], "Callback was NOT successful"


@pytest.mark.xfail_safari
def test_remove_request_handler(driver, pages):
    passed = [False]

    def callback(request):
        passed[0] = True
        request.continue_request()

    test_request_id = driver.network.add_request_handler(callback)
    driver.network.remove_request_handler(request_id=test_request_id)
    pages.load("basicAuth")
    assert not passed[0], "Callback should NOT be successful"


@pytest.mark.xfail_safari
def test_add_authentication_handler(driver, pages):
    driver.network.add_authentication_handler("test", "test")
    pages.load("basicAuth")
    assert driver.find_element(By.TAG_NAME, "h1").text == "authorized", "Authentication was NOT successful"


@pytest.mark.xfail_safari
def test_remove_authentication_handler(driver, pages):
    driver.network.add_authentication_handler("test", "test")
    driver.network.remove_authentication_handler()
    pages.load("basicAuth")
    assert driver.find_element(By.TAG_NAME, "h1").text != "authorized", "Authentication was successful"
