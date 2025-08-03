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

import static org.openqa.selenium.remote.tracing.Tags.EXCEPTION;

import java.io.Closeable;
import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.sql.Timestamp;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.Optional;
import java.util.UUID;
import java.util.logging.Level;
import java.util.logging.Logger;
import org.openqa.selenium.Capabilities;
import org.openqa.selenium.SessionNotCreatedException;
import org.openqa.selenium.grid.config.Config;
import org.openqa.selenium.grid.config.ConfigException;
import org.openqa.selenium.grid.data.CreateSessionResponse;
import org.openqa.selenium.grid.data.RequestId;
import org.openqa.selenium.grid.data.SessionRequest;
import org.openqa.selenium.grid.data.SessionRequestCapability;
import org.openqa.selenium.grid.log.LoggingOptions;
import org.openqa.selenium.grid.security.Secret;
import org.openqa.selenium.grid.security.SecretOptions;
import org.openqa.selenium.grid.sessionqueue.NewSessionQueue;
import org.openqa.selenium.internal.Either;
import org.openqa.selenium.internal.Require;
import org.openqa.selenium.json.Json;
import org.openqa.selenium.remote.http.HttpResponse;
import org.openqa.selenium.remote.tracing.AttributeKey;
import org.openqa.selenium.remote.tracing.AttributeMap;
import org.openqa.selenium.remote.tracing.Span;
import org.openqa.selenium.remote.tracing.Status;
import org.openqa.selenium.remote.tracing.Tracer;

public class JdbcBackedSessionQueue extends NewSessionQueue implements Closeable {
  private static final Logger LOG = Logger.getLogger(JdbcBackedSessionQueue.class.getName());
  private static final String TABLE_NAME = "session_queue";
  private static final Json JSON = new Json();
  private static final String DATABASE_STATEMENT = AttributeKey.DATABASE_STATEMENT.getKey();
  private static final String DATABASE_OPERATION = AttributeKey.DATABASE_OPERATION.getKey();
  private static final String DATABASE_USER = AttributeKey.DATABASE_USER.getKey();
  private static final String DATABASE_CONNECTION_STRING =
      AttributeKey.DATABASE_CONNECTION_STRING.getKey();
  private static String jdbcUser;
  private static String jdbcUrl;
  private final Connection connection;

  public JdbcBackedSessionQueue(
      Tracer tracer, Secret registrationSecret, Connection jdbcConnection) {
    super(tracer, registrationSecret);
    this.connection = Require.nonNull("JDBC Connection Object", jdbcConnection);
    ensureTableExists();
  }

  public static NewSessionQueue create(Config config) {
    Tracer tracer = new LoggingOptions(config).getTracer();
    Secret secret = new SecretOptions(config).getRegistrationSecret();
    Connection connection;
    try {
      JdbcSessionQueueOptions sessionQueueOptions = new JdbcSessionQueueOptions(config);
      jdbcUser = sessionQueueOptions.getJdbcUser();
      jdbcUrl = sessionQueueOptions.getJdbcUrl();
      connection = sessionQueueOptions.getJdbcConnection();
    } catch (SQLException e) {
      throw new ConfigException(e);
    }
    return new JdbcBackedSessionQueue(tracer, secret, connection);
  }

  @Override
  public boolean isReady() {
    try {
      return !connection.isClosed();
    } catch (SQLException e) {
      return false;
    }
  }

  @Override
  public boolean peekEmpty() {
    try (Span span = tracer.getCurrentContext().createSpan("SELECT COUNT(*) FROM session_queue")) {
      AttributeMap attributeMap = tracer.createAttributeMap();
      setCommonSpanAttributes(span);
      setCommonEventAttributes(attributeMap);

      String sql = "SELECT COUNT(*) FROM " + TABLE_NAME;
      try (PreparedStatement stmt = connection.prepareStatement(sql)) {
        String statementStr = stmt.toString();
        span.setAttribute(DATABASE_STATEMENT, statementStr);
        span.setAttribute(DATABASE_OPERATION, "select");
        attributeMap.put(DATABASE_STATEMENT, statementStr);
        attributeMap.put(DATABASE_OPERATION, "select");

        ResultSet rs = stmt.executeQuery();
        if (rs.next()) {
          boolean isEmpty = rs.getInt(1) == 0;
          attributeMap.put("queue.empty", isEmpty);
          span.addEvent("Checked queue emptiness", attributeMap);
          return isEmpty;
        }
      } catch (SQLException e) {
        span.setAttribute("error", true);
        span.setStatus(Status.CANCELLED);
        EXCEPTION.accept(attributeMap, e);
        attributeMap.put(
            AttributeKey.EXCEPTION_MESSAGE.getKey(),
            "Unable to check if queue is empty: " + e.getMessage());
        span.addEvent(AttributeKey.EXCEPTION_EVENT.getKey(), attributeMap);
        LOG.log(Level.SEVERE, "Failed to check if queue is empty", e);
      }
    }
    return false;
  }

