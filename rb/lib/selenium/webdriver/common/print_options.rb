# <copyright file="print_options.rb" company="Selenium Committers">
# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements. See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership. The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License. You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied. See the License for the
# specific language governing permissions and limitations
# under the License.
# </copyright>

module Selenium
  module WebDriver
    # Represents options for printing a page.
    class PrintOptions
      DEFAULT_SCALE = 1.0
      DEFAULT_ORIENTATION = 'portrait'.freeze
      DEFAULT_PAGE_SIZE = {width: 21.0, height: 29.7}.freeze # A4 size in cm
      DEFAULT_MARGINS = {top: 1.0, bottom: 1.0, left: 1.0, right: 1.0}.freeze

      attr_accessor :orientation, :scale, :background, :page_ranges, :page_size, :margins

      def initialize
        @orientation = DEFAULT_ORIENTATION
        @scale = DEFAULT_SCALE
        @background = false
        @page_ranges = nil
        @page_size = DEFAULT_PAGE_SIZE
        @margins = DEFAULT_MARGINS
      end

      # Converts the options to a hash format to be used by WebDriver.
      #
      # @return [Hash]
      def to_h
        options = {
          orientation: @orientation,
          scale: @scale,
          background: @background,
          pageRanges: @page_ranges,
          paperWidth: @page_size[:width],
          paperHeight: @page_size[:height],
          marginTop: @margins[:top],
          marginBottom: @margins[:bottom],
          marginLeft: @margins[:left],
          marginRight: @margins[:right]
        }

        options.compact
      end

      # Sets the page size to a predefined size.
      #
      # @param [Symbol] size The predefined size (:letter, :legal, :a4, :tabloid).
      def set_page_size(size)
        predefined_sizes = {
          letter: {width: 21.59, height: 27.94},
          legal: {width: 21.59, height: 35.56},
          a4: {width: 21.0, height: 29.7},
          tabloid: {width: 27.94, height: 43.18}
        }

        raise ArgumentError, "Invalid page size: #{size}" unless predefined_sizes.key?(size)

        @page_size = predefined_sizes[size]
      end
    end
  end
end
