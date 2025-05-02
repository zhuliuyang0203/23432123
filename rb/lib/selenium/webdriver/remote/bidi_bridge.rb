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
    module Remote
      class BiDiBridge < Bridge
        attr_reader :bidi

        # @rbs (Hash[untyped, untyped]) -> Selenium::WebDriver::BiDi
        def create_session(capabilities)
          super
          socket_url = @capabilities[:web_socket_url]
          @bidi = Selenium::WebDriver::BiDi.new(url: socket_url)
        end

        # @rbs (String) -> Hash[untyped, untyped]
        def get(url)
          browsing_context.navigate(url)
        end

        # @rbs () -> Hash[untyped, untyped]
        def go_back
          browsing_context.traverse_history(-1)
        end

        # @rbs () -> Hash[untyped, untyped]
        def go_forward
          browsing_context.traverse_history(1)
        end

        # @rbs () -> Hash[untyped, untyped]
        def refresh
          browsing_context.reload
        end

        # @rbs () -> nil
        def quit
          super
        ensure
          bidi.close
        end

        # @rbs () -> Array[untyped]
        def close
          execute(:close_window).tap { |handles| bidi.close if handles.empty? }
        end

        private

        # @rbs () -> Selenium::WebDriver::BiDi::BrowsingContext
        def browsing_context
          @browsing_context ||= WebDriver::BiDi::BrowsingContext.new(self)
        end
      end # BiDiBridge
    end # Remote
  end # WebDriver
end # Selenium
