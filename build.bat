echo "Begin building WINDOWS"
rd /s/q ".\ElevatorGame\bin\Release\net8.0\win-x64"
dotnet publish ElevatorGame -c Release -r win-x64 -p:ExtraDefineConstants=WINDOWS -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rd /s/q ".\ElevatorGame\bin\Release\net8.0\win-x64\publish"
xcopy /y /s ".\SteamAssets\installscripts\installscript.vdf" ".\ElevatorGame\bin\Release\net8.0\win-x64"

echo "Begin building LINUX"
rd /s/q ".\ElevatorGame\bin\Release\net8.0\linux-x64"
dotnet publish ElevatorGame -c Release -r linux-x64 -p:ExtraDefineConstants=LINUX -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rd /s/q ".\ElevatorGame\bin\Release\net8.0\linux-x64\publish"

echo "Begin building OSX"
rd /s/q ".\ElevatorGame\bin\Release\net8.0\osx-x64"
dotnet publish ElevatorGame -c Release -r osx-x64 -p:ExtraDefineConstants=OSX -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained true
rd /s/q ".\ElevatorGame\bin\Release\net8.0\osx-x64\publish"

pause
