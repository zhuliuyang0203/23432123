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
import platform
from pathlib import Path
from dataclasses import dataclass

import pytest

from selenium import webdriver
from selenium.webdriver.remote.server import Server
from test.selenium.webdriver.common.network import get_lan_ip
from test.selenium.webdriver.common.webserver import SimpleWebServer


drivers = (
    "chrome",
    "edge",
    "firefox",
    "ie",
    "remote",
    "safari",
    "webkitgtk",
    "wpewebkit",
)


def pytest_addoption(parser):
    parser.addoption(
        "--driver",
        action="append",
        choices=drivers,
        dest="drivers",
        metavar="DRIVER",
        help="Driver to run tests against ({})".format(", ".join(drivers)),
    )
    parser.addoption(
        "--browser-binary",
        action="store",
        dest="binary",
        help="Location of the browser binary",
    )
    parser.addoption(
        "--driver-binary",
        action="store",
        dest="executable",
        help="Location of the service executable binary",
    )
    parser.addoption(
        "--browser-args",
        action="store",
        dest="args",
        help="Arguments to start the browser with",
    )
    parser.addoption(
        "--headless",
        action="store_true",
        dest="headless",
        help="Run tests in headless mode",
    )
    parser.addoption(
        "--use-lan-ip",
        action="store_true",
        dest="use_lan_ip",
        help="Start test server with lan ip instead of localhost",
    )
    parser.addoption(
        "--bidi",
        action="store_true",
        dest="bidi",
        help="Enable BiDi support",
    )


def pytest_ignore_collect(collection_path, config):
    drivers_opt = config.getoption("drivers")
    _drivers = set(drivers).difference(drivers_opt or drivers)
    if drivers_opt:
        _drivers.add("unit")
    if len([d for d in _drivers if d.lower() in collection_path.parts]) > 0:
        return True
    return None


def pytest_generate_tests(metafunc):
    if "driver" in metafunc.fixturenames and metafunc.config.option.drivers:
        metafunc.parametrize("driver", metafunc.config.option.drivers, indirect=True)


driver_instance = None
selenium_driver = None


class ContainerProtocol:
    def __contains__(self, name):
        if name.lower() in self.__dict__:
            return True
        return False


@dataclass
class SupportedDrivers(ContainerProtocol):
    chrome: str = "Chrome"
    firefox: str = "Firefox"
    safari: str = "Safari"
    edge: str = "Edge"
    ie: str = "Ie"
    webkitgtk: str = "WebKitGTK"
    wpewebkit: str = "WPEWebKit"
    remote: str = "Remote"


@dataclass
class SupportedOptions(ContainerProtocol):
    chrome: str = "ChromeOptions"
    firefox: str = "FirefoxOptions"
    edge: str = "EdgeOptions"
    safari: str = "SafariOptions"
    ie: str = "IeOptions"
    remote: str = "FirefoxOptions"
    webkitgtk: str = "WebKitGTKOptions"
    wpewebkit: str = "WPEWebKitOptions"


@dataclass
class SupportedBidiDrivers(ContainerProtocol):
    chrome: str = "Chrome"
    firefox: str = "Firefox"
    edge: str = "Edge"
    remote: str = "Remote"


