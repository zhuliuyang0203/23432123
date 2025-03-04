// <copyright file="Request.cs" company="Selenium Committers">
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

using System.Threading.Tasks;

#nullable enable

namespace OpenQA.Selenium.BiDi.Modules.Network;

public class Request
{
    private readonly BiDi _bidi;

    internal Request(BiDi bidi, string id)
    {
        _bidi = bidi;
        Id = id;
    }

    public string Id { get; private set; }

    public Task ContinueAsync(ContinueRequestOptions? options = null)
    {
        return _bidi.Network.ContinueRequestAsync(this, options);
    }

    public Task FailAsync()
    {
        return _bidi.Network.FailRequestAsync(this);
    }

    public Task ProvideResponseAsync(ProvideResponseOptions? options = null)
    {
        return _bidi.Network.ProvideResponseAsync(this, options);
    }

    public Task ContinueResponseAsync(ContinueResponseOptions? options = null)
    {
        return _bidi.Network.ContinueResponseAsync(this, options);
    }

    public Task ContinueWithAuthAsync(AuthCredentials credentials, ContinueWithAuthOptions? options = null)
    {
        return _bidi.Network.ContinueWithAuthAsync(this, credentials, options);
    }

    public Task ContinueWithAuthAsync(ContinueWithDefaultAuthOptions? options = null)
    {
        return _bidi.Network.ContinueWithAuthAsync(this, options);
    }

    public Task ContinueWithAuthAsync(ContinueWithCancelledAuthOptions? options = null)
    {
        return _bidi.Network.ContinueWithAuthAsync(this, options);
    }
}
