_oldDir="$CWD"

cd "./ElevatorGame/bin/Release/net8.0/$1-x64"
rm "../$1.zip"
zip -r "../$1.zip" *

cd "$_oldDir"
