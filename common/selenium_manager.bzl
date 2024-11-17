# This file has been generated using `bazel run scripts:selenium_manager`

load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_file")

def selenium_manager():
    http_file(
        name = "download_sm_linux",
        executable = True,
        sha256 = "97ab346b907a813c236f1c6b9eb0e1b878702374b0768894415629c2cf05d97e",
        url = "https://github.com/SeleniumHQ/selenium_manager_artifacts/releases/download/selenium-manager-33ad1b2/selenium-manager-linux",
    )

    http_file(
        name = "download_sm_macos",
        executable = True,
        sha256 = "ef27b5c2d274dc4ab4417334116a1530571edc3deaf4740068e35484e275f28a",
        url = "https://github.com/SeleniumHQ/selenium_manager_artifacts/releases/download/selenium-manager-33ad1b2/selenium-manager-macos",
    )

    http_file(
        name = "download_sm_windows",
        executable = True,
        sha256 = "15113137d8d0d3648be9948c52e56e1f4c605bc5d9623962991198e8d0d413b6",
        url = "https://github.com/SeleniumHQ/selenium_manager_artifacts/releases/download/selenium-manager-33ad1b2/selenium-manager-windows.exe",
    )

def _selenium_manager_artifacts_impl(_ctx):
    selenium_manager()

selenium_manager_artifacts = module_extension(
    implementation = _selenium_manager_artifacts_impl,
)
