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

#nullable enable

namespace OpenQA.Selenium.BiDi.Modules.Script;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Number), "number")]
[JsonDerivedType(typeof(String), "string")]
[JsonDerivedType(typeof(Null), "null")]
[JsonDerivedType(typeof(Undefined), "undefined")]
[JsonDerivedType(typeof(Channel), "channel")]
[JsonDerivedType(typeof(Array), "array")]
[JsonDerivedType(typeof(Date), "date")]
[JsonDerivedType(typeof(Map), "map")]
[JsonDerivedType(typeof(Object), "object")]
[JsonDerivedType(typeof(RegExp), "regexp")]
[JsonDerivedType(typeof(Set), "set")]
public abstract record LocalValue
{
    public static implicit operator LocalValue(int value) { return new Number(value); }
    public static implicit operator LocalValue(string value) { return new String(value); }

    // TODO: Extend converting from types
    public static LocalValue ConvertFrom(object? value)
    {
        switch (value)
        {
            case LocalValue:
                return (LocalValue)value;
            case null:
                return new Null();
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

                    return new Object(values);
                }
        }
    }

    public abstract record PrimitiveProtocolLocalValue : LocalValue
    {

    }

    public record Number(long Value) : PrimitiveProtocolLocalValue
    {
        public static explicit operator Number(int n) => new Number(n);
    }

    public record String(string Value) : PrimitiveProtocolLocalValue;

    public record Null : PrimitiveProtocolLocalValue;

    public record Undefined : PrimitiveProtocolLocalValue;

    public record Channel(Channel.ChannelProperties Value) : LocalValue
    {
        [JsonInclude]
        internal string type = "channel";

        public record ChannelProperties(Script.Channel Channel)
        {
            public SerializationOptions? SerializationOptions { get; set; }

            public ResultOwnership? Ownership { get; set; }
        }
    }

    public record Array(IEnumerable<LocalValue> Value) : LocalValue;

    public record Date(string Value) : LocalValue;

    public record Map(IDictionary<string, LocalValue> Value) : LocalValue; // seems to implement IDictionary

    public record Object(IEnumerable<IEnumerable<LocalValue>> Value) : LocalValue;

    public record RegExp(RegExp.RegExpValue Value) : LocalValue
    {
        public record RegExpValue(string Pattern)
        {
            public string? Flags { get; set; }
        }
    }

    public record Set(IEnumerable<LocalValue> Value) : LocalValue;
}
