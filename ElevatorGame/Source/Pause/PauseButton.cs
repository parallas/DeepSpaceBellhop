using System;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Pause;

public class PauseButton(
    string imagePath,
    Vector2 position,
    Sprite hoverSprite,
    int index,
    Action<int> setSelectedButton,
    Action onClick
)
{
    private Sprite _sprite;
    private Rectangle _bounds;

    public void LoadContent()
    {
        var file = ContentLoader.Load<AsepriteFile>(imagePath)!;
        _sprite = file.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, ["Button"]);
        _sprite.Origin = new Vector2(_sprite.Width * 0.5f, _sprite.Height * 0.5f);
        _bounds = new Rectangle((int)position.X, (int)position.Y, _sprite.Width, _sprite.Height);
    }

    public void Update(GameTime gameTime, bool isSelected)
    {
        if (_bounds.Contains(MainGame.Cursor.ViewPosition))
        {
            setSelectedButton?.Invoke(index);
        }

        if (isSelected)
        {
            if (Keybindings.Confirm.Pressed)
            {
                onClick?.Invoke();
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, bool isSelected)
    {
        _sprite.Draw(spriteBatch, position);

        Vector2 hoverTextPosition = new Vector2(_bounds.Center.X, _bounds.Top - 3);
        hoverSprite?.Draw(spriteBatch, hoverTextPosition);
    }
}
