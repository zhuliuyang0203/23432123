# frozen_string_literal: true

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

module Selenium
  module WebDriver
    class BiDi
      autoload :Session, 'selenium/webdriver/bidi/session'
      autoload :LogInspector, 'selenium/webdriver/bidi/log_inspector'
      autoload :LogHandler, 'selenium/webdriver/bidi/log_handler'
      autoload :BrowsingContext, 'selenium/webdriver/bidi/browsing_context'
      autoload :Struct, 'selenium/webdriver/bidi/struct'
      autoload :Network, 'selenium/webdriver/bidi/network'
      autoload :InterceptedRequest, 'selenium/webdriver/bidi/network/intercepted_request'
      autoload :InterceptedResponse, 'selenium/webdriver/bidi/network/intercepted_response'
      autoload :InterceptedAuth, 'selenium/webdriver/bidi/network/intercepted_auth'
      autoload :InterceptedItem, 'selenium/webdriver/bidi/network/intercepted_item'

      # @rbs (url: String) -> void
      def initialize(url:)
        @ws = WebSocketConnection.new(url: url)
      end

      # @rbs () -> nil
      def close
        @ws.close
      end

      # @rbs () -> Hash[untyped, untyped]
      def callbacks
        @ws.callbacks
      end

      # @rbs (String) -> void
      def add_callback(event, &block)
        @ws.add_callback(event, &block)
      end

      # @rbs (String, Integer) -> void
      def remove_callback(event, id)
        @ws.remove_callback(event, id)
      end

      # @rbs () -> Selenium::WebDriver::BiDi::Session
      def session
        @session ||= Session.new(self)
      end

      # @rbs (String, **String | String | Integer | String | bool) -> Hash[untyped, untyped]
      def send_cmd(method, **params)
        data = {method: method, params: params.compact}
        message = @ws.send_cmd(**data)
        raise Error::WebDriverError, error_message(message) if message['error']

        message['result']
      end

      # @rbs (Hash[untyped, untyped]) -> String
      def error_message(message)
        "#{message['error']}: #{message['message']}\n#{message['stacktrace']}"
      end
    end # BiDi
  end # WebDriver
end # Selenium
