rd /s/q ".\ElevatorGame\bin\Release\net8.0\win-x64"
dotnet publish ElevatorGame -c Release -r win-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishAot=true --self-contained false
xcopy /y /s ".\SteamAssets\installscripts\dotnet-installscript-win.bat" ".\ElevatorGame\bin\Release\net8.0\win-x64\publish"

rd /s/q ".\ElevatorGame\bin\Release\net8.0\linux-x64"
dotnet publish ElevatorGame -c Release -r linux-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishAot=true --self-contained true
rd /s/q ".\ElevatorGame\bin\Release\net8.0\linux-x64\publish"

rd /s/q ".\ElevatorGame\bin\Release\net8.0\osx-x64"
dotnet publish ElevatorGame -c Release -r osx-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false -p:PublishAot=true --self-contained true
rd /s/q ".\ElevatorGame\bin\Release\net8.0\osx-x64\publish"

pause
