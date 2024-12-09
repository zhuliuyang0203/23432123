// <copyright file="CookieJar.cs" company="Selenium Committers">
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
using System.Collections.ObjectModel;

namespace OpenQA.Selenium
{
    internal class CookieJar(WebDriver driver) : ICookieJar
    {
        public ReadOnlyCollection<Cookie> AllCookies => GetAllCookies();

        public void AddCookie(Cookie cookie)
        {
            if (cookie is null)
            {
                throw new ArgumentNullException(nameof(cookie));
            }

            driver.InternalExecute(DriverCommand.AddCookie, new() { { "cookie", cookie } });
        }

        public void DeleteCookieNamed(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            driver.InternalExecute(DriverCommand.DeleteCookie, new() { { "name", name } });
        }

        public void DeleteCookie(Cookie cookie)
        {
            if (cookie is null)
            {
                throw new ArgumentNullException(nameof(cookie));
            }

            DeleteCookieNamed(cookie.Name);
        }

        public void DeleteAllCookies()
        {
            driver.InternalExecute(DriverCommand.DeleteAllCookies, null);
        }

        public Cookie GetCookieNamed(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var rawCookie = driver.InternalExecute(DriverCommand.GetCookie, new() { { "name", name } }).Value;

            return Cookie.FromDictionary((Dictionary<string, object>)rawCookie);
        }

        private ReadOnlyCollection<Cookie> GetAllCookies()
        {
            List<Cookie> toReturn = new List<Cookie>();
            object returned = driver.InternalExecute(DriverCommand.GetAllCookies, new Dictionary<string, object>()).Value;

            try
            {
                object[] cookies = returned as object[];
                if (cookies != null)
                {
                    foreach (object rawCookie in cookies)
                    {
                        Dictionary<string, object> cookieDictionary = rawCookie as Dictionary<string, object>;
                        if (rawCookie != null)
                        {
                            toReturn.Add(Cookie.FromDictionary(cookieDictionary));
                        }
                    }
                }

                return new ReadOnlyCollection<Cookie>(toReturn);
            }
            catch (Exception e)
            {
                throw new WebDriverException("Unexpected problem getting cookies", e);
            }
        }
    }
}
