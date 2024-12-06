// <copyright file="LocateNodesCommand.cs" company="Selenium Committers">
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
using System.Collections;
using System.Collections.Generic;

#nullable enable

namespace OpenQA.Selenium.BiDi.Modules.BrowsingContext;

internal class LocateNodesCommand(LocateNodesCommandParameters @params) : Command<LocateNodesCommandParameters>(@params);

internal record LocateNodesCommandParameters(BrowsingContext Context, Locator Locator) : CommandParameters
{
    public long? MaxNodeCount { get; set; }

    public Script.SerializationOptions? SerializationOptions { get; set; }

    public IEnumerable<Script.SharedReference>? StartNodes { get; set; }
}

public record LocateNodesOptions : CommandOptions
{
    public long? MaxNodeCount { get; set; }

    public Script.SerializationOptions? SerializationOptions { get; set; }

    public IEnumerable<Script.SharedReference>? StartNodes { get; set; }
}

public record LocateNodesResult : IReadOnlyList<Script.RemoteValue.Node>
{
    private readonly IReadOnlyList<Script.RemoteValue.Node> _nodes;

    internal LocateNodesResult(IReadOnlyList<Script.RemoteValue.Node> nodes)
    {
        _nodes = nodes;
    }

    public Script.RemoteValue.Node this[int index] => _nodes[index];

    public int Count => _nodes.Count;

    public IEnumerator<Script.RemoteValue.Node> GetEnumerator() => _nodes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => (_nodes as IEnumerable).GetEnumerator();
}
