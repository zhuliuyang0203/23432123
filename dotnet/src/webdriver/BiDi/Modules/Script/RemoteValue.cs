// <copyright file="RemoteValue.cs" company="Selenium Committers">
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
using System.Dynamic;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Modules.Script;

// https://github.com/dotnet/runtime/issues/72604
//[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
//[JsonDerivedType(typeof(NumberRemoteValue), "number")]
//[JsonDerivedType(typeof(BooleanRemoteValue), "boolean")]
//[JsonDerivedType(typeof(BigIntRemoteValue), "bigint")]
//[JsonDerivedType(typeof(StringRemoteValue), "string")]
//[JsonDerivedType(typeof(NullRemoteValue), "null")]
//[JsonDerivedType(typeof(UndefinedRemoteValue), "undefined")]
//[JsonDerivedType(typeof(SymbolRemoteValue), "symbol")]
//[JsonDerivedType(typeof(ArrayRemoteValue), "array")]
//[JsonDerivedType(typeof(ObjectRemoteValue), "object")]
//[JsonDerivedType(typeof(FunctionRemoteValue), "function")]
//[JsonDerivedType(typeof(RegExpRemoteValue), "regexp")]
//[JsonDerivedType(typeof(DateRemoteValue), "date")]
//[JsonDerivedType(typeof(MapRemoteValue), "map")]
//[JsonDerivedType(typeof(SetRemoteValue), "set")]
//[JsonDerivedType(typeof(WeakMapRemoteValue), "weakmap")]
//[JsonDerivedType(typeof(WeakSetRemoteValue), "weakset")]
//[JsonDerivedType(typeof(GeneratorRemoteValue), "generator")]
//[JsonDerivedType(typeof(ErrorRemoteValue), "error")]
//[JsonDerivedType(typeof(ProxyRemoteValue), "proxy")]
//[JsonDerivedType(typeof(PromiseRemoteValue), "promise")]
//[JsonDerivedType(typeof(TypedArrayRemoteValue), "typedarray")]
//[JsonDerivedType(typeof(ArrayBufferRemoteValue), "arraybuffer")]
//[JsonDerivedType(typeof(NodeListRemoteValue), "nodelist")]
//[JsonDerivedType(typeof(HtmlCollectionRemoteValue), "htmlcollection")]
//[JsonDerivedType(typeof(NodeRemoteValue), "node")]
//[JsonDerivedType(typeof(WindowProxyRemoteValue), "window")]
public abstract record RemoteValue
{
    public static explicit operator double(RemoteValue remoteValue) => (double)((NumberRemoteValue)remoteValue).Value;

    public static explicit operator int(RemoteValue remoteValue) => (int)(double)remoteValue;
    public static explicit operator long(RemoteValue remoteValue) => (long)(double)remoteValue;

    public static explicit operator string?(RemoteValue remoteValue)
    {
        return remoteValue switch
        {
            StringRemoteValue stringValue => stringValue.Value,
            NullRemoteValue => null,
            _ => throw new InvalidCastException($"Cannot convert {remoteValue} to string")
        };
    }

