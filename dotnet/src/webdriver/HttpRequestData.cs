// <copyright file="HttpRequestData.cs" company="Selenium Committers">
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

using System.Collections.Generic;

#nullable enable

namespace OpenQA.Selenium
{
    /// <summary>
    /// Represents the response data for an intercepted HTTP call.
    /// </summary>
    public class HttpRequestData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestData"/> type.
        /// </summary>
        public HttpRequestData()
        {
        }

        internal HttpRequestData(string? method, string? url, string? postData, Dictionary<string, string>? headers, string? requestId)
        {
            this.Method = method;
            this.Url = url;
            this.PostData = postData;
            this.Headers = headers;
            this.RequestId = requestId;
        }

        /// <summary>
        /// Gets the method of the HTTP request.
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// Gets the URL of the HTTP request.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets the POST data of the HTTP request, if any.
        /// </summary>
        public string? PostData { get; set; }

        /// <summary>
        /// Gets the headers of the HTTP request.
        /// </summary>
        public Dictionary<string, string>? Headers { get; set; }

        /// <summary>
        /// Gets the ID of the HTTP request.
        /// </summary>
        public string? RequestId { get; }
    }
}
