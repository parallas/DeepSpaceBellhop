using System.Linq;
using Engine.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ElevatorGame.Source;

public static class Keybindings
{
    public class FallbackMappedInput(MappedInput[] fallbacks) : MappedInput
    {
        public MappedInput[] Fallbacks { get; } = fallbacks;

        public override bool IsDown => Fallbacks.Any(input => input.IsDown);
        public override bool Pressed => Fallbacks.Any(input => input.Pressed);
        public override bool Released => Fallbacks.Any(input => input.Released);
    }

    public static MappedInput Confirm { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Z),
        new MappedInput.Keyboard(Keys.Space),
        new MappedInput.Keyboard(Keys.Enter),

        new MappedInput.Mouse(MouseButtons.LeftButton),

        new MappedInput.GamePad(Buttons.A, PlayerIndex.One),
    ]);

    public static MappedInput GoBack { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.X),
        new MappedInput.Keyboard(Keys.Escape),

        new MappedInput.GamePad(Buttons.B, PlayerIndex.One),
    ]);

    public static MappedInput Pause { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Escape),
        new MappedInput.Keyboard(Keys.C),

        new MappedInput.GamePad(Buttons.Start, PlayerIndex.One),
    ]);

    public static MappedInput Up { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Up),
        new MappedInput.Keyboard(Keys.W),

        new MappedInput.GamePad(Buttons.LeftThumbstickUp, PlayerIndex.One),
        new MappedInput.GamePad(Buttons.DPadUp, PlayerIndex.One),
    ]);

    public static MappedInput Down { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Down),
        new MappedInput.Keyboard(Keys.S),

        new MappedInput.GamePad(Buttons.LeftThumbstickDown, PlayerIndex.One),
        new MappedInput.GamePad(Buttons.DPadDown, PlayerIndex.One),
    ]);

    public static MappedInput Right { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Right),
        new MappedInput.Keyboard(Keys.D),

        new MappedInput.GamePad(Buttons.DPadRight, PlayerIndex.One),
    ]);

    public static MappedInput Left { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Left),
        new MappedInput.Keyboard(Keys.A),

        new MappedInput.GamePad(Buttons.DPadLeft, PlayerIndex.One),
    ]);
}
