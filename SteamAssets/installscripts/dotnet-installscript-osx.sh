#!/bin/sh

sh ./dotnet-install.sh --runtime --version latest --channel 8.0
&& rm ./dotnet-install.sh

# delete self
rm -- "$0"
