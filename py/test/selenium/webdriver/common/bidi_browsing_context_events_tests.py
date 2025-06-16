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

import threading

import pytest

from selenium.webdriver.common.bidi.browsing_context import ReadinessState
from selenium.webdriver.common.by import By
from selenium.webdriver.common.window import WindowTypes
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait


def test_browsing_context_events_initialized(driver):
    """Test that the browsing context module is initialized and supports events."""
    assert driver.browsing_context is not None
    assert hasattr(driver.browsing_context, "on_browsing_context_created")
    assert hasattr(driver.browsing_context, "on_browsing_context_destroyed")
    assert hasattr(driver.browsing_context, "add_event_handler")
    assert hasattr(driver.browsing_context, "remove_event_handler")


def test_browsing_context_created_event_window(driver):
    """Test that context created events are fired when creating windows."""
    events_received = []
    event_received = threading.Event()

    def on_context_created(context_info):
        events_received.append(context_info)
        event_received.set()

    # Subscribe to context created events
    callback_id = driver.browsing_context.on_browsing_context_created(on_context_created)

    try:
        new_context_id = driver.browsing_context.create(type=WindowTypes.WINDOW)

        assert event_received.wait(timeout=10), "Context created event was not received"

        assert len(events_received) >= 1, "Should receive at least one context created event"

        # Check that one of the events matches the one we created
        new_context_events = [event for event in events_received if event.context == new_context_id]
        assert len(new_context_events) >= 1, f"Should receive event for new context {new_context_id}"

        context_info = new_context_events[0]
        assert context_info.context == new_context_id
        assert context_info.url == "about:blank"
        assert context_info.parent is None

        driver.browsing_context.close(new_context_id)

    finally:
        driver.browsing_context.remove_event_handler("context_created", callback_id)


def test_browsing_context_created_event_tab(driver):
    """Test that context created events are fired when creating tabs."""
    events_received = []
    event_received = threading.Event()

    def on_context_created(context_info):
        events_received.append(context_info)
        event_received.set()

    callback_id = driver.browsing_context.on_browsing_context_created(on_context_created)

    try:
        new_context_id = driver.browsing_context.create(type=WindowTypes.TAB)

        assert event_received.wait(timeout=10), "Context created event was not received"

        assert len(events_received) >= 1, "Should receive at least one context created event"

        new_context_events = [event for event in events_received if event.context == new_context_id]
        assert len(new_context_events) >= 1, f"Should receive event for new context {new_context_id}"

        context_info = new_context_events[0]
        assert context_info.context == new_context_id
        assert context_info.url == "about:blank"
        assert context_info.parent is None

        driver.browsing_context.close(new_context_id)

    finally:
        driver.browsing_context.remove_event_handler("context_created", callback_id)


@pytest.mark.xfail_firefox(reason="Fixed in Firefox 141 (nightly as of 2025-06-16)")
def test_browsing_context_destroyed_event(driver):
    """Test that context destroyed events are fired when closing contexts."""
    events_received = []
    event_received = threading.Event()

    def on_context_destroyed(context_info):
        events_received.append(context_info)
        event_received.set()

    new_context_id = driver.browsing_context.create(type=WindowTypes.WINDOW)

    callback_id = driver.browsing_context.on_browsing_context_destroyed(on_context_destroyed)

    try:
        driver.browsing_context.close(new_context_id)

        assert event_received.wait(timeout=10), "Context destroyed event was not received"

        assert len(events_received) == 1
        context_info = events_received[0]
        assert context_info.context == new_context_id

    finally:
        driver.browsing_context.remove_event_handler("context_destroyed", callback_id)


def test_navigation_started_event(driver, pages):
    """Test that navigation started events are fired when navigating."""
    events_received = []
    event_received = threading.Event()

    def on_navigation_started(navigation_info):
        events_received.append(navigation_info)
        event_received.set()

    callback_id = driver.browsing_context.on_navigation_started(on_navigation_started)

    try:
        context_id = driver.current_window_handle

        url = pages.url("bidi/logEntryAdded.html")
        driver.browsing_context.navigate(context=context_id, url=url, wait=ReadinessState.COMPLETE)

        assert event_received.wait(timeout=10), "Navigation started event was not received"

        assert len(events_received) == 1
        navigation_info = events_received[0]
        assert navigation_info.context == context_id
        assert "/bidi/logEntryAdded.html" in navigation_info.url

    finally:
        driver.browsing_context.remove_event_handler("navigation_started", callback_id)


