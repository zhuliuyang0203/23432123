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

import org.openqa.selenium.Beta;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chromium.ChromiumDriver;
import org.openqa.selenium.chromium.ChromiumDriverCommandExecutor;
import org.openqa.selenium.internal.Require;
import org.openqa.selenium.remote.CommandInfo;
import org.openqa.selenium.remote.RemoteWebDriver;
import org.openqa.selenium.remote.RemoteWebDriverBuilder;
import org.openqa.selenium.remote.http.ClientConfig;
import org.openqa.selenium.remote.service.DriverFinder;
import org.openqa.selenium.remote.service.DriverService;

import java.util.Map;
import java.util.stream.Collectors;
import java.util.stream.Stream;

/**
 * A {@link WebDriver} implementation that controls a Chromium-based Opera browser running on the local
 * machine. It requires an <code>operadriver</code> executable to be available in PATH.
 *
 * @see <a href="https://github.com/operasoftware/operachromiumdriver">operadriver</a>
 */
public class OperaDriver extends ChromiumDriver {

  public OperaDriver() {
    this(new OperaOptions());
  }

  public OperaDriver(OperaOptions options) {
    this(new OperaDriverService.Builder().build(), options);
  }

  public OperaDriver(OperaDriverService service) {
    this(service, new OperaOptions());
  }

  public OperaDriver(OperaDriverService service, OperaOptions options) {
    this(service, options, ClientConfig.defaultConfig());
  }

  public OperaDriver(OperaDriverService service, OperaOptions options, ClientConfig clientConfig) {
    super(generateExecutor(service, options, clientConfig), options, OperaOptions.CAPABILITY);
    casting = new AddHasCasting().getImplementation(getCapabilities(), getExecuteMethod());
    cdp = new AddHasCdp().getImplementation(getCapabilities(), getExecuteMethod());
  }

  private static OperaDriver.OperaDriverCommandExecutor generateExecutor(
    OperaDriverService service, OperaOptions options, ClientConfig clientConfig) {
    Require.nonNull("Driver service", service);
    Require.nonNull("Driver options", options);
    Require.nonNull("Driver clientConfig", clientConfig);
    DriverFinder finder = new DriverFinder(service, options);
    service.setExecutable(finder.getDriverPath());
    if (finder.hasBrowserPath()) {
      options.setBinary(finder.getBrowserPath());
      options.setCapability("browserVersion", (Object) null);
    }
    return new OperaDriver.OperaDriverCommandExecutor(service, clientConfig);
  }

  @Beta
  public static RemoteWebDriverBuilder builder() {
    return RemoteWebDriver.builder().oneOf(new OperaOptions());
  }

  private static class OperaDriverCommandExecutor extends ChromiumDriverCommandExecutor {
    public OperaDriverCommandExecutor(DriverService service, ClientConfig clientConfig) {
      super(service, getExtraCommands(), clientConfig);
    }

    private static Map<String, CommandInfo> getExtraCommands() {
      return Stream.of(
          new AddHasCasting().getAdditionalCommands(), new AddHasCdp().getAdditionalCommands())
        .flatMap((m) -> m.entrySet().stream())
        .collect(Collectors.toUnmodifiableMap(Map.Entry::getKey, Map.Entry::getValue));
    }
  }
}
