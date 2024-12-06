// <copyright file="RealmInfo.cs" company="Selenium Committers">
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

#nullable enable

namespace OpenQA.Selenium.BiDi.Modules.Script;

// https://github.com/dotnet/runtime/issues/72604
//[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
//[JsonDerivedType(typeof(Window), "window")]
//[JsonDerivedType(typeof(DedicatedWorker), "dedicated-worker")]
//[JsonDerivedType(typeof(SharedWorker), "shared-worker")]
//[JsonDerivedType(typeof(ServiceWorker), "service-worker")]
//[JsonDerivedType(typeof(Worker), "worker")]
//[JsonDerivedType(typeof(PaintWorklet), "paint-worklet")]
//[JsonDerivedType(typeof(AudioWorklet), "audio-worklet")]
//[JsonDerivedType(typeof(Worklet), "worklet")]
public abstract record RealmInfo(BiDi BiDi, Realm Realm, string Origin) : EventArgs(BiDi)
{
    public record Window(BiDi BiDi, Realm Realm, string Origin, BrowsingContext.BrowsingContext Context) : RealmInfo(BiDi, Realm, Origin)
    {
        public string? Sandbox { get; set; }
    }

    public record DedicatedWorker(BiDi BiDi, Realm Realm, string Origin, IReadOnlyList<Realm> Owners) : RealmInfo(BiDi, Realm, Origin);

    public record SharedWorker(BiDi BiDi, Realm Realm, string Origin) : RealmInfo(BiDi, Realm, Origin);

    public record ServiceWorker(BiDi BiDi, Realm Realm, string Origin) : RealmInfo(BiDi, Realm, Origin);

    public record Worker(BiDi BiDi, Realm Realm, string Origin) : RealmInfo(BiDi, Realm, Origin);

    public record PaintWorklet(BiDi BiDi, Realm Realm, string Origin) : RealmInfo(BiDi, Realm, Origin);

    public record AudioWorklet(BiDi BiDi, Realm Realm, string Origin) : RealmInfo(BiDi, Realm, Origin);

    public record Worklet(BiDi BiDi, Realm Realm, string Origin) : RealmInfo(BiDi, Realm, Origin);
}
