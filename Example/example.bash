SCRIPT_DIR="$(cd -- "$(dirname -- "$0")" && pwd)"
cd "$SCRIPT_DIR" || exit 1

# SIMPLEDOC_OUT_DOCKER_CONTAINER_TAG заменить на какой-нибудь свой

SIMPLEDOC_INPUT_PATH=$SCRIPT_DIR/cgmes_example.json \
SIMPLEDOC_TITLE="Common Grid Model Exchange Standard (CGMES)" \
SIMPLEDOC_DESCRIPTION="CGMES (Common Grid Model Exchange Standard) is a data exchange standard for power systems developed by ENTSO-E. It enables transmission system operators to share consistent grid models for coordinated planning, analysis, and operation of interconnected electricity networks." \
SIMPLEDOC_OUT_DOCKER_CONTAINER_TAG="gitea.simpleperson.ru/pub/test-ontology:latest" \
/usr/bin/bash $SCRIPT_DIR/../publish.bash