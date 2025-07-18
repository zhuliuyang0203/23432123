package org.openqa.selenium.bidi.emulation;

import java.util.Map;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.bidi.BiDi;
import org.openqa.selenium.bidi.Command;
import org.openqa.selenium.bidi.HasBiDi;
import org.openqa.selenium.internal.Require;

public class Emulation {
  private final BiDi bidi;

  public Emulation(WebDriver driver) {
    Require.nonNull("WebDriver", driver);

    if (!(driver instanceof HasBiDi)) {
      throw new IllegalArgumentException("WebDriver must implement BiDi interface");
    }

    this.bidi = ((HasBiDi) driver).getBiDi();
  }

  public Map<String, Object> setGeolocationOverride(SetGeolocationOverrideParameters parameters) {
    Require.nonNull("SetGeolocationOverride parameters", parameters);

    return bidi.send(
        new Command<>("emulation.setGeolocationOverride", parameters.toMap(), Map.class));
  }
}
