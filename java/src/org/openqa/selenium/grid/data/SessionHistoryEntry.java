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

package org.openqa.selenium.grid.data;

import java.time.Instant;
import java.util.Objects;
import org.openqa.selenium.internal.Require;
import org.openqa.selenium.remote.SessionId;

public class SessionHistoryEntry {
  private final SessionId sessionId;
  private final Instant startTime;
  private Instant stopTime;

  public SessionHistoryEntry(SessionId sessionId, Instant startTime, Instant stopTime) {
    this.sessionId = Require.nonNull("Session ID", sessionId);
    this.startTime = Require.nonNull("Start time", startTime);
    this.stopTime = stopTime; // Can be null for ongoing sessions
  }

  public SessionId getSessionId() {
    return sessionId;
  }

  public Instant getStartTime() {
    return startTime;
  }

  public Instant getStopTime() {
    return stopTime;
  }

  public void setStopTime(Instant stopTime) {
    this.stopTime = stopTime;
  }

  @Override
  public boolean equals(Object o) {
    if (this == o) return true;
    if (!(o instanceof SessionHistoryEntry)) return false;
    SessionHistoryEntry that = (SessionHistoryEntry) o;
    return Objects.equals(sessionId, that.sessionId)
        && Objects.equals(startTime, that.startTime)
        && Objects.equals(stopTime, that.stopTime);
  }

  @Override
  public int hashCode() {
    return Objects.hash(sessionId, startTime, stopTime);
  }
}
