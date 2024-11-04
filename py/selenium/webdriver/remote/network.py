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

import trio

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
    def __init__(self, driver):
        self.driver = driver
        self.listeners = {}
        self.intercepts = defaultdict(lambda: {"event_name": None, "handlers": []})
        self.bidi_network = None
        self.conn = None

        self.remove_request_handler = self.remove_intercept
        self.clear_request_handlers = self.clear_intercepts

    async def get(self, url, conn, wait="complete"):
        params = NavigateParameters(context=self.driver.current_window_handle, url=url, wait=wait)
        await conn.execute(Navigate(params).cmd())

    async def add_listener(self, event, callback):
        event_name = event.event_class
        if event_name in self.listeners:
            return
        self.listeners[event_name] = self.conn.listen(event)
        try:
            async for event in self.listeners[event_name]:
                request_data = event.params
                if request_data.isBlocked:
                    await callback(request_data)
        except trio.ClosedResourceError:
            pass

    async def add_handler(self, event, handler, urlPatterns=None, conn=None, task_status=trio.TASK_STATUS_IGNORED):
        if not self.conn:
            self.conn = conn
            self.bidi_network = network.Network(conn)

        event_name = event.event_class
        phase_name = event_name.split(".")[-1]

        await self.conn.execute(session_subscribe(event_name))

        params = AddInterceptParameters(phases=[phase_name], urlPatterns=urlPatterns)
        result = await self.bidi_network.add_intercept(params)
        intercept = result["intercept"]

        self.intercepts[intercept]["event_name"] = event_name
        self.intercepts[intercept]["handlers"].append(handler)
        task_status.started(intercept)
        await self.add_listener(event=event, callback=self.handle_events)

    async def add_request_handler(self, handler, urlPatterns=None, conn=None, task_status=trio.TASK_STATUS_IGNORED):
        intercept = await self.add_handler(BeforeRequestSent, handler, urlPatterns, conn, task_status)
        return intercept

    async def handle_events(self, event_params):
        if isinstance(event_params, BeforeRequestSentParameters):
            json = self.handle_requests(event_params)
            params = ContinueRequestParameters(**json)
            await self.bidi_network.continue_request(params)

    def handle_requests(self, params):
        request = params.request
        for intercept in params.intercepts:
            for handler in self.intercepts[intercept]["handlers"]:
                request = handler(request)
        return request

    async def remove_listener(self, event_name):
        listener = self.listeners.pop(event_name)
        listener.close()

    async def remove_intercept(self, intercept):
        await self.bidi_network.remove_intercept(
            params=network.RemoveInterceptParameters(intercept),
        )
        event_name = self.intercepts.pop(intercept)["event_name"]
        remaining = [i for i in self.intercepts.values() if i["event_name"] == event_name]
        if len(remaining) == 0:
            await self.remove_listener(event_name)
            await self.conn.execute(session_unsubscribe(event_name))

    async def clear_intercepts(self):
        for intercept in self.intercepts:
            await self.remove_intercept(intercept)

    async def disable_cache(self):
        # Bidi 'network.setCacheBehavior' is not implemented in v130
        self.driver.execute_cdp_cmd("Network.setCacheDisabled", {"cacheDisabled": True})
