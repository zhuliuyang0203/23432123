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

package org.openqa.selenium.grid.sessionmap.local;

import static org.openqa.selenium.remote.RemoteTags.SESSION_ID;
import static org.openqa.selenium.remote.RemoteTags.SESSION_ID_EVENT;

import java.net.URI;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.ConcurrentMap;
import java.util.concurrent.locks.ReadWriteLock;
import java.util.concurrent.locks.ReentrantReadWriteLock;
import java.util.logging.Logger;
import org.openqa.selenium.NoSuchSessionException;
import org.openqa.selenium.events.Event;
import org.openqa.selenium.events.EventBus;
import org.openqa.selenium.grid.config.Config;
import org.openqa.selenium.grid.data.NodeRemovedEvent;
import org.openqa.selenium.grid.data.NodeRestartedEvent;
import org.openqa.selenium.grid.data.Session;
import org.openqa.selenium.grid.data.SessionClosedEvent;
import org.openqa.selenium.grid.log.LoggingOptions;
import org.openqa.selenium.grid.server.EventBusOptions;
import org.openqa.selenium.grid.sessionmap.SessionMap;
import org.openqa.selenium.internal.Require;
import org.openqa.selenium.remote.SessionId;
import org.openqa.selenium.remote.tracing.AttributeKey;
import org.openqa.selenium.remote.tracing.AttributeMap;
import org.openqa.selenium.remote.tracing.Span;
import org.openqa.selenium.remote.tracing.Tracer;

public class LocalSessionMap extends SessionMap {

  private static final Logger LOG = Logger.getLogger(LocalSessionMap.class.getName());

  private final EventBus bus;
  private final IndexedSessionMap knownSessions = new IndexedSessionMap();
  private final ReadWriteLock sessionMapLock = new ReentrantReadWriteLock();

  public LocalSessionMap(Tracer tracer, EventBus bus) {
    super(tracer);

    this.bus = Require.nonNull("Event bus", bus);

    bus.addListener(SessionClosedEvent.listener(this::remove));

    bus.addListener(
        NodeRemovedEvent.listener(
            nodeStatus -> {
              batchRemoveByUri(nodeStatus.getExternalUri(), NodeRemovedEvent.class);
            }));

    bus.addListener(
        NodeRestartedEvent.listener(
            previousNodeStatus -> {
              batchRemoveByUri(previousNodeStatus.getExternalUri(), NodeRestartedEvent.class);
            }));
  }

  public static SessionMap create(Config config) {
    Tracer tracer = new LoggingOptions(config).getTracer();
    EventBus bus = new EventBusOptions(config).getEventBus();

    return new LocalSessionMap(tracer, bus);
  }

  @Override
  public boolean isReady() {
    return bus.isReady();
  }

  @Override
  public boolean add(Session session) {
    Require.nonNull("Session", session);
    SessionId id = session.getId();
    sessionMapLock.writeLock().lock();
    try {
      knownSessions.put(id, session);
    } finally {
      sessionMapLock.writeLock().unlock();
    }

    try (Span span = tracer.getCurrentContext().createSpan("local_sessionmap.add")) {
      AttributeMap attributeMap = tracer.createAttributeMap();
      attributeMap.put(AttributeKey.LOGGER_CLASS.getKey(), getClass().getName());
      SESSION_ID.accept(span, id);
      SESSION_ID_EVENT.accept(attributeMap, id);
      span.addEvent("Added session into local Session Map", attributeMap);
    }

    return true;
  }

  @Override
  public Session get(SessionId id) {
    Require.nonNull("Session ID", id);

    Session session;
    sessionMapLock.readLock().lock();
    try {
      session = knownSessions.get(id);
      if (session == null) {
        throw new NoSuchSessionException("Unable to find session with ID: " + id);
      }
    } finally {
      sessionMapLock.readLock().unlock();
    }
    return session;
  }

  @Override
  public void remove(SessionId id) {
    Require.nonNull("Session ID", id);
    Session removedSession;
    sessionMapLock.writeLock().lock();
    try {
      removedSession = knownSessions.remove(id);
    } finally {
      sessionMapLock.writeLock().unlock();
    }

    try (Span span = tracer.getCurrentContext().createSpan("local_sessionmap.remove")) {
      AttributeMap attributeMap = tracer.createAttributeMap();
      attributeMap.put(AttributeKey.LOGGER_CLASS.getKey(), getClass().getName());
      SESSION_ID.accept(span, id);
      SESSION_ID_EVENT.accept(attributeMap, id);
      String sessionDeletedMessage =
          String.format(
              "Deleted session from local Session Map, Id: %s, Node: %s",
              id,
              removedSession != null ? String.valueOf(removedSession.getUri()) : "unidentified");
      span.addEvent(sessionDeletedMessage, attributeMap);
      LOG.info(sessionDeletedMessage);
    }
  }

