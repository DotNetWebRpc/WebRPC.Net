dotnet build ..\src\abstractions\abstractions.csproj -c Release
dotnet build ..\src\client\client.csproj -c Release
dotnet build ..\src\host\host.csproj -c Release

dotnet pack ..\src\abstractions\abstractions.csproj -o ..\..\..\nuget /p:PackageVersion=1.0.25
dotnet pack ..\src\client\client.csproj -o ..\..\..\nuget /p:PackageVersion=1.0.25
dotnet pack ..\src\host\host.csproj -o ..\..\..\nuget /p:PackageVersion=1.0.25

pause