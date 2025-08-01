// <copyright file="Command.cs" company="Selenium Committers">
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

using OpenQA.Selenium.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace OpenQA.Selenium;

/// <summary>
/// Provides a way to send commands to the remote server
/// </summary>
public class Command
{
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = $"All trimming-unsafe access points to {nameof(s_jsonSerializerOptions)} are annotated as such")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = $"All AOT-unsafe access points to {nameof(s_jsonSerializerOptions)} are annotated as such")]
    private static class JsonOptionsHolder
    {
        public readonly static JsonSerializerOptions s_jsonSerializerOptions = new()
        {
            TypeInfoResolver = GetTypeInfoResolver(),
            Converters = { new ResponseValueJsonConverter() }
        };

        private static IJsonTypeInfoResolver GetTypeInfoResolver()
        {
#if NET8_0_OR_GREATER
            if (!System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported)
            {
                return CommandJsonSerializerContext.Default;
            }
#endif
            return JsonTypeInfoResolver.Combine(CommandJsonSerializerContext.Default, new DefaultJsonTypeInfoResolver());
        }
    }

    private readonly Dictionary<string, object?> _parameters;

    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class using a command name and a JSON-encoded string for the parameters.
    /// </summary>
    /// <param name="name">Name of the command</param>
    /// <param name="jsonParameters">Parameters for the command as a JSON-encoded string.</param>
    public Command(string name, string jsonParameters)
    {
        this.SessionId = null;
        this._parameters = ConvertParametersFromJson(jsonParameters) ?? new Dictionary<string, object?>();
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class for a Session
    /// </summary>
    /// <param name="sessionId">Session ID the driver is using</param>
    /// <param name="name">Name of the command</param>
    /// <param name="parameters">Parameters for that command</param>
    /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
    public Command(SessionId? sessionId, string name, Dictionary<string, object?>? parameters)
    {
        this.SessionId = sessionId;
        this._parameters = parameters ?? new Dictionary<string, object?>();
        this.Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Gets the SessionID of the command
    /// </summary>
    [JsonPropertyName("sessionId")]
    public SessionId? SessionId { get; }

    /// <summary>
    /// Gets the command name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; }

    /// <summary>
    /// Gets the parameters of the command
    /// </summary>
    [JsonPropertyName("parameters")]
    public Dictionary<string, object?> Parameters
    {
        [RequiresUnreferencedCode("Adding untyped parameter values for JSON serialization has best-effort AOT support. Ensure only Selenium types and well-known .NET types are added.")]
        [RequiresDynamicCode("Adding untyped parameter values for JSON serialization has best-effort AOT support. Ensure only Selenium types and well-known .NET types are added.")]
        get => _parameters;
    }

    /// <summary>
    /// Gets the parameters of the command as a JSON-encoded string.
    /// </summary>
    public string ParametersAsJsonString
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = $"All trimming-unsafe access points to {nameof(_parameters)} are annotated as such")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = $"All AOT-unsafe access points to {nameof(_parameters)} are annotated as such")]
        get
        {
            if (HasParameters())
            {
                try
                {
                    return JsonSerializer.Serialize(this._parameters, JsonOptionsHolder.s_jsonSerializerOptions);
                }
                catch (NotSupportedException ex)
                {
                    throw new WebDriverException("Attempted to serialize an unsupported type. Ensure you are using Selenium types, or well-known .NET types such as Dictionary<string, object> and object[]", ex);
                }
            }
            else
            {
                return "{}";
            }
        }
    }

    internal bool HasParameters()
    {
        return this._parameters != null && this._parameters.Count > 0;
    }

    internal bool TryGetValueAndRemoveIfNotNull(string key, [NotNullWhen(true)] out object? value)
    {
        if (this._parameters.TryGetValue(key, out value))
        {
            if (value is not null)
            {
                this._parameters.Remove(key);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns a string of the Command object
    /// </summary>
    /// <returns>A string representation of the Command Object</returns>
    public override string ToString()
    {
        return string.Concat("[", this.SessionId, "]: ", this.Name, " ", this.ParametersAsJsonString);
    }

    /// <summary>
    /// Gets the command parameters as a <see cref="Dictionary{K, V}"/>, with a string key, and an object value.
    /// </summary>
    /// <param name="value">The JSON-encoded string representing the command parameters.</param>
    /// <returns>A <see cref="Dictionary{K, V}"/> with a string keys, and an object value. </returns>
    /// <exception cref="JsonException">If <paramref name="value"/> is not a JSON object.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/> is <see langword="null"/>.</exception>
    private static Dictionary<string, object?>? ConvertParametersFromJson(string value)
    {
        Dictionary<string, object?>? parameters = JsonSerializer.Deserialize<Dictionary<string, object?>>(value, CommandJsonSerializerContext.Default.DictionaryStringObject!);
        return parameters;
    }
}

// Built-in types
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(byte))]
[JsonSerializable(typeof(sbyte))]
[JsonSerializable(typeof(char))]
[JsonSerializable(typeof(decimal))]
[JsonSerializable(typeof(double))]
[JsonSerializable(typeof(float))]
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(uint))]
[JsonSerializable(typeof(nint))]
[JsonSerializable(typeof(nuint))]
[JsonSerializable(typeof(long))]
[JsonSerializable(typeof(ulong))]
[JsonSerializable(typeof(short))]
[JsonSerializable(typeof(ushort))]
[JsonSerializable(typeof(string))]

// Selenium WebDriver types
[JsonSerializable(typeof(char[]))]
[JsonSerializable(typeof(byte[]))]
[JsonSerializable(typeof(Chromium.ChromiumNetworkConditions))]
[JsonSerializable(typeof(Cookie))]
[JsonSerializable(typeof(ReturnedCookie))]
[JsonSerializable(typeof(Proxy))]

// Selenium Dictionaries, primarily used in Capabilities
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(Dictionary<string, bool>))]
[JsonSerializable(typeof(Dictionary<string, byte>))]
[JsonSerializable(typeof(Dictionary<string, sbyte>))]
[JsonSerializable(typeof(Dictionary<string, char>))]
[JsonSerializable(typeof(Dictionary<string, decimal>))]
[JsonSerializable(typeof(Dictionary<string, double>))]
[JsonSerializable(typeof(Dictionary<string, float>))]
[JsonSerializable(typeof(Dictionary<string, int>))]
[JsonSerializable(typeof(Dictionary<string, uint>))]
[JsonSerializable(typeof(Dictionary<string, nint>))]
[JsonSerializable(typeof(Dictionary<string, nuint>))]
[JsonSerializable(typeof(Dictionary<string, long>))]
[JsonSerializable(typeof(Dictionary<string, ulong>))]
[JsonSerializable(typeof(Dictionary<string, short>))]
[JsonSerializable(typeof(Dictionary<string, ushort>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(object[]))]
[JsonSourceGenerationOptions(Converters = [typeof(ResponseValueJsonConverter)])]
internal partial class CommandJsonSerializerContext : JsonSerializerContext;
