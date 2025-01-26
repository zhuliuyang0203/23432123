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

import com.google.auto.service.AutoService;
import org.openqa.selenium.Capabilities;
import org.openqa.selenium.WebDriverException;
import org.openqa.selenium.chromium.ChromiumDriverLogLevel;
import org.openqa.selenium.remote.service.DriverFinder;
import org.openqa.selenium.remote.service.DriverService;

import java.io.File;
import java.io.IOException;
import java.time.Duration;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import static java.util.Collections.unmodifiableList;
import static org.openqa.selenium.remote.Browser.OPERA;

/**
 * Manages the life and death of a operadriver server.
 */
public class OperaDriverService extends DriverService {

  public static final String OPERA_DRIVER_NAME = "operadriver";

  /**
   * System property that defines the location of the operadriver executable that will be used by
   * the {@link #createDefaultService() default service}.
   */
  public static final String OPERA_DRIVER_EXE_PROPERTY = "webdriver.opera.driver";

  /** System property that toggles the formatting of the timestamps of the logs */
  public static final String OPERA_DRIVER_READABLE_TIMESTAMP = "webdriver.opera.readableTimestamp";

  /**
   * System property that defines the location of the log that will be written by
   * the {@link #createDefaultService() default service}.
   */
  public static final String OPERA_DRIVER_LOG_PROPERTY = "webdriver.opera.logfile";

  /** System property that defines the {@link ChromiumDriverLogLevel} for OperaDriver logs. */
  public static final String OPERA_DRIVER_LOG_LEVEL_PROPERTY = "webdriver.opera.loglevel";

  /**
   * Boolean system property that defines whether OperaDriver should append to existing log file.
   */
  public static final String OPERA_DRIVER_APPEND_LOG_PROPERTY = "webdriver.opera.appendLog";

  /**
   * Boolean system property that defines whether the OperaDriver executable should be started
   * with verbose logging.
   */
  public static final String OPERA_DRIVER_VERBOSE_LOG_PROPERTY = "webdriver.opera.verboseLogging";

  /**
   * Boolean system property that defines whether the OperaDriver executable should be started
   * in silent mode.
   */
  public static final String OPERA_DRIVER_SILENT_OUTPUT_PROPERTY = "webdriver.opera.silentOutput";

  /**
   * System property that defines comma-separated list of remote IPv4 addresses which are allowed to
   * connect to OperaDriver.
   */
  public static final String OPERA_DRIVER_ALLOWED_IPS_PROPERTY = "webdriver.opera.withAllowedIps";

  /**
   * System property that defines whether the OperaDriver executable should check for build version
   * compatibility between OperaDriver and the browser.
   */
  public static final String OPERA_DRIVER_DISABLE_BUILD_CHECK = "webdriver.opera.disableBuildCheck";

  /**
   *
   * @param executable The operadriver executable.
   * @param port Which port to start the operadriver on.
   * @param args The arguments to the launched server.
   * @param environment The environment for the launched server.
   * @throws IOException If an I/O error occurs.
   */
  public OperaDriverService(File executable, int port, List<String> args,
                            Map<String, String> environment) throws IOException {
    super(executable, port, DEFAULT_TIMEOUT, args, environment);
  }

  /**
   *
   * @param executable The operadriver executable.
   * @param port Which port to start the operadriver on.
   * @param timeout Timeout waiting for driver server to start.
   * @param args The arguments to the launched server.
   * @param environment The environment for the launched server.
   * @throws IOException If an I/O error occurs.
   */
  public OperaDriverService(File executable, int port, Duration timeout, List<String> args,
                            Map<String, String> environment) throws IOException {
    super(executable, port, timeout, args, environment);
  }

  /**
   * Configures and returns a new {@link OperaDriverService} using the default configuration. In
   * this configuration, the service will use the operadriver executable identified by the
   * {@link #OPERA_DRIVER_EXE_PROPERTY} system property. Each service created by this method will
   * be configured to use a free port on the current system.
   *
   * @return A new OperaDriverService using the default configuration.
   */

  public String getDriverName() {
    return OPERA_DRIVER_NAME;
  }

  public String getDriverProperty() {
    return OPERA_DRIVER_EXE_PROPERTY;
  }

  @Override
  public Capabilities getDefaultDriverOptions() {
    return new OperaOptions();
  }

  /**
   * Configures and returns a new {@link OperaDriverService} using the default configuration. In this
   * configuration, the service will use the {@code operadriver} executable identified by the {@link
   * DriverFinder#getDriverPath()} (DriverService, Capabilities)}. Each service created by this
   * method will be configured to use a free port on the current system.
   *
   * @return A new OperaDriverService using the default configuration.
   */
  public static OperaDriverService createDefaultService() {
    return new Builder().build();
  }

