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

package org.openqa.selenium.grid.node;

import static org.assertj.core.api.Assertions.assertThat;
import static org.openqa.selenium.remote.http.HttpMethod.GET;

import java.time.Instant;
import java.util.Arrays;
import java.util.List;
import org.junit.jupiter.api.Test;
import org.openqa.selenium.grid.data.SessionHistoryEntry;
import org.openqa.selenium.json.Json;
import org.openqa.selenium.remote.SessionId;
import org.openqa.selenium.remote.http.HttpRequest;
import org.openqa.selenium.remote.http.HttpResponse;

class GetNodeSessionHistoryTest {

  @Test
  void shouldReturnSessionHistoryAsJson() {
    SessionId sessionId = new SessionId("test-session");
    Instant startTime = Instant.now();
    Instant stopTime = startTime.plusSeconds(60);
    SessionHistoryEntry entry = new SessionHistoryEntry(sessionId, startTime, stopTime);

    TestNode node = new TestNode(Arrays.asList(entry));
    GetNodeSessionHistory handler = new GetNodeSessionHistory(node);

    HttpResponse response = handler.execute(new HttpRequest(GET, "/"));

    assertThat(response.getStatus()).isEqualTo(200);
    String content = response.getContentString();
    assertThat(content).contains("test-session");
    assertThat(content).contains("value");

    Json json = new Json();
    Object responseObj = json.toType(content, Object.class);
    assertThat(responseObj).isNotNull();
  }

  @Test
  void shouldReturnEmptyListWhenNoHistory() {
    TestNode node = new TestNode(Arrays.asList());
    GetNodeSessionHistory handler = new GetNodeSessionHistory(node);

    HttpResponse response = handler.execute(new HttpRequest(GET, "/"));

    assertThat(response.getStatus()).isEqualTo(200);
    String content = response.getContentString();
    assertThat(content).contains("value");
    assertThat(content).contains("[]");
  }

  private static class TestNode extends Node {
    private final List<SessionHistoryEntry> history;

    TestNode(List<SessionHistoryEntry> history) {
      super(null, null, null, null, null);
      this.history = history;
    }

    @Override
    public List<SessionHistoryEntry> getSessionHistory() {
      return history;
    }

    @Override
    public org.openqa.selenium.internal.Either<
            org.openqa.selenium.WebDriverException,
            org.openqa.selenium.grid.data.CreateSessionResponse>
        newSession(org.openqa.selenium.grid.data.CreateSessionRequest sessionRequest) {
      return null;
    }

    @Override
    public org.openqa.selenium.remote.http.HttpResponse executeWebDriverCommand(
        org.openqa.selenium.remote.http.HttpRequest req) {
      return null;
    }

    @Override
    public org.openqa.selenium.grid.data.Session getSession(
        org.openqa.selenium.remote.SessionId id) {
      return null;
    }

    @Override
    public org.openqa.selenium.remote.http.HttpResponse uploadFile(
        org.openqa.selenium.remote.http.HttpRequest req, org.openqa.selenium.remote.SessionId id) {
      return null;
    }

    @Override
    public org.openqa.selenium.remote.http.HttpResponse downloadFile(
        org.openqa.selenium.remote.http.HttpRequest req, org.openqa.selenium.remote.SessionId id) {
      return null;
    }

    @Override
    public void stop(org.openqa.selenium.remote.SessionId id) {}

    @Override
    public boolean isSessionOwner(org.openqa.selenium.remote.SessionId id) {
      return false;
    }

    @Override
    public boolean tryAcquireConnection(org.openqa.selenium.remote.SessionId id) {
      return false;
    }

    @Override
    public void releaseConnection(org.openqa.selenium.remote.SessionId id) {}

    @Override
    public boolean isSupporting(org.openqa.selenium.Capabilities capabilities) {
      return false;
    }

    @Override
    public org.openqa.selenium.grid.data.NodeStatus getStatus() {
      return null;
    }

    @Override
    public org.openqa.selenium.grid.node.HealthCheck getHealthCheck() {
      return null;
    }

    @Override
    public void drain() {}
  }
}
