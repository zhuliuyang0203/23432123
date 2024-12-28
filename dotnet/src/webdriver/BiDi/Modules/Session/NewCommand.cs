// <copyright file="NewCommand.cs" company="Selenium Committers">
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

#nullable enable

namespace OpenQA.Selenium.BiDi.Modules.Session;

internal class NewCommand(NewCommandParameters @params) : Command<NewCommandParameters>(@params);

internal record NewCommandParameters(CapabilitiesRequest Capabilities) : CommandParameters;

public record NewOptions : CommandOptions;

public record NewResult(string SessionId, Capability Capability);

public record Capability(bool AcceptInsecureCerts, string BrowserName, string BrowserVersion, string PlatformName, bool SetWindowRect, string UserAgent)
{
    public ProxyConfiguration? Proxy { get; set; }

    public string? WebSocketUrl { get; set; }
}
