dotnet restore --no-cache
dotnet clean src.csproj
dotnet publish src.csproj  -o ./Artifacts/MockProjectWebApi -f net47 -c Docker
copy appsettings.json Artifacts\MockProjectWebApi\
copy Dockerfile Artifacts\MockProjectWebApi\

