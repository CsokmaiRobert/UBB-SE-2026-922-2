$ErrorActionPreference = "Stop"
$solutionPath = "../../BoardRentAndProperty.sln"
dotnet restore $solutionPath
dotnet clean $solutionPath
if (Test-Path "../bin") { rm "../bin" -Recurse -Force }
if (Test-Path "../obj")   { rm "../obj"   -Recurse -Force }
dotnet build $solutionPath -c Debug --force -p:Platform=x64
$src = "../bin/x64/Debug/net8.0-windows10.0.19041.0/win-x64"

cp $src "../../demo-builds/user1" -Recurse -Force
cp $src "../../demo-builds/user2" -Recurse -Force
Start-Process "dotnet" -ArgumentList "run --project ../../NotificationServer/NotificationServer.csproj -c Debug -p:Platform=x64"
Start-Sleep -Seconds 2
Start-Process "../../demo-builds/user1/win-x64/BoardRentAndProperty.exe" -ArgumentList "1"
Start-Process "../../demo-builds/user2/win-x64/BoardRentAndProperty.exe" -ArgumentList "2"
