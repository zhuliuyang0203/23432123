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
import socket

from selenium.webdriver.remote.client_config import ClientConfig


def test_timeout_from_env_variable():
    """Test that timeout is set from GLOBAL_DEFAULT_TIMEOUT environment variable."""
    original_env = os.environ.get("GLOBAL_DEFAULT_TIMEOUT")
    try:
        os.environ["GLOBAL_DEFAULT_TIMEOUT"] = "45"
        config = ClientConfig(remote_server_addr="http://localhost:4444")
        assert config.timeout == 45
    finally:
        if original_env is None:
            del os.environ["GLOBAL_DEFAULT_TIMEOUT"]
        else:
            os.environ["GLOBAL_DEFAULT_TIMEOUT"] = original_env


def test_timeout_from_parameter():
    """Test that timeout is set from constructor parameter when no env variable."""
    original_env = os.environ.get("GLOBAL_DEFAULT_TIMEOUT")
    try:
        if "GLOBAL_DEFAULT_TIMEOUT" in os.environ:
            del os.environ["GLOBAL_DEFAULT_TIMEOUT"]
        config = ClientConfig(remote_server_addr="http://localhost:4444", timeout=30)
        assert config.timeout == 30
    finally:
        if original_env is not None:
            os.environ["GLOBAL_DEFAULT_TIMEOUT"] = original_env


def test_timeout_fallback_to_socket_default():
    """Test that timeout falls back to socket default when no env variable or parameter."""
    original_env = os.environ.get("GLOBAL_DEFAULT_TIMEOUT")
    original_timeout = socket.getdefaulttimeout()
    try:
        if "GLOBAL_DEFAULT_TIMEOUT" in os.environ:
            del os.environ["GLOBAL_DEFAULT_TIMEOUT"]
        socket.setdefaulttimeout(60)
        config = ClientConfig(remote_server_addr="http://localhost:4444")
        assert config.timeout == 60
    finally:
        if original_env is not None:
            os.environ["GLOBAL_DEFAULT_TIMEOUT"] = original_env
        socket.setdefaulttimeout(original_timeout)


def test_env_variable_priority():
    """Test that GLOBAL_DEFAULT_TIMEOUT takes priority over parameter."""
    original_env = os.environ.get("GLOBAL_DEFAULT_TIMEOUT")
    try:
        os.environ["GLOBAL_DEFAULT_TIMEOUT"] = "45"
        config = ClientConfig(remote_server_addr="http://localhost:4444", timeout=30)
        assert config.timeout == 45
    finally:
        if original_env is None:
            del os.environ["GLOBAL_DEFAULT_TIMEOUT"]
        else:
            os.environ["GLOBAL_DEFAULT_TIMEOUT"] = original_env


def test_reset_timeout_with_env_variable():
    """Test that reset_timeout() uses GLOBAL_DEFAULT_TIMEOUT when set."""
    original_env = os.environ.get("GLOBAL_DEFAULT_TIMEOUT")
    try:
        os.environ["GLOBAL_DEFAULT_TIMEOUT"] = "45"
        config = ClientConfig(remote_server_addr="http://localhost:4444", timeout=30)
        config.reset_timeout()
        assert config.timeout == 45
    finally:
        if original_env is None:
            del os.environ["GLOBAL_DEFAULT_TIMEOUT"]
        else:
            os.environ["GLOBAL_DEFAULT_TIMEOUT"] = original_env


def test_reset_timeout_without_env_variable():
    """Test that reset_timeout() uses socket default when no env variable."""
    original_env = os.environ.get("GLOBAL_DEFAULT_TIMEOUT")
    original_timeout = socket.getdefaulttimeout()
    try:
        if "GLOBAL_DEFAULT_TIMEOUT" in os.environ:
            del os.environ["GLOBAL_DEFAULT_TIMEOUT"]
        socket.setdefaulttimeout(60)
        config = ClientConfig(remote_server_addr="http://localhost:4444", timeout=30)
        config.reset_timeout()
        assert config.timeout == 60
    finally:
        if original_env is not None:
            os.environ["GLOBAL_DEFAULT_TIMEOUT"] = original_env
        socket.setdefaulttimeout(original_timeout)
