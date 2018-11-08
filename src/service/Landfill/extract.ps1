$targetFolder ='C:\Landfill';
[System.Reflection.Assembly]::LoadWithPartialName('System.IO.Compression.FileSystem');
[System.IO.Compression.ZipFile]::ExtractToDirectory($env:cliqrIISAppPkg, $targetFolder);