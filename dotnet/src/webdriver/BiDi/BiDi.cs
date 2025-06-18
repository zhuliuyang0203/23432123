// <copyright file="BiDi.cs" company="Selenium Committers">
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

using System;
using System.Threading.Tasks;
using OpenQA.Selenium.BiDi.Communication;

namespace OpenQA.Selenium.BiDi;

public class BiDi : IAsyncDisposable
{
    private readonly Broker _broker;

    private readonly Lazy<Session.SessionModule> _sessionModule;
    private readonly Lazy<BrowsingContext.BrowsingContextModule> _browsingContextModule;
    private readonly Lazy<Browser.BrowserModule> _browserModule;
    private readonly Lazy<Network.NetworkModule> _networkModule;
    private readonly Lazy<Input.InputModule> _inputModule;
    private readonly Lazy<Script.ScriptModule> _scriptModule;
    private readonly Lazy<Log.LogModule> _logModule;
    private readonly Lazy<Storage.StorageModule> _storageModule;

    internal BiDi(string url)
    {
        var uri = new Uri(url);

        _broker = new Broker(this, uri);

        _sessionModule = new Lazy<Session.SessionModule>(() => new Session.SessionModule(_broker));
        _browsingContextModule = new Lazy<BrowsingContext.BrowsingContextModule>(() => new BrowsingContext.BrowsingContextModule(_broker));
        _browserModule = new Lazy<Browser.BrowserModule>(() => new Browser.BrowserModule(_broker));
        _networkModule = new Lazy<Network.NetworkModule>(() => new Network.NetworkModule(_broker));
        _inputModule = new Lazy<Input.InputModule>(() => new Input.InputModule(_broker));
        _scriptModule = new Lazy<Script.ScriptModule>(() => new Script.ScriptModule(_broker));
        _logModule = new Lazy<Log.LogModule>(() => new Log.LogModule(_broker));
        _storageModule = new Lazy<Storage.StorageModule>(() => new Storage.StorageModule(_broker));
    }

    internal Session.SessionModule SessionModule => _sessionModule.Value;
    public BrowsingContext.BrowsingContextModule BrowsingContext => _browsingContextModule.Value;
    public Browser.BrowserModule Browser => _browserModule.Value;
    public Network.NetworkModule Network => _networkModule.Value;
    internal Input.InputModule InputModule => _inputModule.Value;
    public Script.ScriptModule Script => _scriptModule.Value;
    public Log.LogModule Log => _logModule.Value;
    public Storage.StorageModule Storage => _storageModule.Value;

    public Task<Session.StatusResult> StatusAsync()
    {
        return SessionModule.StatusAsync();
    }

    public static async Task<BiDi> ConnectAsync(string url)
    {
        var bidi = new BiDi(url);

        await bidi._broker.ConnectAsync(default).ConfigureAwait(false);

        return bidi;
    }

    public Task EndAsync(Session.EndOptions? options = null)
    {
        return SessionModule.EndAsync(options);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await _broker.DisposeAsync().ConfigureAwait(false);
    }
}
