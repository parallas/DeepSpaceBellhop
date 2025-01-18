using System;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.MainMenu;

public class MainMenuButton(
    Vector2 position,
    string langToken,
    int index,
    Action<int> setSelectedButton,
    Action onClick
)
{
    private Rectangle _bounds;

    private int _animDelay = index * 5;
    private Vector2 _offset = new(-20, 0);
    private float _opacity = 0;

    private RenderTarget2D _renderTarget;

    public int Index { get; set; } = index;

    public void LoadContent()
    {
        var textSize = MainGame.FontBold.MeasureString(LocalizationManager.Get(langToken));
        position.X -= textSize.X / 2;
        _bounds = new Rectangle(
            MathUtil.RoundToInt(position.X),
            MathUtil.RoundToInt(position.Y),
            MathUtil.RoundToInt(textSize.X),
            MathUtil.RoundToInt(textSize.Y)
        );

        _renderTarget = new(MainGame.Graphics.GraphicsDevice, _bounds.Width + 2, _bounds.Height);
    }

    public void Update(bool isSelected)
    {
        if (_animDelay > 0)
            _animDelay--;
        if (_animDelay == 0)
        {
            _offset.X = MathUtil.ExpDecay(_offset.X, 0, 8, 1f / 60f);
            _opacity = MathUtil.ExpDecay(_opacity, 1, 6, 1f / 60f);

            if (MathUtil.Approximately(_opacity, 1, 0.01f))
            {
                // prevents further easing so we can have control over opacity and offset again
                _animDelay = -1;
            }
        }

        if (_bounds.Contains(MainGame.Cursor.ViewPosition))
        {
            setSelectedButton?.Invoke(Index);
        }

        if (isSelected)
        {
            // this prevents the button from activating on mouse click if the mouse not over the button
            if (InputManager.GetReleased(MouseButtons.LeftButton) && _bounds.Contains(MainGame.Cursor.ViewPosition))
            {
                onClick?.Invoke();
            }
            else if (!InputManager.GetReleased(MouseButtons.LeftButton) && Keybindings.Confirm.Pressed)
            {
                onClick?.Invoke();
            }
        }
    }

    public void PreDraw(SpriteBatch spriteBatch, bool isSelected)
    {
        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_renderTarget);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        {
            MainGame.Graphics.GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.DrawStringSpacesFix(
                MainGame.FontBold,
                LocalizationManager.Get(langToken),
                Vector2.One * 2,
                Color.Black,
                6
            );
            spriteBatch.DrawStringSpacesFix(
                MainGame.FontBold,
                LocalizationManager.Get(langToken),
                Vector2.One,
                Color.Black,
                6
            );
            spriteBatch.DrawStringSpacesFix(
                MainGame.FontBold,
                LocalizationManager.Get(langToken),
                Vector2.Zero,
                isSelected ? Color.Yellow : Color.White,
                6
            );
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_renderTarget, position + _offset + Vector2.UnitY * -2, Color.White * _opacity);
    }
}
