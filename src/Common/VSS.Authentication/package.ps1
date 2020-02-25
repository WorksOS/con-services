param(
	[string]$Version = $(throw "-Version required"),
	[string]$NuGetDestination = "http://10.97.96.144/",
	[string]$NuGetApiKey = "qATxVIHO5rIPF3K7",
	[boolean]$DryRun = $false
)
$ErrorActionPreference = "Stop" # http://stackoverflow.com/questions/9948517/how-to-stop-a-powershell-script-on-the-first-error

git checkout master
if (-Not $?) { exit 1 }
git pull
if (-Not $?) { exit 1 }

(Get-Content -Path .\VSS.Authentication.JWT\VSS.Authentication.JWT.csproj) -replace "\<Version\>.*\<\/Version\>", "<Version>$Version</Version>" | Set-Content .\VSS.Authentication.JWT\VSS.Authentication.JWT.csproj

if (-not $DryRun) {
	git commit -am "$Version"
	if (-Not $?) { exit 1 }
	git tag "v$Version"
	if (-Not $?) { exit 1 }
}

Push-Location VSS.Authentication.JWT

Remove-Item -Recurse -Force .\bin\release\

dotnet restore VSS.Authentication.JWT.csproj
if (-Not $?) { exit 1 }

dotnet pack VSS.Authentication.JWT.csproj -c Release
if (-Not $?) { exit 1 }

if (-not $DryRun) {
	Write-Output "Pushing to NuGet"
	NuGet.exe push .\bin\release\*.nupkg -Source $NuGetDestination -ApiKey $NuGetApiKey -Verbosity detailed
	if (-Not $?) { exit 1 }
}

Pop-Location

if (-not $DryRun) {
	Write-Output "Pushing to git"
	git push --tags
	if (-Not $?) { exit 1 }
}
