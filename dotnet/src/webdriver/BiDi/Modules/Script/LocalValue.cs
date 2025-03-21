// <copyright file="LocalValue.cs" company="Selenium Committers">
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
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenQA.Selenium.BiDi.Modules.Script;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(NumberLocalValue), "number")]
[JsonDerivedType(typeof(StringLocalValue), "string")]
[JsonDerivedType(typeof(NullLocalValue), "null")]
[JsonDerivedType(typeof(UndefinedLocalValue), "undefined")]
[JsonDerivedType(typeof(BooleanLocalValue), "boolean")]
[JsonDerivedType(typeof(BigIntLocalValue), "bigint")]
[JsonDerivedType(typeof(ChannelLocalValue), "channel")]
[JsonDerivedType(typeof(ArrayLocalValue), "array")]
[JsonDerivedType(typeof(DateLocalValue), "date")]
[JsonDerivedType(typeof(MapLocalValue), "map")]
[JsonDerivedType(typeof(ObjectLocalValue), "object")]
[JsonDerivedType(typeof(RegExpLocalValue), "regexp")]
[JsonDerivedType(typeof(SetLocalValue), "set")]
public abstract record LocalValue
{
    public static implicit operator LocalValue(bool? value) { return value is bool b ? new BooleanLocalValue(b) : new NullLocalValue(); }
    public static implicit operator LocalValue(int? value) { return value is int i ? new NumberLocalValue(i) : new NullLocalValue(); }
    public static implicit operator LocalValue(double? value) { return value is double d ? new NumberLocalValue(d) : new NullLocalValue(); }
    public static implicit operator LocalValue(string? value) { return value is null ? new NullLocalValue() : new StringLocalValue(value); }

    // TODO: Extend converting from types
    public static LocalValue ConvertFrom(object? value)
    {
        switch (value)
        {
            case LocalValue localValue:
                return localValue;

            case null:
                return new NullLocalValue();

            case bool b:
                return new BooleanLocalValue(b);

            case int i:
                return new NumberLocalValue(i);

            case double d:
                return new NumberLocalValue(d);

            case string str:
                return new StringLocalValue(str);

            case IEnumerable<object?> list:
                return new ArrayLocalValue(list.Select(ConvertFrom).ToList());

            case object:
                {
                    var type = value.GetType();

                    var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    List<List<LocalValue>> values = [];

                    foreach (var property in properties)
                    {
                        values.Add([property.Name, ConvertFrom(property.GetValue(value))]);
                    }

                    return new ObjectLocalValue(values);
                }
        }
    }

    private static readonly BigInteger MaxDouble = new BigInteger(double.MaxValue);
    private static readonly BigInteger MinDouble = new BigInteger(double.MinValue);

    public static LocalValue ConvertFrom(JsonNode? node)
    {
        if (node is null)
        {
            return new NullLocalValue();
        }

        switch (node.GetValueKind())
        {
            case System.Text.Json.JsonValueKind.Null:
                return new NullLocalValue();

            case System.Text.Json.JsonValueKind.True:
                return new BooleanLocalValue(true);

            case System.Text.Json.JsonValueKind.False:
                return new BooleanLocalValue(false);

            case System.Text.Json.JsonValueKind.String:
                return new StringLocalValue(node.ToString());

            case System.Text.Json.JsonValueKind.Number:
                {
                    var numberString = node.ToString();

                    var bigNumber = BigInteger.Parse(numberString);

                    if (bigNumber > MaxDouble || bigNumber < MinDouble)
                    {
                        return new BigIntLocalValue(numberString);
                    }

                    return new NumberLocalValue(double.Parse(numberString));
                }

            case System.Text.Json.JsonValueKind.Array:
                return new ArrayLocalValue(node.AsArray().Select(ConvertFrom));

            case System.Text.Json.JsonValueKind.Object:
                var convertedToListForm = node.AsObject().Select(property => new LocalValue[] { new StringLocalValue(property.Key), ConvertFrom(property.Value) }).ToList();
                return new ObjectLocalValue(convertedToListForm);

            default:
                throw new InvalidOperationException("Invalid JSON node");
        }
    }
}

public abstract record PrimitiveProtocolLocalValue : LocalValue;

public record NumberLocalValue(double Value) : PrimitiveProtocolLocalValue
{
    public static implicit operator NumberLocalValue(double n) => new NumberLocalValue(n);
}

public record StringLocalValue(string Value) : PrimitiveProtocolLocalValue;

public record NullLocalValue : PrimitiveProtocolLocalValue;

public record UndefinedLocalValue : PrimitiveProtocolLocalValue;

public record BooleanLocalValue(bool Value) : PrimitiveProtocolLocalValue;

public record BigIntLocalValue(string Value) : PrimitiveProtocolLocalValue;

public record ChannelLocalValue(ChannelLocalValue.ChannelProperties Value) : LocalValue
{
    // TODO: Revise why we need it
    [JsonInclude]
    internal string type = "channel";

    public record ChannelProperties(Channel Channel)
    {
        public SerializationOptions? SerializationOptions { get; set; }

        public ResultOwnership? Ownership { get; set; }
    }
}

public record ArrayLocalValue(IEnumerable<LocalValue> Value) : LocalValue;

public record DateLocalValue(string Value) : LocalValue;

public record MapLocalValue(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue;

public record ObjectLocalValue(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue;

public record RegExpLocalValue(RegExpValue Value) : LocalValue;

public record SetLocalValue(IEnumerable<LocalValue> Value) : LocalValue;
