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
        var jsonDocument = JsonDocument.ParseValue(ref reader);

        if (jsonDocument.RootElement.ValueKind == JsonValueKind.String)
        {
            return RemoteValue.String(jsonDocument.RootElement.GetString());
        }

        return jsonDocument.RootElement.GetProperty("type").ToString() switch
        {
            "number" => jsonDocument.Deserialize<RemoteNumberValue>(options),
            "boolean" => jsonDocument.Deserialize<RemoteBooleanValue>(options),
            "bigint" => jsonDocument.Deserialize<RemoteBigIntValue>(options),
            "string" => jsonDocument.Deserialize<RemoteStringValue>(options),
            "null" => jsonDocument.Deserialize<RemoteNullValue>(options),
            "undefined" => jsonDocument.Deserialize<RemoteUndefinedValue>(options),
            "symbol" => jsonDocument.Deserialize<RemoteSymbolValue>(options),
            "array" => jsonDocument.Deserialize<RemoteArrayValue>(options),
            "object" => jsonDocument.Deserialize<RemoteObjectValue>(options),
            "function" => jsonDocument.Deserialize<RemoteFunctionValue>(options),
            "regexp" => jsonDocument.Deserialize<RemoteRegExpValue>(options),
            "date" => jsonDocument.Deserialize<RemoteDateValue>(options),
            "map" => jsonDocument.Deserialize<RemoteMapValue>(options),
            "set" => jsonDocument.Deserialize<RemoteSetValue>(options),
            "weakmap" => jsonDocument.Deserialize<RemoteWeakMapValue>(options),
            "weakset" => jsonDocument.Deserialize<RemoteWeakSetValue>(options),
            "generator" => jsonDocument.Deserialize<RemoteGeneratorValue>(options),
            "error" => jsonDocument.Deserialize<RemoteErrorValue>(options),
            "proxy" => jsonDocument.Deserialize<RemoteProxyValue>(options),
            "promise" => jsonDocument.Deserialize<RemotePromiseValue>(options),
            "typedarray" => jsonDocument.Deserialize<RemoteTypedArrayValue>(options),
            "arraybuffer" => jsonDocument.Deserialize<RemoteArrayBufferValue>(options),
            "nodelist" => jsonDocument.Deserialize<RemoteNodeListValue>(options),
            "htmlcollection" => jsonDocument.Deserialize<RemoteHtmlCollectionValue>(options),
            "node" => jsonDocument.Deserialize<RemoteNodeValue>(options),
            "window" => jsonDocument.Deserialize<RemoteWindowProxyValue>(options),
            _ => null,
        };
    }

    public override void Write(Utf8JsonWriter writer, RemoteValue value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
