# Prereqs.
- FMOD Core and FMOD Studio binaries, version 2.02.25 (you can download them from the "Engine" section of the FMOD download page), placed inside of `Engine/`.
- Linux & MacOS users need a valid 64-bit Wine prefix (the WoW64 version is technically optional but recommended), with d3dcompiler_47.dll and the .NET 8.0 sdk installed on it.
- If you choose to use the included build script, Linux users need to have the `zip` program installed.
- These [virtue pixel font](https://chevyray.itch.io/pixel-font-virtue) ttf files: `virtue.ttf`, `virtue_italic.ttf`, and `virtue_narrow_bold.ttf`, placed inside of `ElevatorGame/Content/fonts/`.

# Per-OS directions
- Linux/MacOS (Unix)
  ```shell
  git clone https://github.com/parallas/DeepSpaceBellhop.git
  cd DeepSpaceBellhop
  sh ./build.sh
  ```
- Windows
  ```batch
  git clone https://github.com/parallas/DeepSpaceBellhop.git
  cd DeepSpaceBellhop
  ./build.bat
  ```

# Running
run the game with the command line argument `--no-steam`
