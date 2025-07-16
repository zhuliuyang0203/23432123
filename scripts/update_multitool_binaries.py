#!/usr/bin/env python

"""
This script updates the version of tool binaries defined in a Bazel rules_multitool lockfile.
If the tool has binaries hosted in a public GitHub repo's Release assets, it will update the
lockfile's URL and hash to the latest versions, otherwise it will skip it.

See: https://github.com/theoremlp/rules_multitool

Requires:
  - github module (pip install PyGithub)

```
usage: update_multitool_binaries.py [-h] [--file LOCKFILE_PATH]

options:
  -h, --help            show this help message and exit
  --file LOCKFILE_PATH  path to multitool lockfile (defaults to 'multitool.lock.json' in current directory)
```
"""

import argparse
import hashlib
import json
import os
import re
import urllib.request

try:
    from github import Github
except ModuleNotFoundError:
    exit("requires github module (run: pip install PyGithub)")


def run(lockfile_path):
    with open(lockfile_path) as f:
        data = json.load(f)

    for tool in [tool for tool in data if tool != "$schema"]:
        version = re.search(f"download/(.*?)/{tool}", data[tool]["binaries"][0]["url"])[
            1
        ]
        match = re.search(
            f"github.com/(.*?)/releases", data[tool]["binaries"][0]["url"]
        )
        if match:
            user_repo = match[1]
        else:
            continue
        try:
            new_version = Github().get_repo(user_repo).get_releases()[0].title
        except Exception:
            continue
        if new_version != version:
            print(f"found new version of '{tool}': {new_version}")
            for binary in data[tool]["binaries"]:
                new_url = binary["url"].replace(version, new_version)
                try:
                    with urllib.request.urlopen(new_url) as response:
                        sha256_hash = hashlib.sha256()
                        sha256_hash.update(response.read())
                        new_hash = sha256_hash.hexdigest()
                    binary["url"] = new_url
                    binary["sha256"] = new_hash
                except Exception:
                    continue

    with open(lockfile_path, "w") as f:
        json.dump(data, f, indent=2)

    print(f"\ngenerated new '{lockfile_path}' with updated urls and hashes")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--file",
        dest="lockfile_path",
        default=os.path.join(os.getcwd(), "multitool.lock.json"),
        help="path to multitool lockfile (defaults to 'multitool.lock.json' in current directory)",
    )
    args = parser.parse_args()
    run(args.lockfile_path)


if __name__ == "__main__":
    main()
