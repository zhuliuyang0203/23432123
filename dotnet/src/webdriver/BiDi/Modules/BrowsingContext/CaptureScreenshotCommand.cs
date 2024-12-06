// <copyright file="CaptureScreenshotCommand.cs" company="Selenium Committers">
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

namespace OpenQA.Selenium.BiDi.Modules.BrowsingContext;

internal class CaptureScreenshotCommand(CaptureScreenshotCommandParameters @params) : Command<CaptureScreenshotCommandParameters>(@params);

internal record CaptureScreenshotCommandParameters(BrowsingContext Context) : CommandParameters
{
    public Origin? Origin { get; set; }

    public ImageFormat? Format { get; set; }

    public ClipRectangle? Clip { get; set; }
}

public record CaptureScreenshotOptions : CommandOptions
{
    public Origin? Origin { get; set; }

    public ImageFormat? Format { get; set; }

    public ClipRectangle? Clip { get; set; }
}

public enum Origin
{
    Viewport,
    Document
}

public record struct ImageFormat(string Type)
{
    public double? Quality { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Box), "box")]
[JsonDerivedType(typeof(Element), "element")]
public abstract record ClipRectangle
{
    public record Box(double X, double Y, double Width, double Height) : ClipRectangle;

    public record Element([property: JsonPropertyName("element")] Script.SharedReference SharedReference) : ClipRectangle;
}

public record CaptureScreenshotResult(string Data)
{
    public byte[] ToByteArray() => System.Convert.FromBase64String(Data);
}
