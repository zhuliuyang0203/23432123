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

package org.openqa.selenium.opera;

import static org.assertj.core.api.Assertions.assertThat;
import static org.openqa.selenium.remote.CapabilityType.ACCEPT_INSECURE_CERTS;

import org.junit.jupiter.api.Test;
import org.openqa.selenium.HasCapabilities;
import org.openqa.selenium.JavascriptExecutor;
import org.openqa.selenium.testing.JupiterTestBase;
import org.openqa.selenium.testing.NoDriverBeforeTest;
import org.openqa.selenium.testing.drivers.WebDriverBuilder;

import java.io.IOException;
import java.nio.file.Files;
import java.util.Base64;

/**
 * Functional tests for {@link OperaOptions}.
 */
public class OperaOptionsFunctionalTest extends JupiterTestBase {

  @Test
  @NoDriverBeforeTest
  public void canStartOperaWithCustomOptions() {
    OperaOptions options = new OperaOptions();
    options.addArguments("user-agent=foo;bar");
    localDriver = new WebDriverBuilder().get(options);

    localDriver.get(pages.clickJacker);
    Object userAgent =
      ((JavascriptExecutor) localDriver).executeScript("return window.navigator.userAgent");
    assertThat(userAgent).isEqualTo("foo;bar");
  }

  @Test
  void optionsStayEqualAfterSerialization() {
    OperaOptions options1 = new OperaOptions();
    OperaOptions options2 = new OperaOptions();
    assertThat(options2).isEqualTo(options1);
    options1.asMap();
    assertThat(options2).isEqualTo(options1);
  }

  @Test
  @NoDriverBeforeTest
  public void canSetAcceptInsecureCerts() {
    OperaOptions options = new OperaOptions();
    options.setAcceptInsecureCerts(true);
    localDriver = new WebDriverBuilder().get(options);
    System.out.println(((HasCapabilities) localDriver).getCapabilities());

    assertThat(
      ((HasCapabilities) localDriver).getCapabilities().getCapability(ACCEPT_INSECURE_CERTS))
      .isEqualTo(true);
  }
}
