// <copyright file="StorageModule.cs" company="Selenium Committers">
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

using OpenQA.Selenium.BiDi.Communication;
using System.Threading.Tasks;

#nullable enable

namespace OpenQA.Selenium.BiDi.Modules.Storage;

public class StorageModule(Broker broker) : Module(broker)
{
    public async Task<GetCookiesResult> GetCookiesAsync(GetCookiesOptions? options = null)
    {
        var @params = new GetCookiesCommandParameters();

        if (options is not null)
        {
            @params.Filter = options.Filter;
            @params.Partition = options.Partition;
        }

        return await Broker.ExecuteCommandAsync<GetCookiesResult>(new GetCookiesCommand(@params), options).ConfigureAwait(false);
    }

    public async Task<DeleteCookiesResult> DeleteCookiesAsync(DeleteCookiesOptions? options = null)
    {
        var @params = new DeleteCookiesCommandParameters();

        if (options is not null)
        {
            @params.Filter = options.Filter;
            @params.Partition = options.Partition;
        }

        return await Broker.ExecuteCommandAsync<DeleteCookiesResult>(new DeleteCookiesCommand(@params), options).ConfigureAwait(false);
    }

    public async Task<SetCookieResult> SetCookieAsync(PartialCookie cookie, SetCookieOptions? options = null)
    {
        var @params = new SetCookieCommandParameters(cookie);

        if (options is not null)
        {
            @params.Partition = options.Partition;
        }

        return await Broker.ExecuteCommandAsync<SetCookieResult>(new SetCookieCommand(@params), options).ConfigureAwait(false);
    }
}