  @Override
  public HttpResponse addToQueue(SessionRequest request) {
    Require.nonNull("SessionRequest to add", request);

    try (Span span =
        tracer
            .getCurrentContext()
            .createSpan(
                "INSERT into session_queue (request_id, payload, enqueue_time) values (?, ?, ?)")) {
      AttributeMap attributeMap = tracer.createAttributeMap();
      setCommonSpanAttributes(span);
      setCommonEventAttributes(attributeMap);

      String sql =
          "INSERT INTO " + TABLE_NAME + " (request_id, payload, enqueue_time) VALUES (?, ?, ?)";
      try (PreparedStatement stmt = connection.prepareStatement(sql)) {
        stmt.setString(1, request.getRequestId().toString());
        stmt.setString(2, JSON.toJson(request));
        stmt.setTimestamp(3, Timestamp.from(request.getEnqueued()));

        String statementStr = stmt.toString();
        span.setAttribute(DATABASE_STATEMENT, statementStr);
        span.setAttribute(DATABASE_OPERATION, "insert");
        attributeMap.put(DATABASE_STATEMENT, statementStr);
        attributeMap.put(DATABASE_OPERATION, "insert");

        int rowCount = stmt.executeUpdate();
        attributeMap.put("rows.added", rowCount);
        span.addEvent("Inserted into the database", attributeMap);

        HttpResponse resp = new HttpResponse();
        resp.setStatus(rowCount > 0 ? 200 : 500);
        return resp;
      } catch (SQLException e) {
        span.setAttribute("error", true);
        span.setStatus(Status.CANCELLED);
        EXCEPTION.accept(attributeMap, e);
        attributeMap.put(
            AttributeKey.EXCEPTION_MESSAGE.getKey(),
            "Unable to add session request to the database: " + e.getMessage());
        span.addEvent(AttributeKey.EXCEPTION_EVENT.getKey(), attributeMap);

        HttpResponse resp = new HttpResponse();
        resp.setStatus(500);
        return resp;
      }
    }
  }

  @Override
  public boolean retryAddToQueue(SessionRequest request) {
    HttpResponse response = addToQueue(request);
    return response.getStatus() == 200;
  }

  @Override
  public Optional<SessionRequest> remove(RequestId requestId) {
    Require.nonNull("RequestId to remove", requestId);

    try (Span span =
        tracer
            .getCurrentContext()
            .createSpan("SELECT and DELETE session request from session_queue")) {
      AttributeMap attributeMap = tracer.createAttributeMap();
      setCommonSpanAttributes(span);
      setCommonEventAttributes(attributeMap);

      String select = "SELECT payload FROM " + TABLE_NAME + " WHERE request_id = ?";
      String delete = "DELETE FROM " + TABLE_NAME + " WHERE request_id = ?";

      try (PreparedStatement selectStmt = connection.prepareStatement(select)) {
        selectStmt.setString(1, requestId.toString());

        String statementStr = selectStmt.toString();
        span.setAttribute(DATABASE_STATEMENT, statementStr);
        span.setAttribute(DATABASE_OPERATION, "select");
        attributeMap.put(DATABASE_STATEMENT, statementStr);
        attributeMap.put(DATABASE_OPERATION, "select");

        ResultSet rs = selectStmt.executeQuery();
        if (rs.next()) {
          String payload = rs.getString("payload");

          try (PreparedStatement deleteStmt = connection.prepareStatement(delete)) {
            deleteStmt.setString(1, requestId.toString());

            String deleteStatementStr = deleteStmt.toString();
            span.setAttribute(DATABASE_STATEMENT, deleteStatementStr);
            span.setAttribute(DATABASE_OPERATION, "delete");
            attributeMap.put(DATABASE_STATEMENT, deleteStatementStr);
            attributeMap.put(DATABASE_OPERATION, "delete");

            int rowCount = deleteStmt.executeUpdate();
            attributeMap.put("rows.deleted", rowCount);
            span.addEvent("Removed session request from queue", attributeMap);

            return Optional.of(JSON.toType(payload, SessionRequest.class));
          }
        } else {
          attributeMap.put("request.found", false);
          span.addEvent("Session request not found in queue", attributeMap);
        }
      } catch (SQLException e) {
        span.setAttribute("error", true);
        span.setStatus(Status.CANCELLED);
        EXCEPTION.accept(attributeMap, e);
        attributeMap.put(
            AttributeKey.EXCEPTION_MESSAGE.getKey(),
            "Unable to remove session request from queue: " + e.getMessage());
        span.addEvent(AttributeKey.EXCEPTION_EVENT.getKey(), attributeMap);
        LOG.log(Level.SEVERE, "Failed to remove session request from queue", e);
      }
    }
    return Optional.empty();
  }

