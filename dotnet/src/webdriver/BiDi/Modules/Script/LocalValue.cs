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
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

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

            case long l:
                return new NumberLocalValue(l);

            case DateTime dt:
                return new DateLocalValue(dt.ToString("o"));

            case BigInteger bigInt:
                return new BigIntLocalValue(bigInt.ToString());

            case string str:
                return new StringLocalValue(str);

            case IDictionary<string, string?> dictionary:
                {
                    var bidiObject = new List<List<LocalValue>>(dictionary.Count);
                    foreach (var item in dictionary)
                    {
                        bidiObject.Add([new StringLocalValue(item.Key), ConvertFrom(item.Value)]);
                    }

                    return new ObjectLocalValue(bidiObject);
                }

            case IDictionary<string, object?> dictionary:
                {
                    var bidiObject = new List<List<LocalValue>>(dictionary.Count);
                    foreach (var item in dictionary)
                    {
                        bidiObject.Add([new StringLocalValue(item.Key), ConvertFrom(item.Value)]);
                    }

                    return new ObjectLocalValue(bidiObject);
                }

            case IDictionary<int, object?> dictionary:
                {
                    var bidiObject = new List<List<LocalValue>>(dictionary.Count);
                    foreach (var item in dictionary)
                    {
                        bidiObject.Add([ConvertFrom(item.Key), ConvertFrom(item.Value)]);
                    }

                    return new MapLocalValue(bidiObject);
                }

            case IEnumerable<object?> list:
                return new ArrayLocalValue(list.Select(ConvertFrom).ToList());

            case object:
                {
                    const System.Reflection.BindingFlags Flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;

                    var properties = value.GetType().GetProperties(Flags);

                    var values = new List<List<LocalValue>>(properties.Length);
                    foreach (var property in properties)
                    {
                        object? propertyValue;
                        try
                        {
                            propertyValue = property.GetValue(value);
                        }
                        catch (Exception ex)
                        {
                            throw new BiDiException($"Could not retrieve property {property.Name} from {property.DeclaringType}", ex);
                        }
                        values.Add([property.Name, ConvertFrom(propertyValue)]);
                    }

                    return new ObjectLocalValue(values);
                }
        }
    }
}

public abstract record PrimitiveProtocolLocalValue : LocalValue;

public record NumberLocalValue(double Value) : PrimitiveProtocolLocalValue
{
    public static explicit operator NumberLocalValue(double n) => new NumberLocalValue(n);
}

public record StringLocalValue(string Value) : PrimitiveProtocolLocalValue;

public record NullLocalValue : PrimitiveProtocolLocalValue;

public record UndefinedLocalValue : PrimitiveProtocolLocalValue;

public record BooleanLocalValue(bool Value) : PrimitiveProtocolLocalValue;

public record BigIntLocalValue(string Value) : PrimitiveProtocolLocalValue;

public record ChannelLocalValue(ChannelProperties Value) : LocalValue
{
    // TODO: Revise why we need it
    [JsonInclude]
    internal string type = "channel";
}

public record ArrayLocalValue(IEnumerable<LocalValue> Value) : LocalValue;

public record DateLocalValue(string Value) : LocalValue;

public record MapLocalValue(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue;

public record ObjectLocalValue(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue;

public record RegExpLocalValue(RegExpValue Value) : LocalValue;

public record SetLocalValue(IEnumerable<LocalValue> Value) : LocalValue;
