rm -rf "./ElevatorGame/bin/Release/net8.0/osx-x64"
dotnet publish ElevatorGame -c Release -r osx-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rm -rf "./ElevatorGame/bin/Release/net8.0/osx-x64/publish"

rm -rf "./ElevatorGame/bin/Release/net8.0/linux-x64"
dotnet publish ElevatorGame -c Release -r linux-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rm -rf "./ElevatorGame/bin/Release/net8.0/linux-x64/publish"

rm -rf "./ElevatorGame/bin/Release/net8.0/win-x64"
dotnet publish ElevatorGame -c Release -r win-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained false
cp "./SteamAssets/installscripts/dotnet-installscript-win.bat" "./ElevatorGame/bin/Release/net8.0/win-x64/publish/"

