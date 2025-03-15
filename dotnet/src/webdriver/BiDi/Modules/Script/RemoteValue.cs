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
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenQA.Selenium.BiDi.Modules.Script;

// https://github.com/dotnet/runtime/issues/72604
//[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
//[JsonDerivedType(typeof(RemoteNumberValue), "number")]
//[JsonDerivedType(typeof(RemoteBooleanValue), "boolean")]
//[JsonDerivedType(typeof(RemoteBigIntValue), "bigint")]
//[JsonDerivedType(typeof(RemoteStringValue), "string")]
//[JsonDerivedType(typeof(RemoteNullValue), "null")]
//[JsonDerivedType(typeof(RemoteUndefinedValue), "undefined")]
//[JsonDerivedType(typeof(RemoteSymbolValue), "symbol")]
//[JsonDerivedType(typeof(RemoteArrayValue), "array")]
//[JsonDerivedType(typeof(RemoteObjectValue), "object")]
//[JsonDerivedType(typeof(RemoteFunctionValue), "function")]
//[JsonDerivedType(typeof(RemoteRegExpValue), "regexp")]
//[JsonDerivedType(typeof(RemoteDateValue), "date")]
//[JsonDerivedType(typeof(RemoteMapValue), "map")]
//[JsonDerivedType(typeof(RemoteSetValue), "set")]
//[JsonDerivedType(typeof(RemoteWeakMapValue), "weakmap")]
//[JsonDerivedType(typeof(RemoteWeakSetValue), "weakset")]
//[JsonDerivedType(typeof(RemoteGeneratorValue), "generator")]
//[JsonDerivedType(typeof(RemoteErrorValue), "error")]
//[JsonDerivedType(typeof(RemoteProxyValue), "proxy")]
//[JsonDerivedType(typeof(RemotePromiseValue), "promise")]
//[JsonDerivedType(typeof(RemoteTypedArrayValue), "typedarray")]
//[JsonDerivedType(typeof(RemoteArrayBufferValue), "arraybuffer")]
//[JsonDerivedType(typeof(RemoteNodeListValue), "nodelist")]
//[JsonDerivedType(typeof(RemoteHtmlCollectionValue), "htmlcollection")]
//[JsonDerivedType(typeof(RemoteNodeValue), "node")]
//[JsonDerivedType(typeof(RemoteWindowProxyValue), "window")]
public abstract record RemoteValue
{
    public static implicit operator double(RemoteValue remoteValue) => (double)((RemoteNumberValue)remoteValue).Value;

    public static implicit operator int(RemoteValue remoteValue) => (int)(double)remoteValue;
    public static implicit operator long(RemoteValue remoteValue) => (long)(double)remoteValue;

    public static implicit operator string?(RemoteValue remoteValue)
    {
        return remoteValue switch
        {
            RemoteStringValue stringValue => stringValue.Value,
            RemoteNullValue => null,
            _ => throw new InvalidCastException($"Cannot convert {remoteValue} to string")
        };
    }

    // TODO: extend types
    public TResult? ConvertTo<TResult>()
    {
        var type = typeof(TResult);

        if (type == typeof(bool))
        {
            return (TResult)(Convert.ToBoolean(((RemoteBooleanValue)this).Value) as object);
        }
        if (type == typeof(int))
        {
            return (TResult)(Convert.ToInt32(((RemoteNumberValue)this).Value) as object);
        }
        else if (type == typeof(string))
        {
            return (TResult)(((RemoteStringValue)this).Value as object);
        }
        else if (type is object)
        {
            // :)
            return (TResult)new object();
        }

        throw new BiDiException("Cannot convert .....");
    }
}

public record RemoteBigIntValue(string Value) : PrimitiveProtocolRemoteValue;

public record RemoteNumberValue(double Value) : PrimitiveProtocolRemoteValue;

public record RemoteBooleanValue(bool Value) : PrimitiveProtocolRemoteValue;

public record RemoteStringValue(string Value) : PrimitiveProtocolRemoteValue;

public record RemoteNullValue : PrimitiveProtocolRemoteValue;

public record RemoteUndefinedValue : PrimitiveProtocolRemoteValue;

public record RemoteSymbolValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteArrayValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<RemoteValue>? Value { get; set; }
}

public record RemoteObjectValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<IReadOnlyList<RemoteValue>>? Value { get; set; }
}

public record RemoteFunctionValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteRegExpValue(RemoteRegExpValue.RegExpValue Value) : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public record RegExpValue(string Pattern)
    {
        public string? Flags { get; set; }
    }
}

public record RemoteDateValue(string Value) : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteMapValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<IReadOnlyList<RemoteValue>>? Value { get; set; }
}

public record RemoteSetValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<RemoteValue>? Value { get; set; }
}

public record RemoteWeakMapValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteWeakSetValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteGeneratorValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteErrorValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteProxyValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemotePromiseValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteTypedArrayValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteArrayBufferValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }
}

public record RemoteNodeListValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<RemoteValue>? Value { get; set; }
}

public record RemoteHtmlCollectionValue : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public IReadOnlyList<RemoteValue>? Value { get; set; }
}

public record RemoteNodeValue : RemoteValue, ISharedReference
{
    [JsonInclude]
    public string? SharedId { get; internal set; }

    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    [JsonInclude]
    public NodeProperties? Value { get; internal set; }
}

public record RemoteWindowProxyValue(RemoteWindowProxyValue.Properties Value) : RemoteValue
{
    public Handle? Handle { get; set; }

    public InternalId? InternalId { get; set; }

    public record Properties(BrowsingContext.BrowsingContext Context);
}

public abstract record PrimitiveProtocolRemoteValue : RemoteValue;

public enum Mode
{
    Open,
    Closed
}
