#!/bin/bash
# 获取脚本所在目录
SCRIPT_DIR=$(cd "$(dirname "$0")"; pwd)

sed -i 's/\r$//' bash/docker_entrypoint.sh
sed -i 's/\r$//' bash/auto_xvfb.sh
echo "build dockerfile"
docker build -t msspeechserver:V1.0.3 "$SCRIPT_DIR/"


