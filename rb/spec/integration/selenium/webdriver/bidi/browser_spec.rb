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

require_relative '../spec_helper'

module Selenium
  module WebDriver
    class BiDi
      describe Browser do
        it 'creates an user context' do
          reset_driver!(web_socket_url: true) do |driver|
            browser = described_class.new(driver.bidi)
            user_context = browser.create_user_context
            expect(user_context).not_to be_nil
            expect(user_context['userContext']).to be_a String
          end
        end

        it 'gets user contexts' do
          reset_driver!(web_socket_url: true) do |driver|
            browser = described_class.new(driver.bidi)
            2.times { browser.create_user_context }
            expect(browser.user_contexts['userContexts'].count).to eq 3
          end
        end

        it 'removes an user context' do
          reset_driver!(web_socket_url: true) do |driver|
            browser = described_class.new(driver.bidi)
            user_context = browser.create_user_context
            expect(browser.user_contexts['userContexts'].count).to eq 2
            browser.remove_user_context(user_context['userContext'])
            expect(browser.user_contexts['userContexts'].count).to eq 1
          end
        end

        it 'throws an error when removing the default user context' do
          reset_driver!(web_socket_url: true) do |driver|
            browser = described_class.new(driver.bidi)
            expect {
              browser.remove_user_context('default')
            }.to raise_error(Error::WebDriverError, /user context cannot be removed/)
          end
        end

        it 'throws an error when removing a non-existent user context' do
          reset_driver!(web_socket_url: true) do |driver|
            browser = described_class.new(driver.bidi)
            expect {
              browser.remove_user_context('fake_context')
            }.to raise_error(Error::WebDriverError, /Failed to find context with id/)
          end
        end

        it 'closes the browser' do
          reset_driver!(web_socket_url: true) do |driver|
            browser = described_class.new(driver.bidi)
            browser.close
            expect(browser).to be_nil
          end
        end
      end
    end
  end
end
