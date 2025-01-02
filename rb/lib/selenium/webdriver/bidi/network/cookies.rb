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
      module Cookies
        def add_cookie(name, value)
          cookies.push(
            'name' => name,
            'value' => {
              'type' => 'string',
              'value' => value
            }
          )
        end

        def remove_cookie(name)
          cookies.delete_if { |cookie| cookie['name'] == name }
        end

        def set_cookie_header(**args)
          cookies.push(
            'name' => args[:name],
            'value' => {
              'type' => 'string',
              'value' => 'input'
            },
            'domain' => args[:domain],
            'httpOnly' => args[:http_only],
            'expiry' => args[:expiry],
            'maxAge' => args[:max_age],
            'path' => args[:path],
            'sameSite' => args[:same_site],
            'secure' => args[:secure]
          )
        end
      end
    end

    # BiDi
  end # WebDriver
end # Selenium

