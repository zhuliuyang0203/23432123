// <copyright file="GetCookiesCommand.cs" company="Selenium Committers">
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

using OpenQA.Selenium.BiDi.Communication;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable enable

namespace OpenQA.Selenium.BiDi.Modules.Storage;

internal class GetCookiesCommand(GetCookiesCommandParameters @params) : Command<GetCookiesCommandParameters>(@params);

internal record GetCookiesCommandParameters : CommandParameters
{
    public CookieFilter? Filter { get; set; }

    public PartitionDescriptor? Partition { get; set; }
}

public record GetCookiesOptions : CommandOptions
{
    public CookieFilter? Filter { get; set; }

    public PartitionDescriptor? Partition { get; set; }
}

public record GetCookiesResult : IReadOnlyList<Network.Cookie>
{
    private readonly IReadOnlyList<Network.Cookie> _cookies;

    internal GetCookiesResult(IReadOnlyList<Network.Cookie> cookies, PartitionKey partitionKey)
    {
        _cookies = cookies;
        PartitionKey = partitionKey;
    }

    public PartitionKey PartitionKey { get; init; }

    public Network.Cookie this[int index] => _cookies[index];

    public int Count => _cookies.Count;

    public IEnumerator<Network.Cookie> GetEnumerator() => _cookies.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => (_cookies as IEnumerable).GetEnumerator();
}

public class CookieFilter
{
    public string? Name { get; set; }

    public Network.BytesValue? Value { get; set; }

    public string? Domain { get; set; }

    public string? Path { get; set; }

    public long? Size { get; set; }

    public bool? HttpOnly { get; set; }

    public bool? Secure { get; set; }

    public Network.SameSite? SameSite { get; set; }

    public DateTimeOffset? Expiry { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Context), "context")]
[JsonDerivedType(typeof(StorageKey), "storageKey")]
public abstract record PartitionDescriptor
{
    public record Context([property: JsonPropertyName("context")] BrowsingContext.BrowsingContext Descriptor) : PartitionDescriptor;

    public record StorageKey : PartitionDescriptor
    {
        public string? UserContext { get; set; }

        public string? SourceOrigin { get; set; }
    }
}
