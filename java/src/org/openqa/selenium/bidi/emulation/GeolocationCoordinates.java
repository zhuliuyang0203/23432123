package org.openqa.selenium.bidi.emulation;

import java.util.HashMap;
import java.util.Map;

public class GeolocationCoordinates {
  private final double latitude;
  private final double longitude;
  private final double accuracy;
  private final Double altitude;
  private final Double altitudeAccuracy;
  private final Double heading;
  private final Double speed;

  // Constructor with default accuracy = 1.0
  public GeolocationCoordinates(
      double latitude,
      double longitude,
      Double altitude,
      Double altitudeAccuracy,
      Double heading,
      Double speed) {
    this(latitude, longitude, 1.0, altitude, altitudeAccuracy, heading, speed);
  }

  public GeolocationCoordinates(
      double latitude,
      double longitude,
      double accuracy,
      Double altitude,
      Double altitudeAccuracy,
      Double heading,
      Double speed) {

    if (latitude < -90.0 || latitude > 90.0) {
      throw new IllegalArgumentException("Latitude must be between -90.0 and 90.0");
    }
    if (longitude < -180.0 || longitude > 180.0) {
      throw new IllegalArgumentException("Longitude must be between -180.0 and 180.0");
    }
    if (accuracy < 0.0) {
      throw new IllegalArgumentException("Accuracy must be >= 0.0");
    }
    if (altitudeAccuracy != null && altitude == null) {
      throw new IllegalArgumentException("altitudeAccuracy cannot be set without altitude");
    }
    if (altitudeAccuracy != null && altitudeAccuracy < 0.0) {
      throw new IllegalArgumentException("Altitude accuracy must be >= 0.0");
    }
    if (heading != null && (heading < 0.0 || heading >= 360.0)) {
      throw new IllegalArgumentException("Heading must be between 0.0 and 360.0");
    }
    if (speed != null && speed < 0.0) {
      throw new IllegalArgumentException("Speed must be >= 0.0");
    }

    this.latitude = latitude;
    this.longitude = longitude;
    this.accuracy = accuracy;
    this.altitude = altitude;
    this.altitudeAccuracy = altitudeAccuracy;
    this.heading = heading;
    this.speed = speed;
  }

  public double getLatitude() {
    return latitude;
  }

  public double getLongitude() {
    return longitude;
  }

  public double getAccuracy() {
    return accuracy;
  }

  public Double getAltitude() {
    return altitude;
  }

  public Double getAltitudeAccuracy() {
    return altitudeAccuracy;
  }

  public Double getHeading() {
    return heading;
  }

  public Double getSpeed() {
    return speed;
  }

  public Map<String, Object> toMap() {
    Map<String, Object> map = new HashMap<>();
    map.put("latitude", getLatitude());
    map.put("longitude", getLongitude());
    map.put("accuracy", getAccuracy());
    if (getAltitude() != null) {
      map.put("altitude", getAltitude());
    }
    if (getAltitudeAccuracy() != null) {
      map.put("altitudeAccuracy", getAltitudeAccuracy());
    }
    if (getHeading() != null) {
      map.put("heading", getHeading());
    }
    if (getSpeed() != null) {
      map.put("speed", getSpeed());
    }
    return Map.copyOf(map);
  }
}
