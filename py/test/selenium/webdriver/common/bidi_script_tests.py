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

from selenium.webdriver.common.bidi.log import LogLevel
from selenium.webdriver.common.bidi.script import RealmType, ResultOwnership
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait


def has_shadow_root(node):
    if isinstance(node, dict):
        shadow_root = node.get("shadowRoot")
        if shadow_root and isinstance(shadow_root, dict):
            return True

        children = node.get("children", [])
        for child in children:
            if "value" in child and has_shadow_root(child["value"]):
                return True

    return False


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


def test_add_preload_script(driver, pages):
    """Test adding a preload script."""
    function_declaration = "() => { window.preloadExecuted = true; }"

    script_id = driver.script._add_preload_script(function_declaration)
    assert script_id is not None
    assert isinstance(script_id, str)

    # Navigate to a page to trigger the preload script
    pages.load("blank.html")

    # Check if the preload script was executed
    result = driver.script._evaluate(
        "window.preloadExecuted", {"context": driver.current_window_handle}, await_promise=False
    )
    assert result.result["value"] is True


def test_add_preload_script_with_arguments(driver, pages):
    """Test adding a preload script with channel arguments."""
    function_declaration = "(channelFunc) => { channelFunc('test_value'); window.preloadValue = 'received'; }"

    arguments = [{"type": "channel", "value": {"channel": "test-channel", "ownership": "root"}}]

    script_id = driver.script._add_preload_script(function_declaration, arguments=arguments)
    assert script_id is not None

    pages.load("blank.html")

    result = driver.script._evaluate(
        "window.preloadValue", {"context": driver.current_window_handle}, await_promise=False
    )
    assert result.result["value"] == "received"


def test_add_preload_script_with_contexts(driver, pages):
    """Test adding a preload script with specific contexts."""
    function_declaration = "() => { window.contextSpecific = true; }"
    contexts = [driver.current_window_handle]

    script_id = driver.script._add_preload_script(function_declaration, contexts=contexts)
    assert script_id is not None

    pages.load("blank.html")

    result = driver.script._evaluate(
        "window.contextSpecific", {"context": driver.current_window_handle}, await_promise=False
    )
    assert result.result["value"] is True


def test_add_preload_script_with_user_contexts(driver, pages):
    """Test adding a preload script with user contexts."""
    function_declaration = "() => { window.contextSpecific = true; }"
    user_context = driver.browser.create_user_context()

    context1 = driver.browsing_context.create(type="window", user_context=user_context)
    driver.switch_to.window(context1)

    user_contexts = [user_context]

    script_id = driver.script._add_preload_script(function_declaration, user_contexts=user_contexts)
    assert script_id is not None

    pages.load("blank.html")

    result = driver.script._evaluate(
        "window.contextSpecific", {"context": driver.current_window_handle}, await_promise=False
    )
    assert result.result["value"] is True


def test_add_preload_script_with_sandbox(driver, pages):
    """Test adding a preload script with sandbox."""
    function_declaration = "() => { window.sandboxScript = true; }"

    script_id = driver.script._add_preload_script(function_declaration, sandbox="test-sandbox")
    assert script_id is not None

    pages.load("blank.html")

    # calling evaluate without sandbox should return undefined
    result = driver.script._evaluate(
        "window.sandboxScript", {"context": driver.current_window_handle}, await_promise=False
    )
    assert result.result["type"] == "undefined"

    # calling evaluate within the sandbox should return True
    result = driver.script._evaluate(
        "window.sandboxScript",
        {"context": driver.current_window_handle, "sandbox": "test-sandbox"},
        await_promise=False,
    )
    assert result.result["value"] is True


def test_add_preload_script_invalid_arguments(driver):
    """Test that providing both contexts and user_contexts raises an error."""
    function_declaration = "() => {}"

    with pytest.raises(ValueError, match="Cannot specify both contexts and user_contexts"):
        driver.script._add_preload_script(function_declaration, contexts=["context1"], user_contexts=["user1"])


def test_remove_preload_script(driver, pages):
    """Test removing a preload script."""
    function_declaration = "() => { window.removableScript = true; }"

    script_id = driver.script._add_preload_script(function_declaration)
    driver.script._remove_preload_script(script_id=script_id)

    # Navigate to a page after removing the script
    pages.load("blank.html")

    # The script should not have executed
    result = driver.script._evaluate(
        "typeof window.removableScript", {"context": driver.current_window_handle}, await_promise=False
    )
    assert result.result["value"] == "undefined"


