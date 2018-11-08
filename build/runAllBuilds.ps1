
function runBuild ($filePath) {
    $filename = [System.IO.Path]::GetFileNameWithoutExtension($filePath)
    $logFile = $filename + ".log"
    Write-Host "Building: " $filePath
    & dotnet build $filePath > $logFile
    $errCode = $LASTEXITCODE
    $color = $host.ui.RawUI.ForegroundColor
    
    if($errCode -eq 0) {
        $host.ui.RawUI.ForegroundColor = "Green"
        Write-Host $filename "Build passed"
    }
    else {
        $host.ui.RawUI.ForegroundColor = "Red"
        Write-Host $filename "Failed to build, Error:" $errCode
    }
    $host.ui.RawUI.ForegroundColor = $color
    return $errCode
}

[Boolean]$hasErrors = $false
Get-ChildItem -Path ../src -Filter "*.MonoRepo.sln" -Recurse | ForEach-Object {
    $errCode = runBuild($_.FullName)
    Write-Host ""
    if($errCode -ne 0) {
        $hasErrors = $true
    }
}

if($hasErrors) {
    Write-Host There are errors in the builds
    exit 1
} else {
    Write-Host All builds passsed
    exit 0
}
