# This file has been generated using `bazel run scripts:pinned_browsers`

load("@bazel_tools//tools/build_defs/repo:http.bzl", "http_archive")
load("//common/private:deb_archive.bzl", "deb_archive")
load("//common/private:dmg_archive.bzl", "dmg_archive")
load("//common/private:drivers.bzl", "local_drivers")
load("//common/private:pkg_archive.bzl", "pkg_archive")

def pin_browsers():
    local_drivers(name = "local_drivers")

    http_archive(
        name = "linux_firefox",
        url = "https://ftp.mozilla.org/pub/firefox/releases/134.0.2/linux-x86_64/en-US/firefox-134.0.2.tar.bz2",
        sha256 = "980b8cdebd7abdbed30a560a1a564d429c4e832c9e24e407b4c8a9fec24e0109",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

filegroup(
    name = "files",
    srcs = glob(["**/*"]),
)

exports_files(["firefox/firefox"])

js_library(
    name = "firefox-js",
    data = [":files"],
)
""",
    )

    dmg_archive(
        name = "mac_firefox",
        url = "https://ftp.mozilla.org/pub/firefox/releases/134.0.2/mac/en-US/Firefox%20134.0.2.dmg",
        sha256 = "7832a8d1e1a13322dec1b52d32e1e64d2b6254de8348575f2d22af4975d56a85",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["Firefox.app"])

js_library(
    name = "firefox-js",
    data = glob(["Firefox.app/**/*"]),
)
""",
    )

    http_archive(
        name = "linux_beta_firefox",
        url = "https://ftp.mozilla.org/pub/firefox/releases/135.0b9/linux-x86_64/en-US/firefox-135.0b9.tar.xz",
        sha256 = "a5aa65623c3ad4b6884b7617db0f7635880e042d1ed02376f52d8065884be03f",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

filegroup(
    name = "files",
    srcs = glob(["**/*"]),
)

exports_files(["firefox/firefox"])

js_library(
    name = "firefox-js",
    data = [":files"],
)
""",
    )

    dmg_archive(
        name = "mac_beta_firefox",
        url = "https://ftp.mozilla.org/pub/firefox/releases/135.0b9/mac/en-US/Firefox%20135.0b9.dmg",
        sha256 = "31d395ddc49b5f18863c2e060610d3a7089a4af46df12d73f1d56d5afe78baa3",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["Firefox.app"])

js_library(
    name = "firefox-js",
    data = glob(["Firefox.app/**/*"]),
)
""",
    )

    http_archive(
        name = "linux_geckodriver",
        url = "https://github.com/mozilla/geckodriver/releases/download/v0.35.0/geckodriver-v0.35.0-linux64.tar.gz",
        sha256 = "ac26e9ba8f3b8ce0fbf7339b9c9020192f6dcfcbf04a2bcd2af80dfe6bb24260",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["geckodriver"])

js_library(
    name = "geckodriver-js",
    data = ["geckodriver"],
)
""",
    )

    http_archive(
        name = "mac_geckodriver",
        url = "https://github.com/mozilla/geckodriver/releases/download/v0.35.0/geckodriver-v0.35.0-macos.tar.gz",
        sha256 = "ccff606851fd84d30a864e4bbc03535523a4038bf9a9e787a30817a8776fada1",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["geckodriver"])

js_library(
    name = "geckodriver-js",
    data = ["geckodriver"],
)
""",
    )

    pkg_archive(
        name = "mac_edge",
        url = "https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/32cc7c00-5d56-4296-81bc-798f79706d6c/MicrosoftEdge-132.0.2957.127.pkg",
        sha256 = "dc5d6fc311d0a93dc9a665051738c5f855c95e24ae49d5696c68148f14daa97e",
        move = {
            "MicrosoftEdge-132.0.2957.127.pkg/Payload/Microsoft Edge.app": "Edge.app",
        },
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["Edge.app"])

