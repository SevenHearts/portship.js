#!/usr/bin/env bash
set -euo pipefail
"${1}" "${2}" "${3}" "${4}" "/dev/stdout" | convert - "${5}"
