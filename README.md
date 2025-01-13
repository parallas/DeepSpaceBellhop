# Deep Space Bellhop
Transport strange and unfamiliar characters between floors as an elevator operator.

This file will be packaged with every copy of the game, located in the same folder as the "DeepSpaceBellhop.dll" file.

### CONTROLS
Mouse controls are supported for every action besides moving the elevator. The mouse cursor will change shape depending on context to communicate what action Left Click will do.

The Elevator movement inputs can be held down to move several floors consecutively.

#### keyboard/mouse controls
Pause:
C | Escape

Go Back:
X | Escape

Confirm / Fast-Forward Dialog:
Z | Space | Enter | Left Click (mouse)

Move Elevator up:
Up | W

Move Elevator down:
Down | S

Open Phone screen / Close Tickets screen:
Right | D

Open Tickets screen / Close Phone screen:
Left | A

(Phone screen) Scroll up:
Up | W | Scroll Wheel Up (mouse)

(Phone screen) Scroll down:
Down | S | Scroll Wheel Down (mouse)

(Settings screen) Cycle settings tab down:
E | ]

(Settings screen) Cycle settings tab up:
Q | [

#### gamepad & steam deck controls
Pause:
Start

Go Back:
Face Button East (xbox: B, dualshock: Circle)

Confirm / Fast-Forward Dialog:
Face Button South (xbox: A, dualshock: X)

Move Elevator up:
Left Stick Up | DPad Up

Move Elevator down:
Left Stick Down | DPad Down

Open Phone screen / Close Tickets screen:
DPad Right

Open Tickets screen / Close Phone screen:
DPad Left

(Phone screen) Scroll up:
DPad Up | Left Stick Up

(Phone screen) Scroll down:
DPad Down | Left Stick Down

(Settings screen) Cycle settings tab down:
Right Shoulder Button (xbox: RB, dualshock: R1)

(Settings screen) Cycle settings tab up:
Left Shoulder Button (xbox: LB, dualshock: L1)

#### DEBUG-only controls
When compiled in DEBUG mode, additional keyboard-exclusive controls are available:

  (note: official releases of the game will not be compiled with DEBUG enabled, so you will not be able to access these functions on those builds)

  Toggle the debug gui: F3

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
