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

package org.openqa.selenium.bidi.browser;

import java.util.Map;

public class ClientWindowInfo {
  private final String clientWindow;
  private final String state;
  private final Integer width;
  private final Integer height;
  private final Integer x;
  private final Integer y;

  public ClientWindowInfo(
      String clientWindow, String state, Integer width, Integer height, Integer x, Integer y) {
    this.clientWindow = clientWindow;
    this.state = state;
    this.width = width;
    this.height = height;
    this.x = x;
    this.y = y;
  }

  public static ClientWindowInfo fromJson(Map<String, Object> map) {
    return new ClientWindowInfo(
        (String) map.get("clientWindow"),
        (String) map.get("state"),
        (Integer) map.get("width"),
        (Integer) map.get("height"),
        (Integer) map.get("x"),
        (Integer) map.get("y"));
  }

  public String getClientWindow() {
    return clientWindow;
  }

  public String getState() {
    return state;
  }

  public Integer getWidth() {
    return width;
  }

  public Integer getHeight() {
    return height;
  }

  public Integer getX() {
    return x;
  }

  public Integer getY() {
    return y;
  }
}
