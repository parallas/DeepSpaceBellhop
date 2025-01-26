echo "===== Begin building OSX-arm64 ====="
rd /s/q "./ElevatorGame/bin/Release/net8.0/osx-arm64"
dotnet publish ElevatorGame -c Release -r osx-arm64 -p:ExtraDefineConstants=OSX -p:UseSteamworks=false -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rd /s/q "./ElevatorGame/bin/Release/net8.0/osx-arm64/publish"
sh ./zip.sh osx-arm64

echo "===== Begin building LINUX-x64 ====="
rd /s/q "./ElevatorGame/bin/Release/net8.0/linux-x64"
dotnet publish ElevatorGame -c Release -r linux-x64 -p:ExtraDefineConstants=LINUX -p:UseSteamworks=false -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rd /s/q "./ElevatorGame/bin/Release/net8.0/linux-x64/publish"
sh ./zip.sh linux-x64

echo "===== Begin building WINDOWS-x64 ====="
rd /s/q "./ElevatorGame/bin/Release/net8.0/win-x64"
dotnet publish ElevatorGame -c Release -r win-x64 -p:ExtraDefineConstants=WINDOWS -p:UseSteamworks=false -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rd /s/q "./ElevatorGame/bin/Release/net8.0/win-x64/publish"
xcopy /y /s ".\SteamAssets\installscripts\installscript.vdf" ".\ElevatorGame\bin\Release\net8.0\win-x64"
sh ./zip.sh win-x64

pause