def test_evaluate_expression(driver, pages):
    """Test evaluating a simple expression."""
    pages.load("blank.html")

    result = driver.script._evaluate("1 + 2", {"context": driver.current_window_handle}, await_promise=False)

    assert result.realm is not None
    assert result.result["type"] == "number"
    assert result.result["value"] == 3
    assert result.exception_details is None


def test_evaluate_with_await_promise(driver, pages):
    """Test evaluating an expression that returns a promise."""
    pages.load("blank.html")

    result = driver.script._evaluate(
        "Promise.resolve(42)", {"context": driver.current_window_handle}, await_promise=True
    )

    assert result.result["type"] == "number"
    assert result.result["value"] == 42


def test_evaluate_with_exception(driver, pages):
    """Test evaluating an expression that throws an exception."""
    pages.load("blank.html")

    result = driver.script._evaluate(
        "throw new Error('Test error')", {"context": driver.current_window_handle}, await_promise=False
    )

    assert result.exception_details is not None
    assert "Test error" in str(result.exception_details)


def test_evaluate_with_result_ownership(driver, pages):
    """Test evaluating with different result ownership settings."""
    pages.load("blank.html")

    # Test with ROOT ownership
    result = driver.script._evaluate(
        "({ test: 'value' })",
        {"context": driver.current_window_handle},
        await_promise=False,
        result_ownership=ResultOwnership.ROOT,
    )

    # ROOT result ownership should return a handle
    assert "handle" in result.result

    # Test with NONE ownership
    result = driver.script._evaluate(
        "({ test: 'value' })",
        {"context": driver.current_window_handle},
        await_promise=False,
        result_ownership=ResultOwnership.NONE,
    )

    assert "handle" not in result.result
    assert result.result is not None


def test_evaluate_with_serialization_options(driver, pages):
    """Test evaluating with serialization options."""
    pages.load("shadowRootPage.html")

    serialization_options = {"maxDomDepth": 2, "maxObjectDepth": 2, "includeShadowTree": "all"}

    result = driver.script._evaluate(
        "document.body",
        {"context": driver.current_window_handle},
        await_promise=False,
        serialization_options=serialization_options,
    )
    root_node = result.result["value"]

    # maxDomDepth will contain a children property
    assert "children" in result.result["value"]
    # the page will have atleast one shadow root
    assert has_shadow_root(root_node)


def test_evaluate_with_user_activation(driver, pages):
    """Test evaluating with user activation."""
    pages.load("blank.html")

    result = driver.script._evaluate(
        "navigator.userActivation ? navigator.userActivation.isActive : false",
        {"context": driver.current_window_handle},
        await_promise=False,
        user_activation=True,
    )

    # the value should be True if user activation is active
    assert result.result["value"] is True


def test_call_function(driver, pages):
    """Test calling a function."""
    pages.load("blank.html")

    result = driver.script._call_function(
        "(a, b) => a + b",
        await_promise=False,
        target={"context": driver.current_window_handle},
        arguments=[{"type": "number", "value": 5}, {"type": "number", "value": 3}],
    )

    assert result.result["type"] == "number"
    assert result.result["value"] == 8


def test_call_function_with_this(driver, pages):
    """Test calling a function with a specific 'this' value."""
    pages.load("blank.html")

    # First set up an object
    driver.script._evaluate(
        "window.testObj = { value: 10 }", {"context": driver.current_window_handle}, await_promise=False
    )

    result = driver.script._call_function(
        "function() { return this.value; }",
        await_promise=False,
        target={"context": driver.current_window_handle},
        this={"type": "object", "value": [["value", {"type": "number", "value": 20}]]},
    )

    assert result.result["type"] == "number"
    assert result.result["value"] == 20


def test_call_function_with_user_activation(driver, pages):
    """Test calling a function with user activation."""
    pages.load("blank.html")

    result = driver.script._call_function(
        "() => navigator.userActivation ? navigator.userActivation.isActive : false",
        await_promise=False,
        target={"context": driver.current_window_handle},
        user_activation=True,
    )

    # the value should be True if user activation is active
    assert result.result["value"] is True


