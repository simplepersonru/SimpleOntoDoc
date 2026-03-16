# Необходимо передать параметром SIMPLEDOC_OUT_DOCKER_CONTAINER_TAG
# Например SIMPLEDOC_OUT_DOCKER_CONTAINER_TAG=gitea.mydomain.ru/admin/my-ontology:latest
# Туда пушнется собранный контейнер с nginx

SCRIPT_DIR="$(cd -- "$(dirname -- "$0")" && pwd)"
cd "$SCRIPT_DIR" || exit 1

PORT=53499

docker run --rm -d --name plantuml_for_simple_onto_doc -p $PORT:8080 plantuml/plantuml-server

until curl -sf http://localhost:$PORT > /dev/null; do
  echo "Waiting for plantuml..."
  sleep 2
done

cp $SIMPLEDOC_INPUT_PATH ./out/ontology.json
docker pull gitea.simpleperson.ru/admin/simple-onto-doc:latest

SIMPLEDOC_INPUT_PATH=/out/ontology.json

docker run --rm \
  -v ./out:/out \
  --net=host \
  -e SIMPLEDOC_PLANTUML_URL=http://localhost:$PORT \
  -e SIMPLEDOC_OUTPUT_PATH=/out \
  -e SIMPLEDOC_INPUT_PATH \
  -e SIMPLEDOC_TITLE \
  -e SIMPLEDOC_DESCRIPTION \
  gitea.simpleperson.ru/pub/simple-onto-doc:latest 

docker stop plantuml_for_simple_onto_doc
docker build -t nginx_static_site .
docker tag nginx_static_site $SIMPLEDOC_OUT_DOCKER_CONTAINER_TAG
docker push $SIMPLEDOC_OUT_DOCKER_CONTAINER_TAG