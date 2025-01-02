powershell ./dotnet-install.ps1 --runtime --version latest --channel 8.0
&& del ./dotnet-install.ps1

@REM delete self
(goto) 2>nul & del "%~f0"
