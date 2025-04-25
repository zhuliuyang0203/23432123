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
import re
import shutil
import socket
import subprocess
import time
import urllib

from selenium.webdriver.common.selenium_manager import SeleniumManager


class Server:
    """Manage a Selenium Grid (Remote) Server in standalone mode.

    This class contains functionality for downloading the server and starting/stopping it.

    For more information on Selenium Grid, see:
        - https://www.selenium.dev/documentation/grid/getting_started/

    Parameters:
    -----------
    host : str
        Hostname or IP address to bind to (determined automatically if not specified)
    port : int or str
        Port to listen on (4444 if not specified)
    path : str
        Path/filename of existing server .jar file (Selenium Manager is used if not specified)
    version : str
        Version of server to download (latest version if not specified)
    env: dict
        Environment variables passed to server environment
    """

    def __init__(self, host=None, port=4444, path=None, version=None, env=None):
        if path and version:
            raise TypeError("Not allowed to specify a version when using an existing server path")

        self.host = host
        self.port = self._validate_port(port)
        self.path = self._validate_path(path)
        self.version = self._validate_version(version)
        self.env = env

        self.process = None
        self.status_url = self._get_status_url()

    def _get_status_url(self):
        host = self.host if self.host is not None else "localhost"
        return f"http://{host}:{self.port}/status"

    def _validate_path(self, path):
        if path and not os.path.exists(path):
            raise OSError(f"Can't find server .jar located at {path}")
        return path

    def _validate_port(self, port):
        try:
            port = int(port)
        except ValueError:
            raise TypeError(f"{__class__.__name__}.__init__() got an invalid port: '{port}'")
        if not (0 <= port <= 65535):
            raise ValueError("port must be 0-65535")
        return port

    def _validate_version(self, version):
        if version:
            if not re.match(r"^\d+\.\d+\.\d+$", str(version)):
                raise TypeError(f"{__class__.__name__}.__init__() got an invalid version: '{version}'")
        return version

    def _wait_for_server(self, timeout=10):
        start = time.time()
        while time.time() - start < timeout:
            try:
                urllib.request.urlopen(self.status_url)
                return True
            except urllib.error.URLError:
                time.sleep(0.2)
        return False

    def start(self):
        """Start the server.

        Selenium Manager will detect the server location and download it if necessary,
        unless an existing server path was specified.
        """
        if self.path is None:
            selenium_manager = SeleniumManager()
            args = ["--grid"]
            if self.version:
                args.append(self.version)
            self.path = selenium_manager.binary_paths(args)["driver_path"]

        java_path = shutil.which("java")
        if java_path is None:
            raise OSError("Can't find java on system PATH. JRE is required to run the Selenium server")

        command = [
            java_path,
            "-jar",
            self.path,
            "standalone",
            "--port",
            str(self.port),
            "--selenium-manager",
            "true",
            "--enable-managed-downloads",
            "true",
        ]
        if self.host is not None:
            command.extend(["--host", self.host])

        host = self.host if self.host is not None else "localhost"

        try:
            with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as sock:
                sock.connect((host, self.port))
            raise ConnectionError(f"Selenium server is already running, or something else is using port {self.port}")
        except ConnectionRefusedError:
            print(f"Starting Selenium server at: {self.path}")
            self.process = subprocess.Popen(command, env=self.env)
            print(f"Selenium server running as process: {self.process.pid}")
            if not self._wait_for_server():
                raise TimeoutError(f"Timed out waiting for Selenium server at {self.status_url}")
            print("Selenium server is ready")
        return self.process

    def stop(self):
        """Stop the server."""
        if self.process is None:
            raise RuntimeError("Selenium server isn't running")
        else:
            if self.process.poll() is None:
                self.process.terminate()
                self.process.wait()
            self.process = None
            print("Selenium server has been terminated")
