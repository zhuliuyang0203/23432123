// <copyright file="BiDiConnectionJsonSerializerContext.cs" company="Selenium Committers">
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

using System.Text.Json;
using System.Text.Json.Serialization;

using OpenQA.Selenium.BiDi.Communication.Json.Converters;

namespace OpenQA.Selenium.BiDi.Communication.Json;

#region https://github.com/dotnet/runtime/issues/72604
[JsonSerializable(typeof(MessageSuccess))]
[JsonSerializable(typeof(MessageError))]
[JsonSerializable(typeof(MessageEvent))]
#endregion

[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(Command))]

[JsonSerializable(typeof(EventArgs))]
[JsonSerializable(typeof(BrowsingContextEventArgs))]
[JsonSerializable(typeof(Modules.Session.StatusCommand))]
[JsonSerializable(typeof(Modules.Session.StatusResult))]
[JsonSerializable(typeof(Modules.Session.NewCommand))]
[JsonSerializable(typeof(Modules.Session.NewResult))]
[JsonSerializable(typeof(Modules.Session.EndCommand))]
[JsonSerializable(typeof(Modules.Session.SubscribeCommand))]
[JsonSerializable(typeof(Modules.Session.SubscribeResult))]
[JsonSerializable(typeof(Modules.Session.UnsubscribeByIdCommand))]
[JsonSerializable(typeof(Modules.Session.UnsubscribeByAttributesCommand))]

internal partial class BiDiConnectionJsonSerializerContext : JsonSerializerContext
{
    public static JsonSerializerOptions CreateOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,

            // BiDi returns special numbers such as "NaN" as strings
            // Additionally, -0 is returned as a string "-0"
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals | JsonNumberHandling.AllowReadingFromString,
            Converters =
            {
                new BiDiDateTimeOffsetConverter(),
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            },
            TypeInfoResolverChain =
            {
                Default
            }
        };
    }
}
