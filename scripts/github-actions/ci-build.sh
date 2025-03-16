#!/usr/bin/env bash

set -eufo pipefail
# We want to see what's going on
set -x

run_bazel_tests() {
  # shellcheck disable=SC2046
  bazel test --config=remote-ci --build_tests_only \
    --keep_going --flaky_test_attempts=2 \
    //... -- $(cat .skipped-tests | tr '\n' ' ')
}

# Prepare log directory if BAZEL_LOG_FILE is set and run tests
if [ -n "${BAZEL_LOG_FILE:-}" ]; then
  LOG_DIR=$(dirname "$BAZEL_LOG_FILE")
  if mkdir -p "$LOG_DIR"; then
    run_bazel_tests 2>&1 | tee "$BAZEL_LOG_FILE"
  else
    echo "Error: Failed to create directory for BAZEL_LOG_FILE" >&2
    exit 1
  fi
else
  run_bazel_tests
fi

# Build the packages we want to ship to users
bazel build --config=remote-ci --build_tag_filters=release-artifact //...
