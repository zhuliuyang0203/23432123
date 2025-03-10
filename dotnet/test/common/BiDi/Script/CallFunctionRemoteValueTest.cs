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
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Undefined()));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNull()
    {
        var response = await context.Script.CallFunctionAsync("() => { return null; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Null>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Null()));
    }

    [Test]
    public async Task CanCallFunctionAndReturnEmptyString()
    {
        var response = await context.Script.CallFunctionAsync("() => { return ''; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.String>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.String(string.Empty)));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNonEmptyString()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 'whoa'; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.String>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.String("whoa")));
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
    public async Task CanCallFunctionAndReturnNumber5()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 5; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(5)));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumberNegative5()
    {
        var response = await context.Script.CallFunctionAsync("() => { return -5; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(-5)));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumber0()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 0; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());
        Assert.That(response.Result, Is.EqualTo(new RemoteValue.Number(0)));
    }

    [Test]
    public async Task CanCallFunctionAndReturnNumberNegative0()
    {
        var response = await context.Script.CallFunctionAsync("() => { return -0; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Number>());

        var expectedNegativeZero = ((RemoteValue.Number)response.Result).Value;
        Assert.That(IsNegativeZero(expectedNegativeZero));

        static bool IsNegativeZero(double d)
        {
            // '== double.NegativeZero' does not work, -0 == 0 is considered true
            // Need special verification to tell if a double is -0
            return double.IsNegative(d) && d == 0;
        }
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
        var response = await context.Script.CallFunctionAsync("() => { return { key: 'value' }; }", false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Object>());

        var expected = new RemoteValue.Object
        {
            Value = [[new RemoteValue.String("key"), new RemoteValue.String("value")]]
        };
        Assert.That(((RemoteValue.Object)response.Result).Value, Is.EqualTo(expected.Value));
    }

    [Test]
    public async Task CanCallFunctionAndReturnMap()
    {
        var expected = new RemoteValue.Map
        {
            Value = [[new RemoteValue.String("key"), new RemoteValue.String("value")]]
        };

        var response = await context.Script.CallFunctionAsync($$"""
            () => {
              const map = new Map();
              map.set('key', 'value');
              return map;
            }
            """, false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Map>());
        Assert.That(((RemoteValue.Map)response.Result).Value, Is.EqualTo(expected.Value));
    }

    [Test]
    public async Task CanCallFunctionAndReturnSet()
    {
        var expected = new RemoteValue.Set { Value = [new RemoteValue.String("key")] };
        var response = await context.Script.CallFunctionAsync($$"""
            () => {
              const set = new Set();
              set.add('key');
              return set;
            }
            """, false);

        Assert.That(response.Result, Is.AssignableTo<RemoteValue.Set>());
        Assert.That(((RemoteValue.Set)response.Result).Value, Is.EqualTo(expected.Value));
    }
}
