CDP_VERSIONS = [
    "v134",
    "v132",
    "v133",
]

CDP_DEPS = ["//java/src/org/openqa/selenium/devtools/%s" % v for v in CDP_VERSIONS]
