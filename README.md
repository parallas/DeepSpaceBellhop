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

### LICENSE
All source code and localizations, unless otherwise specified in this README or inline, is licensed under the [MIT license](https://github.com/Parallas/DeepSpaceBellhop/blob/main/LICENSE).

Graphical assets (excluding fonts) and audio are licensed under the [CC BY-SA 4.0 license](https://creativecommons.org/licenses/by-sa/4.0/).

### PACKAGES, TOOLS, AND ATTRIBUTION
Runtime: .NET 8.0, x64 only

Audio Engine: FMOD Studio by Firelight Technologies Pty Ltd.

Uses a fork of FmodForFoxes to interface with FMOD: [PixelDough/FmodForFoxes](https://github.com/PixelDough/FmodForFoxes/tree/mac-support) ([license](https://github.com/Martenfur/FmodForFoxes/blob/develop/LICENSE.md))

Steam integration: Steamworks SDK v157

Uses a fork of Facepunch.Steamworks to interface with Steamworks SDK: [tmaster-terrarian/Facepunch.Steamworks](https://github.com/tmaster-terrarian/Facepunch.Steamworks) ([license](https://github.com/FacePunch/Facepunch.Steamworks/blob/master/LICENSE))

Uses a fork (of a fork) of the Coroutines system that was initially created by [ChevyRay](https://github.com/ChevyRay): [tmaster-terrarian/Coroutines](https://github.com/tmaster-terrarian/Parallas.Coroutines). Detailed attribution is embedded in the file: [/Engine/Coroutines.cs](https://raw.githubusercontent.com/Parralas/DeepSpaceBellhop/refs/heads/main/Engine/Coroutines.cs).
