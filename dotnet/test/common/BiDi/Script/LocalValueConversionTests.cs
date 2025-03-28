// <copyright file="LocalValueConversionTests.cs" company="Selenium Committers">
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
using OpenQA.Selenium.BiDi.Modules.Script;

namespace OpenQA.Selenium.BiDi.Script;

class LocalValueConversionTests
{
    [Test]
    public void CanConvertNullBoolToLocalValue()
    {
        bool? arg = null;
        LocalValue result = arg;
        Assert.That(result, Is.TypeOf<NullLocalValue>());
    }

    [Test]
    public void CanConvertTrueToLocalValue()
    {
        LocalValue result = true;
        Assert.That(result, Is.TypeOf<BooleanLocalValue>());
        Assert.That((result as BooleanLocalValue).Value, Is.True);
    }

    [Test]
    public void CanConvertFalseToLocalValue()
    {
        LocalValue result = false;
        Assert.That(result, Is.TypeOf<BooleanLocalValue>());
        Assert.That((result as BooleanLocalValue).Value, Is.False);
    }

    [Test]
    public void CanConvertNullIntToLocalValue()
    {
        int? arg = null;
        LocalValue result = arg;
        Assert.That(result, Is.TypeOf<NullLocalValue>());
    }

    [Test]
    public void CanConvertZeroIntToLocalValue()
    {
        LocalValue result = 0;
        Assert.That(result, Is.TypeOf<NumberLocalValue>());
        Assert.That((result as NumberLocalValue).Value, Is.Zero);
    }

    [Test]
    public void CanConvertNullDoubleToLocalValue()
    {
        double? arg = null;
        LocalValue result = arg;
        Assert.That(result, Is.TypeOf<NullLocalValue>());
    }

    [Test]
    public void CanConvertZeroDoubleToLocalValue()
    {
        double arg = 0;
        LocalValue result = arg;
        Assert.That(result, Is.TypeOf<NumberLocalValue>());
        Assert.That((result as NumberLocalValue).Value, Is.Zero);
    }

    [Test]
    public void CanConvertNullStringToLocalValue()
    {
        string arg = null;
        LocalValue result = arg;
        Assert.That(result, Is.TypeOf<NullLocalValue>());
    }

    [Test]
    public void CanConvertStringToLocalValue()
    {
        LocalValue result = "value";
        Assert.That(result, Is.TypeOf<StringLocalValue>());
        Assert.That((result as StringLocalValue).Value, Is.EqualTo("value"));
    }
}
