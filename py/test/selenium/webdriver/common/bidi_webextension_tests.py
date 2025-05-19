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

EXTENSION_ID = "webextensions-selenium-example-v3@example.com"
EXTENSION_PATH = "common/extensions/webextensions-selenium-example-signed"
EXTENSION_ARCHIVE_PATH = "common/extensions/webextensions-selenium-example.xpi"

# Function to find the project root directory
def find_project_root():
    current_dir = os.path.dirname(os.path.abspath(__file__))

    while current_dir != os.path.dirname(current_dir):
        extensions_dir = os.path.join(current_dir, "common", "extensions")
        if os.path.isdir(extensions_dir):
            return current_dir
        current_dir = os.path.dirname(current_dir)

    return os.path.dirname(os.path.abspath(__file__))


@pytest.fixture
def locate_project_path():
    """Locate the project path for the extension files."""
    project_root = find_project_root()
    return project_root


def test_webextension_initialized(driver):
    """Test that the webextension module is initialized properly."""
    assert driver.webextension is not None


@pytest.mark.xfail_chrome
@pytest.mark.xfail_edge
def test_install_extension_path(driver, locate_project_path):
    """Test installing an extension from a directory path."""
    path = os.path.join(locate_project_path, EXTENSION_PATH)

    ex_in = driver.webextension.install(path=path)
    assert ex_in.get("extension") == EXTENSION_ID

    driver.webextension.uninstall(ex_in)

@pytest.mark.xfail_chrome
@pytest.mark.xfail_edge
def test_install_archive_extension_path(driver, locate_project_path):
    """Test installing an extension from an archive path."""
    path = os.path.join(locate_project_path, EXTENSION_ARCHIVE_PATH)

    ex = driver.webextension.install(archive_path=path)
    assert ex.get("extension") == EXTENSION_ID

    driver.webextension.uninstall(ex)

@pytest.mark.xfail_chrome
@pytest.mark.xfail_edge
def test_install_base64_extension_path(driver, locate_project_path):
    """Test installing an extension from a base64 encoded string."""
    path = os.path.join(locate_project_path, EXTENSION_ARCHIVE_PATH)

    with open(path, "rb") as file:
        base64_encoded = base64.b64encode(file.read()).decode("utf-8")

    ex = driver.webextension.install(base64_value=base64_encoded)
    assert ex.get("extension") == EXTENSION_ID

    driver.webextension.uninstall(ex)

@pytest.mark.xfail_chrome
@pytest.mark.xfail_edge
def test_install_with_extension_id_uninstall(driver, locate_project_path):
    """Test uninstalling an extension using just the extension ID."""
    path = os.path.join(locate_project_path, EXTENSION_PATH)

    ex = driver.webextension.install(path=path)
    extension_id = ex.get("extension")
    assert extension_id == EXTENSION_ID

    # Uninstall using the extension ID
    driver.webextension.uninstall(extension_id)
