// <copyright file="BrowserModule.cs" company="Selenium Committers">
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
using System.Threading.Tasks;
using OpenQA.Selenium.BiDi.Communication;

#nullable enable

namespace OpenQA.Selenium.BiDi.Modules.Browser;

public sealed class BrowserModule(Broker broker) : Module(broker)
{
    public async Task CloseAsync(CloseOptions? options = null)
    {
        await Broker.ExecuteCommandAsync(new CloseCommand(), options).ConfigureAwait(false);
    }

    public async Task<UserContextInfo> CreateUserContextAsync(CreateUserContextOptions? options = null)
    {
        return await Broker.ExecuteCommandAsync<UserContextInfo>(new CreateUserContextCommand(), options).ConfigureAwait(false);
    }

    public async Task<GetUserContextsResult> GetUserContextsAsync(GetUserContextsOptions? options = null)
    {
        return await Broker.ExecuteCommandAsync<GetUserContextsResult>(new GetUserContextsCommand(), options).ConfigureAwait(false);
    }

    public async Task RemoveUserContextAsync(UserContext userContext, RemoveUserContextOptions? options = null)
    {
        var @params = new RemoveUserContextCommandParameters(userContext);

        await Broker.ExecuteCommandAsync(new RemoveUserContextCommand(@params), options).ConfigureAwait(false);
    }
}
