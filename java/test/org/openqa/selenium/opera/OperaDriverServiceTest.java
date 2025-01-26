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

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyInt;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.doReturn;
import static org.mockito.Mockito.spy;
import static org.mockito.Mockito.verify;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.Tag;
import org.openqa.selenium.chromium.ChromiumDriverLogLevel;

import java.io.File;
import java.time.Duration;
import java.util.Arrays;
import java.util.List;

@Tag("UnitTests")
public class OperaDriverServiceTest {

  @Test
  public void builderPassesTimeoutToDriverService() {
    File exe = new File("someFile");
    Duration defaultTimeout = Duration.ofSeconds(20);
    Duration customTimeout = Duration.ofSeconds(60);

    OperaDriverService.Builder builderMock = spy(OperaDriverService.Builder.class);
    builderMock.build();

    verify(builderMock).createDriverService(any(), anyInt(), eq(defaultTimeout), any(), any());

    builderMock.withTimeout(customTimeout);
    builderMock.build();
    verify(builderMock).createDriverService(any(), anyInt(), eq(customTimeout), any(), any());
  }

  @Test
  void testScoring() {
    OperaDriverService.Builder builder = new OperaDriverService.Builder();
    assertThat(builder.score(new OperaOptions())).isPositive();
  }

  @Test
  void logLevelLastWins() {
    OperaDriverService.Builder builderMock = spy(OperaDriverService.Builder.class);

    List<String> silentLast = Arrays.asList("--port=1", "--log-level=OFF");
    builderMock.withLoglevel(ChromiumDriverLogLevel.ALL).usingPort(1).withSilent(true).build();
    verify(builderMock).createDriverService(any(), anyInt(), any(), eq(silentLast), any());

    List<String> silentFirst = Arrays.asList("--port=1", "--log-level=DEBUG");
    builderMock.withSilent(true).withLoglevel(ChromiumDriverLogLevel.DEBUG).usingPort(1).build();
    verify(builderMock).createDriverService(any(), anyInt(), any(), eq(silentFirst), any());

    List<String> verboseLast = Arrays.asList("--port=1", "--log-level=ALL");
    builderMock.withLoglevel(ChromiumDriverLogLevel.OFF).usingPort(1).withVerbose(true).build();
    verify(builderMock).createDriverService(any(), anyInt(), any(), eq(verboseLast), any());

    List<String> verboseFirst = Arrays.asList("--port=1", "--log-level=INFO");
    builderMock.withVerbose(true).withLoglevel(ChromiumDriverLogLevel.INFO).usingPort(1).build();
    verify(builderMock).createDriverService(any(), anyInt(), any(), eq(verboseFirst), any());
  }

  // Setting these to false makes no sense; we're just going to ignore it.
  @Test
  void ignoreFalseLogging() {
    OperaDriverService.Builder builderMock = spy(OperaDriverService.Builder.class);

    List<String> falseSilent = Arrays.asList("--port=1", "--log-level=DEBUG");
    builderMock.withLoglevel(ChromiumDriverLogLevel.DEBUG).usingPort(1).withSilent(false).build();
    verify(builderMock).createDriverService(any(), anyInt(), any(), eq(falseSilent), any());
  }
}