    // TODO: extend types
    public TResult? ConvertTo<TResult>()
    {
        if (this is UndefinedRemoteValue)
        {
            throw new BiDiException($"Unable to convert undefined to {typeof(TResult)}");
        }

        if (this is NullRemoteValue)
        {
            if (default(TResult) == null)
            {
                return default;
            }

            throw new BiDiException($"Unable to convert null to non-nullable value type {typeof(TResult)}");
        }

        if (this is BooleanRemoteValue b)
        {
            if (typeof(TResult) == typeof(bool) ||
                typeof(TResult) == typeof(bool?) ||
                typeof(TResult) == typeof(object))
            {
                return (TResult)(object)b.Value;
            }
        }

        if (this is DateRemoteValue date)
        {
            if (typeof(TResult) == typeof(DateTime) ||
                typeof(TResult) == typeof(DateTime?) ||
                typeof(TResult) == typeof(object))
            {
                return (TResult)(object)DateTime.Parse(date.Value);
            }

            if (typeof(TResult) == typeof(DateTimeOffset) ||
                typeof(TResult) == typeof(DateTimeOffset?))
            {
                return (TResult)(object)DateTimeOffset.Parse(date.Value);
            }

            if (typeof(TResult) == typeof(string))
            {
                return (TResult)(object)date.Value;
            }
        }

        if (this is NumberRemoteValue number)
        {
            if (typeof(TResult) == typeof(double) ||
                typeof(TResult) == typeof(double?) ||
                typeof(TResult) == typeof(object))
            {
                return (TResult)(object)number.Value;
            }

            if (typeof(TResult) == typeof(int) ||
                typeof(TResult) == typeof(int?))
            {
                if (double.IsPositiveInfinity(number.Value) ||
                    double.IsNegativeInfinity(number.Value) ||
                    double.IsNaN(number.Value))
                {
                    throw new BiDiException($"Cannot represent {number.Value} as an {nameof(Int32)}");
                }

                return (TResult)(object)(int)number.Value;
            }

            if (typeof(TResult) == typeof(float) ||
                typeof(TResult) == typeof(float?))
            {
                return (TResult)(object)(float)number.Value;
            }

            if (typeof(TResult) == typeof(BigInteger) ||
                typeof(TResult) == typeof(BigInteger?))
            {
                return (TResult)(object)new BigInteger(number.Value);
            }
        }

        if (this is BigIntRemoteValue bigInt)
        {
            var bigNumber = BigInteger.Parse(bigInt.Value);
            if (typeof(TResult) == typeof(BigInteger) ||
                typeof(TResult) == typeof(BigInteger?) ||
                typeof(TResult) == typeof(object))
            {
                return (TResult)(object)bigNumber;
            }

            if (typeof(TResult) == typeof(double) ||
                typeof(TResult) == typeof(double?) ||
                typeof(TResult) == typeof(object))
            {
                return (TResult)(object)(double)bigNumber;
            }

            if (typeof(TResult) == typeof(int) ||
                typeof(TResult) == typeof(int?))
            {
                return (TResult)(object)(int)bigNumber;
            }

            if (typeof(TResult) == typeof(float) ||
                typeof(TResult) == typeof(float?))
            {
                return (TResult)(object)(float)bigNumber;
            }
        }

        if (this is StringRemoteValue str)
        {
            if (typeof(TResult) == typeof(string) ||
                typeof(TResult) == typeof(object))
            {
                return (TResult)(object)str.Value;
            }
        }

        if (this is RegExpRemoteValue regex)
        {
            if (typeof(TResult) == typeof(string) ||
                typeof(TResult) == typeof(object))
            {
                if (string.IsNullOrEmpty(regex.Value.Pattern))
                {
                    return (TResult)(object)$"/{regex.Value.Pattern}";
                }

                return (TResult)(object)$"/{regex.Value.Pattern}/{regex.Value.Flags}";
            }

            if (typeof(TResult) == typeof(System.Text.RegularExpressions.Regex))
            {
                return (TResult)(object)new System.Text.RegularExpressions.Regex(regex.Value.Pattern, RegExpValue.JavaScriptFlagsToRegexOptions(regex.Value.Flags));
            }
        }

        if (this is ArrayRemoteValue array)
        {
            if (typeof(TResult) == typeof(IReadOnlyList<RemoteValue>) ||
                typeof(TResult) == typeof(IEnumerable<RemoteValue>) ||
                typeof(TResult) == typeof(object))
            {
                return (TResult)(object)array.Value;
            }

            if (typeof(TResult) == typeof(RemoteValue[]))
            {
                return (TResult)(object)array.Value.ToArray();
            }

            if (typeof(TResult) == typeof(List<RemoteValue>))
            {
                return (TResult)(object)array.Value.ToList();
            }
        }

        if (this is ObjectRemoteValue obj)
        {
            if (typeof(TResult) == typeof(IDictionary<string, RemoteValue>) ||
               typeof(TResult) == typeof(Dictionary<string, RemoteValue>))
            {
                return (TResult)(object)obj.Value.ToDictionary(value => ((StringRemoteValue)value[0]).Value, value => value[1]);
            }

            if (typeof(TResult) == typeof(IDictionary<RemoteValue, RemoteValue>) ||
               typeof(TResult) == typeof(Dictionary<RemoteValue, RemoteValue>))
            {
                return (TResult)(object)obj.Value.ToDictionary(value => value[0], value => value[1]);
            }

            if (typeof(TResult) == typeof(object))
            {
                // Handle dynamic here
                IDictionary<string, object?> values = new ExpandoObject();
                foreach (IReadOnlyList<RemoteValue> property in obj.Value)
                {
                    string propertyName = ((StringRemoteValue)property[0]).Value;
                    values[propertyName] = property[1].ConvertTo<dynamic>();
                }

                return (TResult)(object)values;
            }
        }

        if (this is MapRemoteValue map)
        {
            if (typeof(TResult) == typeof(IDictionary<RemoteValue, RemoteValue>) ||
               typeof(TResult) == typeof(Dictionary<RemoteValue, RemoteValue>))
            {
                return (TResult)(object)map.Value.ToDictionary(value => value[0], value => value[1]);
            }
            if (typeof(TResult) == typeof(IDictionary<string, RemoteValue>) ||
               typeof(TResult) == typeof(Dictionary<string, RemoteValue>))
            {
                return (TResult)(object)map.Value.ToDictionary(value => ((StringRemoteValue)value[0]).Value, value => value[1]);
            }
        }

        if (this is SetRemoteValue set)
        {
            if (typeof(TResult) == typeof(IReadOnlyList<RemoteValue>) ||
                typeof(TResult) == typeof(IEnumerable<RemoteValue>) ||
                typeof(TResult) == typeof(object))
            {
                return (TResult)(object)set.Value;
            }

            if (typeof(TResult) == typeof(RemoteValue[]))
            {
                return (TResult)(object)set.Value.ToArray();
            }

            if (typeof(TResult) == typeof(List<RemoteValue>))
            {
                return (TResult)(object)set.Value.ToList();
            }

            if (typeof(TResult) == typeof(ISet<RemoteValue>) ||
#if NET8_0_OR_GREATER
                typeof(TResult) == typeof(IReadOnlySet<RemoteValue>) ||
#endif
                typeof(TResult) == typeof(HashSet<RemoteValue>))
            {
                return (TResult)(object)new HashSet<RemoteValue>(set.Value);
            }
        }

        throw new BiDiException($"Cannot convert to type {typeof(TResult)} from {this}");
    }
}

