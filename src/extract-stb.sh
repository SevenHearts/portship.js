#!/usr/bin/env bash
set -euo pipefail
"${1}" "${2}" "${3}" "${4}" "/dev/stdout" | node "$(dirname "$0")/convert-stb.mjs" "${5}"
