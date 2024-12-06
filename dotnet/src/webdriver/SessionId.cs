// <copyright file="SessionId.cs" company="Selenium Committers">
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

#nullable enable

namespace OpenQA.Selenium
{
    /// <summary>
    /// Provides a mechanism for maintaining a session for a test
    /// </summary>
    public class SessionId : IEquatable<SessionId>, IEquatable<string>
    {
        private readonly string sessionOpaqueKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionId"/> class
        /// </summary>
        /// <param name="opaqueKey">Key for the session in use</param>
        /// <exception cref="ArgumentNullException">If <paramref name="opaqueKey"/> is <see langword="null"/>.</exception>
        public SessionId(string opaqueKey)
        {
            this.sessionOpaqueKey = opaqueKey ?? throw new ArgumentNullException(nameof(opaqueKey));
        }

        /// <summary>
        /// Get the value of the key
        /// </summary>
        /// <returns>The key in use</returns>
        public override string? ToString()
        {
            return this.sessionOpaqueKey;
        }

        /// <summary>
        /// Get the hash code of the key
        /// </summary>
        /// <returns>The hash code of the key</returns>
        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(this.sessionOpaqueKey);
        }

        /// <summary>
        /// Indicates whether the current session ID value is the same as <paramref name="obj"/>.
        /// </summary>
        /// <param name="obj">The session to compare to.</param>
        /// <returns><see langword="true"/> if the values are equal; otherwise, <see langword="false"/>.</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as SessionId);
        }

        /// <summary>
        /// Indicates whether the current session ID value is the same as <paramref name="other"/>.
        /// </summary>
        /// <param name="other">A value to compare with this session ID.</param>
        /// <returns><see langword="true"/> if the current session ID is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(SessionId? other)
        {
            return other is not null && Equals(other.sessionOpaqueKey);
        }

        /// <summary>
        /// Indicates whether the current session ID value is the same as <paramref name="other"/>.
        /// </summary>
        /// <param name="other">A value to compare with this session ID></param>
        /// <returns><see langword="true"/> if the current session ID is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
        public bool Equals(string? other)
        {
            return string.Equals(this.sessionOpaqueKey, other, StringComparison.Ordinal);
        }

        /// <summary>
        /// Compares the two values to determine equality.
        /// </summary>
        /// <param name="left">The value to compare with <paramref name="right"/>.</param>
        /// <param name="right">The value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(SessionId? left, SessionId? right)
        {
            if (left is null)
            {
                return right is null;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Compares the two values to determine inequality.
        /// </summary>
        /// <param name="left">The value to compare with <paramref name="right"/>.</param>
        /// <param name="right">The value to compare with <paramref name="left"/>.</param>
        /// <returns><see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(SessionId? left, SessionId? right)
        {
            return !(left == right);
        }
    }
}