js_library(
    name = "edge-js",
    data = glob(["Edge.app/**/*"]),
)
""",
    )

    deb_archive(
        name = "linux_edge",
        url = "https://packages.microsoft.com/repos/edge/pool/main/m/microsoft-edge-stable/microsoft-edge-stable_132.0.2957.127-1_amd64.deb",
        sha256 = "ba80bca3192b3168269146e09fb38fdb596911a74a6666f81de6f7db312f7631",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

filegroup(
    name = "files",
    srcs = glob(["**/*"]),
)

exports_files(["opt/microsoft/msedge/microsoft-edge"])

js_library(
    name = "edge-js",
    data = [":files"],
)
""",
    )

    http_archive(
        name = "linux_edgedriver",
        url = "https://msedgedriver.azureedge.net/132.0.2957.127/edgedriver_linux64.zip",
        sha256 = "d5a40743779e52d81ca4f0073ff47965347f9e906cac75f35fda0222480b78dc",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["msedgedriver"])

js_library(
    name = "msedgedriver-js",
    data = ["msedgedriver"],
)
""",
    )

    http_archive(
        name = "mac_edgedriver",
        url = "https://msedgedriver.azureedge.net/132.0.2957.127/edgedriver_mac64.zip",
        sha256 = "0f73b6612245fcf5b719f27bc964832b25e349bfc64173b56e4ed26ca040ac64",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["msedgedriver"])

js_library(
    name = "msedgedriver-js",
    data = ["msedgedriver"],
)
""",
    )

    http_archive(
        name = "linux_chrome",
        url = "https://storage.googleapis.com/chrome-for-testing-public/132.0.6834.110/linux64/chrome-linux64.zip",
        sha256 = "26dd8ed1b29cc74ab2fc79b55c65bac94734116caf5de9dcf39811288a9a8cc3",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

filegroup(
    name = "files",
    srcs = glob(["**/*"]),
)

exports_files(["chrome-linux64/chrome"])

js_library(
    name = "chrome-js",
    data = [":files"],
)
""",
    )

    http_archive(
        name = "mac_chrome",
        url = "https://storage.googleapis.com/chrome-for-testing-public/132.0.6834.110/mac-x64/chrome-mac-x64.zip",
        sha256 = "daf7c01d494e6bef5171cd1fb30d5c826b9bdd582ce87386e7989ffc4aa5237e",
        strip_prefix = "chrome-mac-x64",
        patch_cmds = [
            "mv 'Google Chrome for Testing.app' Chrome.app",
            "mv 'Chrome.app/Contents/MacOS/Google Chrome for Testing' Chrome.app/Contents/MacOS/Chrome",
        ],
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["Chrome.app"])

js_library(
    name = "chrome-js",
    data = glob(["Chrome.app/**/*"]),
)
""",
    )

    http_archive(
        name = "linux_chromedriver",
        url = "https://storage.googleapis.com/chrome-for-testing-public/132.0.6834.110/linux64/chromedriver-linux64.zip",
        sha256 = "5ff7a0c21c9117eb2b4f813908106ab5b478c2a172a86bbc8fccc4b37aa0a644",
        strip_prefix = "chromedriver-linux64",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["chromedriver"])

js_library(
    name = "chromedriver-js",
    data = ["chromedriver"],
)
""",
    )

    http_archive(
        name = "mac_chromedriver",
        url = "https://storage.googleapis.com/chrome-for-testing-public/132.0.6834.110/mac-x64/chromedriver-mac-x64.zip",
        sha256 = "6355b126c81e8de595f52d7bb472b945c701efcac9bcbdbe360ad4a967bd54c7",
        strip_prefix = "chromedriver-mac-x64",
        build_file_content = """
load("@aspect_rules_js//js:defs.bzl", "js_library")
package(default_visibility = ["//visibility:public"])

exports_files(["chromedriver"])

js_library(
    name = "chromedriver-js",
    data = ["chromedriver"],
)
""",
    )

def _pin_browsers_extension_impl(_ctx):
    pin_browsers()

pin_browsers_extension = module_extension(
    implementation = _pin_browsers_extension_impl,
)
