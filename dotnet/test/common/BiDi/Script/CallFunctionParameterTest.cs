// <copyright file="CallFunctionParameterTest.cs" company="Selenium Committers">
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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Script;

class CallFunctionParameterTest : BiDiTestFixture
{
    [Test]
    public async Task CanCallFunctionWithDeclaration()
    {
        var res = await context.Script.CallFunctionAsync("() => { return 1 + 2; }", false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Realm, Is.Not.Null);
        Assert.That((res.Result as RemoteValue.Number).Value, Is.EqualTo(3));
    }

    [Test]
    public async Task CanCallFunctionWithDeclarationImplicitCast()
    {
        var res = await context.Script.CallFunctionAsync<int>("() => { return 1 + 2; }", false);

        Assert.That(res, Is.EqualTo(3));
    }

    [Test]
    public async Task CanEvaluateScriptWithUserActivationTrue()
    {
        await context.Script.EvaluateAsync("window.open();", true);

        var res = await context.Script.CallFunctionAsync<bool>("""
            () => navigator.userActivation.isActive && navigator.userActivation.hasBeenActive
            """, true, new() { UserActivation = true });

        Assert.That(res, Is.True);
    }

    [Test]
    public async Task CanEvaluateScriptWithUserActivationFalse()
    {
        await context.Script.EvaluateAsync("window.open();", true);

        var res = await context.Script.CallFunctionAsync<bool>("""
            () => navigator.userActivation.isActive && navigator.userActivation.hasBeenActive
            """, true);

        Assert.That(res, Is.False);
    }

