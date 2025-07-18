package org.openqa.selenium.bidi.emulation;

import java.util.Map;

public class GeolocationPositionError {
  String type = "positionUnavailable";

  public Map<String, Object> toMap() {
    return Map.of("type", type);
  }
}
