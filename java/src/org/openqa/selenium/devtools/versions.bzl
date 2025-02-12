CDP_VERSIONS = [
    "v85",  # Required by Firefox
    "v131",
    "v132",
    "v130",
]

CDP_DEPS = ["//java/src/org/openqa/selenium/devtools/%s" % v for v in CDP_VERSIONS]
