# Deep Space Bellhop
Transport strange and unfamiliar characters between floors as an elevator operator.

This file will usually be packaged with each copy of the game, located in the same folder as the "DeepSpaceBellhop.dll" file.

### CONTROLS
Mouse controls are supported for every action besides moving the elevator. The mouse cursor will change shape depending on context to communicate what action Left Click will do.

The Elevator movement inputs can be held down to move several floors consecutively.

Pause:
Escape | C | Start (gamepad)

Go Back:
X | Escape | B (gamepad)

Confirm / Fast-Forward Dialog:
Z | Space | Enter | A (gamepad) | Left Click (mouse)

Move Elevator up:
Up | W | Left Stick Up (gamepad) | Directional Pad Up (gamepad)

Move Elevator down:
Down | S | Left Stick Down (gamepad) | Directional Pad Down (gamepad)

Open Phone screen / Close Tickets screen:
Right | D | Left Stick Right (gamepad) | Directional Pad Right (gamepad)

Open Tickets screen / Close Phone screen:
Left | A | Left Stick Left (gamepad) | Directional Pad Left (gamepad)

(Phone screen) Scroll up:
Up | W | Left Stick Up (gamepad) | Directional Pad Up (gamepad) | Scroll Wheel Up (mouse)

(Phone screen) Scroll down:
Down | S | Left Stick Down (gamepad) | Directional Pad Down (gamepad) | Scroll Wheel Down (mouse)

When compiled with the DEBUG flag, additional keyboard-exclusive inputs are available:

  (note: official releases of the game will not be compiled with DEBUG enabled, so you will not be able to access these functions on those builds)

  Open the interactive debug menu: F3

  Advance to next day: Y

  Prevent additional characters from spawning (and promptly enter closing time): T

### PACKAGES, TOOLS, AND ATTRIBUTION
Runtime: .NET 8.0, x64 only

Audio Engine: FMOD Studio by Firelight Technologies Pty Ltd.

(uses a fork of FmodForFoxes to interface with FMOD: [PixelDough/FmodForFoxes](https://github.com/PixelDough/FmodForFoxes/tree/mac-support))

Steam integration: Steamworks SDK v157

(uses a fork of Facepunch.Steamworks to interface with Steamworks SDK: [tmaster-terrarian/Facepunch.Steamworks](https://github.com/tmaster-terrarian/Facepunch.Steamworks))

This repository includes a significant (although heavily modified) portion of the Coroutines system that was initially created by [ChevyRay](https://github.com/ChevyRay). Detailed attribution is embedded in the file: [/Engine/Coroutines.cs](https://raw.githubusercontent.com/PixelDough/ElevatorGame/refs/heads/main/Engine/Coroutines.cs).

### BUILDING
Windows 10 and 11, MacOS, and Arch Linux are confirmed and tested to be capable of building. There are likely many other Linux distros that work as well.

Since it's being compiled without steam support, you will need to supply the `--no-steam` command-line argument to the game when you launch it.

If you wish to compile the game yourself, there are some components required to build successfully:
- FMOD Core and FMOD Studio binaries, version 2.02.25 (you can download them from the "Engine" section of the FMOD download page).
- Linux & MacOS need A valid 64-bit Wine prefix (the WoW64 version is technically optional but recommended), with d3dcompiler_47.dll and the .NET 8.0 sdk installed on it.
- Linux needs the `zip` command installed, if you choose to use the included build script. You can get it from most of the popular package managers.

There are two build scripts for different operating systems, with `build.bat` being made for Windows, and `build.sh` for Linux or Mac OS.

To make a build, run the corresponding build script in the repository's root folder. The script will output a build for each supported platform (if on Linux or MacOS, it also creates a matching zip folder for each)

After the script completes, builds are located at `./ElevatorGame/bin/Release/net8.0/`.
