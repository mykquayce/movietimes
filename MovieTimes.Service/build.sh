#! /bin/bash

docker image ls | tail --line +2 | grep 'mcr.microsoft.com/dotnet/core/' | grep --invert-match '<none>' | awk '{system("docker pull " $1 ":" $2)}'

if [ $? -ne 0 ]; then
    exit 1
fi

docker build --tag eassbhhtgu/movietimes-service:latest .

if [ $? -ne 0 ]; then
    exit 1
fi

docker push eassbhhtgu/movietimes-service:latest
