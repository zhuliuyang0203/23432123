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

package org.openqa.selenium.grid.sessionqueue.jdbc;

import static org.assertj.core.api.Assertions.*;
import static org.openqa.selenium.remote.http.HttpMethod.POST;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import java.sql.Statement;
import java.time.Instant;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;
import org.openqa.selenium.SessionNotCreatedException;
import org.openqa.selenium.grid.config.Config;
import org.openqa.selenium.grid.config.ConfigException;
import org.openqa.selenium.grid.config.MapConfig;
import org.openqa.selenium.grid.data.RequestId;
import org.openqa.selenium.grid.data.SessionRequest;
import org.openqa.selenium.grid.data.SessionRequestCapability;
import org.openqa.selenium.grid.security.Secret;
import org.openqa.selenium.grid.sessionqueue.NewSessionQueue;
import org.openqa.selenium.internal.Either;
import org.openqa.selenium.remote.http.Contents;
import org.openqa.selenium.remote.http.HttpRequest;
import org.openqa.selenium.remote.http.HttpResponse;
import org.openqa.selenium.remote.tracing.DefaultTestTracer;
import org.openqa.selenium.remote.tracing.Tracer;

class JdbcBackedSessionQueueTest {
  private static Connection connection;
  private static final Tracer tracer = DefaultTestTracer.createTracer();
  private static final Secret secret = new Secret("test-secret");

  @BeforeAll
  public static void createDB() throws SQLException {
    connection = DriverManager.getConnection("jdbc:hsqldb:mem:sessionqueue", "SA", "");
    Statement createStatement = connection.createStatement();
    createStatement.executeUpdate(
        "CREATE TABLE session_queue (request_id VARCHAR(64) PRIMARY KEY, payload CLOB NOT NULL,"
            + " enqueue_time TIMESTAMP NOT NULL)");
  }

  @AfterAll
  public static void killDBConnection() throws SQLException {
    connection.close();
  }

  @Test
  void shouldThrowIllegalArgumentExceptionIfConnectionObjectIsNull() {
    assertThatThrownBy(() -> new JdbcBackedSessionQueue(tracer, secret, null))
        .isInstanceOf(IllegalArgumentException.class);
  }

  @Test
  void canAddAndRemoveSessionRequest() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    RequestId requestId = new RequestId(UUID.randomUUID());

    // Create a proper HttpRequest for SessionRequest constructor
    HttpRequest httpRequest = new HttpRequest(POST, "/session");
    httpRequest.setContent(Contents.utf8String("{\"capabilities\":{\"browserName\":\"chrome\"}}"));

    SessionRequest request = new SessionRequest(requestId, httpRequest, Instant.now());

    HttpResponse response = queue.addToQueue(request);
    assertThat(response.getStatus()).isEqualTo(200);

