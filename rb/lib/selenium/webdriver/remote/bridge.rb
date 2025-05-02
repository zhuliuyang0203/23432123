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
      class Bridge
        autoload :COMMANDS, 'selenium/webdriver/remote/bridge/commands'
        autoload :LocatorConverter, 'selenium/webdriver/remote/bridge/locator_converter'

        include Atoms

        PORT = 4444

        attr_accessor :http, :file_detector
        attr_reader :capabilities

        class << self
          attr_reader :extra_commands
          attr_writer :element_class, :locator_converter

          def add_command(name, verb, url, &block)
            @extra_commands ||= {}
            @extra_commands[name] = [verb, url]
            define_method(name, &block)
          end

          # @rbs () -> Selenium::WebDriver::Remote::Bridge::LocatorConverter
          def locator_converter
            @locator_converter ||= LocatorConverter.new
          end

          # @rbs () -> Class
          def element_class
            @element_class ||= Element
          end
        end

        #
        # Initializes the bridge with the given server URL
        # @param [String, URI] url url for the remote server
        # @param [Object] http_client an HTTP client instance that implements the same protocol as Http::Default
        # @api private
        #

        # @rbs (url: URI::HTTP, ?http_client: nil) -> void
        def initialize(url:, http_client: nil)
          uri = url.is_a?(URI) ? url : URI.parse(url)
          uri.path += '/' unless uri.path.end_with?('/')

          @http = http_client || Http::Default.new
          @http.server_url = uri
          @file_detector = nil

          @locator_converter = self.class.locator_converter
        end

        #
        # Creates session.
        #

        # @rbs (Hash[untyped, untyped]) -> Selenium::WebDriver::Remote::Bridge
        def create_session(capabilities)
          response = execute(:new_session, {}, prepare_capabilities_payload(capabilities))

          @session_id = response['sessionId']
          capabilities = response['capabilities']

          raise Error::WebDriverError, 'no sessionId in returned payload' unless @session_id

          @capabilities = Capabilities.json_create(capabilities)

          case @capabilities[:browser_name]
          when 'chrome', 'chrome-headless-shell'
            extend(WebDriver::Chrome::Features)
          when 'firefox'
            extend(WebDriver::Firefox::Features)
          when 'msedge', 'MicrosoftEdge'
            extend(WebDriver::Edge::Features)
          when 'Safari', 'Safari Technology Preview'
            extend(WebDriver::Safari::Features)
          when 'internet explorer'
            extend(WebDriver::IE::Features)
          end
        end

        #
        # Returns the current session ID.
        #

        # @rbs () -> String
        def session_id
          @session_id || raise(Error::WebDriverError, 'no current session exists')
        end

        # @rbs () -> Symbol
        def browser
          @browser ||= begin
            name = @capabilities.browser_name
            name ? name.tr(' -', '_').downcase.to_sym : 'unknown'
          end
        end

        # @rbs () -> Hash[untyped, untyped]
        def status
          execute :status
        end

        # @rbs (String) -> nil
        def get(url)
          execute :get, {}, {url: url}
        end

        #
        # timeouts
        #

        # @rbs () -> Hash[untyped, untyped]
        def timeouts
          execute :get_timeouts, {}
        end

        # @rbs (Hash[untyped, untyped]) -> nil
        def timeouts=(timeouts)
          execute :set_timeout, {}, timeouts
        end

        #
        # alerts
        #

        # @rbs () -> nil
        def accept_alert
          execute :accept_alert
        end

        # @rbs () -> nil
        def dismiss_alert
          execute :dismiss_alert
        end

        # @rbs (String) -> nil
        def alert=(keys)
          execute :send_alert_text, {}, {value: keys.chars, text: keys}
        end

        # @rbs () -> String?
        def alert_text
          execute :get_alert_text
        end

        #
        # navigation
        #

        # @rbs () -> nil
        def go_back
          execute :back
        end

        # @rbs () -> nil
        def go_forward
          execute :forward
        end

        # @rbs () -> String
        def url
          execute :get_current_url
        end

        # @rbs () -> String
        def title
          execute :get_title
        end

        # @rbs () -> String
        def page_source
          execute :get_page_source
        end

        #
        # Create a new top-level browsing context
        # https://w3c.github.io/webdriver/#new-window
        # @param type [String] Supports two values: 'tab' and 'window'.
        #  Use 'tab' if you'd like the new window to share an OS-level window
        #  with the current browsing context.
        #  Use 'window' otherwise
        # @return [Hash] Containing 'handle' with the value of the window handle
        #  and 'type' with the value of the created window type
        #
        # @rbs (Symbol) -> Hash[untyped, untyped]
        def new_window(type)
          execute :new_window, {}, {type: type}
        end

        # @rbs (String) -> nil
        def switch_to_window(name)
          execute :switch_to_window, {}, {handle: name}
        end

        # @rbs (String) -> nil
        def switch_to_frame(id)
          id = find_element_by('id', id) if id.is_a? String
          execute :switch_to_frame, {}, {id: id}
        end

        # @rbs () -> nil
        def switch_to_parent_frame
          execute :switch_to_parent_frame
        end

        # @rbs () -> nil
        def switch_to_default_content
          switch_to_frame nil
        end

        QUIT_ERRORS = [IOError].freeze

        # @rbs () -> nil
        def quit
          execute :delete_session
          http.close
        rescue *QUIT_ERRORS
          nil
        end

        # @rbs () -> Array[untyped]?
        def close
          execute :close_window
        end

        # @rbs () -> nil
        def refresh
          execute :refresh
        end

        #
        # window handling
        #

        # @rbs () -> Array[untyped]
        def window_handles
          execute :get_window_handles
        end

        # @rbs () -> String
        def window_handle
          execute :get_window_handle
        end

        # @rbs (Integer, Integer, ?Symbol) -> Hash[untyped, untyped]
        def resize_window(width, height, handle = :current)
          raise Error::WebDriverError, 'Switch to desired window before changing its size' unless handle == :current

          set_window_rect(width: width, height: height)
        end

        # @rbs (?Symbol) -> Selenium::WebDriver::Dimension
        def window_size(handle = :current)
          unless handle == :current
            raise Error::UnsupportedOperationError,
                  'Switch to desired window before getting its size'
          end

          data = execute :get_window_rect
          Dimension.new data['width'], data['height']
        end

        # @rbs () -> Hash[untyped, untyped]
        def minimize_window
          execute :minimize_window
        end

        # @rbs (?Symbol) -> Hash[untyped, untyped]
        def maximize_window(handle = :current)
          unless handle == :current
            raise Error::UnsupportedOperationError,
                  'Switch to desired window before changing its size'
          end

          execute :maximize_window
        end

        # @rbs () -> Hash[untyped, untyped]
        def full_screen_window
          execute :fullscreen_window
        end

        # @rbs (Integer, Integer) -> Hash[untyped, untyped]
        def reposition_window(x, y)
          set_window_rect(x: x, y: y)
        end

        # @rbs () -> Selenium::WebDriver::Point
        def window_position
          data = execute :get_window_rect
          Point.new data['x'], data['y']
        end

        # @rbs (?x: nil | Integer, ?y: nil | Integer, ?width: Integer | nil, ?height: Integer | nil) -> Hash[untyped, untyped]
        def set_window_rect(x: nil, y: nil, width: nil, height: nil)
          params = {x: x, y: y, width: width, height: height}
          params.update(params) { |_k, v| Integer(v) unless v.nil? }
          execute :set_window_rect, {}, params
        end

        # @rbs () -> Selenium::WebDriver::Rectangle
        def window_rect
          data = execute :get_window_rect
          Rectangle.new data['x'], data['y'], data['width'], data['height']
        end

        # @rbs () -> String
        def screenshot
          execute :take_screenshot
        end

        # @rbs (String) -> String
        def element_screenshot(element)
          execute :take_element_screenshot, id: element
        end

        #
        # javascript execution
        #

        # @rbs (String, *nil) -> String
        def execute_script(script, *args)
          result = execute :execute_script, {}, {script: script, args: args}
          unwrap_script_result result
        end

        # @rbs (String, *Array[untyped] | nil) -> Hash[untyped, untyped]
        def execute_async_script(script, *args)
          result = execute :execute_async_script, {}, {script: script, args: args}
          unwrap_script_result result
        end

        #
        # cookies
        #

        # @rbs () -> Selenium::WebDriver::Manager
        def manage
          @manage ||= WebDriver::Manager.new(self)
        end

        # @rbs (Hash[untyped, untyped]) -> nil
        def add_cookie(cookie)
          execute :add_cookie, {}, {cookie: cookie}
        end

        # @rbs ((String | Symbol)?) -> nil
        def delete_cookie(name)
          raise ArgumentError, 'Cookie name cannot be null or empty' if name.nil? || name.to_s.strip.empty?

          execute :delete_cookie, name: name
        end

        # @rbs (String) -> Hash[untyped, untyped]?
        def cookie(name)
          execute :get_cookie, name: name
        end

        # @rbs () -> Array[untyped]
        def cookies
          execute :get_all_cookies
        end

        # @rbs () -> nil
        def delete_all_cookies
          execute :delete_all_cookies
        end

        #
        # actions
        #

        # @rbs (?async: bool, ?devices: Array[untyped], ?duration: Integer) -> Selenium::WebDriver::ActionBuilder
        def action(async: false, devices: [], duration: 250)
          ActionBuilder.new self, async: async, devices: devices, duration: duration
        end
        alias actions action

        # @rbs (Array[untyped]) -> nil
        def send_actions(data)
          execute :actions, {}, {actions: data}
        end

        # @rbs () -> nil
        def release_actions
          execute :release_actions
        end

        # @rbs (?Hash[untyped, untyped]) -> String
        def print_page(options = {})
          execute :print_page, {}, {options: options}
        end

        # @rbs (String) -> nil
        def click_element(element)
          execute :element_click, id: element
        end

        # @rbs (String, Array[untyped]) -> nil
        def send_keys_to_element(element, keys)
          keys = upload_if_necessary(keys) if @file_detector
          text = keys.join
          execute :element_send_keys, {id: element}, {value: text.chars, text: text}
        end

        # @rbs (String) -> nil
        def clear_element(element)
          execute :element_clear, id: element
        end

        # @rbs (String) -> nil
        def submit_element(element)
          script = "/* submitForm */ var form = arguments[0];\n" \
                   "while (form.nodeName != \"FORM\" && form.parentNode) {\n  " \
                   "form = form.parentNode;\n" \
                   "}\n" \
                   "if (!form) { throw Error('Unable to find containing form element'); }\n" \
                   "if (!form.ownerDocument) { throw Error('Unable to find owning document'); }\n" \
                   "var e = form.ownerDocument.createEvent('Event');\n" \
                   "e.initEvent('submit', true, true);\n" \
                   "if (form.dispatchEvent(e)) { HTMLFormElement.prototype.submit.call(form) }\n"

          execute_script(script, Bridge.element_class::ELEMENT_KEY => element)
        rescue Error::JavascriptError
          raise Error::UnsupportedOperationError, 'To submit an element, it must be nested inside a form element'
        end

        #
        # element properties
        #

        # @rbs (String) -> String
        def element_tag_name(element)
          execute :get_element_tag_name, id: element
        end

        # @rbs (Selenium::WebDriver::Element, String) -> String
        def element_attribute(element, name)
          WebDriver.logger.debug "Using script for :getAttribute of #{name}", id: :script
          execute_atom :getAttribute, element, name
        end

        # @rbs (String, String) -> String?
        def element_dom_attribute(element, name)
          execute :get_element_attribute, id: element, name: name
        end

        # @rbs (String, Symbol) -> String
        def element_property(element, name)
          execute :get_element_property, id: element, name: name
        end

        # @rbs (String) -> String
        def element_aria_role(element)
          execute :get_element_aria_role, id: element
        end

        # @rbs (String) -> String
        def element_aria_label(element)
          execute :get_element_aria_label, id: element
        end

        def element_value(element)
          element_property element, 'value'
        end

        # @rbs (String) -> String
        def element_text(element)
          execute :get_element_text, id: element
        end

        # @rbs (String) -> Selenium::WebDriver::Point
        def element_location(element)
          data = execute :get_element_rect, id: element

          Point.new data['x'], data['y']
        end

        # @rbs (String) -> Selenium::WebDriver::Rectangle
        def element_rect(element)
          data = execute :get_element_rect, id: element

          Rectangle.new data['x'], data['y'], data['width'], data['height']
        end

        # @rbs (String) -> Selenium::WebDriver::Point
        def element_location_once_scrolled_into_view(element)
          send_keys_to_element(element, [''])
          element_location(element)
        end

        # @rbs (String) -> Selenium::WebDriver::Dimension
        def element_size(element)
          data = execute :get_element_rect, id: element

          Dimension.new data['width'], data['height']
        end

        # @rbs (String) -> bool
        def element_enabled?(element)
          execute :is_element_enabled, id: element
        end

        # @rbs (String) -> bool
        def element_selected?(element)
          execute :is_element_selected, id: element
        end

        # @rbs (Selenium::WebDriver::Element) -> bool
        def element_displayed?(element)
          WebDriver.logger.debug 'Using script for :isDisplayed', id: :script
          execute_atom :isDisplayed, element
        end

        # @rbs (String, String) -> String
        def element_value_of_css_property(element, prop)
          execute :get_element_css_value, id: element, property_name: prop
        end

        #
        # finding elements
        #

        # @rbs () -> Selenium::WebDriver::Element
        def active_element
          Bridge.element_class.new self, element_id_from(execute(:get_active_element))
        end

        alias switch_to_active_element active_element

        # @rbs (String, String, ?Array[untyped]) -> Selenium::WebDriver::Element?
        def find_element_by(how, what, parent_ref = [])
          how, what = @locator_converter.convert(how, what)

          return execute_atom(:findElements, Support::RelativeLocator.new(what).as_json).first if how == 'relative'

          parent_type, parent_id = parent_ref
          id = case parent_type
               when :element
                 execute :find_child_element, {id: parent_id}, {using: how, value: what.to_s}
               when :shadow_root
                 execute :find_shadow_child_element, {id: parent_id}, {using: how, value: what.to_s}
               else
                 execute :find_element, {}, {using: how, value: what.to_s}
               end

          Bridge.element_class.new self, element_id_from(id)
        end

        # @rbs (String, String, ?Array[untyped]) -> Array[untyped]
        def find_elements_by(how, what, parent_ref = [])
          how, what = @locator_converter.convert(how, what)

          return execute_atom :findElements, Support::RelativeLocator.new(what).as_json if how == 'relative'

          parent_type, parent_id = parent_ref
          ids = case parent_type
                when :element
                  execute :find_child_elements, {id: parent_id}, {using: how, value: what.to_s}
                when :shadow_root
                  execute :find_shadow_child_elements, {id: parent_id}, {using: how, value: what.to_s}
                else
                  execute :find_elements, {}, {using: how, value: what.to_s}
                end

          ids.map { |id| Bridge.element_class.new self, element_id_from(id) }
        end

        # @rbs (String) -> Selenium::WebDriver::ShadowRoot?
        def shadow_root(element)
          id = execute :get_element_shadow_root, id: element
          ShadowRoot.new self, shadow_root_id_from(id)
        end

        #
        # virtual-authenticator
        #

        # @rbs (Selenium::WebDriver::VirtualAuthenticatorOptions) -> Selenium::WebDriver::VirtualAuthenticator
        def add_virtual_authenticator(options)
          authenticator_id = execute :add_virtual_authenticator, {}, options.as_json
          VirtualAuthenticator.new(self, authenticator_id, options)
        end

        # @rbs (String) -> void
        def remove_virtual_authenticator(id)
          execute :remove_virtual_authenticator, {authenticatorId: id}
        end

        # @rbs (Hash[untyped, untyped], String) -> Hash[untyped, untyped]?
        def add_credential(credential, id)
          execute :add_credential, {authenticatorId: id}, credential
        end

        # @rbs (String) -> Array[untyped]
        def credentials(authenticator_id)
          execute :get_credentials, {authenticatorId: authenticator_id}
        end

        # @rbs (String, String) -> Hash[untyped, untyped]
        def remove_credential(credential_id, authenticator_id)
          execute :remove_credential, {credentialId: credential_id, authenticatorId: authenticator_id}
        end

        # @rbs (String) -> Hash[untyped, untyped]
        def remove_all_credentials(authenticator_id)
          execute :remove_all_credentials, {authenticatorId: authenticator_id}
        end

        # @rbs (bool, String) -> Hash[untyped, untyped]
        def user_verified(verified, authenticator_id)
          execute :set_user_verified, {authenticatorId: authenticator_id}, {isUserVerified: verified}
        end

        #
        # federated-credential management
        #

        # @rbs () -> nil
        def cancel_fedcm_dialog
          execute :cancel_fedcm_dialog
        end

        # @rbs (Integer) -> nil
        def select_fedcm_account(index)
          execute :select_fedcm_account, {}, {accountIndex: index}
        end

        # @rbs () -> String?
        def fedcm_dialog_type
          execute :get_fedcm_dialog_type
        end

        # @rbs () -> String?
        def fedcm_title
          execute(:get_fedcm_title).fetch('title')
        end

        # @rbs () -> nil
        def fedcm_subtitle
          execute(:get_fedcm_title).fetch('subtitle', nil)
        end

        # @rbs () -> Array[untyped]?
        def fedcm_account_list
          execute :get_fedcm_account_list
        end

        # @rbs (bool) -> nil
        def fedcm_delay(enabled)
          execute :set_fedcm_delay, {}, {enabled: enabled}
        end

        # @rbs () -> nil
        def reset_fedcm_cooldown
          execute :reset_fedcm_cooldown
        end

        # @rbs () -> nil
        def click_fedcm_dialog_button
          execute :click_fedcm_dialog_button, {}, {dialogButton: 'ConfirmIdpLoginContinue'}
        end

        # @rbs () -> nil
        def bidi
          msg = 'BiDi must be enabled by setting #web_socket_url to true in options class'
          raise(WebDriver::Error::WebDriverError, msg)
        end

        # @rbs () -> Hash[untyped, untyped]
        def command_list
          COMMANDS
        end

        private

        #
        # executes a command on the remote server.
        #
        # @return [WebDriver::Remote::Response]
        #

        # @rbs (Symbol, ?Hash[untyped, untyped], ?Hash[untyped, untyped]?) -> (Hash[untyped, untyped] | String)?
        def execute(command, opts = {}, command_hash = nil)
          verb, path = commands(command) || raise(ArgumentError, "unknown command: #{command.inspect}")
          path = path.dup

          path[':session_id'] = session_id if path.include?(':session_id')

          begin
            opts.each { |key, value| path[key.inspect] = escaper.escape(value.to_s) }
          rescue IndexError
            raise ArgumentError, "#{opts.inspect} invalid for #{command.inspect}"
          end

          WebDriver.logger.debug("-> #{verb.to_s.upcase} #{path}", id: :command)
          http.call(verb, path, command_hash)['value']
        end

        # @rbs () -> URI::RFC2396_Parser
        def escaper
          @escaper ||= defined?(URI::RFC2396_PARSER) ? URI::RFC2396_PARSER : URI::DEFAULT_PARSER
        end

        # @rbs (Symbol) -> Array[untyped]
        def commands(command)
          command_list[command] || Bridge.extra_commands[command]
        end

        # @rbs (String) -> String
        def unwrap_script_result(arg)
          case arg
          when Array
            arg.map { |e| unwrap_script_result(e) }
          when Hash
            element_id = element_id_from(arg)
            return Bridge.element_class.new(self, element_id) if element_id

            shadow_root_id = shadow_root_id_from(arg)
            return ShadowRoot.new self, shadow_root_id if shadow_root_id

            arg.each { |k, v| arg[k] = unwrap_script_result(v) }
          else
            arg
          end
        end

        # @rbs (Hash[untyped, untyped]) -> nil
        def element_id_from(id)
          id['ELEMENT'] || id[Bridge.element_class::ELEMENT_KEY]
        end

        # @rbs (Hash[untyped, untyped]) -> nil
        def shadow_root_id_from(id)
          id[ShadowRoot::ROOT_KEY]
        end

        # @rbs (Hash[untyped, untyped]) -> Hash[untyped, untyped]
        def prepare_capabilities_payload(capabilities)
          capabilities = {alwaysMatch: capabilities} if !capabilities['alwaysMatch'] && !capabilities['firstMatch']
          {capabilities: capabilities}
        end
      end # Bridge
    end # Remote
  end # WebDriver
end # Selenium
