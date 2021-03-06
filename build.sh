#! /bin/bash

docker secret ls | tail --line +2 | grep 'MySqlCineworldPassword'

if [ $? -ne 0 ]; then
    openssl rand -base64 201 | sed 's/[^0-9A-Za-z]//g' | sed -z 's/\n//g' | docker secret create MySqlCineworldPassword -
fi

docker pull jaegertracing/all-in-one:latest
docker pull mariadb:latest
docker pull eassbhhtgu/movietimes-api:latest
docker pull eassbhhtgu/movietimes-service:latest

if [ $? -ne 0 ]; then
    exit 1
fi

docker stack ls | tail --line +2 | grep 'movietimes'

if [ $? -ne 0 ]; then
    docker stack deploy --compose-file docker-compose.yml movietimes
else
    docker service ls | tail --line +2 | grep 'movietimes_jaeger'           | awk '{system("docker service update --image " $5 " " $2)}'
    docker service ls | tail --line +2 | grep 'movietimes_mariadb'          | awk '{system("docker service update --image " $5 " " $2)}'
    docker service ls | tail --line +2 | grep 'movietimes_api'              | awk '{system("docker service update --image " $5 " " $2)}'
    docker service ls | tail --line +2 | grep 'movietimes_service'          | awk '{system("docker service update --image " $5 " " $2)}'
fi

if [ $? -ne 0 ]; then
    exit 1
fi

docker container ls -a | tail --line +2 | grep 'jaegertracing:latest'                          | grep 'Exited (0)' | awk '{system("docker rm " $1)}'
docker container ls -a | tail --line +2 | grep 'mariadb:latest'                                | grep 'Exited (0)' | awk '{system("docker rm " $1)}'
docker container ls -a | tail --line +2 | grep 'eassbhhtgu/movietimes-api:latest'              | grep 'Exited (0)' | awk '{system("docker rm " $1)}'
docker container ls -a | tail --line +2 | grep 'eassbhhtgu/movietimes-service:latest'          | grep 'Exited (0)' | awk '{system("docker rm " $1)}'

if [ $? -ne 0 ]; then
    exit 1
fi

docker image ls | tail --line +2 | grep '<none>' | awk '{system("docker rmi " $3)}'