  @Override
  public List<SessionRequest> getNextAvailable(Map<Capabilities, Long> stereotypes) {
    try (Span span =
        tracer
            .getCurrentContext()
            .createSpan("SELECT next available session request from session_queue")) {
      AttributeMap attributeMap = tracer.createAttributeMap();
      setCommonSpanAttributes(span);
      setCommonEventAttributes(attributeMap);

      String sql = "SELECT payload FROM " + TABLE_NAME + " ORDER BY enqueue_time ASC LIMIT 1";
      try (PreparedStatement stmt = connection.prepareStatement(sql)) {
        String statementStr = stmt.toString();
        span.setAttribute(DATABASE_STATEMENT, statementStr);
        span.setAttribute(DATABASE_OPERATION, "select");
        attributeMap.put(DATABASE_STATEMENT, statementStr);
        attributeMap.put(DATABASE_OPERATION, "select");

        ResultSet rs = stmt.executeQuery();
        if (rs.next()) {
          String payload = rs.getString("payload");
          SessionRequest request = JSON.toType(payload, SessionRequest.class);
          attributeMap.put("requests.found", 1);
          span.addEvent("Retrieved next available session request", attributeMap);
          return List.of(request);
        } else {
          attributeMap.put("requests.found", 0);
          span.addEvent("No session requests available", attributeMap);
        }
      } catch (SQLException e) {
        span.setAttribute("error", true);
        span.setStatus(Status.CANCELLED);
        EXCEPTION.accept(attributeMap, e);
        attributeMap.put(
            AttributeKey.EXCEPTION_MESSAGE.getKey(),
            "Unable to get next available session request: " + e.getMessage());
        span.addEvent(AttributeKey.EXCEPTION_EVENT.getKey(), attributeMap);
        LOG.log(Level.SEVERE, "Failed to get next available session request", e);
      }
    }
    return List.of();
  }

  @Override
  public boolean complete(
      RequestId reqId, Either<SessionNotCreatedException, CreateSessionResponse> result) {
    return remove(reqId).isPresent();
  }

  @Override
  public int clearQueue() {
    try (Span span =
        tracer.getCurrentContext().createSpan("DELETE all session requests from session_queue")) {
      AttributeMap attributeMap = tracer.createAttributeMap();
      setCommonSpanAttributes(span);
      setCommonEventAttributes(attributeMap);

      String sql = "DELETE FROM " + TABLE_NAME;
      try (Statement stmt = connection.createStatement()) {
        String statementStr = sql;
        span.setAttribute(DATABASE_STATEMENT, statementStr);
        span.setAttribute(DATABASE_OPERATION, "delete");
        attributeMap.put(DATABASE_STATEMENT, statementStr);
        attributeMap.put(DATABASE_OPERATION, "delete");

        int rowCount = stmt.executeUpdate(sql);
        attributeMap.put("rows.deleted", rowCount);
        span.addEvent("Cleared all session requests from queue", attributeMap);
        return rowCount;
      } catch (SQLException e) {
        span.setAttribute("error", true);
        span.setStatus(Status.CANCELLED);
        EXCEPTION.accept(attributeMap, e);
        attributeMap.put(
            AttributeKey.EXCEPTION_MESSAGE.getKey(),
            "Unable to clear session queue: " + e.getMessage());
        span.addEvent(AttributeKey.EXCEPTION_EVENT.getKey(), attributeMap);
        LOG.log(Level.SEVERE, "Failed to clear session queue", e);
        return 0;
      }
    }
  }

