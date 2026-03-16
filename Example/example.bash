SCRIPT_DIR="$(cd -- "$(dirname -- "$0")" && pwd)"
cd "$SCRIPT_DIR" || exit 1

# SIMPLEDOC_OUT_DOCKER_CONTAINER_TAG заменить на какой-нибудь свой

SIMPLEDOC_INPUT_PATH=$SCRIPT_DIR/test_ontology_classes.json \
SIMPLEDOC_TITLE="Food ontology documentation" \
SIMPLEDOC_DESCRIPTION="Food / Restaurant / Food Supply Chain ecosystem ontological scheme that includes some diversity" \
SIMPLEDOC_OUT_DOCKER_CONTAINER_TAG="gitea.simpleperson.ru/pub/test-ontology:latest" \
/usr/bin/bash $SCRIPT_DIR/../publish.bash