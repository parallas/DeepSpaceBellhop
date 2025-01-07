chmod +x ./zip.sh

echo "Begin building OSX"
rm -rf "./ElevatorGame/bin/Release/net8.0/osx-x64"
dotnet publish ElevatorGame -c Release -r osx-x64 -p:ExtraDefineConstants=OSX -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rm -rf "./ElevatorGame/bin/Release/net8.0/osx-x64/publish"
sh ./zip.sh osx

echo "Begin building LINUX"
rm -rf "./ElevatorGame/bin/Release/net8.0/linux-x64"
dotnet publish ElevatorGame -c Release -r linux-x64 -p:ExtraDefineConstants=LINUX -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rm -rf "./ElevatorGame/bin/Release/net8.0/linux-x64/publish"
sh ./zip.sh linux

echo "Begin building WINDOWS"
rm -rf "./ElevatorGame/bin/Release/net8.0/win-x64"
dotnet publish ElevatorGame -c Release -r win-x64 -p:ExtraDefineConstants=WINDOWS -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rm -rf "./ElevatorGame/bin/Release/net8.0/win-x64/publish"
cp "./SteamAssets/installscripts/installscript.vdf" "./ElevatorGame/bin/Release/net8.0/win-x64"
sh ./zip.sh win
