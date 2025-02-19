$projectFolder = $(Get-Item (Split-Path -Path $PSScriptRoot -Parent)).FullName;

. "${projectFolder}/Scripts/_version.ps1";

$publishPath = [IO.Path]::Combine($publishPath, "windows");

Write-Host "Preparing to publish project to ${publishPath}.";

# restore the base project dependencies
Set-Location -Path "${projectFolder}";
dotnet clean;
dotnet restore "${projectFile}" #/p:PublishReadyToRun=true;
if (-not $?) {
    Write-Host "Project file not found.";
    Exit 1;
}

# https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/publish-unpackaged-cli?view=net-maui-8.0
$workloadName = 'maui-windows';
if (-not (dotnet workload list | Select-String -Pattern $workloadName)) {
    Write-Host "MAUI Windows workload not found. Installing..."
    dotnet workload install $workloadName;
} else {
    Write-Host "MAUI Windows workload is already installed."
}
# $targetRuntime = "win10-x64"; # /p:RuntimeIdentifierOverride=$targetRuntime
$targetFramework = "${dotnetTarget}-windows10.0.19041.0";
dotnet publish $projectFile -c $configuration -f $targetFramework /p:Version=$buildVersion -v minimal --no-restore --nologo;
if (-not $?) {
    Write-Host "Project failed to publish for Windows.";
    dotnet workload update $workloadName;
    Exit 1;
}

Write-Host "Project published to ${publishPath}.";