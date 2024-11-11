// <copyright file="WebDriver.Extensions.cs" company="Selenium Committers">
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

using OpenQA.Selenium.BiDi.Modules.BrowsingContext;
using System.Threading.Tasks;

#nullable enable

namespace OpenQA.Selenium.BiDi;

public static class WebDriverExtensions
{
    public static async Task<BiDi> AsBiDiAsync(this IWebDriver webDriver)
    {
        var webSocketUrl = ((IHasCapabilities)webDriver).Capabilities.GetCapability("webSocketUrl");

        if (webSocketUrl is null) throw new System.Exception("The driver is not compatible with bidirectional protocol or it is not enabled in driver options.");

        var bidi = await BiDi.ConnectAsync(webSocketUrl.ToString()!).ConfigureAwait(false);

        return bidi;
    }

    public static async Task<BrowsingContext> AsBiDiContextAsync(this IWebDriver webDriver)
    {
        var bidi = await webDriver.AsBiDiAsync();

        var currentBrowsingContext = new BrowsingContext(bidi, webDriver.CurrentWindowHandle);

        return currentBrowsingContext;
    }
}
