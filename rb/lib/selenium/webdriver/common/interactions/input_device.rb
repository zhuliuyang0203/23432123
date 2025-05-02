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

require 'securerandom'

module Selenium
  module WebDriver
    module Interactions
      #
      # Superclass for the input device sources
      # Manages Array of Interaction instances for the device
      #
      # @api private
      #

      class InputDevice
        attr_reader :name, :actions, :type

        # @rbs (?String?) -> void
        def initialize(name = nil)
          @name = name || SecureRandom.uuid
          @actions = []
        end

        # @rbs (Selenium::WebDriver::Interactions::KeyInput::TypingInteraction | Selenium::WebDriver::Interactions::PointerMove | Selenium::WebDriver::Interactions::PointerPress | Selenium::WebDriver::Interactions::Pause | Selenium::WebDriver::Interactions::Scroll) -> Array[untyped]
        def add_action(action)
          raise TypeError, "#{action.inspect} is not a valid action" unless action.class < Interaction

          @actions << action
        end

        # @rbs () -> Array[untyped]
        def clear_actions
          @actions.clear
        end

        # @rbs (?Integer) -> Array[untyped]
        def create_pause(duration = 0)
          add_action(Pause.new(self, duration))
        end

        # @rbs () -> Hash[untyped, untyped]
        def encode
          {type: type, id: name, actions: @actions.map(&:encode)} unless @actions.empty?
        end
      end # InputDevice
    end # Interactions
  end # WebDriver
end # Selenium
