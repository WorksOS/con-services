dotnet restore --no-cache
dotnet publish  -o ./Artifacts/MockProjectWebApi -f net462 -c Docker
dotnet publish  -o ./Artifacts/MockProjectWebApi_Linux -f netcoreapp1.1 -c Docker
copy appsettings.json Artifacts\MockProjectWebApi\
copy appsettings.json Artifacts\MockProjectWebApi_Linux\
copy Dockerfile Artifacts\MockProjectWebApi\
copy Dockerfile_linux Artifacts\MockProjectWebApi_Linux\Dockerfile

