#!/usr/bin/env bash

# This script updates the development dependendencies used for the Python bindings.
#
# When you run it, it will
# - create and activate a temporary virtual env
# - install the current package dependencies
# - upgrade the package dependencies to the latest versions available on PyPI
# - generate new requirements.txt and requirements_lock.txt files
# - deactivate and remove the temporary virtual env
#
# After running this, create a new Pull Request with the changes. You should
# also manually check package dependency versions in `py/pyproject.toml` and
# `py/tox.ini` and update those if needed.


set -e


REQUIREMENTS_FILE="./py/requirements.txt"
VENV="./temp_virtualenv"


cd "$(git rev-parse --show-toplevel)"

if [[ ! -f "${REQUIREMENTS_FILE}" ]]; then
    echo "can't find: ${REQUIREMENTS_FILE}"
    exit 1
fi

if [[ -d "${VENV}" ]]; then
    echo "${VENV} already exists"
    exit 1
fi

echo "creating virtual env: ${VENV}"
python3 -m venv "${VENV}"

echo "activating virtual env"
source "${VENV}/bin/activate"

echo "upgrading pip"
python -m pip install --upgrade pip > /dev/null

echo "installing dev dependencies from: ${REQUIREMENTS_FILE}"
pip install -r "${REQUIREMENTS_FILE}" > /dev/null

echo "upgrading outdated dependencies ..."
echo
pip list --outdated | while read -r line; do
    if [[ ! "${line}" =~ "Version Latest" && ! "${line}" =~ "----" ]]; then
        read -ra fields <<< "${line}"
        echo "upgrading ${fields[0]} from ${fields[1]} to ${fields[2]}"
        pip install --upgrade "${fields[0]}==${fields[2]}" > /dev/null
    fi
done

echo
echo "generating new ${REQUIREMENTS_FILE}"
pip freeze > "${REQUIREMENTS_FILE}"

echo "generating new lock file"
#bazel run //py:requirements.update

echo
echo "deleting virtual env: ${VENV}"
deactivate
rm -rf "${VENV}"

echo
git status

echo
echo "done!"