  @Override
  public List<SessionRequestCapability> getQueueContents() {
    try (Span span =
        tracer.getCurrentContext().createSpan("SELECT all session requests from session_queue")) {
      AttributeMap attributeMap = tracer.createAttributeMap();
      setCommonSpanAttributes(span);
      setCommonEventAttributes(attributeMap);

      String sql = "SELECT request_id, payload FROM " + TABLE_NAME + " ORDER BY enqueue_time ASC";
      List<SessionRequestCapability> contents = new ArrayList<>();

      try (PreparedStatement stmt = connection.prepareStatement(sql)) {
        String statementStr = stmt.toString();
        span.setAttribute(DATABASE_STATEMENT, statementStr);
        span.setAttribute(DATABASE_OPERATION, "select");
        attributeMap.put(DATABASE_STATEMENT, statementStr);
        attributeMap.put(DATABASE_OPERATION, "select");

        ResultSet rs = stmt.executeQuery();
        while (rs.next()) {
          String requestIdStr = rs.getString("request_id");
          String payload = rs.getString("payload");

          try {
            RequestId requestId = new RequestId(UUID.fromString(requestIdStr));
            SessionRequest request = JSON.toType(payload, SessionRequest.class);

            SessionRequestCapability capability =
                new SessionRequestCapability(requestId, request.getDesiredCapabilities());
            contents.add(capability);
          } catch (Exception e) {
            LOG.log(
                Level.WARNING, "Failed to parse session request from queue: " + requestIdStr, e);
          }
        }

        attributeMap.put("queue.contents.size", contents.size());
        span.addEvent("Retrieved queue contents", attributeMap);
        return contents;
      } catch (SQLException e) {
        span.setAttribute("error", true);
        span.setStatus(Status.CANCELLED);
        EXCEPTION.accept(attributeMap, e);
        attributeMap.put(
            AttributeKey.EXCEPTION_MESSAGE.getKey(),
            "Unable to get queue contents: " + e.getMessage());
        span.addEvent(AttributeKey.EXCEPTION_EVENT.getKey(), attributeMap);
        LOG.log(Level.SEVERE, "Failed to get queue contents", e);
      }
    }
    return List.of();
  }

  @Override
  public void close() {
    try {
      if (!connection.isClosed()) {
        connection.close();
      }
    } catch (SQLException e) {
      LOG.log(Level.WARNING, "Failed to close JDBC connection for SessionQueue", e);
    }
  }

  private void ensureTableExists() {
    String sql =
        "CREATE TABLE IF NOT EXISTS "
            + TABLE_NAME
            + " ("
            + "request_id VARCHAR(64) PRIMARY KEY,"
            + "payload CLOB NOT NULL,"
            + "enqueue_time TIMESTAMP NOT NULL"
            + ")";
    try (Statement stmt = connection.createStatement()) {
      stmt.execute(sql);
    } catch (SQLException e) {
      LOG.log(Level.SEVERE, "Failed to create session_queue table", e);
    }
  }

  private void setCommonSpanAttributes(Span span) {
    span.setAttribute("span.kind", Span.Kind.CLIENT.toString());
    if (jdbcUser != null) {
      span.setAttribute(DATABASE_USER, jdbcUser);
    }
    if (jdbcUrl != null) {
      span.setAttribute(DATABASE_CONNECTION_STRING, jdbcUrl);
    }
  }

  private void setCommonEventAttributes(AttributeMap attributeMap) {
    if (jdbcUser != null) {
      attributeMap.put(DATABASE_USER, jdbcUser);
    }
    if (jdbcUrl != null) {
      attributeMap.put(DATABASE_CONNECTION_STRING, jdbcUrl);
    }
  }
}
