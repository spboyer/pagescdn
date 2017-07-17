#!/bin/bash -c

source ./_development.sh
rm -rf ./bin/Docker/publish
dotnet clean
dotnet restore ./pagescdn.csproj
dotnet publish ./pagescdn.csproj -c Release -o ./bin/Docker/publish

docker build -t pagescdn .

docker run -d -p 5300:80 pagescdn:latest

open http://localhost:5300