def test_call_function_with_serialization_options(driver, pages):
    """Test calling a function with serialization options."""
    pages.load("shadowRootPage.html")

    serialization_options = {"maxDomDepth": 2, "maxObjectDepth": 2, "includeShadowTree": "all"}

    result = driver.script._call_function(
        "() => document.body",
        await_promise=False,
        target={"context": driver.current_window_handle},
        serialization_options=serialization_options,
    )

    root_node = result.result["value"]

    # maxDomDepth will contain a children property
    assert "children" in result.result["value"]
    # the page will have atleast one shadow root
    assert has_shadow_root(root_node)


def test_call_function_with_exception(driver, pages):
    """Test calling a function that throws an exception."""
    pages.load("blank.html")

    result = driver.script._call_function(
        "() => { throw new Error('Function error'); }",
        await_promise=False,
        target={"context": driver.current_window_handle},
    )

    assert result.exception_details is not None
    assert "Function error" in str(result.exception_details)


def test_call_function_with_await_promise(driver, pages):
    """Test calling a function that returns a promise."""
    pages.load("blank.html")

    result = driver.script._call_function(
        "() => Promise.resolve('async result')", await_promise=True, target={"context": driver.current_window_handle}
    )

    assert result.result["type"] == "string"
    assert result.result["value"] == "async result"


def test_call_function_with_result_ownership(driver, pages):
    """Test calling a function with different result ownership settings."""
    pages.load("blank.html")

    # Call a function that returns an object with ownership "root"
    result = driver.script._call_function(
        "function() { return { greet: 'Hi', number: 42 }; }",
        await_promise=False,
        target={"context": driver.current_window_handle},
        result_ownership="root",
    )

    # Verify that a handle is returned
    assert result.result["type"] == "object"
    assert "handle" in result.result
    handle = result.result["handle"]

    # Use the handle in another function call
    result2 = driver.script._call_function(
        "function() { return this.number + 1; }",
        await_promise=False,
        target={"context": driver.current_window_handle},
        this={"handle": handle},
    )

    assert result2.result["type"] == "number"
    assert result2.result["value"] == 43


def test_get_realms(driver, pages):
    """Test getting all realms."""
    pages.load("blank.html")

    realms = driver.script._get_realms()

    assert len(realms) > 0
    assert all(hasattr(realm, "realm") for realm in realms)
    assert all(hasattr(realm, "origin") for realm in realms)
    assert all(hasattr(realm, "type") for realm in realms)


def test_get_realms_filtered_by_context(driver, pages):
    """Test getting realms filtered by context."""
    pages.load("blank.html")

    realms = driver.script._get_realms(context=driver.current_window_handle)

    assert len(realms) > 0
    # All realms should be associated with the specified context
    for realm in realms:
        if realm.context is not None:
            assert realm.context == driver.current_window_handle


def test_get_realms_filtered_by_type(driver, pages):
    """Test getting realms filtered by type."""
    pages.load("blank.html")

    realms = driver.script._get_realms(type=RealmType.WINDOW)

    assert len(realms) > 0
    # All realms should be of the WINDOW type
    for realm in realms:
        assert realm.type == RealmType.WINDOW


def test_disown_handles(driver, pages):
    """Test disowning handles."""
    pages.load("blank.html")

    # Create an object with root ownership (this will return a handle)
    result = driver.script._evaluate(
        "({foo: 'bar'})", target={"context": driver.current_window_handle}, await_promise=False, result_ownership="root"
    )

    handle = result.result["handle"]
    assert handle is not None

    # Use the handle in a function call (this should succeed)
    result_before = driver.script._call_function(
        "function(obj) { return obj.foo; }",
        await_promise=False,
        target={"context": driver.current_window_handle},
        arguments=[{"handle": handle}],
    )

    assert result_before.result["value"] == "bar"

    # Disown the handle
    driver.script._disown(handles=[handle], target={"context": driver.current_window_handle})

    # Try using the disowned handle (this should fail)
    with pytest.raises(Exception):
        driver.script._call_function(
            "function(obj) { return obj.foo; }",
            await_promise=False,
            target={"context": driver.current_window_handle},
            arguments=[{"handle": handle}],
        )
