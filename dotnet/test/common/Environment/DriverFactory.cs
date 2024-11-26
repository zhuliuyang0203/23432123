// <copyright file="DriverFactory.cs" company="Selenium Committers">
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

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Safari;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OpenQA.Selenium.Environment
{
    public class DriverFactory
    {
        string driverPath;
        string browserBinaryLocation;
        private Dictionary<Browser, Type> optionsTypes = new Dictionary<Browser, Type>();

        public DriverFactory(string driverPath, string browserBinaryLocation)
        {
            this.driverPath = driverPath;
            this.browserBinaryLocation = browserBinaryLocation;

            this.PopulateOptionsTypes();
        }

        private void PopulateOptionsTypes()
        {
            this.optionsTypes[Browser.Chrome] = typeof(ChromeOptions);
            this.optionsTypes[Browser.Edge] = typeof(EdgeOptions);
            this.optionsTypes[Browser.Firefox] = typeof(FirefoxOptions);
            this.optionsTypes[Browser.IE] = typeof(InternetExplorerOptions);
            this.optionsTypes[Browser.Safari] = typeof(SafariOptions);
        }

        public event EventHandler<DriverStartingEventArgs> DriverStarting;

        public IWebDriver CreateDriver(Type driverType)
        {
            return CreateDriverWithOptions(driverType, null);
        }

        public IWebDriver CreateDriverWithOptions(Type driverType, DriverOptions driverOptions)
        {
            Console.WriteLine($"Creating new driver of {driverType} type...");

            Browser browser = Browser.All;
            DriverService service = null;
            DriverOptions options = null;

            List<Type> constructorArgTypeList = new List<Type>();
            IWebDriver driver = null;
            if (typeof(ChromeDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.Chrome;
                options = GetDriverOptions<ChromeOptions>(driverType, driverOptions);

                var chromeOptions = (ChromeOptions)options;
                chromeOptions.AddArguments("--no-sandbox", "--disable-dev-shm-usage");

                service = ChromeDriverService.CreateDefaultService();
                if (!string.IsNullOrEmpty(this.browserBinaryLocation))
                {
                    ((ChromeOptions)options).BinaryLocation = this.browserBinaryLocation;
                }
            }
            else if (typeof(EdgeDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.Edge;
                options = GetDriverOptions<EdgeOptions>(driverType, driverOptions);

                var edgeOptions = (EdgeOptions)options;
                edgeOptions.AddArguments("--no-sandbox", "--disable-dev-shm-usage");

                service = EdgeDriverService.CreateDefaultService();
                if (!string.IsNullOrEmpty(this.browserBinaryLocation))
                {
                    ((EdgeOptions)options).BinaryLocation = this.browserBinaryLocation;
                }
            }
            else if (typeof(InternetExplorerDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.IE;
                options = GetDriverOptions<InternetExplorerOptions>(driverType, driverOptions);
                service = InternetExplorerDriverService.CreateDefaultService();
            }
            else if (typeof(FirefoxDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.Firefox;
                options = GetDriverOptions<FirefoxOptions>(driverType, driverOptions);
                service = FirefoxDriverService.CreateDefaultService();
                if (!string.IsNullOrEmpty(this.browserBinaryLocation))
                {
                    ((FirefoxOptions)options).BinaryLocation = this.browserBinaryLocation;
                }
            }
            else if (typeof(SafariDriver).IsAssignableFrom(driverType))
            {
                browser = Browser.Safari;
                options = GetDriverOptions<SafariOptions>(driverType, driverOptions);
                service = SafariDriverService.CreateDefaultService();
            }

            if (!String.IsNullOrEmpty(this.driverPath) && service != null)
            {
                service.DriverServicePath = Path.GetDirectoryName(this.driverPath);
                service.DriverServiceExecutableName = Path.GetFileName(this.driverPath);
            }

            this.OnDriverLaunching(options);

            driver = (IWebDriver)Activator.CreateInstance(driverType, service, options);
            return driver;
        }

        protected void OnDriverLaunching(DriverOptions options)
        {
            if (this.DriverStarting != null)
            {
                DriverStartingEventArgs args = new DriverStartingEventArgs(options);
                this.DriverStarting(this, args);
            }
        }

        private T GetDriverOptions<T>(Type driverType, DriverOptions overriddenOptions) where T : DriverOptions, new()
        {
            T options = new T();
            Type optionsType = typeof(T);

            PropertyInfo defaultOptionsProperty = driverType.GetProperty("DefaultOptions", BindingFlags.Public | BindingFlags.Static);
            if (defaultOptionsProperty != null && defaultOptionsProperty.PropertyType == optionsType)
            {
                options = (T)defaultOptionsProperty.GetValue(null, null);
            }

            if (overriddenOptions != null)
            {
                options.PageLoadStrategy = overriddenOptions.PageLoadStrategy;
                options.UnhandledPromptBehavior = overriddenOptions.UnhandledPromptBehavior;
                options.Proxy = overriddenOptions.Proxy;

                options.ScriptTimeout = overriddenOptions.ScriptTimeout;
                options.PageLoadTimeout = overriddenOptions.PageLoadTimeout;
                options.ImplicitWaitTimeout = overriddenOptions.ImplicitWaitTimeout;

                options.UseWebSocketUrl = overriddenOptions.UseWebSocketUrl;
            }

            return options;
        }
    }
}
