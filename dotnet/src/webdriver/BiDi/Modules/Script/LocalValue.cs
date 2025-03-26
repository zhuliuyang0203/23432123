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

using System.Collections.Generic;
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
    public static implicit operator LocalValue(int value) { return new NumberLocalValue(value); }
    public static implicit operator LocalValue(string? value) { return value is null ? new NullLocalValue() : new StringLocalValue(value); }

    // TODO: Extend converting from types
    public static LocalValue ConvertFrom(object? value)
    {
        switch (value)
        {
            case LocalValue:
                return (LocalValue)value;
            case null:
                return new NullLocalValue();
            case int:
                return (int)value;
            case string:
                return (string)value;
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
