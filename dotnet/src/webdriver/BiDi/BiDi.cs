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

using OpenQA.Selenium.BiDi.Communication;
using OpenQA.Selenium.BiDi.Communication.Json;
using OpenQA.Selenium.BiDi.Communication.Json.Converters;
using System;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi;

public class BiDi : IAsyncDisposable
{
    private readonly bool _ownsConnection;
    protected BiDiConnection BiDiConnection { get; }

    private readonly Lazy<Modules.BrowsingContext.BrowsingContextModule> _browsingContextModule;
    private readonly Lazy<Modules.Browser.BrowserModule> _browserModule;
    private readonly Lazy<Modules.Network.NetworkModule> _networkModule;
    private readonly Lazy<Modules.Input.InputModule> _inputModule;
    private readonly Lazy<Modules.Script.ScriptModule> _scriptModule;
    private readonly Lazy<Modules.Log.LogModule> _logModule;
    private readonly Lazy<Modules.Storage.StorageModule> _storageModule;

    protected BiDi(BiDiConnection connection, bool disposeConnection) : this()
    {
        _ownsConnection = disposeConnection;
        BiDiConnection = connection;
        AddBiDiModuleJsonInfo(connection);
    }

    private BiDi()
    {
        _browsingContextModule = new Lazy<Modules.BrowsingContext.BrowsingContextModule>(() => new Modules.BrowsingContext.BrowsingContextModule(BiDiConnection));
        _browserModule = new Lazy<Modules.Browser.BrowserModule>(() => new Modules.Browser.BrowserModule(BiDiConnection));
        _networkModule = new Lazy<Modules.Network.NetworkModule>(() => new Modules.Network.NetworkModule(BiDiConnection));
        _inputModule = new Lazy<Modules.Input.InputModule>(() => new Modules.Input.InputModule(BiDiConnection));
        _scriptModule = new Lazy<Modules.Script.ScriptModule>(() => new Modules.Script.ScriptModule(BiDiConnection));
        _logModule = new Lazy<Modules.Log.LogModule>(() => new Modules.Log.LogModule(BiDiConnection));
        _storageModule = new Lazy<Modules.Storage.StorageModule>(() => new Modules.Storage.StorageModule(BiDiConnection));
    }

    private BiDiConnection AddBiDiModuleJsonInfo(BiDiConnection connection)
    {
        connection.AddSerializerContextAndConverters(BiDiJsonSerializerContext.Default,
        [
            new BrowsingContextConverter(this),
            new BrowserUserContextConverter(this),
            new InterceptConverter(this),
            new RequestConverter(this),
            new HandleConverter(this),
            new InternalIdConverter(this),
            new PreloadScriptConverter(this),
            new RealmConverter(this),
        ]);

        return connection;
    }

    internal Modules.Session.SessionModule SessionModule => BiDiConnection.SessionModule;
    public Modules.BrowsingContext.BrowsingContextModule BrowsingContext => _browsingContextModule.Value;
    public Modules.Browser.BrowserModule Browser => _browserModule.Value;
    public Modules.Network.NetworkModule Network => _networkModule.Value;
    internal Modules.Input.InputModule InputModule => _inputModule.Value;
    public Modules.Script.ScriptModule Script => _scriptModule.Value;
    public Modules.Log.LogModule Log => _logModule.Value;
    public Modules.Storage.StorageModule Storage => _storageModule.Value;

    public Task<Modules.Session.StatusResult> StatusAsync()
    {
        return SessionModule.StatusAsync();
    }

    public static async Task<BiDi> ConnectAsync(string url)
    {
        var connection = new BiDiConnection(new Uri(url));
        var bidi = new BiDi(connection, disposeConnection: true);

        await bidi.BiDiConnection.ConnectAsync(default).ConfigureAwait(false);

        return bidi;
    }

    public static BiDi Attach(BiDiConnection connection, bool disposeConnection = false)
    {
        return new BiDi(connection, disposeConnection);
    }

    public Task EndAsync(Modules.Session.EndOptions? options = null)
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
        if (_ownsConnection)
        {
            await BiDiConnection.DisposeAsync().ConfigureAwait(false);
        }
    }
}
