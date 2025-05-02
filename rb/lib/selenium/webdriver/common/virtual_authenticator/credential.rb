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

#
# A credential stored in a virtual authenticator.
# @see https://w3c.github.io/webauthn/#credential-parameters
#

module Selenium
  module WebDriver
    class Credential
      class << self
        # @rbs (**Array[untyped] | String) -> Selenium::WebDriver::Credential
        def resident(**opts)
          Credential.new(resident_credential: true, **opts)
        end

        # @rbs (**Array[untyped] | String) -> Selenium::WebDriver::Credential
        def non_resident(**opts)
          Credential.new(resident_credential: false, **opts)
        end

        # @rbs (Array[untyped]) -> String
        def encode(byte_array)
          Base64.urlsafe_encode64(byte_array&.pack('C*'))
        end

        # @rbs (String) -> Array[untyped]
        def decode(base64)
          Base64.urlsafe_decode64(base64).unpack('C*')
        end

        # @rbs (Hash[untyped, untyped]) -> Selenium::WebDriver::Credential
        def from_json(opts)
          user_handle = opts['userHandle'] ? decode(opts['userHandle']) : nil
          new(id: decode(opts['credentialId']),
              resident_credential: opts['isResidentCredential'],
              rp_id: opts['rpId'],
              private_key: opts['privateKey'],
              sign_count: opts['signCount'],
              user_handle: user_handle)
        end
      end

      attr_reader :id, :resident_credential, :rp_id, :user_handle, :private_key, :sign_count
      alias resident_credential? resident_credential

      # @rbs (id: Array[untyped], resident_credential: bool, rp_id: String | nil, private_key: Array[untyped] | String, **nil | Array[untyped] | Integer | Array[untyped] | Integer?) -> void
      def initialize(id:, resident_credential:, rp_id:, private_key:, **opts)
        @id = id
        @resident_credential = resident_credential
        @rp_id = rp_id
        @user_handle = opts.delete(:user_handle) { nil }
        @private_key = private_key
        @sign_count = opts.delete(:sign_count) { 0 }

        raise ArgumentError, "Invalid arguments: #{opts.keys}" unless opts.empty?
      end

      #
      # @api private
      #

      # @rbs (*untyped) -> Hash[untyped, untyped]
      def as_json(*)
        credential_data = {'credentialId' => Credential.encode(id),
                           'isResidentCredential' => resident_credential?,
                           'rpId' => rp_id,
                           'privateKey' => Credential.encode(private_key),
                           'signCount' => sign_count}
        credential_data['userHandle'] = Credential.encode(user_handle) if user_handle
        credential_data
      end
    end # Credential
  end # WebDriver
end # Selenium
