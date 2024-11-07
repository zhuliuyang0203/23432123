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
import os
import subprocess
import time

import pytest

from selenium.common.exceptions import WebDriverException
from selenium.webdriver.edge.service import Service


@pytest.mark.xfail_edge(raises=WebDriverException)
@pytest.mark.no_driver_after_test
def test_uses_edgedriver_logging(clean_driver, driver_executable) -> None:
    log_file = "msedgedriver.log"
    service_args = ["--append-log"]

    service = Service(
        log_output=log_file,
        service_args=service_args,
        executable_path=driver_executable,
    )
    driver2 = None
    try:
        driver1 = clean_driver(service=service)
        with open(log_file) as fp:
            lines = len(fp.readlines())
        driver2 = clean_driver(service=service)
        with open(log_file) as fp:
            assert len(fp.readlines()) >= 2 * lines
    finally:
        driver1.quit()
        if driver2:
            driver2.quit()
        os.remove(log_file)


@pytest.mark.no_driver_after_test
def test_log_output_as_filename(clean_driver, driver_executable) -> None:
    log_file = "msedgedriver.log"
    service = Service(log_output=log_file, executable_path=driver_executable)
    try:
        assert "--log-path=msedgedriver.log" in service.service_args
        driver = clean_driver(service=service)
        with open(log_file) as fp:
            assert "Starting Microsoft Edge WebDriver" in fp.readline()
    finally:
        driver.quit()
        os.remove(log_file)


@pytest.mark.no_driver_after_test
def test_log_output_as_file(clean_driver, driver_executable) -> None:
    log_name = "msedgedriver.log"
    log_file = open(log_name, "w", encoding="utf-8")
    service = Service(log_output=log_file, executable_path=driver_executable)
    try:
        driver = clean_driver(service=service)
        time.sleep(1)
        with open(log_name) as fp:
            assert "Starting Microsoft Edge WebDriver" in fp.readline()
    finally:
        driver.quit()
        log_file.close()
        os.remove(log_name)


@pytest.mark.no_driver_after_test
def test_log_output_as_stdout(clean_driver, capfd, driver_executable) -> None:
    service = Service(log_output=subprocess.STDOUT, executable_path=driver_executable)
    driver = clean_driver(service=service)

    out, err = capfd.readouterr()
    assert "Starting Microsoft Edge WebDriver" in out
    driver.quit()


@pytest.mark.no_driver_after_test
def test_log_output_null_default(driver, capfd) -> None:
    out, err = capfd.readouterr()
    assert "Starting Microsoft Edge WebDriver" not in out
    driver.quit()


@pytest.fixture
def service():
    return Service()


@pytest.mark.usefixtures("service")
class TestEdgeDriverService:
    service_path = "/path/to/msedgedriver"

    @pytest.fixture(autouse=True)
    def setup_and_teardown(self):
        os.environ["SE_EDGEDRIVER"] = self.service_path
        yield
        os.environ.pop("SE_EDGEDRIVER", None)

    def test_uses_path_from_env_variable(self, service):
        assert "msedgedriver" in service.path

    def test_updates_path_after_setting_env_variable(self, service):
        new_path = "/foo/bar"
        os.environ["SE_EDGEDRIVER"] = new_path
        service.executable_path = self.service_path  # Simulating the update

        assert "msedgedriver" in service.executable_path
