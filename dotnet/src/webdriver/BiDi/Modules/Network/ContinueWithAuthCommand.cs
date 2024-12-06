// <copyright file="ContinueWithAuthCommand.cs" company="Selenium Committers">
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
using System.Text.Json.Serialization;

#nullable enable

namespace OpenQA.Selenium.BiDi.Modules.Network;

internal class ContinueWithAuthCommand(ContinueWithAuthParameters @params) : Command<ContinueWithAuthParameters>(@params);

[JsonPolymorphic(TypeDiscriminatorPropertyName = "action")]
[JsonDerivedType(typeof(Credentials), "provideCredentials")]
[JsonDerivedType(typeof(Default), "default")]
[JsonDerivedType(typeof(Cancel), "cancel")]
internal abstract record ContinueWithAuthParameters(Request Request) : CommandParameters
{
    internal record Credentials(Request Request, [property: JsonPropertyName("credentials")] AuthCredentials AuthCredentials) : ContinueWithAuthParameters(Request);

    internal record Default(Request Request) : ContinueWithAuthParameters(Request);

    internal record Cancel(Request Request) : ContinueWithAuthParameters(Request);
}

public record ContinueWithAuthOptions : CommandOptions;

public record ContinueWithDefaultAuthOptions : CommandOptions;

public record ContinueWithCancelledAuthOptions : CommandOptions;
