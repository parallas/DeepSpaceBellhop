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
        _sprite = file.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        _bounds = new Rectangle(MathUtil.RoundToInt(position.X), MathUtil.RoundToInt(position.Y), _sprite.Width, _sprite.Height);
    }

    public void Update(GameTime gameTime, bool isSelected)
    {
        if (_bounds.Contains(MainGame.Cursor.ViewPosition))
        {
            setSelectedButton?.Invoke(index);
        }

        if (isSelected)
        {
            bool mouseClick = InputManager.GetPressed(MouseButtons.LeftButton);

            // this prevents the button from activating on mouse click if the mouse not over the button
            if (mouseClick && _bounds.Contains(MainGame.Cursor.ViewPosition))
            {
                onClick?.Invoke();
            }
            else if (!mouseClick && Keybindings.Confirm.Pressed)
            {
                onClick?.Invoke();
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch, bool isSelected)
    {
        _sprite.Color = isSelected ? Color.Yellow : Color.Gray;
        _sprite.Draw(spriteBatch, position);

        if (isSelected)
        {
            Vector2 hoverTextPosition = new Vector2(_bounds.Center.X, _bounds.Top);
            hoverSprite?.Draw(spriteBatch, hoverTextPosition);
        }
    }
}
