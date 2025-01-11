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

package org.openqa.selenium.print;

import java.util.HashMap;
import java.util.Map;

public class PageSize {
  private final double height;
  private final double width;

    // Predefined constants
    public static final PageSize A4 = new PageSize(27.94, 21.59); // A4 size in cm
    public static final PageSize LEGAL = new PageSize(35.56, 21.59); // Legal size in cm
    public static final PageSize TABLOID = new PageSize(43.18, 27.94); // Tabloid size in cm
    public static final PageSize LETTER = new PageSize(27.94, 21.59); // Letter size in cm
    
    public PageSize(double height, double width) {
        this.height = height;
        this.width = width;
    }

    // Default constructor (e.g., default to A4)
    public PageSize() {
        this(A4.getHeight(), A4.getWidth());
    }

    // Getters for Height and Width
    public double getHeight() {
        return height;
    }

    public double getWidth() {
        return width;
    }

    public Map<String, Object> toMap() {
    Map<String, Object> map = new HashMap<>();
    map.put("width", width);
    map.put("height", height);
    return map;
    }

    @Override
    public String toString() {
    return "PageSize[width=" + this.getWidth() + ", height=" + this.getHeight() + "]";
}

}
