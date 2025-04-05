// <copyright file="BiDiBuilder.cs" company="Selenium Committers">
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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Communication;

public class BiDiBuilder
{
    private readonly BiDiConnection _connection;
    private readonly BiDi _coreBidi;
    public List<Modules.Module> Extensions { get; } = [];

    public BiDiBuilder(string url) : this(new Uri(url))
    {
    }

    public BiDiBuilder(Uri url)
    {
        _connection = new BiDiConnection(url);
        _coreBidi = BiDi.Attach(_connection, disposeConnection: true);
    }

    public BiDiBuilder AddExtension<TModule>(Func<BiDiConnection, TModule> addModuleFactory, out TModule module)
        where TModule : Modules.Module
    {
        var newModule = addModuleFactory(_connection);
        Extensions.Add(newModule);
        module = newModule;

        return this;
    }

    public async Task<BiDi> BuildAsync()
    {
        await _connection.ConnectAsync(default);

        return _coreBidi;
    }
}
