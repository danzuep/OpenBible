if ([string]::IsNullOrEmpty($Env:AndroidSigningPassword))
{
	Write-Host "Keystore secret password not set, exiting.";
	return;
}

$projectDirectory = (Get-Item (Split-Path -Path $PSScriptRoot -Parent));
$projectFolder = $projectDirectory.FullName;
$projectFile = "${projectFolder}/${projectAppName}/${projectAppName}.csproj";
$publishOutputFolder = "publish";
$publishPath = [IO.Path]::Combine($projectFolder, $publishOutputFolder);

. "${projectFolder}/Scripts/version.ps1";

Write-Host "Preparing to publish project to ${publishPath}.";

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

# https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-8.0
if (-not (dotnet workload list | Select-String -Pattern 'maui-android')) {
    Write-Host "MAUI Android workload not found. Installing..."
    dotnet workload install maui-android;
} else {
    Write-Host "MAUI Android workload is already installed."
}

dotnet publish $projectFile -c $configuration -f $targetFramework -p:Version=$buildVersion -v minimal --no-restore --nologo;
if (-not $?) {
    Write-Host "Project failed to publish for Android.";
    Exit 1;
}
