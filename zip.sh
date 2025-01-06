_oldDir="$CWD"

cd "./ElevatorGame/bin/Release/net8.0/$1-x64"
zip "$1.zip" .* *

cd "$_oldDir"
