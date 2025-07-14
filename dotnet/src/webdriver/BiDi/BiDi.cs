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

    private Session.SessionModule? _sessionModule;
    private BrowsingContext.BrowsingContextModule? _browsingContextModule;
    private Browser.BrowserModule? _browserModule;
    private Network.NetworkModule? _networkModule;
    private Input.InputModule? _inputModule;
    private Script.ScriptModule? _scriptModule;
    private Log.LogModule? _logModule;
    private Storage.StorageModule? _storageModule;

    private readonly object _moduleLock = new();

    internal BiDi(string url)
    {
        var uri = new Uri(url);

        _broker = new Broker(this, uri);
    }

    internal Session.SessionModule SessionModule
    {
        get
        {
            if (_sessionModule is null)
            {
                lock (_moduleLock)
                {
                    if (_sessionModule is null)
                    {
                        _sessionModule = new Session.SessionModule(_broker);
                    }
                }
            }
            return _sessionModule;
        }
    }

    public BrowsingContext.BrowsingContextModule BrowsingContext
    {
        get
        {
            if (_browsingContextModule is null)
            {
                lock (_moduleLock)
                {
                    if (_browsingContextModule is null)
                    {
                        _browsingContextModule = new BrowsingContext.BrowsingContextModule(_broker);
                    }
                }
            }
            return _browsingContextModule;
        }
    }

    public Browser.BrowserModule Browser
    {
        get
        {
            if (_browserModule is null)
            {
                lock (_moduleLock)
                {
                    if (_browserModule is null)
                    {
                        _browserModule = new Browser.BrowserModule(_broker);
                    }
                }
            }
            return _browserModule;
        }
    }

    public Network.NetworkModule Network
    {
        get
        {
            if (_networkModule is null)
            {
                lock (_moduleLock)
                {
                    if (_networkModule is null)
                    {
                        _networkModule = new Network.NetworkModule(_broker);
                    }
                }
            }
            return _networkModule;
        }
    }

    internal Input.InputModule InputModule
    {
        get
        {
            if (_inputModule is null)
            {
                lock (_moduleLock)
                {
                    if (_inputModule is null)
                    {
                        _inputModule = new Input.InputModule(_broker);
                    }
                }
            }
            return _inputModule;
        }
    }

    public Script.ScriptModule Script
    {
        get
        {
            if (_scriptModule is null)
            {
                lock (_moduleLock)
                {
                    if (_scriptModule is null)
                    {
                        _scriptModule = new Script.ScriptModule(_broker);
                    }
                }
            }
            return _scriptModule;
        }
    }

    public Log.LogModule Log
    {
        get
        {
            if (_logModule is null)
            {
                lock (_moduleLock)
                {
                    if (_logModule is null)
                    {
                        _logModule = new Log.LogModule(_broker);
                    }
                }
            }
            return _logModule;
        }
    }

    public Storage.StorageModule Storage
    {
        get
        {
            if (_storageModule is null)
            {
                lock (_moduleLock)
                {
                    if (_storageModule is null)
                    {
                        _storageModule = new Storage.StorageModule(_broker);
                    }
                }
            }
            return _storageModule;
        }
    }

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
