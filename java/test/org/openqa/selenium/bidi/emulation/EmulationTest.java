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

import static java.lang.Math.abs;
import static org.openqa.selenium.testing.drivers.Browser.FIREFOX;

import java.util.List;
import java.util.Map;
import org.junit.jupiter.api.Test;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WindowType;
import org.openqa.selenium.bidi.browsingcontext.BrowsingContext;
import org.openqa.selenium.bidi.browsingcontext.CreateContextParameters;
import org.openqa.selenium.bidi.browsingcontext.ReadinessState;
import org.openqa.selenium.bidi.module.Browser;
import org.openqa.selenium.bidi.module.Permission;
import org.openqa.selenium.bidi.permissions.PermissionState;
import org.openqa.selenium.testing.Ignore;
import org.openqa.selenium.testing.JupiterTestBase;
import org.openqa.selenium.testing.NeedsFreshDriver;

class EmulationTest extends JupiterTestBase {
  private static final String GET_GEOLOCATION_PERMISSION =
      "() => {return navigator.permissions.query({ name: 'geolocation' })"
          + ".then(val => val.state, err => err.message)}";

  Object getBrowserGeolocation(WebDriver driver, String userContext, String origin) {
    JavascriptExecutor executor = (JavascriptExecutor) driver;

    Permission permission = new Permission(driver);
    permission.setPermission(
        Map.of("name", "geolocation"), PermissionState.GRANTED, origin, userContext);

    return executor.executeAsyncScript(
        "const callback = arguments[arguments.length - 1];\n"
            + "        navigator.geolocation.getCurrentPosition(\n"
            + "            position => {\n"
            + "                const coords = position.coords;\n"
            + "                callback({\n"
            + "                    latitude: coords.latitude,\n"
            + "                    longitude: coords.longitude,\n"
            + "                    accuracy: coords.accuracy,\n"
            + "                    altitude: coords.altitude,\n"
            + "                    altitudeAccuracy: coords.altitudeAccuracy,\n"
            + "                    heading: coords.heading,\n"
            + "                    speed: coords.speed,\n"
            + "                    timestamp: position.timestamp\n"
            + "                });\n"
            + "            },\n"
            + "            error => {\n"
            + "                callback({ error: error.message });\n"
            + "            }\n"
            + "        );");
  }

  @Test
  @NeedsFreshDriver
  void getGeolocationOverrideWithCoordinatesInContext() {
    BrowsingContext context = new BrowsingContext(driver, driver.getWindowHandle());
    String contextId = context.getId();

    String url = appServer.whereIs("blank.html");
    context.navigate(url, ReadinessState.COMPLETE);
    driver.switchTo().window(context.getId());

    String origin =
        (String) ((JavascriptExecutor) driver).executeScript("return window.location.origin;");

    Emulation emul = new Emulation(driver);
    GeolocationCoordinates coords =
        new GeolocationCoordinates(37.7749, -122.4194, 10.0, null, null, null);
    emul.setGeolocationOverride(
        new SetGeolocationOverrideParameters(coords, null, List.of(contextId), null));

    Object result = getBrowserGeolocation(driver, null, origin);
    Map<String, Object> r = ((Map<String, Object>) result);

    System.out.println(r);

    assert !r.containsKey("error");

    double latitude = ((Number) r.get("latitude")).doubleValue();
    double longitude = ((Number) r.get("longitude")).doubleValue();
    double accuracy = ((Number) r.get("accuracy")).doubleValue();

    assert abs(latitude - coords.getLatitude()) < 0.0001;
    assert abs(longitude - coords.getLongitude()) < 0.0001;
    assert abs(accuracy - coords.getAccuracy()) < 0.0001;
  }

  @Test
  void canSetGeolocationOverrideWithMultipleUserContexts() {
    Browser browser = new Browser(driver);
    String userContext1 = browser.createUserContext();
    String userContext2 = browser.createUserContext();

    BrowsingContext context1 =
        new BrowsingContext(
            driver, new CreateContextParameters(WindowType.TAB).userContext(userContext1));
    BrowsingContext context2 =
        new BrowsingContext(
            driver, new CreateContextParameters(WindowType.TAB).userContext(userContext2));

    GeolocationCoordinates coords =
        new GeolocationCoordinates(45.5, -122.4194, 10.0, null, null, null, null);

    Emulation emulation = new Emulation(driver);
    emulation.setGeolocationOverride(
        new SetGeolocationOverrideParameters(
            coords, null, null, List.of(userContext1, userContext2)));

    // Test first user context
    driver.switchTo().window(context1.getId());
    context1.navigate(appServer.whereIs("blank.html"), ReadinessState.COMPLETE);
    String origin1 =
        (String) ((JavascriptExecutor) driver).executeScript("return window.location.origin;");

    Map<String, Object> r =
        (Map<String, Object>) getBrowserGeolocation(driver, userContext1, origin1);

    assert !r.containsKey("error");

    double latitude1 = ((Number) r.get("latitude")).doubleValue();
    double longitude1 = ((Number) r.get("longitude")).doubleValue();
    double accuracy1 = ((Number) r.get("accuracy")).doubleValue();

    assert abs(latitude1 - coords.getLatitude()) < 0.0001;
    assert abs(longitude1 - coords.getLongitude()) < 0.0001;
    assert abs(accuracy1 - coords.getAccuracy()) < 0.0001;

    // Test second user context
    driver.switchTo().window(context2.getId());
    context2.navigate(appServer.whereIs("blank.html"), ReadinessState.COMPLETE);
    String origin2 =
        (String) ((JavascriptExecutor) driver).executeScript("return window.location.origin;");
    Map<String, Object> r2 =
        (Map<String, Object>) getBrowserGeolocation(driver, userContext2, origin2);

    assert !r2.containsKey("error");

    double latitude2 = ((Number) r2.get("latitude")).doubleValue();
    double longitude2 = ((Number) r2.get("longitude")).doubleValue();
    double accuracy2 = ((Number) r2.get("accuracy")).doubleValue();

    assert abs(latitude2 - coords.getLatitude()) < 0.0001;
    assert abs(longitude2 - coords.getLongitude()) < 0.0001;
    assert abs(accuracy2 - coords.getAccuracy()) < 0.0001;

    context1.close();
    context2.close();
    browser.removeUserContext(userContext1);
    browser.removeUserContext(userContext2);
  }

  @Test
  @Ignore(FIREFOX)
  void canSetGeolocationOverrideWithError() {
    BrowsingContext context = new BrowsingContext(driver, WindowType.TAB);
    String contextId = context.getId();

    String url = appServer.whereIs("blank.html");
    context.navigate(url, ReadinessState.COMPLETE);

    // Switch to the new context
    driver.switchTo().window(contextId);

    String origin =
        (String) ((JavascriptExecutor) driver).executeScript("return window.location.origin;");

    GeolocationPositionError error = new GeolocationPositionError();

    Emulation emul = new Emulation(driver);
    emul.setGeolocationOverride(
        new SetGeolocationOverrideParameters(null, error, List.of(contextId), null));

    Object result = getBrowserGeolocation(driver, null, origin);
    Map<String, Object> r = ((Map<String, Object>) result);

    assert r.containsKey("error");

    context.close();
  }
}