def test_dom_content_loaded_event(driver, pages):
    """Test that DOM content loaded events are fired."""
    events_received = []
    event_received = threading.Event()

    def on_dom_content_loaded(navigation_info):
        events_received.append(navigation_info)
        event_received.set()

    callback_id = driver.browsing_context.on_dom_content_loaded(on_dom_content_loaded)

    try:
        context_id = driver.current_window_handle

        url = pages.url("bidi/logEntryAdded.html")
        driver.browsing_context.navigate(context=context_id, url=url, wait=ReadinessState.COMPLETE)

        assert event_received.wait(timeout=10), "DOM content loaded event was not received"

        assert len(events_received) == 1
        navigation_info = events_received[0]
        assert navigation_info.context == context_id
        assert "/bidi/logEntryAdded.html" in navigation_info.url

    finally:
        driver.browsing_context.remove_event_handler("dom_content_loaded", callback_id)


def test_browsing_context_loaded_event(driver, pages):
    """Test that browsing context loaded events are fired."""
    events_received = []
    event_received = threading.Event()

    def on_browsing_context_loaded(navigation_info):
        events_received.append(navigation_info)
        event_received.set()

    callback_id = driver.browsing_context.on_browsing_context_loaded(on_browsing_context_loaded)

    try:
        context_id = driver.current_window_handle

        url = pages.url("bidi/logEntryAdded.html")
        driver.browsing_context.navigate(context=context_id, url=url, wait=ReadinessState.COMPLETE)

        assert event_received.wait(timeout=10), "Browsing context loaded event was not received"

        assert len(events_received) == 1
        navigation_info = events_received[0]
        assert navigation_info.context == context_id
        assert "/bidi/logEntryAdded.html" in navigation_info.url

    finally:
        driver.browsing_context.remove_event_handler("load", callback_id)


def test_fragment_navigated_event(driver, pages):
    """Test that fragment navigated events are fired when navigating to anchors."""
    events_received = []
    event_received = threading.Event()

    def on_fragment_navigated(navigation_info):
        events_received.append(navigation_info)
        event_received.set()

    context_id = driver.current_window_handle

    # navigate to a page with an anchor
    url = pages.url("linked_image.html")
    driver.browsing_context.navigate(context=context_id, url=url, wait=ReadinessState.COMPLETE)

    callback_id = driver.browsing_context.on_fragment_navigated(on_fragment_navigated)

    try:
        # Navigate to an anchor on the page
        anchor_url = pages.url("linked_image.html#linkToAnchorOnThisPage")
        driver.browsing_context.navigate(context=context_id, url=anchor_url, wait=ReadinessState.COMPLETE)

        assert event_received.wait(timeout=10), "Fragment navigated event was not received"

        assert len(events_received) == 1
        navigation_info = events_received[0]
        assert navigation_info.context == context_id
        assert "linkToAnchorOnThisPage" in navigation_info.url

    finally:
        driver.browsing_context.remove_event_handler("fragment_navigated", callback_id)


def test_user_prompt_opened_event(driver, pages):
    """Test that user prompt opened events are fired when alerts are shown."""
    events_received = []
    event_received = threading.Event()

    def on_user_prompt_opened(prompt_params):
        events_received.append(prompt_params)
        event_received.set()

    callback_id = driver.browsing_context.on_user_prompt_opened(on_user_prompt_opened)

    try:
        context_id = driver.current_window_handle

        pages.load("alerts.html")

        # Trigger alert
        driver.find_element(By.ID, "alert").click()

        assert event_received.wait(timeout=10), "User prompt opened event was not received"

        assert len(events_received) == 1
        prompt_params = events_received[0]
        assert prompt_params.context == context_id
        assert prompt_params.type == "alert"
        assert events_received[0].message == "cheese"

        # accept the alert
        driver.browsing_context.handle_user_prompt(context=context_id, accept=True)

    finally:
        driver.browsing_context.remove_event_handler("user_prompt_opened", callback_id)


def test_user_prompt_closed_event(driver, pages):
    """Test that user prompt closed events are fired when prompts are handled."""
    events_received = []
    event_received = threading.Event()

    def on_user_prompt_closed(prompt_params):
        events_received.append(prompt_params)
        event_received.set()

    callback_id = driver.browsing_context.on_user_prompt_closed(on_user_prompt_closed)

    try:
        context_id = driver.current_window_handle

        pages.load("alerts.html")

        driver.find_element(By.ID, "prompt").click()

        WebDriverWait(driver, 5).until(EC.alert_is_present())

        # Handle the prompt
        driver.browsing_context.handle_user_prompt(context=context_id, accept=True, user_text="selenium")

        assert event_received.wait(timeout=10), "User prompt closed event was not received"

        assert len(events_received) == 1
        prompt_params = events_received[0]
        assert prompt_params.context == context_id
        assert prompt_params.accepted is True
        assert prompt_params.type == "prompt"
        assert prompt_params.user_text == "selenium"

    finally:
        driver.browsing_context.remove_event_handler("user_prompt_closed", callback_id)


