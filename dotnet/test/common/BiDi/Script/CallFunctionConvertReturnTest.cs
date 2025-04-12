// <copyright file="CallFunctionConvertReturnTest.cs" company="Selenium Committers">
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
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace OpenQA.Selenium.BiDi.Script;

public class CallFunctionConvertReturnTest : BiDiTestFixture
{
    [Test]
    public async Task ReturnUndefinedCannotDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return undefined; }", false);

        Assert.That(() => response.Result.ConvertTo<object>(), Throws.Exception);
    }

    [Test]
    public async Task ReturnNullCanSerializeToNullable()
    {
        var response = await context.Script.CallFunctionAsync("() => { return null; }", false);

        Assert.That(response.Result.ConvertTo<object>(), Is.Null);
        Assert.That(response.Result.ConvertTo<int?>(), Is.Null);
        Assert.That(() => response.Result.ConvertTo<int>(), Throws.Exception);
    }

    [Test]
    public async Task ReturnTrueCanDeserializeToBool()
    {
        var response = await context.Script.CallFunctionAsync("() => { return true; }", false);

        Assert.That(response.Result.ConvertTo<bool>(), Is.True);
        Assert.That(response.Result.ConvertTo<object>(), Is.True);
    }

    [Test]
    public async Task ReturnFalseCanDeserializeToBool()
    {
        var response = await context.Script.CallFunctionAsync("() => { return false; }", false);

        Assert.That(response.Result.ConvertTo<bool>(), Is.False);
        Assert.That(response.Result.ConvertTo<object>(), Is.False);
    }

    [Test]
    public async Task ReturnStringCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 'whoa'; }", false);

        Assert.That(response.Result.ConvertTo<string>(), Is.EqualTo("whoa"));
        Assert.That(response.Result.ConvertTo<object>(), Is.EqualTo("whoa"));
    }

    [Test]
    public async Task ReturnDateCanDeserialize()
    {
        const string PinnedDateTimeString = "2025-03-09T00:30:33.083Z";

        var response = await context.Script.CallFunctionAsync($$"""() => { return new Date('{{PinnedDateTimeString}}'); }""", false);

        Assert.That(response.Result.ConvertTo<DateTime>, Is.EqualTo(DateTime.Parse(PinnedDateTimeString)));
        Assert.That(response.Result.ConvertTo<DateTime?>, Is.EqualTo(DateTime.Parse(PinnedDateTimeString)));
        Assert.That(response.Result.ConvertTo<DateTimeOffset>, Is.EqualTo(DateTimeOffset.Parse(PinnedDateTimeString)));
        Assert.That(response.Result.ConvertTo<DateTimeOffset?>, Is.EqualTo(DateTimeOffset.Parse(PinnedDateTimeString)));
        Assert.That(response.Result.ConvertTo<object>, Is.EqualTo(DateTime.Parse(PinnedDateTimeString)));
    }


    [Test]
    public async Task ReturnDateNowCanDeserialize()
    {
        DateTime before = DateTime.Now;
        var response = await context.Script.CallFunctionAsync("""() => { return new Date(); }""", false);
        DateTime after = DateTime.Now;

        Assert.That(response.Result.ConvertTo<DateTime>, Is.InRange(before, after));
        Assert.That(response.Result.ConvertTo<DateTime?>, Is.InRange(before, after));
        Assert.That(response.Result.ConvertTo<object>, Is.InRange(before, after));
    }

    [Test]
    public async Task ReturnFiveCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 5; }", false);

        Assert.That(response.Result.ConvertTo<double>(), Is.EqualTo(5));
        Assert.That(response.Result.ConvertTo<double?>(), Is.EqualTo(5));
        Assert.That(response.Result.ConvertTo<int>(), Is.EqualTo(5));
        Assert.That(response.Result.ConvertTo<int?>(), Is.EqualTo(5));
        Assert.That(response.Result.ConvertTo<float>(), Is.EqualTo(5));
        Assert.That(response.Result.ConvertTo<float?>(), Is.EqualTo(5));
        Assert.That(response.Result.ConvertTo<BigInteger>(), Is.EqualTo(new BigInteger(5)));
        Assert.That(response.Result.ConvertTo<BigInteger?>(), Is.EqualTo(new BigInteger(5)));

        Assert.That(response.Result.ConvertTo<object>(), Is.EqualTo(5));
    }

    [Test]
    public async Task ReturnMinusFiveCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return -5; }", false);

        Assert.That(response.Result.ConvertTo<double>(), Is.EqualTo(-5));
        Assert.That(response.Result.ConvertTo<double?>(), Is.EqualTo(-5));
        Assert.That(response.Result.ConvertTo<int>(), Is.EqualTo(-5));
        Assert.That(response.Result.ConvertTo<int?>(), Is.EqualTo(-5));
        Assert.That(response.Result.ConvertTo<float>(), Is.EqualTo(-5));
        Assert.That(response.Result.ConvertTo<float?>(), Is.EqualTo(-5));
        Assert.That(response.Result.ConvertTo<BigInteger>(), Is.EqualTo(new BigInteger(-5)));
        Assert.That(response.Result.ConvertTo<BigInteger?>(), Is.EqualTo(new BigInteger(-5)));

        Assert.That(response.Result.ConvertTo<object>(), Is.EqualTo(-5));
    }

    [Test]
    public async Task ReturnZeroCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 0; }", false);

        Assert.That(response.Result.ConvertTo<double>(), Is.Zero);
        Assert.That(response.Result.ConvertTo<double?>(), Is.Zero);
        Assert.That(response.Result.ConvertTo<int>(), Is.Zero);
        Assert.That(response.Result.ConvertTo<int?>(), Is.Zero);
        Assert.That(response.Result.ConvertTo<float>(), Is.Zero);
        Assert.That(response.Result.ConvertTo<float?>(), Is.Zero);
        Assert.That(response.Result.ConvertTo<BigInteger>(), Is.EqualTo(BigInteger.Zero));
        Assert.That(response.Result.ConvertTo<BigInteger?>(), Is.EqualTo(BigInteger.Zero));

        Assert.That(response.Result.ConvertTo<object>(), Is.Zero);
    }

    [Test]
    public async Task ReturnNegativeZeroCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return -0; }", false);

        var @double = response.Result.ConvertTo<double>();
        Assert.That(@double, Is.Zero);
        Assert.That(double.IsNegative(@double), Is.True);

        var nullableDouble = response.Result.ConvertTo<double?>();
        Assert.That(nullableDouble, Is.Zero);
        Assert.That(double.IsNegative(nullableDouble.Value), Is.True);

        Assert.That(response.Result.ConvertTo<int>(), Is.Zero);
        Assert.That(response.Result.ConvertTo<int?>(), Is.Zero);


        var @float = response.Result.ConvertTo<float>();
        Assert.That(@float, Is.Zero);
        Assert.That(float.IsNegative(@float), Is.True);

        var nullableFloat = response.Result.ConvertTo<float?>();
        Assert.That(nullableFloat, Is.Zero);
        Assert.That(float.IsNegative(nullableFloat.Value), Is.True);

        Assert.That(response.Result.ConvertTo<BigInteger>(), Is.EqualTo(BigInteger.Zero));
        Assert.That(response.Result.ConvertTo<BigInteger?>(), Is.EqualTo(BigInteger.Zero));

        Assert.That(response.Result.ConvertTo<object>(), Is.Zero);
    }

    [Test]
    public async Task ReturnInfinityCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return Number.POSITIVE_INFINITY; }", false);

        Assert.That(response.Result.ConvertTo<double>(), Is.EqualTo(double.PositiveInfinity));
        Assert.That(response.Result.ConvertTo<double?>(), Is.EqualTo(double.PositiveInfinity));

        Assert.That(() => response.Result.ConvertTo<int>(), Throws.Exception);
        Assert.That(() => response.Result.ConvertTo<int?>(), Throws.Exception);

        Assert.That(response.Result.ConvertTo<float>(), Is.EqualTo(float.PositiveInfinity));
        Assert.That(response.Result.ConvertTo<float?>(), Is.EqualTo(float.PositiveInfinity));

        Assert.That(() => response.Result.ConvertTo<BigInteger>(), Throws.Exception);
        Assert.That(() => response.Result.ConvertTo<BigInteger?>(), Throws.Exception);
    }

    [Test]
    public async Task ReturnNegativeInfinityCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return Number.NEGATIVE_INFINITY; }", false);

        Assert.That(response.Result.ConvertTo<double>(), Is.EqualTo(double.NegativeInfinity));
        Assert.That(response.Result.ConvertTo<double?>(), Is.EqualTo(double.NegativeInfinity));

        var nullableDouble = response.Result.ConvertTo<double?>();
        Assert.That(double.IsNegativeInfinity(nullableDouble.Value));

        Assert.That(() => response.Result.ConvertTo<int>(), Throws.Exception);
        Assert.That(() => response.Result.ConvertTo<int?>(), Throws.Exception);

        Assert.That(response.Result.ConvertTo<float>(), Is.EqualTo(float.NegativeInfinity));
        Assert.That(response.Result.ConvertTo<float?>(), Is.EqualTo(float.NegativeInfinity));

        Assert.That(() => response.Result.ConvertTo<BigInteger>(), Throws.Exception);
        Assert.That(() => response.Result.ConvertTo<BigInteger?>(), Throws.Exception);
    }

    [Test]
    public async Task ReturnNaNCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return NaN; }", false);

        Assert.That(response.Result.ConvertTo<double>(), Is.NaN);
        Assert.That(response.Result.ConvertTo<double?>(), Is.NaN);

        Assert.That(() => response.Result.ConvertTo<int>(), Throws.Exception);
        Assert.That(() => response.Result.ConvertTo<int?>(), Throws.Exception);

        Assert.That(response.Result.ConvertTo<float>(), Is.NaN);
        Assert.That(response.Result.ConvertTo<float?>(), Is.NaN);

        Assert.That(() => response.Result.ConvertTo<BigInteger>(), Throws.Exception);
        Assert.That(() => response.Result.ConvertTo<BigInteger?>(), Throws.Exception);
    }

    [Test]
    public async Task ReturnSmallBigIntCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return 5n; }", false);

        Assert.That(response.Result.ConvertTo<double>(), Is.EqualTo(5));
        Assert.That(response.Result.ConvertTo<double?>(), Is.EqualTo(5));

        Assert.That(response.Result.ConvertTo<int>(), Is.EqualTo(5));
        Assert.That(response.Result.ConvertTo<int?>(), Is.EqualTo(5));

        Assert.That(response.Result.ConvertTo<float>(), Is.EqualTo(5));
        Assert.That(response.Result.ConvertTo<float?>(), Is.EqualTo(5));

        Assert.That(response.Result.ConvertTo<BigInteger>(), Is.EqualTo(new BigInteger(5)));
        Assert.That(response.Result.ConvertTo<BigInteger?>(), Is.EqualTo(new BigInteger(5)));
    }

    [Test]
    public async Task ReturnLargeBigIntCanDeserialize()
    {
        BigInteger moreThanMaxDouble = new BigInteger(double.MaxValue) * 2;
        var response = await context.Script.CallFunctionAsync($$"""() => { return BigInt('{{moreThanMaxDouble}}'); }""", false);

        Assert.That(response.Result.ConvertTo<double>(), Is.EqualTo(double.PositiveInfinity));
        Assert.That(response.Result.ConvertTo<double?>(), Is.EqualTo(double.PositiveInfinity));

        Assert.That(() => response.Result.ConvertTo<int>(), Throws.Exception);
        Assert.That(() => response.Result.ConvertTo<int?>(), Throws.Exception);

        Assert.That(response.Result.ConvertTo<float>(), Is.EqualTo(float.PositiveInfinity));
        Assert.That(response.Result.ConvertTo<float?>(), Is.EqualTo(float.PositiveInfinity));

        Assert.That(response.Result.ConvertTo<BigInteger>(), Is.EqualTo(moreThanMaxDouble));
        Assert.That(response.Result.ConvertTo<BigInteger?>(), Is.EqualTo(moreThanMaxDouble));
    }

    [Test]
    public async Task ReturnRegexCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return /foo*/i; }", false);

        Assert.That(response.Result.ConvertTo<string>(), Is.EqualTo("/foo*/i"));
        Assert.That(response.Result.ConvertTo<object>(), Is.EqualTo("/foo*/i"));

        var regex = response.Result.ConvertTo<System.Text.RegularExpressions.Regex>();
        Assert.That(regex.ToString(), Is.EqualTo("foo*"));
        Assert.That(regex.Options, Is.EqualTo(System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    [Test]
    public async Task ReturnArrayCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return ['hi']; }", false);

        Assert.That(response.Result.ConvertTo<IEnumerable<RemoteValue>>(), Is.EqualTo(new RemoteValue[] { new StringRemoteValue("hi") }));
        Assert.That(response.Result.ConvertTo<List<RemoteValue>>(), Is.EqualTo(new RemoteValue[] { new StringRemoteValue("hi") }));
        Assert.That(response.Result.ConvertTo<RemoteValue[]>(), Is.EqualTo(new RemoteValue[] { new StringRemoteValue("hi") }));
        Assert.That(response.Result.ConvertTo<object>(), Is.EqualTo(new RemoteValue[] { new StringRemoteValue("hi") }));
    }

    [Test]
    public async Task ReturnObjectCanDeserialize()
    {
        var response = await context.Script.CallFunctionAsync("() => { return { objKey: 'objValue' }; }", false);

        Assert.That(response.Result.ConvertTo<Dictionary<string, RemoteValue>>(), Is.EqualTo(new Dictionary<string, RemoteValue>
        {
            {"objKey", new StringRemoteValue("objValue") }
        }));

        Assert.That(response.Result.ConvertTo<Dictionary<RemoteValue, RemoteValue>>(), Is.EqualTo(new Dictionary<RemoteValue, RemoteValue>
        {
            {new StringRemoteValue("objKey"), new StringRemoteValue("objValue") }
        }));

        dynamic d = response.Result.ConvertTo<dynamic>();

        Assert.That(d.objKey, Is.EqualTo("objValue"));
    }


    [Test]
    public async Task ReturnSetCanDeserialize()
    {
        var expected = new SetRemoteValue { Value = [new StringRemoteValue("setKey")] };
        var response = await context.Script.CallFunctionAsync($$"""
            () => {
              const set = new Set();
              set.add('setKey');
              return set;
            }
            """, false);

        Assert.That(response.Result.ConvertTo<IEnumerable<RemoteValue>>(), Is.EqualTo(new RemoteValue[] { new StringRemoteValue("setKey") }));
        Assert.That(response.Result.ConvertTo<List<RemoteValue>>(), Is.EqualTo(new RemoteValue[] { new StringRemoteValue("setKey") }));
        Assert.That(response.Result.ConvertTo<RemoteValue[]>(), Is.EqualTo(new RemoteValue[] { new StringRemoteValue("setKey") }));

        Assert.That(response.Result.ConvertTo<object>(), Is.EqualTo(new HashSet<RemoteValue> { new StringRemoteValue("setKey") }));
        Assert.That(response.Result.ConvertTo<HashSet<RemoteValue>>(), Is.EqualTo(new HashSet<RemoteValue> { new StringRemoteValue("setKey") }));
        Assert.That(response.Result.ConvertTo<ISet<RemoteValue>>(), Is.EqualTo(new HashSet<RemoteValue> { new StringRemoteValue("setKey") }));
        Assert.That(response.Result.ConvertTo<IReadOnlySet<RemoteValue>>(), Is.EqualTo(new HashSet<RemoteValue> { new StringRemoteValue("setKey") }));
    }
}
