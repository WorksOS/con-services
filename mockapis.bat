dotnet publish ./test/UnitTests/MockProjectWebApi -o ./Artifacts/WebApi -f net462 -c Docker
copy ./test/UnitTests/MockProjectWebApi/appsettings.json Artifacts/WebApi/
copy ./test/UnitTests/MockProjectWebApi Artifacts/WebApi/

