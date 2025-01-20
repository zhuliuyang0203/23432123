// <copyright file="Response.cs" company="Selenium Committers">
// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The SFC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>

using OpenQA.Selenium.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

#nullable enable

namespace OpenQA.Selenium
{
    /// <summary>
    /// Handles reponses from the browser
    /// </summary>
    public class Response
    {
        private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
        {
            TypeInfoResolver = ResponseJsonSerializerContext.Default,
            Converters = { new ResponseValueJsonConverter() } // we still need it to make `Object` as `Dictionary`
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class
        /// </summary>
        [Obsolete("Set all values using the Response(string, object, WebDriverResult) constructor instead. This constructor will be removed in Selenium 4.30")]
        public Response()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class
        /// </summary>
        /// <param name="sessionId">Session ID in use</param>
        [Obsolete("Set all values using the Response(string, object, WebDriverResult) constructor instead. This constructor will be removed in Selenium 4.30")]
        public Response(SessionId? sessionId)
        {
            this.SessionId = sessionId?.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Response"/> class
        /// </summary>
        /// <param name="sessionId">The Session ID in use, if any.</param>
        /// <param name="value">The JSON payload of the response.</param>
        /// <param name="status">The WebDriver result status of the response.</param>
        public Response(string? sessionId, object? value, WebDriverResult status)
        {
            this.SessionId = sessionId;
            this.Value = value;
            this.Status = status;
        }

        /// <summary>
        /// Returns a new <see cref="Response"/> from a JSON-encoded string.
        /// </summary>
        /// <param name="value">The JSON string to deserialize into a <see cref="Response"/>.</param>
        /// <returns>A <see cref="Response"/> object described by the JSON string.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="JsonException">If <paramref name="value"/> is not a valid JSON object.</exception>
        public static Response FromJson(string value)
        {
            Dictionary<string, object?> rawResponse = JsonSerializer.Deserialize<Dictionary<string, object?>>(value, s_jsonSerializerOptions)
                ?? throw new WebDriverException("JSON success response returned \"null\" value");

            object? contents;
            string? sessionId = null;

            if (rawResponse.TryGetValue("sessionId", out object? s) && s is not null)
            {
                sessionId = s.ToString();
            }

            if (rawResponse.TryGetValue("value", out object? valueObj))
            {
                contents = valueObj;
            }
            else
            {
                // If the returned object does *not* have a "value" property
                // the response value should be the entirety of the response.
                // TODO: Remove this if statement altogether; there should
                // never be a spec-compliant response that does not contain a
                // value property.

                // Special-case for the new session command, where the "capabilities"
                // property of the response is the actual value we're interested in.
                if (rawResponse.TryGetValue("capabilities", out object? capabilities))
                {
                    contents = capabilities;
                }
                else
                {
                    contents = rawResponse;
                }
            }

            if (contents is Dictionary<string, object?> valueDictionary)
            {
                // Special case code for the new session command. If the response contains
                // sessionId and capabilities properties, fix up the session ID and value members.
                if (valueDictionary.TryGetValue("sessionId", out object? session))
                {
                    sessionId = session.ToString();
                    if (valueDictionary.TryGetValue("capabilities", out object? capabilities))
                    {
                        contents = capabilities;
                    }
                    else
                    {
                        contents = valueDictionary["value"];
                    }
                }
            }

            return new Response(sessionId, contents, WebDriverResult.Success);
        }

        /// <summary>
        /// Gets or sets the value from JSON.
        /// </summary>
        public object? Value
        {
            get;

            [Obsolete("The Response type will be immutable and this setter will be removed in Selenium 4.30")]
            set;
        }

        /// <summary>
        /// Gets or sets the session ID.
        /// </summary>
        public string? SessionId
        {
            get;

            [Obsolete("The Response type will be immutable and this setter will be removed in Selenium 4.30")]
            set;
        }

        /// <summary>
        /// Gets or sets the status value of the response.
        /// </summary>
        public WebDriverResult Status
        {
            get;

            [Obsolete("The Response type will be immutable and this setter will be removed in Selenium 4.30")]
            set;
        }

        /// <summary>
        /// Returns a new <see cref="Response"/> from a JSON-encoded string.
        /// </summary>
        /// <param name="value">The JSON string to deserialize into a <see cref="Response"/>.</param>
        /// <returns>A <see cref="Response"/> object described by the JSON string.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="JsonException">If <paramref name="value"/> is not a valid JSON object.</exception>
        /// <exception cref="WebDriverException">If the JSON dictionary is not in the expected state, per spec.</exception>
        public static Response FromErrorJson(string value)
        {
            Dictionary<string, object?> deserializedResponse = JsonSerializer.Deserialize<Dictionary<string, object?>>(value, s_jsonSerializerOptions)
                ?? throw new WebDriverException("JSON error response returned \"null\" value");

            if (!deserializedResponse.TryGetValue("value", out object? valueObject))
            {
                throw new WebDriverException($"The 'value' property was not found in the response:{Environment.NewLine}{value}");
            }

            if (valueObject is not Dictionary<string, object?> valueDictionary)
            {
                throw new WebDriverException($"The 'value' property is not a dictionary of <string, object>{Environment.NewLine}{value}");
            }

            if (!valueDictionary.TryGetValue("error", out object? errorObject))
            {
                throw new WebDriverException($"The 'value > error' property was not found in the response:{Environment.NewLine}{value}");
            }

            if (errorObject is not string errorString)
            {
                throw new WebDriverException($"The 'value > error' property is not a string{Environment.NewLine}{value}");
            }

            WebDriverResult status = WebDriverError.ResultFromError(errorString);

            return new Response(sessionId: null, valueDictionary, status);
        }

        /// <summary>
        /// Returns this object as a JSON-encoded string.
        /// </summary>
        /// <returns>A JSON-encoded string representing this <see cref="Response"/> object.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// Returns the object as a string.
        /// </summary>
        /// <returns>A string with the Session ID, status value, and the value from JSON.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "({0} {1}: {2})", this.SessionId, this.Status, this.Value);
        }
    }

    [JsonSerializable(typeof(Dictionary<string, object>))]
    internal sealed partial class ResponseJsonSerializerContext : JsonSerializerContext;
}
