$projectDirectory = (Get-Item (Split-Path -Path $PSScriptRoot -Parent));
$projectFolder = $projectDirectory.FullName;
$projectFile = "${projectFolder}/${projectAppName}/${projectAppName}.csproj";
$publishPath = [IO.Path]::Combine($projectFolder, "publish", "windows");

. "${projectFolder}/Scripts/version.ps1";

Write-Host "Preparing to publish project to ${publishPath}.";

# restore the base project dependencies
Set-Location -Path "${projectFolder}";
# dotnet clean;

# https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/publish-unpackaged-cli?view=net-maui-8.0
if (-not (dotnet workload list | Select-String -Pattern 'maui-windows')) {
    Write-Host "MAUI Windows workload not found. Installing..."
    dotnet workload install maui-windows;
} else {
    Write-Host "MAUI Windows workload is already installed."
}
$targetFramework = "${dotnetTarget}-windows10.0.19041.0";
dotnet publish "${projectFile}" -c $configuration -f $targetFramework /p:Version=$buildVersion /p:WindowsPackageType=None -v minimal --nologo;
if (-not $?) {
    Write-Host "Project failed to publish for Windows.";
    Exit 1;
}

Write-Host "Project published to ${publishPath}.";