$projectName = "Bible.App"; #(Get-Item $projectFile).BaseName
$target="../${projectName}/${projectName}.csproj";

#dotnet clean $target
#dotnet build $target

# build to release folder
dotnet publish $target -c Release -o release --nologo