    Optional<SessionRequest> removed = queue.remove(requestId);
    assertThat(removed).isPresent();
    assertThat(removed.get().getRequestId()).isEqualTo(requestId);
  }

  @Test
  void getNextAvailableReturnsOldest() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue();

    RequestId requestId1 = new RequestId(UUID.randomUUID());
    HttpRequest httpRequest1 = new HttpRequest(POST, "/session");
    httpRequest1.setContent(
        Contents.utf8String("{\"capabilities\":{\"browserName\":\"firefox\"}}"));
    SessionRequest request1 = new SessionRequest(requestId1, httpRequest1, Instant.now());

    RequestId requestId2 = new RequestId(UUID.randomUUID());
    HttpRequest httpRequest2 = new HttpRequest(POST, "/session");
    httpRequest2.setContent(Contents.utf8String("{\"capabilities\":{\"browserName\":\"chrome\"}}"));
    SessionRequest request2 =
        new SessionRequest(requestId2, httpRequest2, Instant.now().plusSeconds(1));

    queue.addToQueue(request1);
    queue.addToQueue(request2);

    // Use getNextAvailable instead of getNextMatchingRequest
    var next = queue.getNextAvailable(Map.of());
    assertThat(next).isNotEmpty();
    assertThat(next.get(0).getRequestId()).isEqualTo(requestId1);
  }

  @Test
  void clearRemovesAllRequests() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue();

    RequestId requestId = new RequestId(UUID.randomUUID());
    HttpRequest httpRequest = new HttpRequest(POST, "/session");
    httpRequest.setContent(Contents.utf8String("{\"capabilities\":{\"browserName\":\"chrome\"}}"));
    SessionRequest request = new SessionRequest(requestId, httpRequest, Instant.now());

    queue.addToQueue(request);
    queue.clearQueue();

    var next = queue.getNextAvailable(Map.of());
    assertThat(next).isEmpty();
  }

  @Test
  void getQueueContentsReturnsAllRequests() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue();

    // Add multiple requests
    RequestId requestId1 = new RequestId(UUID.randomUUID());
    HttpRequest httpRequest1 = new HttpRequest(POST, "/session");
    httpRequest1.setContent(
        Contents.utf8String("{\"capabilities\":{\"firstMatch\":[{\"browserName\":\"firefox\"}]}}"));
    SessionRequest request1 = new SessionRequest(requestId1, httpRequest1, Instant.now());

    RequestId requestId2 = new RequestId(UUID.randomUUID());
    HttpRequest httpRequest2 = new HttpRequest(POST, "/session");
    httpRequest2.setContent(
        Contents.utf8String("{\"capabilities\":{\"firstMatch\":[{\"browserName\":\"chrome\"}]}}"));
    SessionRequest request2 =
        new SessionRequest(requestId2, httpRequest2, Instant.now().plusSeconds(1));

    queue.addToQueue(request1);
    queue.addToQueue(request2);

    // Get queue contents
    var contents = queue.getQueueContents();
    assertThat(contents).hasSize(2);

    // Verify first request (oldest)
    SessionRequestCapability first = contents.get(0);
    assertThat(first.getRequestId()).isEqualTo(requestId1);
    assertThat(first.getDesiredCapabilities()).isNotNull();

    // Verify second request
    SessionRequestCapability second = contents.get(1);
    assertThat(second.getRequestId()).isEqualTo(requestId2);
    assertThat(second.getDesiredCapabilities()).isNotNull();
  }

  @Test
  void getQueueContentsReturnsEmptyListWhenQueueIsEmpty() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue();

    var contents = queue.getQueueContents();
    assertThat(contents).isEmpty();
  }

  @Test
  void peekEmptyReturnsTrueWhenQueueIsEmpty() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue();

    assertThat(queue.peekEmpty()).isTrue();
  }

  @Test
  void peekEmptyReturnsFalseWhenQueueHasRequests() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue();

    RequestId requestId = new RequestId(UUID.randomUUID());
    HttpRequest httpRequest = new HttpRequest(POST, "/session");
    httpRequest.setContent(Contents.utf8String("{\"capabilities\":{\"browserName\":\"chrome\"}}"));
    SessionRequest request = new SessionRequest(requestId, httpRequest, Instant.now());

    queue.addToQueue(request);
    assertThat(queue.peekEmpty()).isFalse();
  }

  @Test
  void isReadyReturnsTrueWhenConnectionIsOpen() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    assertThat(queue.isReady()).isTrue();
  }

  @Test
  void removeReturnsEmptyWhenRequestNotFound() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue();

    RequestId nonExistentId = new RequestId(UUID.randomUUID());
    Optional<SessionRequest> removed = queue.remove(nonExistentId);
    assertThat(removed).isEmpty();
  }

  @Test
  void retryAddToQueueDelegatesToAddToQueue() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    RequestId requestId = new RequestId(UUID.randomUUID());

    HttpRequest httpRequest = new HttpRequest(POST, "/session");
    httpRequest.setContent(Contents.utf8String("{\"capabilities\":{\"browserName\":\"chrome\"}}"));
    SessionRequest request = new SessionRequest(requestId, httpRequest, Instant.now());

    boolean result = queue.retryAddToQueue(request);
    assertThat(result).isTrue();

    // Verify it was actually added
    Optional<SessionRequest> removed = queue.remove(requestId);
    assertThat(removed).isPresent();
  }

  @Test
  void completeDoesNotThrowException() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    RequestId requestId = new RequestId(UUID.randomUUID());

    // complete() method should not throw any exception - requires Either parameter
    assertThatCode(
            () -> queue.complete(requestId, Either.left(new SessionNotCreatedException("test"))))
        .doesNotThrowAnyException();
  }

  @Test
  void getNextAvailableReturnsEmptyWhenQueueIsEmpty() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue();

    var next = queue.getNextAvailable(Map.of());
    assertThat(next).isEmpty();
  }

  @Test
  void clearQueueReturnsCorrectRowCount() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue(); // Start with empty queue

    // Add multiple requests
    RequestId requestId1 = new RequestId(UUID.randomUUID());
    HttpRequest httpRequest1 = new HttpRequest(POST, "/session");
    httpRequest1.setContent(Contents.utf8String("{\"capabilities\":{\"browserName\":\"chrome\"}}"));
    SessionRequest request1 = new SessionRequest(requestId1, httpRequest1, Instant.now());

    RequestId requestId2 = new RequestId(UUID.randomUUID());
    HttpRequest httpRequest2 = new HttpRequest(POST, "/session");
    httpRequest2.setContent(
        Contents.utf8String("{\"capabilities\":{\"browserName\":\"firefox\"}}"));
    SessionRequest request2 = new SessionRequest(requestId2, httpRequest2, Instant.now());

    queue.addToQueue(request1);
    queue.addToQueue(request2);

    // Clear queue and verify row count
    int deletedRows = queue.clearQueue();
    assertThat(deletedRows).isEqualTo(2);

    // Verify queue is actually empty
    assertThat(queue.peekEmpty()).isTrue();
  }

  @Test
  void addToQueueHandlesDuplicateRequestIds() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    RequestId requestId = new RequestId(UUID.randomUUID());

    HttpRequest httpRequest1 = new HttpRequest(POST, "/session");
    httpRequest1.setContent(Contents.utf8String("{\"capabilities\":{\"browserName\":\"chrome\"}}"));
    SessionRequest request1 = new SessionRequest(requestId, httpRequest1, Instant.now());

    HttpRequest httpRequest2 = new HttpRequest(POST, "/session");
    httpRequest2.setContent(
        Contents.utf8String("{\"capabilities\":{\"browserName\":\"firefox\"}}"));
    SessionRequest request2 = new SessionRequest(requestId, httpRequest2, Instant.now());

    // First add should succeed
    HttpResponse response1 = queue.addToQueue(request1);
    assertThat(response1.getStatus()).isEqualTo(200);

    // Second add with same ID should fail due to primary key constraint
    HttpResponse response2 = queue.addToQueue(request2);
    assertThat(response2.getStatus()).isEqualTo(500);
  }

  @Test
  void getQueueContentsHandlesLargeQueue() {
    JdbcBackedSessionQueue queue = getSessionQueue();
    queue.clearQueue();

    // Add multiple requests to test ordering
    int numRequests = 5;
    RequestId[] requestIds = new RequestId[numRequests];

    for (int i = 0; i < numRequests; i++) {
      requestIds[i] = new RequestId(UUID.randomUUID());
      HttpRequest httpRequest = new HttpRequest(POST, "/session");
      httpRequest.setContent(
          Contents.utf8String("{\"capabilities\":{\"browserName\":\"browser" + i + "\"}}"));
      SessionRequest request =
          new SessionRequest(requestIds[i], httpRequest, Instant.now().plusSeconds(i));
      queue.addToQueue(request);
    }

    var contents = queue.getQueueContents();
    assertThat(contents).hasSize(numRequests);

    // Verify ordering (oldest first)
    for (int i = 0; i < numRequests; i++) {
      assertThat(contents.get(i).getRequestId()).isEqualTo(requestIds[i]);
    }
  }

  @Test
  void closeConnectionDoesNotThrowException() {
    JdbcBackedSessionQueue queue = getSessionQueue();

    // close() method should not throw any exception
    assertThatCode(() -> queue.close()).doesNotThrowAnyException();
  }

  @Test
  void createWithValidConfigReturnsNewSessionQueue() {
    // Create a config with JDBC settings
    Map<String, Object> configMap =
        Map.of(
            "sessionqueue",
                Map.of(
                    "jdbc-url", "jdbc:hsqldb:mem:testqueue",
                    "jdbc-user", "SA",
                    "jdbc-password", ""),
            "logging", Map.of("tracing", false),
            "server", Map.of("registration-secret", "test-secret"));

    Config config = new MapConfig(configMap);

    // Test that create method returns a NewSessionQueue instance
    NewSessionQueue queue = JdbcBackedSessionQueue.create(config);

    assertThat(queue).isNotNull();
    assertThat(queue).isInstanceOf(JdbcBackedSessionQueue.class);
    assertThat(queue.isReady()).isTrue();

    // Test basic functionality
    assertThat(queue.peekEmpty()).isTrue();

    // Clean up
    if (queue instanceof JdbcBackedSessionQueue) {
      ((JdbcBackedSessionQueue) queue).close();
    }
  }

  @Test
  void createWithInvalidJdbcUrlThrowsConfigException() {
    // Create a config with invalid JDBC URL
    Map<String, Object> configMap =
        Map.of(
            "sessionqueue",
                Map.of(
                    "jdbc-url", "invalid:jdbc:url",
                    "jdbc-user", "SA",
                    "jdbc-password", ""),
            "logging", Map.of("tracing", false),
            "server", Map.of("registration-secret", "test-secret"));

    Config config = new MapConfig(configMap);

    // Test that create method throws ConfigException for invalid JDBC URL
    assertThatThrownBy(() -> JdbcBackedSessionQueue.create(config))
        .isInstanceOf(ConfigException.class)
        .hasCauseInstanceOf(SQLException.class);
  }

  @Test
  void createWithMissingConfigThrowsException() {
    // Create a config missing required JDBC settings
    Map<String, Object> configMap =
        Map.of(
            "logging", Map.of("tracing", false),
            "server", Map.of("registration-secret", "test-secret"));

    Config config = new MapConfig(configMap);

    // Test that create method throws JdbcException for missing config
    assertThatThrownBy(() -> JdbcBackedSessionQueue.create(config))
        .isInstanceOf(JdbcException.class)
        .hasMessageContaining("Missing JDBC Url value");
  }

  private JdbcBackedSessionQueue getSessionQueue() {
    return new JdbcBackedSessionQueue(tracer, secret, connection);
  }
}
