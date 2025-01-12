using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.MainMenu;

public abstract class ISettingsOption
{
    public int Index { get; set; }

    public Action<int> SetSelected { get; set; }

    public string LangToken { get; set; }

    public abstract void LoadContent();
    public abstract void Update(bool isSelected);
    public abstract void Draw(SpriteBatch spriteBatch, bool isSelected);
}

public abstract class SettingsOption<T> : ISettingsOption
{
    public required Action<T> SetValue { get; set; }
    public required Func<T> GetValue { get; set; }
}
