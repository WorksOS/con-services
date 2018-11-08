Get-ChildItem -Path ../src -Filter "*.Local.sln" -Recurse | % {
    
    $newfile = $_.FullName -replace ".Local.sln", ".MonoRepo.sln"
    if(![System.IO.File]::Exists($newfile)){
        # file with path $path doesn't exist
        Write-Host "New to copy " $newfile
        & Copy-Item $_.FullName $newfile
    }
    else {
        Write-Host "Already have " $newfile
    }
}