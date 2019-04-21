#! /bin/bash

docker image ls | tail --line +2 | grep 'mcr.microsoft.com/dotnet/core' | awk '{system("docker pull " $1 ":" $2)}'

if [ $? -ne 0 ]; then
    exit 1
fi

docker build --tag eassbhhtgu/movietimes-cineworldservice:latest .

if [ $? -ne 0 ]; then
    exit 1
fi

docker image ls | tail --line +2 | grep none | awk '{system("docker rmi " $3)}'

if [ $? -ne 0 ]; then
    exit 1
fi

docker push eassbhhtgu/movietimes-cineworldservice:latest
