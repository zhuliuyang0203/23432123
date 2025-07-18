package org.openqa.selenium.bidi.emulation;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class SetGeolocationOverrideParameters {
  private final GeolocationCoordinates coordinates;
  private final GeolocationPositionError error;
  private final List<String> contexts;
  private final List<String> userContexts;

  SetGeolocationOverrideParameters(
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
