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
from collections import defaultdict

from selenium.webdriver.common.bidi import network
from selenium.webdriver.common.bidi.browsing_context import Navigate
from selenium.webdriver.common.bidi.browsing_context import NavigateParameters
from selenium.webdriver.common.bidi.network import AddInterceptParameters
from selenium.webdriver.common.bidi.network import BeforeRequestSent
from selenium.webdriver.common.bidi.network import BeforeRequestSentParameters
from selenium.webdriver.common.bidi.network import ContinueRequestParameters
from selenium.webdriver.common.bidi.session import session_subscribe
from selenium.webdriver.common.bidi.session import session_unsubscribe


class Network:
    def __init__(self, conn, driver):
        self.intercepts = defaultdict(lambda: {"event": None, "handlers": []})
        self.callback_ids = {}
        self.driver = driver
        self.conn = conn
        self.bidi_network = network.Network(self.conn)

        self.remove_request_handler = self.remove_intercept
        self.clear_request_handlers = self.clear_intercepts

    def get(self, url, wait="none"):
        params = NavigateParameters(context=self.driver.current_window_handle, url=url, wait=wait)
        self.conn.execute(Navigate(params).cmd())

    def add_handler(self, event, handler, urlPatterns=None):
        event_name = event.event_class
        phase_name = event_name.split(".")[-1]

        self.conn.execute(session_subscribe(event_name))

        params = AddInterceptParameters(phases=[phase_name], urlPatterns=urlPatterns)
        result = self.bidi_network.add_intercept(params)
        intercept = result["intercept"]

        self.intercepts[intercept]["event"] = event
        self.intercepts[intercept]["handlers"].append(handler)
        if not self.callback_ids.get(event_name):
            self.callback_ids[event_name] = self.conn.add_callback(event, self.handle_events)
        return intercept

    def add_request_handler(self, handler, urlPatterns=None):
        intercept = self.add_handler(BeforeRequestSent, handler, urlPatterns)
        return intercept

    def handle_events(self, event):
        event_params = event.params
        if isinstance(event_params, BeforeRequestSentParameters) and event_params.isBlocked:
            json = self.handle_requests(event_params)
            params = ContinueRequestParameters(**json)
            self.bidi_network.continue_request(params)

    def handle_requests(self, params):
        request = params.request
        for intercept in params.intercepts:
            for handler in self.intercepts[intercept]["handlers"]:
                request = handler(request)
        return request

    def remove_intercept(self, intercept):
        self.bidi_network.remove_intercept(
            params=network.RemoveInterceptParameters(intercept),
        )

        event = self.intercepts.pop(intercept)["event"]
        event_name = event.event_class

        remaining = [i for i in self.intercepts.values() if i["event"].event_class == event_name]
        if len(remaining) == 0:
            self.conn.execute(session_unsubscribe(event_name))
            callback_id = self.callback_ids.pop(event_name)
            self.conn.remove_callback(event, callback_id)

    def clear_intercepts(self):
        for intercept in self.intercepts:
            self.remove_intercept(intercept)
