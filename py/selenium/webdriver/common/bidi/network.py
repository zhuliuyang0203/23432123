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

from .session import session_subscribe
from .session import session_unsubscribe


class Network:
    EVENTS = {
        'before_request': 'network.beforeRequestSent',
        'response_started': 'network.responseStarted',
        'response_completed': 'network.responseCompleted',
        'auth_required': 'network.authRequired',
        'fetch_error': 'network.fetchError'
    }

    PHASES = {
        'before_request': 'beforeRequestSent',
        'response_started': 'responseStarted',
        'auth_required': 'authRequired'
    }

    def __init__(self, conn):
        self.conn = conn
        self.callbacks = {}
        self.subscriptions = {
            'network.responseStarted': [],
            'network.beforeRequestSent': [],
            'network.authRequired': []
        }


    def command_iterator(self, command):
        """Generator to yield command."""
        yield command

    def has_callbacks(self):
        """Checks if there are any callbacks set."""
        return len(self.callbacks) > 0

    def __add_intercept(self, phases=None, contexts=None, url_patterns=None):
        """Add an intercept to the network."""
        if phases is None:
            phases = []
        if contexts is None and url_patterns is None:
            params = {
                'phases': phases,
            }
        elif contexts is None:
            params = {
                'phases': phases,
                'urlPatterns': url_patterns
            }
        elif url_patterns is None:
            params = {
                'phases': phases,
                'contexts': contexts
            }
        else:
            params = {
                'phases': phases,
                'contexts': contexts,
                'urlPatterns': url_patterns
            }
        command = {'method': 'network.addIntercept', 'params': params}
        self.conn.execute(self.command_iterator(command))

    def __remove_intercept(self, intercept=None, request_id=None):
        """Remove an intercept from the network."""
        if request_id is not None:
            command = {'method': 'network.removeIntercept', 'requestId': request_id}
            self.conn.execute(self.command_iterator(command))
        elif intercept is not None:
            command = {'method': 'network.removeIntercept', 'intercept': intercept}
            self.conn.execute(self.command_iterator(command))
        else:
            raise ValueError('Either requestId or intercept must be specified')

    def __continue_with_auth(self, request_id, username, password):
        """Continue with authentication."""
        command = {'method': 'network.continueWithAuth', 'params': 
                    {
                        'request': request_id,
                        'action': 'provideCredentials',
                        'credentials': {
                            'type': 'password',
                            'username': username,
                            'password': password
                        }
                    }
        }
        self.conn.execute(self.command_iterator(command))

    def __on(self, event, callback):
        """Set a callback function to subscribe to a network event."""
        event = self.EVENTS.get(event, event)
        self.callbacks[event] = callback
        if len(self.subscriptions[event]) == 0:
            # session_subscribe(self.conn, event, self.__handle_event)
            return self.conn.add_callback(event, self.__handle_event)

    def __handle_event(self, event, data):
        """Perform callback function on event."""
        if event in self.callbacks:
            self.callbacks[event](data)

    def add_authentication_handler(self, username, password):
        """Adds an authentication handler."""
        self.__add_intercept(phases=[self.PHASES['auth_required']])
        self.__on('auth_required', lambda data: self.__continue_with_auth(data['request']['request'], username, password))
        self.subscriptions['auth_required'] = [username, password]

    def remove_authentication_handler(self):
        """Removes an authentication handler."""
        self.__remove_intercept(intercept='auth_required')
        del self.subscriptions['auth_required']
        session_unsubscribe(self.conn, self.EVENTS['auth_required'])

    def add_request_handler(self, callback, url_pattern=''):
        """Adds a request handler that executes a callback function when a
        request matches the given URL pattern.

        Parameters:
            callback (function): A function to be executed when url is matched by a URL pattern
                The callback function receives a `Response` object as its argument.
            url_pattern (str, optional): A substring to match against the response URL.
                Default is an empty string, which matches all URLs.

        Returns:
            str: The request ID of the intercepted response.
        """
        self.__add_intercept(phases=[self.PHASES['before_request']])
        request_id = None
        def callback_on_url_match(data):
            nonlocal request_id
            if url_pattern in data['request']['url']:
                # create request object to pass to callback
                request_id = data['request'].get('requestId', None)
                url = data['request'].get('url')
                method = data['request'].get('method')
                headers = data['request'].get('headers', {})
                body = data['request'].get('body', None)
                request = Request(request_id, url, method, headers, body, self)
                callback(request)
        request_id = self.__on('before_request', callback_on_url_match)
        self.callbacks[request_id] = callback
        if 'before_request' not in self.subscriptions or not self.subscriptions.get('before_request'):
            self.subscriptions['before_request'] = [request_id]
        else:
            self.subscriptions['before_request'].append(request_id)
        return request_id

    def remove_request_handler(self, request_id):
        """Removes a request handler."""
        self.__remove_intercept(request_id=request_id)
        self.subscriptions['before_request'].remove(request_id)
        del self.callbacks[request_id]
        if len(self.subscriptions['before_request']) == 0:
            session_unsubscribe(self.conn, self.EVENTS['before_request']) 

    def add_response_handler(self, callback, url_pattern=''):
        """Adds a response handler that executes a callback function when a
        response matches the given URL pattern.

        Parameters:
            callback (function): A function to be executed when url is matched by a url_pattern
                The callback function receives a `Response` object as its argument.
            url_pattern (str, optional): A substring to match against the response URL.
                Default is an empty string, which matches all URLs.

        Returns:
            str: The request ID of the intercepted response.
        """
        self.__add_intercept(phases=[self.PHASES['response_started']])
        request_id = None
        def callback_on_url_match(data):
            # create response object to pass to callback
            nonlocal request_id
            if url_pattern in data['response']['url']:
                request_id = data['request'].get('requestId', None)
                url = data['response'].get('url')
                status_code = data['response'].get('status')
                body = data['response'].get('body', None)
                headers = data['response'].get('headers', {})
                response = Response(request_id, url, status_code, headers, body, self)
                callback(response)
        request_id = self.__on('response_started', callback_on_url_match)
        self.callbacks[request_id] = callback
        if 'response_started' not in self.subscriptions or not self.subscriptions.get('response_started'):
            self.subscriptions['response_started'] = [request_id]
        else:
            self.subscriptions['response_started'].append(request_id)
        return request_id

    def remove_response_handler(self, response_id):
        """Removes a response handler."""
        self.__remove_intercept(request_id=response_id)
        self.subscriptions['response_started'].remove(response_id)
        del self.callbacks[response_id]
        if len(self.subscriptions['response_started']) == 0:
            session_unsubscribe(self.conn, self.EVENTS['response_started']) 

class Request:
    def __init__(self, request_id, url, method, headers, body, network: Network):
        self.request_id = request_id
        self.url = url
        self.method = method
        self.headers = headers
        self.body = body
        self.network = network

    def continue_request(self):
        """Continue after sending a request."""
        params = {
            'requestId': self.request_id
        }
        if self.url is not None:
            params['url'] = self.url
        if self.method is not None:
            params['method'] = self.method
        if self.headers is not None:
            params['headers'] = self.headers
        if self.body is not None:
            params['body'] = self.body
        command = {'method': 'network.continueRequest', 'params': params}
        self.network.conn.execute(self.command_iterator(command))

class Response:
    def __init__(self, request_id, url, status_code, headers, body, network: Network):
        self.request_id = request_id
        self.url = url
        self.status_code = status_code
        self.headers = headers
        self.body = body
        self.network = network

    def continue_response(self):
        """Continue after receiving a response."""
        params = {
            'requestId': self.request_id,
            'status': self.status_code
        }
        if self.headers is not None:
            params['headers'] = self.headers
        if self.body is not None:
            params['body'] = self.body
        command = {'method': 'network.continueResponse', 'params': params}
        self.network.conn.execute(self.command_iterator(command))
