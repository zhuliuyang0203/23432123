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

import base64
import os
import pytest

from selenium.webdriver.common.by import By
from selenium.webdriver.support.wait import WebDriverWait


EXTENSION_ID = "webextensions-selenium-example-v3@example.com"
EXTENSION_PATH = "webextensions-selenium-example-signed"
EXTENSION_ARCHIVE_PATH = "webextensions-selenium-example.xpi"

extensions = os.path.abspath("../../../../../../../common/extensions/")


def test_webextension_initialized(driver):
    """Test that the webextension module is initialized properly."""
    assert driver.webextension is not None


@pytest.mark.xfail_chrome
@pytest.mark.xfail_edge
def test_install_extension_path(driver, pages):
    """Test installing an extension from a directory path."""
    path = os.path.join(extensions, EXTENSION_PATH)

    ex_in = driver.webextension.install(path=path)
    assert ex_in.get("extension") == EXTENSION_ID

    pages.load("blank.html")
    injected = WebDriverWait(driver, timeout=2).until(
        lambda dr: dr.find_element(By.ID, "webextensions-selenium-example")
    )
    assert injected.text == "Content injected by webextensions-selenium-example"

    driver.webextension.uninstall(ex_in)

    context_id = driver.current_window_handle
    driver.browsing_context.reload(context_id)
    assert len(driver.find_elements(By.ID, "webextensions-selenium-example")) == 0


@pytest.mark.xfail_chrome
@pytest.mark.xfail_edge
def test_install_archive_extension_path(driver, pages):
    """Test installing an extension from an archive path."""
    path = os.path.join(extensions, EXTENSION_ARCHIVE_PATH)

    ex = driver.webextension.install(archive_path=path)
    assert ex.get("extension") == EXTENSION_ID

    pages.load("blank.html")
    injected = WebDriverWait(driver, timeout=2).until(
        lambda dr: dr.find_element(By.ID, "webextensions-selenium-example")
    )
    assert injected.text == "Content injected by webextensions-selenium-example"

    driver.webextension.uninstall(ex)

    context_id = driver.current_window_handle
    driver.browsing_context.reload(context_id)

    assert len(driver.find_elements(By.ID, "webextensions-selenium-example")) == 0


@pytest.mark.xfail_chrome
@pytest.mark.xfail_edge
def test_install_base64_extension_path(driver, pages):
    """Test installing an extension from a base64 encoded string."""
    path = os.path.join(extensions, EXTENSION_ARCHIVE_PATH)

    with open(path, "rb") as file:
        base64_encoded = base64.b64encode(file.read()).decode("utf-8")

    ex = driver.webextension.install(base64_value=base64_encoded)
    assert ex.get("extension") == EXTENSION_ID

    pages.load("blank.html")
    injected = WebDriverWait(driver, timeout=2).until(
        lambda dr: dr.find_element(By.ID, "webextensions-selenium-example")
    )
    assert injected.text == "Content injected by webextensions-selenium-example"

    driver.webextension.uninstall(ex)

    context_id = driver.current_window_handle
    driver.browsing_context.reload(context_id)

    assert len(driver.find_elements(By.ID, "webextensions-selenium-example")) == 0


@pytest.mark.xfail_chrome
@pytest.mark.xfail_edge
def test_install_unsigned_extension(driver, pages):
    """Test installing an unsigned extension."""
    path = os.path.join(extensions, "webextensions-selenium-example")

    ex = driver.webextension.install(path=path)
    assert ex.get("extension") == EXTENSION_ID

    pages.load("blank.html")
    injected = WebDriverWait(driver, timeout=2).until(
        lambda dr: dr.find_element(By.ID, "webextensions-selenium-example")
    )
    assert injected.text == "Content injected by webextensions-selenium-example"

    driver.webextension.uninstall(ex)

    context_id = driver.current_window_handle
    driver.browsing_context.reload(context_id)

    assert len(driver.find_elements(By.ID, "webextensions-selenium-example")) == 0


@pytest.mark.xfail_chrome
@pytest.mark.xfail_edge
def test_install_with_extension_id_uninstall(driver, pages):
    """Test uninstalling an extension using just the extension ID."""
    path = os.path.join(extensions, EXTENSION_PATH)

    ex = driver.webextension.install(path=path)
    extension_id = ex.get("extension")
    assert extension_id == EXTENSION_ID

    pages.load("blank.html")

    # Uninstall using the extension ID
    driver.webextension.uninstall(extension_id)

    context_id = driver.current_window_handle
    driver.browsing_context.reload(context_id)

    assert len(driver.find_elements(By.ID, "webextensions-selenium-example")) == 0
