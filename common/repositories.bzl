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
        url = "https://ftp.mozilla.org/pub/firefox/releases/139.0/linux-x86_64/en-US/firefox-139.0.tar.xz",
        sha256 = "1e3eab20cb84af1510b22e663438bd31ee1473652e10eb48661dcc6f1c0bbbe8",
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
        url = "https://ftp.mozilla.org/pub/firefox/releases/139.0/mac/en-US/Firefox%20139.0.dmg",
        sha256 = "03f1066b70b6e502c4f1a732a63f9f93146c6ea23d0af5e2e3a57d030e255353",
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
        url = "https://ftp.mozilla.org/pub/firefox/releases/140.0b1/linux-x86_64/en-US/firefox-140.0b1.tar.xz",
        sha256 = "816e57b7a46883b9fad4be62161a070dbd57ce23e01403207d941384b644c80d",
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
        url = "https://ftp.mozilla.org/pub/firefox/releases/140.0b1/mac/en-US/Firefox%20140.0b1.dmg",
        sha256 = "1233059971a4fa9ee221872c05ba85ebd3fa4707a08a13e5771c387729eccbc0",
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
        url = "https://github.com/mozilla/geckodriver/releases/download/v0.36.0/geckodriver-v0.36.0-linux64.tar.gz",
        sha256 = "0bde38707eb0a686a20c6bd50f4adcc7d60d4f73c60eb83ee9e0db8f65823e04",
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
        url = "https://github.com/mozilla/geckodriver/releases/download/v0.36.0/geckodriver-v0.36.0-macos.tar.gz",
        sha256 = "b5627bfc29801b8752c9f1e7699018963c39c076aab6576dc14fcb1ce7a256f6",
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
        url = "https://msedge.sf.dl.delivery.mp.microsoft.com/filestreamingservice/files/0db7b32d-8b37-45ad-b2d9-b6b6cfcf91bd/MicrosoftEdge-136.0.3240.92.pkg",
        sha256 = "c3407b12244a854928d5731c5c52be41905ab7f7689a50503929ed21c1fe15e2",
        move = {
            "MicrosoftEdge-136.0.3240.92.pkg/Payload/Microsoft Edge.app": "Edge.app",
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
        url = "https://packages.microsoft.com/repos/edge/pool/main/m/microsoft-edge-stable/microsoft-edge-stable_136.0.3240.92-1_amd64.deb",
        sha256 = "04b4287e74be559c365f36beb38fa8233de24ed8c8a17d95d9ce83547951d8c3",
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
        url = "https://msedgedriver.azureedge.net/136.0.3240.92/edgedriver_linux64.zip",
        sha256 = "8abb53e9a60e5587300835f9e039f12c81ae10b1b34231982b35b99637252109",
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
        url = "https://msedgedriver.azureedge.net/136.0.3240.92/edgedriver_mac64.zip",
        sha256 = "6d24b3fa48acfb71963605ea93778c4486442fdedde5e57aecdfac36e9dbf552",
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
        url = "https://storage.googleapis.com/chrome-for-testing-public/137.0.7151.55/linux64/chrome-linux64.zip",
        sha256 = "ffedd41a261f26e2e60e5d1692e0955c292caffafd15865344d21b845554a3d4",
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
        url = "https://storage.googleapis.com/chrome-for-testing-public/137.0.7151.55/mac-x64/chrome-mac-x64.zip",
        sha256 = "1342558af92f8d8f9cb26c4504f9d49b63c0b1d6fe06485c2e01c763a67deb44",
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
        url = "https://storage.googleapis.com/chrome-for-testing-public/137.0.7151.55/linux64/chromedriver-linux64.zip",
        sha256 = "57b44a6b5193511898eb1c26b3eae8378a763a6d09e7687b801a799d58e7e90d",
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
        url = "https://storage.googleapis.com/chrome-for-testing-public/137.0.7151.55/mac-x64/chromedriver-mac-x64.zip",
        sha256 = "8a6c76cde41275f539dae1c9287c50def6481f17737b6d871237e81c26eda635",
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
