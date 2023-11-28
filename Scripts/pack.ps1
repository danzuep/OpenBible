dotnet clean ../BibleApp.sln --configuration Release
dotnet build ../BibleApp.sln --configuration Release

# build to release folder
dotnet publish BibleApp/Bible.App/Bible.App.csproj -c Release -o release --nologo
