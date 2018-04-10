Invoke-Expression "dotnet restore ./../VSS.Productivity3D.Service.sln --no-cache"
Invoke-Expression "dotnet build ./../VSS.Productivity3D.Service.sln"

Set-Location -Path ./Tools/Minicover

# Instrument assemblies inside 'UnitTests' folder to detect hits for source files inside 'src' folder
Invoke-Expression "dotnet minicover instrument --workdir ../../../ --sources test/UnitTests/WebApiTests/**/*.cs --assemblies test/UnitTests/WebApiTests/**/*.cs"

# Reset hits count in case minicover was run for this project
Invoke-Expression "dotnet minicover reset"

Set-Location ../..

Invoke-Expression "dotnet test --no-build C:\Repos\VSS.Productivity3D.Service\test\UnitTests\WebApiTests\VSS.Productivity3D.WebApi.Tests.csproj"

Set-Location -Path ./Tools/Minicover

# Uninstrument assemblies, it's important if you're going to publish or deploy build outputs
Invoke-Expression "dotnet minicover uninstrument --workdir ../../../"



Set-Location ../..
Exit



# dotnet minicover reset

# cd ../..

# for project in UnitTests/UnitTests.csproj; do dotnet test --no-build $project; done

# cd tools
# cd minicover

# # Uninstrument assemblies, it's important if you're going to publish or deploy build outputs
# dotnet minicover uninstrument --workdir ../../

# # Create html reports inside folder coverage-html
# dotnet minicover htmlreport --workdir ../../ --threshold 90

# # Print console report
# # This command returns failure if the coverage is lower than the threshold
# dotnet minicover report --workdir ../ --threshold 90

# cd ..