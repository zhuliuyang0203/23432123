# This file has been generated using `bazel run scripts:selenium_manager`

load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_file")

def selenium_manager():
    http_file(
        name = "download_sm_linux",
        executable = True,
        sha256 = "7160c0104ae1aa4c138e63dc59e773a5f0552e8921166e831fd0b62f1485a109",
        url = "https://github.com/SeleniumHQ/selenium_manager_artifacts/releases/download/selenium-manager-93882ea/selenium-manager-linux",
    )

    http_file(
        name = "download_sm_macos",
        executable = True,
        sha256 = "271717f6a250ddd3190751ce083a00e34140075187093d669649bda3967cd2b4",
        url = "https://github.com/SeleniumHQ/selenium_manager_artifacts/releases/download/selenium-manager-93882ea/selenium-manager-macos",
    )

    http_file(
        name = "download_sm_windows",
        executable = True,
        sha256 = "2ead2755fc692b96d0cae4d288a07ac55dd97392685d23ddd7e72874744b96f2",
        url = "https://github.com/SeleniumHQ/selenium_manager_artifacts/releases/download/selenium-manager-93882ea/selenium-manager-windows.exe",
    )

def _selenium_manager_artifacts_impl(_ctx):
    selenium_manager()

selenium_manager_artifacts = module_extension(
    implementation = _selenium_manager_artifacts_impl,
)
