# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned
# @('bin','obj') | ForEach-Object { Get-ChildItem -Recurse -Directory -Filter $_ -Force | ForEach-Object { Write-Host "Deleting folder: $($_.FullName)"; Remove-Item $($_.FullName) -Recurse -Force -ErrorAction SilentlyContinue } }; # dotnet nuget locals all --clear

$projectName = "Bible";
$buildVersion = "0.2.0";
$dotnetTarget = "net9.0";
$configuration = "Release";
$publishOutputFolder = "publish";
$projectAppName = "${projectName}.App";
$solutionFileName = "${projectAppName}.sln";
$projectFileName = [IO.Path]::Combine($projectAppName, "${projectAppName}.csproj");
$solutionFile = [IO.Path]::Combine($projectFolder, $solutionFileName);
$projectFile = [IO.Path]::Combine($projectFolder, $projectFileName);
$publishPath = [IO.Path]::Combine($projectFolder, $publishOutputFolder);