  /**
   * Builder used to configure new {@link OperaDriverService} instances.
   */
  @AutoService(DriverService.Builder.class)
  public static class Builder extends DriverService.Builder<
      OperaDriverService, OperaDriverService.Builder> {

    private Boolean disableBuildCheck;
    private Boolean readableTimestamp;
    private Boolean appendLog;
    private Boolean verbose;
    private Boolean silent;
    private String allowedListIps;
    private ChromiumDriverLogLevel logLevel;

    @Override
    public int score(Capabilities capabilities) {
      int score = 0;

      if (OPERA.is(capabilities)) {
        score++;
      }

      if (capabilities.getCapability(OperaOptions.CAPABILITY) != null) {
        score++;
      }

      return score;
    }

    /**
     * Configures the driver server appending to log file.
     *
     * @param appendLog True for appending to log file, false otherwise.
     * @return A self reference.
     */
    public OperaDriverService.Builder withAppendLog(boolean appendLog) {
      this.appendLog = appendLog;
      return this;
    }

    /**
     * Allows the driver to be used with potentially incompatible versions of the browser.
     *
     * @param noBuildCheck True for not enforcing matching versions.
     * @return A self reference.
     */
    public OperaDriverService.Builder withBuildCheckDisabled(boolean noBuildCheck) {
      this.disableBuildCheck = noBuildCheck;
      return this;
    }

    /**
     * Configures the driver server log level.
     *
     * @param logLevel {@link ChromiumDriverLogLevel} for desired log level output.
     * @return A self reference.
     */
    public OperaDriverService.Builder withLoglevel(ChromiumDriverLogLevel logLevel) {
      this.logLevel = logLevel;
      this.silent = false;
      this.verbose = false;
      return this;
    }

    /**
     * Configures the driver server verbosity.
     *
     * @param verbose true for verbose output, false otherwise.
     * @return A self reference.
    */
    public Builder withVerbose(boolean verbose) {
      this.verbose = verbose;
      return this;
    }

    /**
     * Configures the driver server for silent output.
     *
     * @param silent true for silent output, false otherwise.
     * @return A self reference.
    */
    public Builder withSilent(boolean silent) {
      this.silent = silent;
      return this;
    }

    @Override
    protected void loadSystemProperties() {
      parseLogOutput(OPERA_DRIVER_LOG_PROPERTY);
      if (disableBuildCheck == null) {
        this.disableBuildCheck = Boolean.getBoolean(OPERA_DRIVER_DISABLE_BUILD_CHECK);
      }
      if (readableTimestamp == null) {
        this.readableTimestamp = Boolean.getBoolean(OPERA_DRIVER_READABLE_TIMESTAMP);
      }
      if (appendLog == null) {
        this.appendLog = Boolean.getBoolean(OPERA_DRIVER_APPEND_LOG_PROPERTY);
      }
      if (verbose == null && Boolean.getBoolean(OPERA_DRIVER_VERBOSE_LOG_PROPERTY)) {
        withVerbose(Boolean.getBoolean(OPERA_DRIVER_VERBOSE_LOG_PROPERTY));
      }
      if (silent == null && Boolean.getBoolean(OPERA_DRIVER_SILENT_OUTPUT_PROPERTY)) {
        withSilent(Boolean.getBoolean(OPERA_DRIVER_SILENT_OUTPUT_PROPERTY));
      }
      if (allowedListIps == null) {
        this.allowedListIps = System.getProperty(OPERA_DRIVER_ALLOWED_IPS_PROPERTY);
      }
      if (logLevel == null && System.getProperty(OPERA_DRIVER_LOG_LEVEL_PROPERTY) != null) {
        String level = System.getProperty(OPERA_DRIVER_LOG_LEVEL_PROPERTY);
        withLoglevel(ChromiumDriverLogLevel.fromString(level));
      }
    }

    @Override
    protected List<String> createArgs() {
      if (getLogFile() == null) {
        String logFilePath = System.getProperty(OPERA_DRIVER_LOG_PROPERTY);
        if (logFilePath != null) {
          withLogFile(new File(logFilePath));
        }
      }

      List<String> args = new ArrayList<>();
      args.add(String.format("--port=%d", getPort()));
      if (getLogFile() != null) {
        args.add(String.format("--log-path=%s", getLogFile().getAbsolutePath()));
      }
      if (verbose) {
        args.add("--verbose");
      }
      if (silent) {
        args.add("--silent");
      }

      return unmodifiableList(args);
    }

    @Override
    protected OperaDriverService createDriverService(
      File exe, int port, Duration timeout, List<String> args, Map<String, String> environment) {
      try {
        return new OperaDriverService(exe, port, timeout, args, environment);
      } catch (IOException e) {
        throw new WebDriverException(e);
      }
    }
  }
}