class Driver:
    def __init__(self, driver_class, request):
        self.driver_class = driver_class
        self._request = request
        self._driver = None
        self._platform = None
        self._service = None
        self.kwargs = {}
        self.options = driver_class
        self.headless = driver_class
        self.bidi = request.config.option.bidi

    @property
    def supported_drivers(self):
        return SupportedDrivers()

    @property
    def supported_options(self):
        return SupportedOptions()

    @property
    def supported_bidi_drivers(self):
        return SupportedBidiDrivers()

    @property
    def driver_class(self):
        return self._driver_class

    @driver_class.setter
    def driver_class(self, cls_name):
        if cls_name.lower() not in self.supported_drivers:
            raise AttributeError(f"Invalid driver class {cls_name.lower()}")
        self._driver_class = getattr(self.supported_drivers, cls_name.lower())

    @property
    def driver_class(self):
        return self._driver_class

    @driver_class.setter
    def driver_class(self, cls_name):
        if cls_name.lower() not in self.supported_drivers:
            raise AttributeError(f"Invalid driver class {cls_name.lower()}")
        self._driver_class = getattr(self.supported_drivers, cls_name.lower())

    @property
    def exe_platform(self):
        self._platform = platform.system()
        return self._platform

    @property
    def browser_path(self):
        if self._request.config.option.binary:
            return self._request.config.option.binary
        return None

    @property
    def browser_args(self):
        if self._request.config.option.args:
            return self._request.config.option.args
        return None

    @property
    def driver_path(self):
        if self._request.config.option.executable:
            return self._request.config.option.executable
        return None

    @property
    def headless(self):
        self._headless = self._request.config.option.headless
        if self._headless:
            return True
        return False

    @headless.setter
    def headless(self, driver_class):
        if self.headless:
            if driver_class.lower() == "chrome" or driver_class.lower() == "edge":
                self._options.add_argument("--headless=new")
            if driver_class.lower() == "firefox":
                self._options.add_argument("-headless")

    @property
    def bidi(self):
        return self._bidi

    @bidi.setter
    def bidi(self, value):
        self._bidi = value
        if self._bidi:
            self._options.web_socket_url = True
            self._options.unhandled_prompt_behavior = "ignore"

    @property
    def options(self):
        return self._options

    @options.setter
    def options(self, cls_name):
        if cls_name.lower() not in self.supported_options:
            raise AttributeError(f"Invalid Options class {cls_name.lower()}")
        self._options = getattr(webdriver, getattr(self.supported_options, cls_name.lower()))()
        if self.driver_class == self.supported_drivers.firefox:
            # There are issues with window size/position when running Firefox
            # under Wayland, so we use XWayland instead.
            os.environ["MOZ_ENABLE_WAYLAND"] = "0"
        if self.driver_class == self.supported_drivers.remote:
            self._options.set_capability("moz:firefoxOptions", {})
            self._options.enable_downloads = True
            os.environ["MOZ_ENABLE_WAYLAND"] = "0"
        if self.driver_class == self.supported_drivers.webkitgtk:
            self._options.overlay_scrollbars_enabled = False

    @property
    def service(self):
        executable = self.driver_path
        if executable:
            module = getattr(webdriver, self.driver_class.lower())
            self._service = module.service.Service(executable_path=executable)
            return self._service
        return None

    @property
    def driver(self):
        self._driver = self._initialize_driver()
        return self._driver

    @property
    def is_platform_valid(self):
        if self.driver_class.lower() == "safari" and self.exe_platform != "Darwin":
            return False
        if self.driver_class.lower() == "ie" and self.exe_platform != "Windows":
            return False
        if "webkit" in self.driver_class.lower() and self.exe_platform == "Windows":
            return False
        return True

    def _initialize_driver(self):
        if self.options is not None:
            self.kwargs["options"] = self.options
        if self.driver_path is not None:
            self.kwargs["service"] = self.service
        return getattr(webdriver, self.driver_class)(**self.kwargs)

    @property
    def stop_driver(self):
        def fin():
            global driver_instance
            if self._driver is not None:
                self._driver.quit()
            self._driver = None
            driver_instance = None

        return fin


@pytest.fixture(scope="function")
def driver(request):
    global driver_instance
    global selenium_driver
    driver_class = getattr(request, "param", "Chrome").lower()

    if selenium_driver is None:
        selenium_driver = Driver(driver_class, request)

    # skip tests if not available on the platform
    if not selenium_driver.is_platform_valid:
        pytest.skip(f"{driver_class} tests can only run on {selenium_driver.exe_platform}")

    # skip tests in the 'remote' directory if run with a local driver
    if request.node.path.parts[-2] == "remote" and selenium_driver.driver_class != "Remote":
        pytest.skip(f"Remote tests can't be run with driver '{selenium_driver.driver_class}'")

    # skip tests for drivers that don't support BiDi when --bidi is enabled
    if selenium_driver.bidi:
        if driver_class.lower() not in selenium_driver.supported_bidi_drivers:
            pytest.skip(f"{driver_class} does not support BiDi")

    # conditionally mark tests as expected to fail based on driver
    marker = request.node.get_closest_marker(f"xfail_{driver_class.lower()}")
    if marker is not None:
        if "run" in marker.kwargs:
            if marker.kwargs["run"] is False:
                pytest.skip()
                yield
                return
        if "raises" in marker.kwargs:
            marker.kwargs.pop("raises")
        pytest.xfail(**marker.kwargs)

        request.addfinalizer(selenium_driver.stop_driver)

    if driver_instance is None:
        driver_instance = selenium_driver.driver

    yield driver_instance
    # Close the browser after BiDi tests. Those make event subscriptions
    # and doesn't seems to be stable enough, causing the flakiness of the
    # subsequent tests.
    # Remove this when BiDi implementation and API is stable.
    if selenium_driver.bidi:
        request.addfinalizer(selenium_driver.stop_driver)

    if request.node.get_closest_marker("no_driver_after_test"):
        driver_instance = None

    # Close the browser after BiDi tests. Those make event subscriptions
    # and doesn't seems to be stable enough, causing the flakiness of the
    # subsequent tests.
    # Remove this when BiDi implementation and API is stable.
    if selenium_driver.bidi:
        request.addfinalizer(selenium_driver.stop_driver)

    if request.node.get_closest_marker("no_driver_after_test"):
        driver_instance = None


@pytest.fixture(scope="session", autouse=True)
def stop_driver(request):
    def fin():
        global driver_instance
        if driver_instance is not None:
            driver_instance.quit()
        driver_instance = None

    request.addfinalizer(fin)


def pytest_exception_interact(node, call, report):
    if report.failed:
        global driver_instance
        if driver_instance is not None:
            driver_instance.quit()
        driver_instance = None


@pytest.fixture
def pages(driver, webserver):
    class Pages:
        def url(self, name, localhost=False):
            return webserver.where_is(name, localhost)

        def load(self, name):
            driver.get(self.url(name))

    return Pages()


