// <copyright file="RemoteValueConverter.cs" company="Selenium Committers">
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

using OpenQA.Selenium.BiDi.Communication.Json.Internal;
using OpenQA.Selenium.BiDi.Modules.Script;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Communication.Json.Converters.Polymorphic;

// https://github.com/dotnet/runtime/issues/72604
internal class RemoteValueConverter : JsonConverter<RemoteValue>
{
    public override RemoteValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new StringRemoteValue(reader.GetString()!);
        }

        return reader.GetDiscriminator("type") switch
        {
            "number" => JsonSerializer.Deserialize<NumberRemoteValue>(ref reader, options),
            "boolean" => JsonSerializer.Deserialize<BooleanRemoteValue>(ref reader, options),
            "bigint" => JsonSerializer.Deserialize<BigIntRemoteValue>(ref reader, options),
            "string" => JsonSerializer.Deserialize<StringRemoteValue>(ref reader, options),
            "null" => JsonSerializer.Deserialize<NullRemoteValue>(ref reader, options),
            "undefined" => JsonSerializer.Deserialize<UndefinedRemoteValue>(ref reader, options),
            "symbol" => JsonSerializer.Deserialize<SymbolRemoteValue>(ref reader, options),
            "array" => JsonSerializer.Deserialize<ArrayRemoteValue>(ref reader, options),
            "object" => JsonSerializer.Deserialize<ObjectRemoteValue>(ref reader, options),
            "function" => JsonSerializer.Deserialize<FunctionRemoteValue>(ref reader, options),
            "regexp" => JsonSerializer.Deserialize<RegExpRemoteValue>(ref reader, options),
            "date" => JsonSerializer.Deserialize<DateRemoteValue>(ref reader, options),
            "map" => JsonSerializer.Deserialize<MapRemoteValue>(ref reader, options),
            "set" => JsonSerializer.Deserialize<SetRemoteValue>(ref reader, options),
            "weakmap" => JsonSerializer.Deserialize<WeakMapRemoteValue>(ref reader, options),
            "weakset" => JsonSerializer.Deserialize<WeakSetRemoteValue>(ref reader, options),
            "generator" => JsonSerializer.Deserialize<GeneratorRemoteValue>(ref reader, options),
            "error" => JsonSerializer.Deserialize<ErrorRemoteValue>(ref reader, options),
            "proxy" => JsonSerializer.Deserialize<ProxyRemoteValue>(ref reader, options),
            "promise" => JsonSerializer.Deserialize<PromiseRemoteValue>(ref reader, options),
            "typedarray" => JsonSerializer.Deserialize<TypedArrayRemoteValue>(ref reader, options),
            "arraybuffer" => JsonSerializer.Deserialize<ArrayBufferRemoteValue>(ref reader, options),
            "nodelist" => JsonSerializer.Deserialize<NodeListRemoteValue>(ref reader, options),
            "htmlcollection" => JsonSerializer.Deserialize<HtmlCollectionRemoteValue>(ref reader, options),
            "node" => JsonSerializer.Deserialize<NodeRemoteValue>(ref reader, options),
            "window" => JsonSerializer.Deserialize<WindowProxyRemoteValue>(ref reader, options),
            _ => null,
        };
    }

    public override void Write(Utf8JsonWriter writer, RemoteValue value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
