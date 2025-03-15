// <copyright file="CallFunctionRemoteValueTest.cs" company="Selenium Committers">
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
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Script;

public class CallFunctionRemoteValueTest : BiDiTestFixture
{
    [Test]
    public async Task CanCallFunctionAndReturnUndefined()
    {
        var response = await context.Script.CallFunctionAsync("() => { return undefined; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Undefined>());
    }

    [Test]
    public async Task CanCallFunctionAndReturnNull()
    {
        var response = await context.Script.CallFunctionAsync("() => { return null; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Null>());
    }

    [Test]
    public async Task CanCallFunctionAndReturnTrue()
    {
        var response = await context.Script.CallFunctionAsync("() => { return true; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Boolean>());
        Assert.That(((RemoteValue.Boolean)response.Result).Value, Is.True);
    }

    [Test]
    public async Task CanCallFunctionAndReturnFalse()
    {
        var response = await context.Script.CallFunctionAsync("() => { return false; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Boolean>());
        Assert.That(((RemoteValue.Boolean)response.Result).Value, Is.False);
    }


    [Test]
    public async Task CanCallFunctionAndReturnEmptyString()
    {
        var response = await context.Script.CallFunctionAsync("() => { return ''; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.String>());
        Assert.That(((RemoteValue.String)response.Result).Value, Is.Empty);
    }

    [Test]
    public async Task CanCallFunctionAndReturnNonEmptyString()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 'whoa'; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.String>());
        Assert.That(((RemoteValue.String)response.Result).Value, Is.EqualTo("whoa"));
    }

    [Test]
    public async Task CanCallFunctionAndReturnRecentDate()
    {
        const string PinnedDateTimeString = "2025-03-09T00:30:33.083Z";

        var response = await context.Script.CallFunctionAsync($$"""() => { return new Date('{{PinnedDateTimeString}}'); }""", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Date>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Date(PinnedDateTimeString)));
    }

    [Test]
    public async Task CanCallFunctionAndReturnUnixEpoch()
    {
        const string EpochString = "1970-01-01T00:00:00.000Z";

        var response = await context.Script.CallFunctionAsync($$"""() => { return new Date('{{EpochString}}'); }""", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Date>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Date(EpochString)));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumberFive()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 5; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(((RemoteValue.Number)response.Result).Value, Is.EqualTo(5));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumberNegativeFive()
    {
        var response = await context.Script.CallFunctionAsync("() => { return -5; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(((RemoteValue.Number)response.Result).Value, Is.EqualTo(-5));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumberZero()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 0; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(((RemoteValue.Number)response.Result).Value, Is.Zero);
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumberNegativeZero()
    {
        var response = await context.Script.CallFunctionAsync("() => { return -0; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());

        var actualNumberValue = ((RemoteValue.Number)response.Result).Value;
        Assert.That(actualNumberValue, Is.Zero);
        Assert.That(double.IsNegative(actualNumberValue), Is.True);
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumberPositiveInfinity()
    {
        var response = await context.Script.CallFunctionAsync("() => { return Number.POSITIVE_INFINITY; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());

        var expectedInfinity = ((RemoteValue.Number)response.Result).Value;
        Assert.That(double.IsPositiveInfinity(expectedInfinity));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumberNegativeInfinity()
    {
        var response = await context.Script.CallFunctionAsync("() => { return Number.NEGATIVE_INFINITY; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());

        var expectedInfinity = ((RemoteValue.Number)response.Result).Value;
        Assert.That(double.IsNegativeInfinity(expectedInfinity));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumberNaN()
    {
        var response = await context.Script.CallFunctionAsync("() => { return NaN; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        var expectedInfinity = ((RemoteValue.Number)response.Result).Value;
        Assert.That(double.IsNaN(expectedInfinity));
    }

    [Test]
    public async Task CanCallFunctionAndReturnRegExp()
    {
        var response = await context.Script.CallFunctionAsync("() => { return /foo*/g; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.RegExp>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.RegExp(new RemoteValue.RegExp.RegExpValue("foo*") { Flags = "g" })));
    }

    [Test]
    public async Task CanCallFunctionAndReturnArray()
    {
        var response = await context.Script.CallFunctionAsync("() => { return ['hi']; }", false);

        var expectedArray = new RemoteValue.Array { Value = [new RemoteValue.String("hi")] };
        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Array>());
        Assert.That(((RemoteValue.Array)response.Result).Value, Is.EqualTo(expectedArray.Value));
    }

    [Test]
    public async Task CanCallFunctionAndReturnObject()
    {
        var response = await context.Script.CallFunctionAsync("() => { return { objKey: 'objValue' }; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Object>());

        var expected = new RemoteValue.Object
        {
            Value = [[new RemoteValue.String("objKey"), new RemoteValue.String("objValue")]]
        };
        Assert.That(((RemoteValue.Object)response.Result).Value, Is.EqualTo(expected.Value));
    }

    [Test]
    public async Task CanCallFunctionAndReturnMap()
    {
        var expected = new RemoteValue.Map
        {
            Value = [[new RemoteValue.String("mapKey"), new RemoteValue.String("mapValue")]]
        };

        var response = await context.Script.CallFunctionAsync($$"""
            () => {
              const map = new Map();
              map.set('mapKey', 'mapValue');
              return map;
            }
            """, false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Map>());
        Assert.That(((RemoteValue.Map)response.Result).Value, Is.EqualTo(expected.Value));
    }

    [Test]
    public async Task CanCallFunctionAndReturnSet()
    {
        var expected = new RemoteValue.Set { Value = [new RemoteValue.String("setKey")] };
        var response = await context.Script.CallFunctionAsync($$"""
            () => {
              const set = new Set();
              set.add('setKey');
              return set;
            }
            """, false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Set>());
        Assert.That(((RemoteValue.Set)response.Result).Value, Is.EqualTo(expected.Value));
    }
}
