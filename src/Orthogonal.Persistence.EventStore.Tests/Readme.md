docker-compose up

docker inspect -f "{{ .NetworkSettings.IPAddress }}" eventstore-node
docker inspect -f "{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}" e3d8dcc3e5b4