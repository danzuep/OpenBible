if ([string]::IsNullOrEmpty($Env:AndroidSigningPassword))
{
	Write-Host "Keystore secret password not set, exiting.";
	return;
}
if ([string]::IsNullOrEmpty($projectFolder))
{
    if ([string]::IsNullOrEmpty($Env:SourceRoot))
    {
        Write-Host "Root source folder not set, using default.";
        $Env:SourceRoot = "${Env:SYSTEMDRIVE}\Source";
    }
	Write-Host "Project folder not set, using default.";
    $projectFolder = [IO.Path]::Combine($Env:SourceRoot, "GitHub", "BibleApp");
}
Set-Location -Path $projectFolder

$projectName = "Bible.App"; #(Get-Item $projectFile).BaseName
$projectFile="${projectFolder}/${projectName}/${projectName}.csproj";
$configuration="Release";

# restore the base project dependencies
dotnet restore $projectFile

$appName="BibleApp";
# $keystoreJks="${appName}-keystore.jks";
$keystoreFile="${appName}.keystore";
$androidSigningAlias="${appName}-key";
$androidPackageFormats="apk" # "aab;apk"
$buildVersion="0.1.0"
$dotnetTarget="net8.0";
$androidTarget="${dotnetTarget}-android";
$releaseFolder="release";
$publishOutputFolder="publish";

$kestoreFolder = [IO.Path]::Combine($projectFolder, $releaseFolder); # $Env:LOCALAPPDATA
if (-Not (Test-Path -Path $kestoreFolder -PathType Container))
{
    New-Item -ItemType Directory -Path $kestoreFolder
}
$kestorePath = [IO.Path]::Combine($kestoreFolder, $keystoreFile);
if (-Not (Test-Path -Path $kestorePath -PathType Leaf))
{
    # https://learn.microsoft.com/en-us/dotnet/maui/android/deployment/publish-cli?view=net-maui-8.0
    keytool -genkeypair -v -keystore $kestorePath -alias ${androidSigningAlias} -keyalg RSA -keysize 2048 -validity 10000
    keytool -list -keystore $kestorePath
}

dotnet publish $projectFile -c $configuration --framework $androidTarget /p:Version=$buildVersion /p:AndroidPackageFormats=$androidPackageFormats /p:AndroidKeyStore=true /p:AndroidSigningKeyStore="${kestorePath}" /p:AndroidSigningKeyAlias="${androidSigningAlias}" /p:AndroidSigningKeyPass="${Env:AndroidSigningPassword}" /p:AndroidSigningStorePass="${Env:AndroidSigningPassword}" -o "${publishOutputFolder}" --no-restore --nologo
