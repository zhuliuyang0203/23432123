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
import org.openqa.selenium.testing.NeedsSecureServer;

@NeedsSecureServer
class EmulationTest extends JupiterTestBase {
  private static final String GET_GEOLOCATION_PERMISSION =
      "() => {return navigator.permissions.query({ name: 'geolocation' })"
          + ".then(val => val.state, err => err.message)}";

  Object getBrowserGeolocation(WebDriver driver, String userContext, String origin) {
    JavascriptExecutor executor = (JavascriptExecutor) driver;

    System.out.println(
        "DEBUG: getBrowserGeolocation called with origin: "
            + origin
            + ", userContext: "
            + userContext);

    Permission permission = new Permission(driver);

    // Add delay to ensure page is fully loaded
    try {
      Thread.sleep(500);
    } catch (InterruptedException e) {
      Thread.currentThread().interrupt();
    }

    // Check if we're in a secure context
    Object isSecureContext = executor.executeScript("return window.isSecureContext;");
    System.out.println("DEBUG: isSecureContext: " + isSecureContext);

    // Check current URL
    String currentUrl = driver.getCurrentUrl();
    System.out.println("DEBUG: Current URL: " + currentUrl);

    // Try to set permission with retry logic for CI environments
    int maxRetries = 5;
    int retryCount = 0;
    boolean permissionSet = false;

    while (!permissionSet && retryCount < maxRetries) {
      try {
        System.out.println("DEBUG: Attempt " + (retryCount + 1) + " to set geolocation permission");
        permission.setPermission(
            Map.of("name", "geolocation"), PermissionState.GRANTED, origin, userContext);
        permissionSet = true;
        System.out.println("DEBUG: Permission set successfully");
      } catch (Exception e) {
        retryCount++;
        System.out.println(
            "DEBUG: Permission setting failed on attempt " + retryCount + ": " + e.getMessage());
        if (retryCount >= maxRetries) {
          // If permission setting fails after retries, continue anyway
          // as some CI environments may have different permission handling
          System.out.println(
              "Warning: Could not set geolocation permission after "
                  + maxRetries
                  + " attempts: "
                  + e.getMessage());
          break;
        }
        try {
          Thread.sleep(200); // Longer delay before retry
        } catch (InterruptedException ie) {
          Thread.currentThread().interrupt();
          break;
        }
      }
    }

    // Check permission state
    try {
      Object permissionState = executor.executeScript(GET_GEOLOCATION_PERMISSION);
      System.out.println("DEBUG: Geolocation permission state: " + permissionState);
    } catch (Exception e) {
      System.out.println("DEBUG: Could not check permission state: " + e.getMessage());
    }

    System.out.println("DEBUG: Executing geolocation script");
    return executor.executeAsyncScript(
        "const callback = arguments[arguments.length - 1];\n"
            + "        console.log('Starting geolocation request');\n"
            + "        navigator.geolocation.getCurrentPosition(\n"
            + "            position => {\n"
            + "                console.log('Geolocation success:', position);\n"
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
            + "                console.log('Geolocation error:', error);\n"
            + "                callback({ error: error.message });\n"
            + "            },\n"
            + "            { enableHighAccuracy: false, timeout: 10000, maximumAge: 0 }\n"
            + "        );");
  }

