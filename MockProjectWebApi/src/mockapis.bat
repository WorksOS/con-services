dotnet restore --no-cache
dotnet publish  -o ./Artifacts/MockProjectWebApi -f net462 -c Docker
copy appsettings.json Artifacts\MockProjectWebApi\
copy Dockerfile Artifacts\MockProjectWebApi\

