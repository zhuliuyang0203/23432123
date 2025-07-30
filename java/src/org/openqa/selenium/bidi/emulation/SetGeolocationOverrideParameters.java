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

package org.openqa.selenium.bidi.emulation;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class SetGeolocationOverrideParameters {
  private final GeolocationCoordinates coordinates;
  private final GeolocationPositionError error;
  private final List<String> contexts;
  private final List<String> userContexts;

  public SetGeolocationOverrideParameters(
      GeolocationCoordinates coordinates,
      GeolocationPositionError error,
      List<String> contexts,
      List<String> userContexts) {

    this.coordinates = coordinates;
    this.error = error;
    this.contexts = contexts;
    this.userContexts = userContexts;

    if (this.coordinates != null && this.error != null) {
      throw new IllegalArgumentException("Cannot specify both coordinates and error");
    }
    if (this.contexts != null && this.userContexts != null) {
      throw new IllegalArgumentException("Cannot specify both contexts and userContexts");
    }

    if (this.contexts == null && this.userContexts == null) {
      throw new IllegalArgumentException("Must specify either contexts or userContexts");
    }
  }

  public Map<String, Object> toMap() {
    Map<String, Object> param = new HashMap<>();

    if (this.coordinates != null) {
      param.put("coordinates", this.coordinates.toMap());
    }

    if (this.error != null) {
      param.put("error", this.error.toMap());
    }

    if (this.contexts != null) {
      param.put("contexts", this.contexts);
    } else {
      param.put("userContexts", this.userContexts);
    }

    return Map.copyOf(param);
  }
}
