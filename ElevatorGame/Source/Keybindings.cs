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

        public override string ToString()
            => $"X:~{string.Join<MappedInput>(',', Fallbacks)}";

        public IEnumerable<string> GetGlyphIDs()
        {
            List<string> glyphs = [];
            foreach (var input in Fallbacks)
            {
                if (input is Keyboard keyboardInput)
                {
                    glyphs.Add($"keyboard_{keyboardInput.Key}");
                }
                else if (input is GamePad gamePadInput)
                {
                    glyphs.Add($"gamepad_{gamePadInput.Button}");
                }
                else if (input is Mouse mouseInput)
                {
                    glyphs.Add($"mouse_{mouseInput.MouseButton}");
                }
            }
            return glyphs;
        }
    }

    public static FallbackMappedInput Confirm { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Z),
        new MappedInput.Keyboard(Keys.Space),
        new MappedInput.Keyboard(Keys.Enter),

        new MappedInput.Mouse(MouseButtons.LeftButton),

        new MappedInput.GamePad(Buttons.A, PlayerIndex.One),
    ]);

    public static FallbackMappedInput GoBack { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.X),
        new MappedInput.Keyboard(Keys.Escape),

        new MappedInput.GamePad(Buttons.B, PlayerIndex.One),
    ]);

    public static FallbackMappedInput SettingsTabNext { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.E),
        new MappedInput.Keyboard(Keys.OemCloseBrackets),

        new MappedInput.GamePad(Buttons.RightShoulder, PlayerIndex.One),
    ]);

    public static FallbackMappedInput SettingsTabLast { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Q),
        new MappedInput.Keyboard(Keys.OemOpenBrackets),

        new MappedInput.GamePad(Buttons.LeftShoulder, PlayerIndex.One),
    ]);

    public static FallbackMappedInput Pause { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Escape),
        new MappedInput.Keyboard(Keys.C),

        new MappedInput.GamePad(Buttons.Start, PlayerIndex.One),
    ]);

    public static FallbackMappedInput Up { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Up),
        new MappedInput.Keyboard(Keys.W),

        new MappedInput.GamePad(Buttons.LeftThumbstickUp, PlayerIndex.One),
        new MappedInput.GamePad(Buttons.DPadUp, PlayerIndex.One),
    ]);

    public static FallbackMappedInput Down { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Down),
        new MappedInput.Keyboard(Keys.S),

        new MappedInput.GamePad(Buttons.LeftThumbstickDown, PlayerIndex.One),
        new MappedInput.GamePad(Buttons.DPadDown, PlayerIndex.One),
    ]);

    public static FallbackMappedInput Right { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Right),
        new MappedInput.Keyboard(Keys.D),

        new MappedInput.GamePad(Buttons.DPadRight, PlayerIndex.One),
    ]);

    public static FallbackMappedInput Left { get; } = new FallbackMappedInput(
    [
        new MappedInput.Keyboard(Keys.Left),
        new MappedInput.Keyboard(Keys.A),

        new MappedInput.GamePad(Buttons.DPadLeft, PlayerIndex.One),
    ]);
}
