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

import static org.assertj.core.api.Assertions.assertThat;

import org.junit.jupiter.api.Test;
import org.openqa.selenium.json.Json;
import org.openqa.selenium.remote.SessionId;

class SessionClosedEventTest {

  @Test
  void shouldSerializeAndDeserializeWithSuccessStatus() {
    SessionId sessionId = new SessionId("test-session-123");
    SessionClosedEvent originalEvent = new SessionClosedEvent(sessionId, SessionStatus.SUCCESS);

    Json json = new Json();
    String serialized = json.toJson(originalEvent);
    SessionClosedEvent deserializedEvent = json.toType(serialized, SessionClosedEvent.class);

    assertThat(deserializedEvent).isNotNull();
    assertThat(deserializedEvent.getData(SessionId.class)).isEqualTo(sessionId);
    assertThat(deserializedEvent.getStatus()).isEqualTo(SessionStatus.SUCCESS);
  }

  @Test
  void shouldSerializeAndDeserializeWithFailedStatus() {
    SessionId sessionId = new SessionId("test-session-456");
    SessionClosedEvent originalEvent = new SessionClosedEvent(sessionId, SessionStatus.FAILED);

    Json json = new Json();
    String serialized = json.toJson(originalEvent);
    SessionClosedEvent deserializedEvent = json.toType(serialized, SessionClosedEvent.class);

    assertThat(deserializedEvent).isNotNull();
    assertThat(deserializedEvent.getData(SessionId.class)).isEqualTo(sessionId);
    assertThat(deserializedEvent.getStatus()).isEqualTo(SessionStatus.FAILED);
  }

  @Test
  void shouldUseDefaultSuccessStatusWhenNotSpecified() {
    SessionId sessionId = new SessionId("test-session-789");
    SessionClosedEvent event = new SessionClosedEvent(sessionId);

    assertThat(event.getStatus()).isEqualTo(SessionStatus.SUCCESS);
  }

  @Test
  void shouldHandleEventDataSerialization() {
    SessionId sessionId = new SessionId("test-session-abc");
    SessionClosedEvent event = new SessionClosedEvent(sessionId, SessionStatus.FAILED);

    String rawData = event.getRawData();
    assertThat(rawData).isNotEmpty();
    assertThat(rawData).contains(sessionId.toString());
  }
}