def test_context_specific_event_subscription(driver, pages):
    """Test that events can be subscribed to for specific browsing contexts."""
    all_events = []
    specific_events = []
    all_event_received = threading.Event()
    specific_event_received = threading.Event()

    def on_all_contexts(prompt_params):
        all_events.append(prompt_params)
        all_event_received.set()

    def on_specific_context(prompt_params):
        specific_events.append(prompt_params)
        specific_event_received.set()

    # Create two new windows for testing
    target_context_id = driver.browsing_context.create(type=WindowTypes.WINDOW)
    other_context_id = driver.browsing_context.create(type=WindowTypes.WINDOW)

    # Subscribe to all contexts
    all_callback_id = driver.browsing_context.on_user_prompt_opened(on_all_contexts)

    # Subscribe to specific context only
    specific_callback_id = driver.browsing_context.on_user_prompt_opened(
        on_specific_context, contexts=[target_context_id]
    )

    try:
        # Navigate both contexts to the alerts page
        alerts_url = pages.url("alerts.html")
        driver.browsing_context.navigate(context=target_context_id, url=alerts_url, wait=ReadinessState.COMPLETE)
        driver.browsing_context.navigate(context=other_context_id, url=alerts_url, wait=ReadinessState.COMPLETE)

        # Trigger alert in the target context (should trigger both callbacks)
        driver.switch_to.window(target_context_id)
        driver.find_element(By.ID, "alert").click()

        assert all_event_received.wait(timeout=5), "All contexts event was not received for target context"
        assert specific_event_received.wait(timeout=5), "Specific context event was not received for target context"

        # Verify events from target context
        assert len(all_events) == 1, f"Expected 1 event in all_events, got {len(all_events)}"
        assert len(specific_events) == 1, f"Expected 1 event in specific_events, got {len(specific_events)}"
        assert all_events[0].context == target_context_id
        assert specific_events[0].context == target_context_id
        assert all_events[0].type == "alert"
        assert specific_events[0].type == "alert"

        # Accept the alert in target context
        driver.browsing_context.handle_user_prompt(context=target_context_id, accept=True)

        all_event_received.clear()
        specific_event_received.clear()

        # Trigger alert in the other context (should only trigger all_contexts callback)
        driver.switch_to.window(other_context_id)
        driver.find_element(By.ID, "alert").click()

        assert all_event_received.wait(timeout=5), "All contexts event was not received for other context"

        # Verify events - should have 2 in all_events, still 1 in specific_events
        assert len(all_events) == 2, f"Expected 2 events in all_events, got {len(all_events)}"
        assert len(specific_events) == 1, f"Expected 1 event in specific_events, got {len(specific_events)}"
        assert all_events[1].context == other_context_id
        assert all_events[1].type == "alert"

        driver.browsing_context.handle_user_prompt(context=other_context_id, accept=True)

        driver.browsing_context.close(target_context_id)
        driver.browsing_context.close(other_context_id)

    finally:
        driver.browsing_context.remove_event_handler("user_prompt_opened", all_callback_id)
        driver.browsing_context.remove_event_handler("user_prompt_opened", specific_callback_id)


def test_remove_event_handler(driver):
    """Test that event handlers can be properly removed."""
    events_received = []
    event_received = threading.Event()

    def on_context_created(context_info):
        events_received.append(context_info)
        event_received.set()

    callback_id = driver.browsing_context.on_browsing_context_created(on_context_created)

    # Create a window (should trigger the event)
    context_id1 = driver.browsing_context.create(type=WindowTypes.WINDOW)

    initial_events_received = len(events_received)

    assert event_received.wait(timeout=10), "First event was not received"
    assert initial_events_received >= 1

    # Remove the handler
    driver.browsing_context.remove_event_handler("context_created", callback_id)

    event_received.clear()

    # Creating another window should NOT trigger the event
    context_id2 = driver.browsing_context.create(type=WindowTypes.WINDOW)

    # Verify no additional events were received
    assert len(events_received) == initial_events_received

    driver.browsing_context.close(context_id1)
    driver.browsing_context.close(context_id2)


