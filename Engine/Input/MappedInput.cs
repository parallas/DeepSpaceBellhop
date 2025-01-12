using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Engine.Input;

public abstract class MappedInput : IParsable<MappedInput>
{
    public abstract bool IsDown { get; }

    public abstract bool Pressed { get; }

    public abstract bool Released { get; }

    public class Keyboard(Keys key) : MappedInput
    {
        public Keys Key => key;

        public override bool IsDown => InputManager.GetDown(key);

        public override bool Pressed => InputManager.GetPressed(key);

        public override bool Released => InputManager.GetReleased(key);

        public override string ToString()
        {
            return $"KB:{key}";
        }
    }

    public class GamePad(Buttons button, PlayerIndex playerIndex) : MappedInput
    {
        public Buttons Button => button;

        public override bool IsDown => InputManager.GetDown(button, playerIndex);

        public override bool Pressed => InputManager.GetPressed(button, playerIndex);

        public override bool Released => InputManager.GetReleased(button, playerIndex);

        public override string ToString()
        {
            return $"GP:{button}";
        }
    }

    public class Mouse(MouseButtons mouseButton) : MappedInput
    {
        public MouseButtons MouseButton => mouseButton;

        public override bool IsDown => InputManager.GetDown(mouseButton);

        public override bool Pressed => InputManager.GetPressed(mouseButton);

        public override bool Released => InputManager.GetReleased(mouseButton);

        public override string ToString()
        {
            return $"MB:{mouseButton}";
        }
    }

    public static MappedInput Parse(string value, IFormatProvider? formatProvider)
    {
        if(value is null) return null;
        if(value.Length < 4) return null;

        return value[..2] switch
        {
            "KB" => new Keyboard(MapKeyboard(value[3..])),
            "GP" => new GamePad(MapGamepad(value[3..]), PlayerIndex.One),
            "MB" => new Mouse(MapMouse(value[3..])),
            _ => null,
        };
    }

    public static bool TryParse([NotNullWhen(true)] string s, IFormatProvider provider, [MaybeNullWhen(false)] out MappedInput result)
    {
        result = Parse(s, provider);

        return result is not null;
    }

    private static Keys MapKeyboard(string name)
    {
        return Enum.GetValues<Keys>()
            .First(key => Enum.GetName(key).Equals(name));
    }

    private static Buttons MapGamepad(string name)
    {
        return Enum.GetValues<Buttons>()
            .First(key => Enum.GetName(key).Equals(name));
    }

    private static MouseButtons MapMouse(string name)
    {
        return Enum.GetValues<MouseButtons>()
            .First(key => Enum.GetName(key).Equals(name));
    }
}
