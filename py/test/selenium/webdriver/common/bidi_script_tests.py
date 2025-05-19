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

from selenium.webdriver.common.bidi.log import LogLevel
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait


def test_logs_console_messages(driver, pages):
    pages.load("bidi/logEntryAdded.html")

    log_entries = []
    driver.script.add_console_message_handler(log_entries.append)

    driver.find_element(By.ID, "jsException").click()
    driver.find_element(By.ID, "consoleLog").click()

    WebDriverWait(driver, 5).until(lambda _: log_entries)

    log_entry = log_entries[0]
    assert log_entry.level == LogLevel.INFO
    assert log_entry.method == "log"
    assert log_entry.text == "Hello, world!"
    assert log_entry.type_ == "console"


def test_logs_console_errors(driver, pages):
    pages.load("bidi/logEntryAdded.html")
    log_entries = []

    def log_error(entry):
        if entry.level == "error":
            log_entries.append(entry)

    driver.script.add_console_message_handler(log_error)

    driver.find_element(By.ID, "consoleLog").click()
    driver.find_element(By.ID, "consoleError").click()

    WebDriverWait(driver, 5).until(lambda _: log_entries)

    assert len(log_entries) == 1

    log_entry = log_entries[0]
    assert log_entry.level == LogLevel.ERROR
    assert log_entry.method == "error"
    assert log_entry.text == "I am console error"
    assert log_entry.type_ == "console"


def test_logs_multiple_console_messages(driver, pages):
    pages.load("bidi/logEntryAdded.html")

    log_entries = []
    driver.script.add_console_message_handler(log_entries.append)
    driver.script.add_console_message_handler(log_entries.append)

    driver.find_element(By.ID, "jsException").click()
    driver.find_element(By.ID, "consoleLog").click()

    WebDriverWait(driver, 5).until(lambda _: len(log_entries) > 1)
    assert len(log_entries) == 2


def test_removes_console_message_handler(driver, pages):
    pages.load("bidi/logEntryAdded.html")

    log_entries1 = []
    log_entries2 = []

    id = driver.script.add_console_message_handler(log_entries1.append)
    driver.script.add_console_message_handler(log_entries2.append)

    driver.find_element(By.ID, "consoleLog").click()
    WebDriverWait(driver, 5).until(lambda _: len(log_entries1) and len(log_entries2))

    driver.script.remove_console_message_handler(id)
    driver.find_element(By.ID, "consoleLog").click()

    WebDriverWait(driver, 5).until(lambda _: len(log_entries2) == 2)
    assert len(log_entries1) == 1


def test_javascript_error_messages(driver, pages):
    pages.load("bidi/logEntryAdded.html")

    log_entries = []
    driver.script.add_javascript_error_handler(log_entries.append)

    driver.find_element(By.ID, "jsException").click()
    WebDriverWait(driver, 5).until(lambda _: log_entries)

    log_entry = log_entries[0]
    assert log_entry.text == "Error: Not working"
    assert log_entry.level == LogLevel.ERROR
    assert log_entry.type_ == "javascript"


def test_removes_javascript_message_handler(driver, pages):
    pages.load("bidi/logEntryAdded.html")

    log_entries1 = []
    log_entries2 = []

    id = driver.script.add_javascript_error_handler(log_entries1.append)
    driver.script.add_javascript_error_handler(log_entries2.append)

    driver.find_element(By.ID, "jsException").click()
    WebDriverWait(driver, 5).until(lambda _: len(log_entries1) and len(log_entries2))

    driver.script.remove_javascript_error_handler(id)
    driver.find_element(By.ID, "jsException").click()

    WebDriverWait(driver, 5).until(lambda _: len(log_entries2) == 2)
    assert len(log_entries1) == 1
