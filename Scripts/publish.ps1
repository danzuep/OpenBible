# Set-ExecutionPolicy -ExecutionPolicy RemoteSigned

if ([string]::IsNullOrEmpty($Env:AndroidSigningPassword))
{
	Write-Host "Keystore secret password not set, exiting.";
	return;
}

$projectName = "Bible";
$buildVersion = "0.1.0";
$dotnetTarget = "net8.0";
$configuration = "Release";
$projectDirectory = (Get-Item (Split-Path -Path $PSScriptRoot -Parent));
$projectFolder = $projectDirectory.FullName;
$projectAppName = "${projectName}.App";
$projectFile = "${projectFolder}/${projectAppName}/${projectAppName}.csproj";

# restore the base project dependencies
Set-Location -Path "${projectFolder}";
dotnet clean;
dotnet restore "${projectFile}";
if (-not $?) {
    Write-Host "Project file not found.";
    Exit 1;
}

$keystoreFile = "android.keystore";
$androidSigningAlias = "android-key";
$targetFramework = "${dotnetTarget}-android";
$publishOutputFolder = "publish";
$publishPath = [IO.Path]::Combine($projectFolder, $publishOutputFolder);
$kestoreFolder = [IO.Path]::Combine($projectFolder, "release");
# $kestoreFolder = [IO.Path]::Combine($Env:LOCALAPPDATA, "Android"); # %LocalAppData%
if (-Not (Test-Path -Path "${kestoreFolder}" -PathType Container))
{
    New-Item -ItemType Directory -Path "${kestoreFolder}";
}
$kestorePath = [IO.Path]::Combine($kestoreFolder, $keystoreFile);
if (-Not (Test-Path -Path "${kestorePath}" -PathType Leaf))
{
    keytool -genkeypair -v -keystore "${kestorePath}" -alias "${androidSigningAlias}" -keyalg RSA -keysize 2048 -validity 10000;
    keytool -list -keystore "${kestorePath}";
}

# dotnet workload install maui-android;

# https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-8.0
dotnet publish $projectFile -c $configuration -f $targetFramework /p:Version=$buildVersion --no-restore --nologo;
if (-not $?) {
    Write-Host "Project failed to publish for Android.";
    Exit 1;
}

# https://learn.microsoft.com/en-us/dotnet/maui/windows/deployment/publish-unpackaged-cli?view=net-maui-8.0
$runtimeIdentifier = "win10-x64";
$targetFramework = "${dotnetTarget}-windows10.0.19041.0";
dotnet publish $projectFile -c $configuration -f $targetFramework -p:Version=$buildVersion -p:RuntimeIdentifierOverride=$runtimeIdentifier -p:WindowsPackageType=None --no-restore --nologo;
if (-not $?) {
    Write-Host "Project failed to publish for Windows.";
    Exit 1;
}

Write-Host "Project published to ${publishPath}.";