  @Test
  @NeedsFreshDriver
  void getGeolocationOverrideWithCoordinatesInContext() {
    System.out.println("DEBUG: Starting getGeolocationOverrideWithCoordinatesInContext test");

    BrowsingContext context = new BrowsingContext(driver, driver.getWindowHandle());
    String contextId = context.getId();
    System.out.println("DEBUG: Created context with ID: " + contextId);

    // Use secure URL for geolocation (now guaranteed to be available)
    String url = appServer.whereIsSecure("blank.html");
    System.out.println("DEBUG: Using secure URL: " + url);

    context.navigate(url, ReadinessState.COMPLETE);
    driver.switchTo().window(context.getId());

    // Wait for page to be fully loaded
    try {
      Thread.sleep(1000);
    } catch (InterruptedException e) {
      Thread.currentThread().interrupt();
    }

    String origin =
        (String) ((JavascriptExecutor) driver).executeScript("return window.location.origin;");
    System.out.println("DEBUG: Origin: " + origin);

    Emulation emul = new Emulation(driver);
    GeolocationCoordinates coords =
        new GeolocationCoordinates(37.7749, -122.4194, 10.0, null, null, null);
    System.out.println(
        "DEBUG: Setting geolocation override with coordinates: "
            + coords.getLatitude()
            + ", "
            + coords.getLongitude());

    emul.setGeolocationOverride(
        new SetGeolocationOverrideParameters(coords, null, List.of(contextId), null));
    System.out.println("DEBUG: Geolocation override set");

    Object result = getBrowserGeolocation(driver, null, origin);
    Map<String, Object> r = ((Map<String, Object>) result);

    System.out.println("DEBUG: Geolocation result: " + r);

    assert !r.containsKey("error") : "Geolocation failed with error: " + r.get("error");

    double latitude = ((Number) r.get("latitude")).doubleValue();
    double longitude = ((Number) r.get("longitude")).doubleValue();
    double accuracy = ((Number) r.get("accuracy")).doubleValue();

    System.out.println(
        "DEBUG: Checking coordinates - expected: "
            + coords.getLatitude()
            + ", "
            + coords.getLongitude()
            + ", actual: "
            + latitude
            + ", "
            + longitude);

    assert abs(latitude - coords.getLatitude()) < 0.0001
        : "Latitude mismatch: expected " + coords.getLatitude() + ", got " + latitude;
    assert abs(longitude - coords.getLongitude()) < 0.0001
        : "Longitude mismatch: expected " + coords.getLongitude() + ", got " + longitude;
    assert abs(accuracy - coords.getAccuracy()) < 0.0001
        : "Accuracy mismatch: expected " + coords.getAccuracy() + ", got " + accuracy;

    System.out.println("DEBUG: Test completed successfully");
  }

