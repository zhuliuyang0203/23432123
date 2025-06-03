// <copyright file="WebExtensionTest.cs" company="Selenium Committers">
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

using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.WebExtension;

class WebExtensionTest : BiDiTestFixture
{
    [Test]
    [IgnoreBrowser(Selenium.Browser.Chrome, "Web extensions are not supported yet?")]
    [IgnoreBrowser(Selenium.Browser.Edge, "Web extensions are not supported yet?")]
    public async Task CanInstallPathWebExtension()
    {
        string path = Path.GetFullPath("data/extensions/webextensions-selenium-example");

        var result = await bidi.WebExtension.InstallAsync(new ExtensionPath(path));

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    [IgnoreBrowser(Selenium.Browser.Chrome, "Web extensions are not supported yet?")]
    [IgnoreBrowser(Selenium.Browser.Edge, "Web extensions are not supported yet?")]
    public async Task CanInstallArchiveWebExtension()
    {
        string path = Path.GetFullPath("data/extensions/webextensions-selenium-example.zip");

        var result = await bidi.WebExtension.InstallAsync(new ExtensionArchivePath(path));

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    [IgnoreBrowser(Selenium.Browser.Chrome, "Web extensions are not supported yet?")]
    [IgnoreBrowser(Selenium.Browser.Edge, "Web extensions are not supported yet?")]
    public async Task CanInstallBase64WebExtension()
    {
        string base64 = Convert.ToBase64String(File.ReadAllBytes("data/extensions/webextensions-selenium-example.zip"));

        var result = await bidi.WebExtension.InstallAsync(new ExtensionBase64Encoded(base64));

        Assert.That(result, Is.Not.Null);
    }

    [Test]
    [IgnoreBrowser(Selenium.Browser.Chrome, "Web extensions are not supported yet?")]
    [IgnoreBrowser(Selenium.Browser.Edge, "Web extensions are not supported yet?")]
    public async Task CanUninstallExtension()
    {
        string path = Path.GetFullPath("data/extensions/webextensions-selenium-example");

        var result = await bidi.WebExtension.InstallAsync(new ExtensionPath(path));

        await result.Extension.UninstallAsync();
    }
}