  /** Batch remove sessions by URI with proper locking to prevent race conditions */
  private void batchRemoveByUri(URI externalUri, Class<? extends Event> eventClass) {
    sessionMapLock.writeLock().lock();
    Set<SessionId> sessionsToRemove;
    try {
      sessionsToRemove = knownSessions.getSessionsByUri(externalUri);
      if (!sessionsToRemove.isEmpty()) {
        knownSessions.batchRemove(sessionsToRemove);
      }
    } finally {
      sessionMapLock.writeLock().unlock();
    }

    try (Span span = tracer.getCurrentContext().createSpan("local_sessionmap.batch_remove")) {
      AttributeMap attributeMap = tracer.createAttributeMap();
      attributeMap.put(AttributeKey.LOGGER_CLASS.getKey(), getClass().getName());
      attributeMap.put("event.class", eventClass.getName());
      attributeMap.put("node.uri", externalUri.toString());
      attributeMap.put("sessions.count", sessionsToRemove.size());

      LOG.info(
          String.format(
              "Event %s triggered batch remove from local Session Map for Node %s",
              eventClass.getName(), externalUri));
      String eventMessage = "";
      if (!sessionsToRemove.isEmpty()) {
        eventMessage =
            String.format(
                "Batch removed %d sessions belonging to Node %s including: %s",
                sessionsToRemove.size(), externalUri, sessionsToRemove);
      } else {
        eventMessage =
            String.format(
                "No sessions found to remove from local Session Map for Node %s", externalUri);
      }
      span.addEvent(eventMessage, attributeMap);
      LOG.info(eventMessage);
    }
  }

  /** Custom ConcurrentMap implementation that automatically maintains a URI-to-SessionId index */
  private static class IndexedSessionMap {
    private final ConcurrentMap<SessionId, Session> sessions = new ConcurrentHashMap<>();
    private final ConcurrentMap<URI, Set<SessionId>> sessionsByUri = new ConcurrentHashMap<>();

    public Session get(SessionId id) {
      return sessions.get(id);
    }

    public Session put(SessionId id, Session session) {
      Session previous = sessions.put(id, session);

      if (previous != null && previous.getUri() != null) {
        sessionsByUri.computeIfPresent(
            previous.getUri(),
            (k, sessionIds) -> {
              sessionIds.remove(id);
              return sessionIds.isEmpty() ? null : sessionIds;
            });
      }

      URI sessionUri = session.getUri();
      if (sessionUri != null) {
        sessionsByUri.computeIfAbsent(sessionUri, k -> ConcurrentHashMap.newKeySet()).add(id);
      }

      return previous;
    }

    public Session remove(SessionId id) {
      Session removed = sessions.remove(id);

      if (removed != null && removed.getUri() != null) {
        sessionsByUri.computeIfPresent(
            removed.getUri(),
            (k, sessionIds) -> {
              sessionIds.remove(id);
              return sessionIds.isEmpty() ? null : sessionIds;
            });
      }

      return removed;
    }

    public void batchRemove(Set<SessionId> sessionIds) {
      Map<URI, Set<SessionId>> uriToSessionIds = new HashMap<>();

      for (SessionId id : sessionIds) {
        Session session = sessions.get(id);
        if (session != null && session.getUri() != null) {
          uriToSessionIds.computeIfAbsent(session.getUri(), k -> new HashSet<>()).add(id);
        }
      }

      for (SessionId id : sessionIds) {
        sessions.remove(id);
      }

      for (Map.Entry<URI, Set<SessionId>> entry : uriToSessionIds.entrySet()) {
        URI uri = entry.getKey();
        Set<SessionId> idsToRemove = entry.getValue();

        sessionsByUri.computeIfPresent(
            uri,
            (k, existingIds) -> {
              existingIds.removeAll(idsToRemove);
              return existingIds.isEmpty() ? null : existingIds;
            });
      }
    }

    public Set<Map.Entry<SessionId, Session>> entrySet() {
      return sessions.entrySet();
    }

    public Collection<Session> values() {
      return sessions.values();
    }

    public Set<SessionId> getSessionsByUri(URI uri) {
      return sessionsByUri.getOrDefault(uri, Set.of());
    }

    public int size() {
      return sessions.size();
    }

    public boolean isEmpty() {
      return sessions.isEmpty();
    }

    public void clear() {
      sessions.clear();
      sessionsByUri.clear();
    }
  }
}