    [Test]
    public async Task CanCallFunctionWithArguments()
    {
        var res = await context.Script.CallFunctionAsync("(...args)=>{return args}", false, new()
        {
            Arguments = ["abc", 42]
        });

        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Array>());
        Assert.That((string)(res.Result as RemoteValue.Array).Value[0], Is.EqualTo("abc"));
        Assert.That((int)(res.Result as RemoteValue.Array).Value[1], Is.EqualTo(42));
    }

    [Test]
    public async Task CanCallFunctionToGetIFrameBrowsingContext()
    {
        driver.Url = UrlBuilder.WhereIs("click_too_big_in_frame.html");

        var res = await context.Script.CallFunctionAsync("""
            () => document.querySelector('iframe[id="iframe1"]').contentWindow
            """, false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.WindowProxy>());
        Assert.That((res.Result as RemoteValue.WindowProxy).Value, Is.Not.Null);
    }

    [Test]
    public async Task CanCallFunctionToGetElement()
    {
        driver.Url = UrlBuilder.WhereIs("bidi/logEntryAdded.html");

        var res = await context.Script.CallFunctionAsync("""
            () => document.getElementById("consoleLog")
            """, false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Node>());
        Assert.That((res.Result as RemoteValue.Node).Value, Is.Not.Null);
    }

    [Test]
    public async Task CanCallFunctionWithAwaitPromise()
    {
        var res = await context.Script.CallFunctionAsync<string>("""
            async function() {
                await new Promise(r => setTimeout(() => r(), 0));
                return "SOME_DELAYED_RESULT";
            }
            """, awaitPromise: true);

        Assert.That(res, Is.EqualTo("SOME_DELAYED_RESULT"));
    }

    [Test]
    public async Task CanCallFunctionWithAwaitPromiseFalse()
    {
        var res = await context.Script.CallFunctionAsync("""
            async function() {
                await new Promise(r => setTimeout(() => r(), 0));
                return "SOME_DELAYED_RESULT";
            }
            """, awaitPromise: false);

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Promise>());
    }

    [Test]
    public async Task CanCallFunctionWithThisParameter()
    {
        var thisParameter = new LocalValue.Object([["some_property", 42]]);

        var res = await context.Script.CallFunctionAsync<int>("""
            function(){return this.some_property}
            """, false, new() { This = thisParameter });

        Assert.That(res, Is.EqualTo(42));
    }

    [Test]
    public async Task CanCallFunctionWithOwnershipRoot()
    {
        var res = await context.Script.CallFunctionAsync("async function(){return {a:1}}", true, new()
        {
            ResultOwnership = ResultOwnership.Root
        });

        Assert.That(res, Is.Not.Null);
        Assert.That((res.Result as RemoteValue.Object).Handle, Is.Not.Null);
        Assert.That((string)(res.Result as RemoteValue.Object).Value[0][0], Is.EqualTo("a"));
        Assert.That((int)(res.Result as RemoteValue.Object).Value[0][1], Is.EqualTo(1));
    }

    [Test]
    public async Task CanCallFunctionWithOwnershipNone()
    {
        var res = await context.Script.CallFunctionAsync("async function(){return {a:1}}", true, new()
        {
            ResultOwnership = ResultOwnership.None
        });

        Assert.That(res, Is.Not.Null);
        Assert.That((res.Result as RemoteValue.Object).Handle, Is.Null);
        Assert.That((string)(res.Result as RemoteValue.Object).Value[0][0], Is.EqualTo("a"));
        Assert.That((int)(res.Result as RemoteValue.Object).Value[0][1], Is.EqualTo(1));
    }

    [Test]
    public void CanCallFunctionThatThrowsException()
    {
        var action = () => context.Script.CallFunctionAsync("))) !!@@## some invalid JS script (((", false);

        Assert.That(action, Throws.InstanceOf<ScriptEvaluateException>().And.Message.Contain("SyntaxError:"));
    }

    [Test]
    public async Task CanCallFunctionInASandBox()
    {
        // Make changes without sandbox
        await context.Script.CallFunctionAsync("() => { window.foo = 1; }", true);

        var res = await context.Script.CallFunctionAsync("() => window.foo", true, targetOptions: new() { Sandbox = "sandbox" });

        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Undefined>());

        // Make changes in the sandbox
        await context.Script.CallFunctionAsync("() => { window.foo = 2; }", true, targetOptions: new() { Sandbox = "sandbox" });

        // Check if the changes are present in the sandbox
        res = await context.Script.CallFunctionAsync("() => window.foo", true, targetOptions: new() { Sandbox = "sandbox" });

        Assert.That(res.Result, Is.AssignableFrom<RemoteValue.Number>());
        Assert.That((res.Result as RemoteValue.Number).Value, Is.EqualTo(2));
    }

    [Test]
    public async Task CanCallFunctionInARealm()
    {
        await bidi.BrowsingContext.CreateAsync(Modules.BrowsingContext.ContextType.Tab);

        var realms = await bidi.Script.GetRealmsAsync();

        await bidi.Script.CallFunctionAsync("() => { window.foo = 3; }", true, new Target.Realm(realms[0].Realm));
        await bidi.Script.CallFunctionAsync("() => { window.foo = 5; }", true, new Target.Realm(realms[1].Realm));

        var res1 = await bidi.Script.CallFunctionAsync<int>("() => window.foo", true, new Target.Realm(realms[0].Realm));
        var res2 = await bidi.Script.CallFunctionAsync<int>("() => window.foo", true, new Target.Realm(realms[1].Realm));

        Assert.That(res1, Is.EqualTo(3));
        Assert.That(res2, Is.EqualTo(5));
    }

    [Test]
    public void CanCallFunctionWithArgumentUndefined()
    {
        var arg = new LocalValue.Undefined();
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (typeof arg !== 'undefined') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNull()
    {
        var arg = new LocalValue.Null();
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== null) {
              throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentEmptyString()
    {
        var arg = new LocalValue.String(string.Empty);
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== '') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNonEmptyString()
    {
        var arg = new LocalValue.String("whoa");
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== 'whoa') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentRecentDate()
    {
        const string PinnedDateTimeString = "2025-03-09T00:30:33.083Z";

        var arg = new LocalValue.Date(PinnedDateTimeString);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.toISOString() !== '{{PinnedDateTimeString}}') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentEpochDate()
    {
        const string EpochString = "1970-01-01T00:00:00.000Z";

        var arg = new LocalValue.Date(EpochString);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.toISOString() !== '{{EpochString}}') {
                throw new Error("Assert failed: " + arg.toISOString());
              } 
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumber5()
    {
        var arg = new LocalValue.Number(5);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== 5) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberNegative5()
    {
        var arg = new LocalValue.Number(-5);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== -5) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumber0()
    {
        var arg = new LocalValue.Number(0);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== 0) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberNegative0()
    {
        var arg = new LocalValue.Number(double.NegativeZero);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== -0) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberPositiveInfinity()
    {
        var arg = new LocalValue.Number(double.PositiveInfinity);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== Number.POSITIVE_INFINITY) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberNegativeInfinity()
    {
        var arg = new LocalValue.Number(double.NegativeInfinity);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg !== Number.NEGATIVE_INFINITY) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentNumberNaN()
    {
        var arg = new LocalValue.Number(double.NaN);
        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (!isNaN(arg)) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentRegExp()
    {
        var arg = new LocalValue.RegExp(new LocalValue.RegExp.RegExpValue("foo*") { Flags = "g" });

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (!arg.test('foo') || arg.source !== 'foo*' || !arg.global) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentArray()
    {
        var arg = new LocalValue.Array([new LocalValue.String("hi")]);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.length !== 1 || arg[0] !== 'hi') {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentObject()
    {
        var arg = new LocalValue.Object([[new LocalValue.String("key"), new LocalValue.String("value")]]);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.key !== 'value' || Object.keys(arg).length !== 1) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentMap()
    {
        var arg = new LocalValue.Map([[new LocalValue.String("key"), new LocalValue.String("value")]]);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (arg.get('key') !== 'value' || arg.size !== 1) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [arg] });
        }, Throws.Nothing);
    }

    [Test]
    public void CanCallFunctionWithArgumentSet()
    {
        var argument = new LocalValue.Set([new LocalValue.String("key")]);

        Assert.That(async () =>
        {
            await context.Script.CallFunctionAsync($$"""
            (arg) => {
              if (!arg.has('key') || arg.size !== 1) {
                throw new Error("Assert failed: " + arg);
              }
            }
            """, false, new() { Arguments = [argument] });
        }, Throws.Nothing);
    }

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