@pytest.fixture(autouse=True, scope="session")
def server(request):
    drivers = request.config.getoption("drivers")
    if drivers is None or "remote" not in drivers:
        yield None
        return

    jar_path = os.path.join(
        os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
        "java/src/org/openqa/selenium/grid/selenium_server_deploy.jar",
    )

    remote_env = os.environ.copy()
    if platform.system() == "Linux":
        # There are issues with window size/position when running Firefox
        # under Wayland, so we use XWayland instead.
        remote_env["MOZ_ENABLE_WAYLAND"] = "0"

    if Path(jar_path).exists():
        # use the grid server built by bazel
        server = Server(path=jar_path, env=remote_env)
    else:
        # use the local grid server (downloads a new one if needed)
        server = Server(env=remote_env)
    server.start()
    yield server
    server.stop()
    _socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    url = f"http://{_host}:{_port}/status"
    try:
        _socket.connect((_host, _port))
        print(
            "The remote driver server is already running or something else"
            "is using port {}, continuing...".format(_port)
        )
    except Exception:
        remote_env = os.environ.copy()
        if platform.system() == "Linux":
            # There are issues with window size/position when running Firefox
            # under Wayland, so we use XWayland instead.
            remote_env["MOZ_ENABLE_WAYLAND"] = "0"
        print("Starting the Selenium server")
        process = subprocess.Popen(
            [
                "java",
                "-jar",
                _path,
                "standalone",
                "--port",
                "4444",
                "--selenium-manager",
                "true",
                "--enable-managed-downloads",
                "true",
            ],
            env=remote_env,
        )
        print(f"Selenium server running as process: {process.pid}")
        assert wait_for_server(url, 10), f"Timed out waiting for Selenium server at {url}"
        print("Selenium server is ready")
        yield process
        process.terminate()
        process.wait()
        print("Selenium server has been terminated")


@pytest.fixture(autouse=True, scope="session")
def webserver(request):
    host = get_lan_ip() if request.config.getoption("use_lan_ip") else None

    webserver = SimpleWebServer(host=host)
    webserver.start()
    yield webserver
    webserver.stop()


@pytest.fixture
def edge_service():
    from selenium.webdriver.edge.service import Service as EdgeService

    return EdgeService


@pytest.fixture(scope="function")
def driver_executable(request):
    return request.config.option.executable


@pytest.fixture(scope="function")
def clean_service(request):
    _supported_drivers = SupportedDrivers()
    try:
        driver_class = getattr(_supported_drivers, request.config.option.drivers[0].lower())
    except (AttributeError, TypeError):
        raise Exception("This test requires a --driver to be specified.")
    selenium_driver = Driver(driver_class, request)
    yield selenium_driver.service


@pytest.fixture(scope="function")
def clean_driver(request):
    _supported_drivers = SupportedDrivers()
    try:
        driver_class = getattr(_supported_drivers, request.config.option.drivers[0].lower())
    except (AttributeError, TypeError):
        raise Exception("This test requires a --driver to be specified.")
    driver_reference = getattr(webdriver, driver_class)
    yield driver_reference
    if request.node.get_closest_marker("no_driver_after_test"):
        driver_reference = None


@pytest.fixture(scope="function")
def clean_service(request):
    driver_class = get_driver_class(request.config.option.drivers[0])
    yield get_service(driver_class, request.config.option.executable)


@pytest.fixture(scope="function")
def clean_options(request):
    driver_class = get_driver_class(request.config.option.drivers[0])
    yield get_options(driver_class, request.config)


@pytest.fixture
def firefox_options(request):
    _supported_drivers = SupportedDrivers()
    try:
        driver_class = request.config.option.drivers[0]
    except (AttributeError, TypeError):
        raise Exception("This test requires a --driver to be specified")

    # skip tests in the 'remote' directory if run with a local driver
    _driver_class = getattr(_supported_drivers, driver_class.lower())
    if request.node.path.parts[-2] == "remote" and _driver_class != "Remote":
        pytest.skip(f"Remote tests can't be run with driver '{_driver_class}'")

    options = webdriver.FirefoxOptions()
    if request.config.option.headless:
        options.add_argument("-headless")
    return options


@pytest.fixture
def chromium_options(request):
    try:
        driver_option = request.config.option.drivers[0].lower()
    except (AttributeError, TypeError):
        raise Exception("This test requires a --driver to be specified")

    # Skip if not Chrome or Edge
    if driver_option not in ("chrome", "edge"):
        pytest.skip(f"This test requires Chrome or Edge, got {driver_option}")

    # skip tests in the 'remote' directory if run with a local driver
    if request.node.path.parts[-2] == "remote" and get_driver_class(driver_option) != "Remote":
        pytest.skip(f"Remote tests can't be run with driver '{driver_option}'")

    if driver_option == "chrome":
        options = webdriver.ChromeOptions()
    elif driver_option == "edge":
        options = webdriver.EdgeOptions()

    if request.config.option.headless:
        options.add_argument("--headless")

    return options
