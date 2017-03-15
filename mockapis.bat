dotnet publish ./test/MockProjectWebApi -o ./Artifacts/MockProjectWebApi -f net462 -c Docker
copy ./test/MockProjectWebApi/appsettings.json Artifacts/MockProjectWebApi/
copy ./test/MockProjectWebApi Artifacts/WebApi/

