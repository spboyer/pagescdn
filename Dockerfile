FROM microsoft/aspnetcore:2.0.0-preview2
ARG source
WORKDIR /app
EXPOSE 80
COPY ${source:-bin/Docker/publish} .
ENTRYPOINT dotnet pagescdn.dll