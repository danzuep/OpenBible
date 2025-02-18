# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned
# @('bin','obj') | ForEach-Object { Get-ChildItem -Recurse -Directory -Filter $_ -Force | ForEach-Object { Write-Host "Deleting folder: $($_.FullName)"; Remove-Item $($_.FullName) -Recurse -Force -ErrorAction SilentlyContinue } }; # dotnet nuget locals all --clear

$projectName = "Bible";
$buildVersion = "0.1.3";
$dotnetTarget = "net8.0";
$configuration = "Release";
$publishOutputFolder = "publish";
$projectAppName = "${projectName}.App";
$solutionFileName = "${projectAppName}.sln";
$projectFileName = [IO.Path]::Combine($projectAppName, "${projectAppName}.csproj");
$solutionFile = [IO.Path]::Combine($projectFolder, $solutionFileName);
$projectFile = [IO.Path]::Combine($projectFolder, $projectFileName);
$publishPath = [IO.Path]::Combine($projectFolder, $publishOutputFolder);
