// <copyright file="PortUtilities.cs" company="Selenium Committers">
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
using System.Net;
using System.Net.Sockets;

namespace OpenQA.Selenium.Internal;

/// <summary>
/// Encapsulates methods for working with ports.
/// </summary>
public static class PortUtilities
{
    /// <summary>
    /// Finds a random, free port to be listened on.
    /// </summary>
    /// <returns>A random, free port to be listened on.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a free port cannot be found due to socket binding issues.
    /// </exception>
    public static int FindFreePort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, 0));

            return ((IPEndPoint)socket.LocalEndPoint!).Port;
        }
        catch (SocketException ex)
        {
            throw new InvalidOperationException("Unable to find a free port.", ex);
        }
    }
}
