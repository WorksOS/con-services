dotnet restore --no-cache
dotnet clean
dotnet publish src.csproj  -o ./Artifacts/MockProjectWebApi -f net462 -c Docker
copy appsettings.json Artifacts\MockProjectWebApi\
copy Dockerfile Artifacts\MockProjectWebApi\