  @Test
  void canSetGeolocationOverrideWithMultipleUserContexts() {
    System.out.println("DEBUG: Starting canSetGeolocationOverrideWithMultipleUserContexts test");

    Browser browser = new Browser(driver);
    String userContext1 = browser.createUserContext();
    String userContext2 = browser.createUserContext();
    System.out.println("DEBUG: Created user contexts: " + userContext1 + ", " + userContext2);

    BrowsingContext context1 =
        new BrowsingContext(
            driver, new CreateContextParameters(WindowType.TAB).userContext(userContext1));
    BrowsingContext context2 =
        new BrowsingContext(
            driver, new CreateContextParameters(WindowType.TAB).userContext(userContext2));
    System.out.println(
        "DEBUG: Created browsing contexts: " + context1.getId() + ", " + context2.getId());

    GeolocationCoordinates coords =
        new GeolocationCoordinates(45.5, -122.4194, 10.0, null, null, null);

    Emulation emulation = new Emulation(driver);
    System.out.println("DEBUG: Setting geolocation override for multiple user contexts");
    emulation.setGeolocationOverride(
        new SetGeolocationOverrideParameters(
            coords, null, null, List.of(userContext1, userContext2)));

    // Test first user context
    System.out.println("DEBUG: Testing first user context");
    driver.switchTo().window(context1.getId());

    String url1 = appServer.whereIsSecure("blank.html");
    System.out.println("DEBUG: Using secure URL for context1: " + url1);

    context1.navigate(url1, ReadinessState.COMPLETE);

    // Wait for page to be fully loaded
    try {
      Thread.sleep(1000);
    } catch (InterruptedException e) {
      Thread.currentThread().interrupt();
    }

    String origin1 =
        (String) ((JavascriptExecutor) driver).executeScript("return window.location.origin;");
    System.out.println("DEBUG: Origin1: " + origin1);

    Map<String, Object> r =
        (Map<String, Object>) getBrowserGeolocation(driver, userContext1, origin1);

    System.out.println("DEBUG: Context1 result: " + r);
    assert !r.containsKey("error") : "Context1 geolocation failed with error: " + r.get("error");

    double latitude1 = ((Number) r.get("latitude")).doubleValue();
    double longitude1 = ((Number) r.get("longitude")).doubleValue();
    double accuracy1 = ((Number) r.get("accuracy")).doubleValue();

    assert abs(latitude1 - coords.getLatitude()) < 0.0001 : "Context1 latitude mismatch";
    assert abs(longitude1 - coords.getLongitude()) < 0.0001 : "Context1 longitude mismatch";
    assert abs(accuracy1 - coords.getAccuracy()) < 0.0001 : "Context1 accuracy mismatch";

    // Test second user context
    System.out.println("DEBUG: Testing second user context");
    driver.switchTo().window(context2.getId());

    String url2 = appServer.whereIsSecure("blank.html");
    System.out.println("DEBUG: Using secure URL for context2: " + url2);

    context2.navigate(url2, ReadinessState.COMPLETE);

    // Wait for page to be fully loaded
    try {
      Thread.sleep(1000);
    } catch (InterruptedException e) {
      Thread.currentThread().interrupt();
    }

    String origin2 =
        (String) ((JavascriptExecutor) driver).executeScript("return window.location.origin;");
    System.out.println("DEBUG: Origin2: " + origin2);

    Map<String, Object> r2 =
        (Map<String, Object>) getBrowserGeolocation(driver, userContext2, origin2);

    System.out.println("DEBUG: Context2 result: " + r2);
    assert !r2.containsKey("error") : "Context2 geolocation failed with error: " + r2.get("error");

    double latitude2 = ((Number) r2.get("latitude")).doubleValue();
    double longitude2 = ((Number) r2.get("longitude")).doubleValue();
    double accuracy2 = ((Number) r2.get("accuracy")).doubleValue();

    assert abs(latitude2 - coords.getLatitude()) < 0.0001 : "Context2 latitude mismatch";
    assert abs(longitude2 - coords.getLongitude()) < 0.0001 : "Context2 longitude mismatch";
    assert abs(accuracy2 - coords.getAccuracy()) < 0.0001 : "Context2 accuracy mismatch";

    System.out.println("DEBUG: Cleaning up contexts");
    context1.close();
    context2.close();
    browser.removeUserContext(userContext1);
    browser.removeUserContext(userContext2);

    System.out.println("DEBUG: Multiple user contexts test completed successfully");
  }

  @Test
  @Ignore(FIREFOX)
  void canSetGeolocationOverrideWithError() {
    System.out.println("DEBUG: Starting canSetGeolocationOverrideWithError test");

    BrowsingContext context = new BrowsingContext(driver, WindowType.TAB);
    String contextId = context.getId();
    System.out.println("DEBUG: Created context with ID: " + contextId);

    String url = appServer.whereIsSecure("blank.html");
    System.out.println("DEBUG: Using secure URL: " + url);

    context.navigate(url, ReadinessState.COMPLETE);

    // Switch to the new context
    driver.switchTo().window(contextId);

    // Wait for page to be fully loaded
    try {
      Thread.sleep(1000);
    } catch (InterruptedException e) {
      Thread.currentThread().interrupt();
    }

    String origin =
        (String) ((JavascriptExecutor) driver).executeScript("return window.location.origin;");
    System.out.println("DEBUG: Origin: " + origin);

    GeolocationPositionError error = new GeolocationPositionError();
    System.out.println("DEBUG: Setting geolocation override with error");

    Emulation emul = new Emulation(driver);
    emul.setGeolocationOverride(
        new SetGeolocationOverrideParameters(null, error, List.of(contextId), null));

    Object result = getBrowserGeolocation(driver, null, origin);
    Map<String, Object> r = ((Map<String, Object>) result);

    System.out.println("DEBUG: Error test result: " + r);
    assert r.containsKey("error") : "Expected geolocation to fail with error, but got: " + r;

    context.close();
    System.out.println("DEBUG: Error test completed successfully");
  }
}
