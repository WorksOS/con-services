RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/VSS.Productivity3D.Filter.WebApi -o ../../artifacts/FilterWebApiNet47 -f net47
7z a FilterWebApiNet47.zip -r ./artifacts/FilterWebApiNet47/
