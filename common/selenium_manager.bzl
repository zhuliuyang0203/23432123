# This file has been generated using `bazel run scripts:selenium_manager`

load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_file")

def selenium_manager():
    http_file(
        name = "download_sm_linux",
        executable = True,
        sha256 = "0b256b63affe9013c926bc477bfe4346535494403e9cfe338cfa9dd14aa42f8f",
        url = "https://github.com/SeleniumHQ/selenium_manager_artifacts/releases/download/selenium-manager-3503b69/selenium-manager-linux",
    )

    http_file(
        name = "download_sm_macos",
        executable = True,
        sha256 = "816be5ba96deb321b6b0e109a59d6b8c9ff0194f2b3204f1801ad96c190ecfd5",
        url = "https://github.com/SeleniumHQ/selenium_manager_artifacts/releases/download/selenium-manager-3503b69/selenium-manager-macos",
    )

    http_file(
        name = "download_sm_windows",
        executable = True,
        sha256 = "d1a942bd3e5cdb5a70e8e9683f8e964d9d6fcf5be472f9a302451ddc990843d9",
        url = "https://github.com/SeleniumHQ/selenium_manager_artifacts/releases/download/selenium-manager-3503b69/selenium-manager-windows.exe",
    )

def _selenium_manager_artifacts_impl(_ctx):
    selenium_manager()

selenium_manager_artifacts = module_extension(
    implementation = _selenium_manager_artifacts_impl,
)