public record NumberRemoteValue(double Value) : PrimitiveProtocolRemoteValue;

public record BooleanRemoteValue(bool Value) : PrimitiveProtocolRemoteValue;

public record BigIntRemoteValue(string Value) : PrimitiveProtocolRemoteValue;

public record StringRemoteValue(string Value) : PrimitiveProtocolRemoteValue;

public record NullRemoteValue : PrimitiveProtocolRemoteValue;

public record UndefinedRemoteValue : PrimitiveProtocolRemoteValue;

public record SymbolRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record ArrayRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<RemoteValue>? Value { get; set; }
}

public record ObjectRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<IReadOnlyList<RemoteValue>>? Value { get; set; }
}

public record FunctionRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RegExpRemoteValue(RegExpValue Value) : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record DateRemoteValue(string Value) : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record MapRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<IReadOnlyList<RemoteValue>>? Value { get; set; }
}

public record SetRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<RemoteValue>? Value { get; set; }
}

public record WeakMapRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record WeakSetRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record GeneratorRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record ErrorRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record ProxyRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record PromiseRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record TypedArrayRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record ArrayBufferRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record NodeListRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<RemoteValue>? Value { get; set; }
}

public record HtmlCollectionRemoteValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<RemoteValue>? Value { get; set; }
}

public record NodeRemoteValue : RemoteValue, ISharedReference
{
    [JsonInclude]
    public string? SharedId { get; internal set; }

    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    [JsonInclude]
    public NodeProperties? Value { get; internal set; }
}

public record WindowProxyRemoteValue(WindowProxyProperties Value) : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public abstract record PrimitiveProtocolRemoteValue : RemoteValue;

public enum Mode
{
    Open,
    Closed
}
