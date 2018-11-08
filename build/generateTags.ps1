param([String]$repository=".") 

$debug = $False

function getChangedFiles {
    # https://stackoverflow.com/questions/424071/how-to-list-all-the-files-in-a-commit
    $files = & "git" "diff-tree" "--no-commit-id" "--name-only" "-r" "HEAD"
    return $files
}

function getTagsForDirectories($directories) {
    $path = [io.path]::Combine($PSScriptRoot, "..", "tools", "versiontool", "VSS.VersionTool.dll")
    $a = @()
    foreach($dir in $directories) {
        $a += ("-d") # Paramters must be added as a list item, rather than building a string
        $a += ($dir) # so add the -d <directory> seperately
    }

    if($debug) {
        Write-Host "dotnet" $path "tags" "-r" "--search-up-directory" "--references-directory" $repository $a
    }
    # We want to search up a directory, as the directory may actually point to a directory under the project (i.e models/).
    # Search up, will move up the directory path until a project is found
    # We also want to search for all references inside the repository
    return &"dotnet" $path "tags" "-r" "--search-up-directory" "--references-directory" $repository $a
}

if($debug) {
    Write-Host "Moving to directory" $dotepository
}
Set-Location -Path $repository

$directories = @()
# Get all the changed files in the HEAD of the repo
$files = getChangedFiles

#Find all unique directories (no point searching a directory more than once)
foreach($line in $files) {
    $dir = [System.IO.Path]::GetDirectoryName($line)

    If ($directories -notcontains $dir -and -not ([string]::IsNullOrEmpty($dir))) 
    {
        $directories += $dir
    }
}

# Find all tags in the directories using the version tool
foreach($result in getTagsForDirectories($directories)) {
    Write-Host $result
}