def test_clear_all_event_handlers(driver):
    """Test that all event handlers can be cleared at once."""
    events_received = []

    def on_context_created(context_info):
        events_received.append(("created", context_info))

    def on_context_destroyed(context_info):
        events_received.append(("destroyed", context_info))

    # Subscribe to multiple events
    driver.browsing_context.on_browsing_context_created(on_context_created)
    driver.browsing_context.on_browsing_context_destroyed(on_context_destroyed)

    # we will have 1 initial created event triggered by the driver object, so its not empty (len(events_received) is 1)
    initial_events_received = len(events_received)

    # Clear all handlers
    driver.browsing_context.clear_event_handlers()

    # Create and close a window (should NOT trigger any events)
    context_id = driver.browsing_context.create(type=WindowTypes.WINDOW)
    driver.browsing_context.close(context_id)

    # Verify no additional events were received
    assert len(events_received) == initial_events_received


def test_generic_add_event_handler_method(driver):
    """Test the generic add_event_handler method works for all event types."""
    events_received = []
    event_received = threading.Event()

    def on_event(data):
        events_received.append(data)
        event_received.set()

    # add a generic context_created event handler
    callback_id = driver.browsing_context.add_event_handler("context_created", on_event)

    try:
        new_context_id = driver.browsing_context.create(type=WindowTypes.WINDOW)

        assert event_received.wait(timeout=10), "Event was not received through generic handler"

        # Verify we received at least one event
        assert len(events_received) >= 1, "Should receive at least one context created event"

        # Check that one of the events is the newly created context
        new_context_events = [
            event for event in events_received if hasattr(event, "context") and event.context == new_context_id
        ]
        assert len(new_context_events) >= 1, f"Should receive event for new context {new_context_id}"

        context_info = new_context_events[0]
        assert hasattr(context_info, "context")
        assert context_info.context == new_context_id

        driver.browsing_context.close(new_context_id)

    finally:
        driver.browsing_context.remove_event_handler("context_created", callback_id)


@pytest.mark.xfail_firefox
def test_download_will_begin_event(driver, pages):
    """Test that download will begin events are fired when downloads start."""
    events_received = []
    event_received = threading.Event()

    def on_download_will_begin(download_params):
        events_received.append(download_params)
        event_received.set()

    callback_id = driver.browsing_context.on_download_will_begin(on_download_will_begin)

    try:
        context_id = driver.current_window_handle

        download_url = pages.url("downloads/download.html")
        driver.browsing_context.navigate(context=context_id, url=download_url)
        download_xpath_file_1_txt = '//*[@id="file-1"]'

        driver.find_element(By.XPATH, download_xpath_file_1_txt).click()

        assert event_received.wait(timeout=5)

        assert len(events_received) == 1
        download_params = events_received[0]
        assert download_params.context == context_id
        assert download_params.suggested_filename == "file_1.txt"

    finally:
        driver.browsing_context.remove_event_handler("download_will_begin", callback_id)


def test_multiple_navigation_events(driver, pages):
    """Test multiple navigation events in sequence."""
    navigation_events = []

    def on_navigation_event(event_type):
        def handler(navigation_info):
            navigation_events.append((event_type, navigation_info))

        return handler

    # Subscribe to multiple navigation events
    callbacks = [
        driver.browsing_context.on_navigation_started(on_navigation_event("started")),
        driver.browsing_context.on_dom_content_loaded(on_navigation_event("dom_loaded")),
        driver.browsing_context.on_browsing_context_loaded(on_navigation_event("loaded")),
    ]

    try:
        context_id = driver.current_window_handle

        url = pages.url("bidi/logEntryAdded.html")
        driver.browsing_context.navigate(context=context_id, url=url, wait=ReadinessState.COMPLETE)

        assert len(navigation_events) > 0, "No navigation events were received"

        # Check that we have at least one of each expected event type
        event_types = [event[0] for event in navigation_events]
        for expected in ["started", "dom_loaded", "loaded"]:
            assert expected in event_types, f"Navigation event '{expected}' was not received"

        # Verify all events are for the same context and URL
        for event_type, navigation_info in navigation_events:
            assert navigation_info.context == context_id
            assert "/bidi/logEntryAdded.html" in navigation_info.url

    finally:
        event_names = ["navigation_started", "dom_content_loaded", "load"]
        for i, callback_id in enumerate(callbacks):
            driver.browsing_context.remove_event_handler(event_names[i], callback_id)
