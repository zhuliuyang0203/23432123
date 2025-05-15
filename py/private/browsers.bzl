load(
    "//common:browsers.bzl",
    "COMMON_TAGS",
    "chrome_data",
    "edge_data",
    "firefox_data",
)

headless_args = select({
    "@selenium//common:use_headless_browser": [
        "--headless=true",
    ],
    "//conditions:default": [],
})

chrome_args = select({
    "@selenium//common:use_pinned_linux_chrome": [
        "--driver-binary=$(rlocationpath @linux_chromedriver//:chromedriver)",
        "--browser-binary=$(rlocationpath @linux_chrome//:chrome-linux64/chrome)",
        "--browser-args=--disable-dev-shm-usage",
        "--browser-args=--no-sandbox",
    ],
    "@selenium//common:use_pinned_macos_chrome": [
        "--driver-binary=$(rlocationpath @mac_chromedriver//:chromedriver)",
        "--browser-binary=$(rlocationpath @mac_chrome//:Chrome.app)/Contents/MacOS/Chrome",
    ],
    "//conditions:default": [],
}) + headless_args

edge_args = select({
    "@selenium//common:use_pinned_linux_edge": [
        "--driver-binary=$(rlocationpath @linux_edgedriver//:msedgedriver)",
        "--browser-binary=$(rlocationpath @linux_edge//:opt/microsoft/msedge/microsoft-edge)",
        "--browser-args=--disable-dev-shm-usage",
        "--browser-args=--no-sandbox",
    ],
    "@selenium//common:use_pinned_macos_edge": [
        "--driver-binary=$(rlocationpath @mac_edgedriver//:msedgedriver)",
        "--browser-binary='$(rlocationpath @mac_edge//:Edge.app)/Contents/MacOS/Microsoft Edge'",
    ],
    "//conditions:default": [],
}) + headless_args

firefox_args = select({
    "@selenium//common:use_pinned_linux_firefox": [
        "--driver-binary=$(rlocationpath @linux_geckodriver//:geckodriver)",
        "--browser-binary=$(rlocationpath @linux_firefox//:firefox/firefox)",
    ],
    "@selenium//common:use_pinned_macos_firefox": [
        "--driver-binary=$(rlocationpath @mac_geckodriver//:geckodriver)",
        "--browser-binary=$(rlocationpath @mac_firefox//:Firefox.app)/Contents/MacOS/firefox",
    ],
    "//conditions:default": [],
}) + headless_args

BROWSERS = {
    "chrome": {
        "args": ["--driver=chrome"] + chrome_args,
        "data": chrome_data,
        "tags": COMMON_TAGS + ["chrome"],
    },
    "edge": {
        "args": ["--driver=edge"] + edge_args,
        "data": edge_data,
        "tags": COMMON_TAGS + ["edge"],
    },
    "firefox": {
        "args": ["--driver=firefox"] + firefox_args,
        "data": firefox_data,
        "tags": COMMON_TAGS + ["firefox"],
    },
    "ie": {
        "args": ["--driver=ie"],
        "data": [],
        "tags": COMMON_TAGS + ["ie", "skip-rbe"],
    },
    "safari": {
        "args": ["--driver=safari"],
        "data": [],
        "tags": COMMON_TAGS + ["safari", "skip-rbe"],
    },
}
