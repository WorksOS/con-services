Add-Type -AssemblyName System.IO.Compression.FileSystem

function ZipFiles
{
    param ([Parameter(Mandatory=$true)][string] $zipTempDirectory
			, [Parameter(Mandatory=$true)][string] $assemblyDirectory
            , [Parameter(Mandatory=$true)][string] $zipfileName)
    
    If(Test-Path $zipTempDirectory)
	{
		Write-Host "Removing folder $zipTempDirectory"
		Remove-Item $zipTempDirectory -Force -Recurse
	}
    New-Item -ItemType Directory -Force -Path $zipTempDirectory
    <#
        PS5 syntax
    Compress-Archive -Path $assemblyDirectory -DestinationPath $zipTempDirectory\$zipfileName.zip
    #>

    [System.IO.Compression.ZipFile]::CreateFromDirectory($assemblyDirectory, "$zipTempDirectory\$zipfileName")
}

$zipTempDirectory = ".\_Zip\Temp"
$assemblyDirectory = $($args[0])
$zipfileName = $($args[0])+".zip"

ZipFiles $zipTempDirectory $assemblyDirectory $zipfileName
