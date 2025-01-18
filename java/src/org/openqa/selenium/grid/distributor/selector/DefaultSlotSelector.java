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
package org.openqa.selenium.grid.distributor.selector;

import static com.google.common.collect.ImmutableSet.toImmutableSet;

import com.google.common.annotations.VisibleForTesting;
import java.util.Comparator;
import java.util.Locale;
import java.util.Set;
import org.openqa.selenium.Capabilities;
import org.openqa.selenium.grid.config.Config;
import org.openqa.selenium.grid.data.NodeStatus;
import org.openqa.selenium.grid.data.SemanticVersionComparator;
import org.openqa.selenium.grid.data.Slot;
import org.openqa.selenium.grid.data.SlotId;
import org.openqa.selenium.grid.data.SlotMatcher;
import java.util.logging.Logger;
import java.util.logging.Level;

public class DefaultSlotSelector implements SlotSelector {

    private static final Logger LOGGER = Logger.getLogger(DefaultSlotSelector.class.getName());

    public static SlotSelector create(Config config) {
        return new DefaultSlotSelector();
    }

    @Override
    public Set<SlotId> selectSlot(
            Capabilities capabilities, Set<NodeStatus> nodes, SlotMatcher slotMatcher) {
        // First, filter the Nodes that support the required capabilities. Then, the filtered Nodes
        // get ordered in ascendant order by the number of browsers they support.
        // With this, Nodes with diverse configurations (supporting many browsers, e.g. Chrome,
        // Firefox, Safari) are placed at the bottom, so they have more availability when a session
        // requests a browser supported only by a few Nodes (e.g. Safari only supported on macOS
        // Nodes).
        // After that, Nodes are ordered by their load, last session creation, and their id.
        String requestedRpaType = (String) capabilities.getCapability("fw:rpa_type");
        Set<String> names = capabilities.getCapabilityNames();
        for (String name : names) {
            LOGGER.log(Level.WARNING, "Capability {0}", name);
        }
        LOGGER.log(Level.INFO, "Requested RPA Type: {0}", requestedRpaType);

        return nodes.stream()
                .filter(node -> {
                    boolean hasCapacity = node.hasCapacity(capabilities, slotMatcher);
                    if (!hasCapacity) {
                        LOGGER.log(Level.WARNING, "Node {0} does not have capacity for capabilities: {1}", new Object[]{node.getNodeId(), capabilities});
                    }
                    return hasCapacity;
                })
                .sorted(
                        Comparator.comparingLong(this::getNumberOfSupportedBrowsers)
                                // Now sort by node which has the lowest load (natural ordering)
                                .thenComparingDouble(NodeStatus::getLoad)
                                // Then last session created (oldest first), so natural ordering again
                                .thenComparingLong(NodeStatus::getLastSessionCreated)
                                // Then sort by stereotype browserVersion (descending order). SemVer comparison with
                                // considering empty value at first.
                                .thenComparing(
                                        Comparator.comparing(
                                                NodeStatus::getBrowserVersion, new SemanticVersionComparator().reversed()))
                                // And use the node id as a tie-breaker.
                                .thenComparing(NodeStatus::getNodeId))
                .flatMap(node -> {
                    LOGGER.log(Level.WARNING, "Evaluating Node: {0} with load: {1}", new Object[]{node.getNodeId(), node.getLoad()});
                    return node.getSlots().stream()
                            .filter(slot -> {
                                boolean isAvailable = slot.getSession() == null;
                                if (!isAvailable) {
                                    LOGGER.log(Level.WARNING, "Slot {0} on Node {1} is already occupied.", new Object[]{slot.getId(), node.getNodeId()});
                                }
                                return isAvailable;
                            })
                            .filter(slot -> {
                                boolean supportsCapabilities = slot.isSupporting(capabilities, slotMatcher);
                                if (!supportsCapabilities) {
                                    LOGGER.log(Level.WARNING, "Slot {0} on Node {1} does not support capabilities: {2}", new Object[]{slot.getId(), node.getNodeId(), capabilities});
                                }
                                return supportsCapabilities;
                            })
                            .filter(slot -> {
                                boolean noRpaConflict = requestedRpaType == null || doesNotConflictWithExistingRpaType(slot, node, requestedRpaType);
                                if (!noRpaConflict) {
                                    LOGGER.log(Level.WARNING, "Slot {0} on Node {1} conflicts with existing RPA Type: {2}", new Object[]{slot.getId(), node.getNodeId(), requestedRpaType});
                                }
                                return noRpaConflict;
                            })
                            .map(Slot::getId);
                })
                .collect(toImmutableSet());
    }

    private boolean doesNotConflictWithExistingRpaType(Slot slot, NodeStatus node, String requestedRpaType) {
        LOGGER.log(Level.WARNING, "Checking RPA conflict for Slot {0} on Node {1}", new Object[]{slot.getId(), node.getNodeId()});
        return node.getSlots().stream()
                .noneMatch(existingSlot -> {
                    if (existingSlot.getSession() != null) {
                        Capabilities sessionCapabilities = existingSlot.getSession().getCapabilities();
                        String existingRpaType = (String) sessionCapabilities.getCapability("fw:rpa_type");
                        boolean conflict = requestedRpaType.equals(existingRpaType);
                        if (conflict) {
                            LOGGER.log(Level.WARNING, "Conflict detected: Slot {0} on Node {1} has RPA Type {2}", new Object[]{existingSlot.getId(), node.getNodeId(), existingRpaType});
                        }
                        return conflict;
                    }
                    return false;
                });
    }

    @VisibleForTesting
    long getNumberOfSupportedBrowsers(NodeStatus nodeStatus) {
        return nodeStatus.getSlots().stream()
                .map(slot -> slot.getStereotype().getBrowserName().toLowerCase(Locale.ENGLISH))
                .distinct()
                .count();
    }
}
