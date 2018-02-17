#!/bin/bash
(cd ./test/UnitTests/VSS.Productivity3D.Scheduler.Tests && dotnet test VSS.Productivity3D.Scheduler.Tests.csproj -f netcoreapp1.1 )
if [[ $rc != 0 ]]; then exit $rc; fi