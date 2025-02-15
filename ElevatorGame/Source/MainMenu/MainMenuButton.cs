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

    private float _highlightValue = 0;

    private RenderTarget2D _renderTarget;

    public int Index { get; set; } = index;

    public void LoadContent()
    {
        var textSize = MainGame.FontBold.MeasureString(LocalizationManager.Get(langToken));
        // position.X -= textSize.X / 2;
        position.X -= 26;
        _bounds = new Rectangle(
            MathUtil.RoundToInt(position.X),
            MathUtil.RoundToInt(position.Y),
            MathUtil.RoundToInt(textSize.X),
            MathUtil.RoundToInt(textSize.Y)
        );

        _renderTarget = new(MainGame.Graphics.GraphicsDevice, _bounds.Width + 12, _bounds.Height);
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
            _highlightValue = MathUtil.ExpDecay(_highlightValue, 1, 13, 1f / 60f);

            // this prevents the button from activating on mouse click if the mouse not over the button
            if (InputManager.GetPressed(MouseButtons.LeftButton) && _bounds.Contains(MainGame.Cursor.ViewPosition))
            {
                onClick?.Invoke();
            }
            else if (!InputManager.GetPressed(MouseButtons.LeftButton) && Keybindings.Confirm.Pressed)
            {
                onClick?.Invoke();
            }
        }
        else
        {
            _highlightValue = MathUtil.ExpDecay(_highlightValue, 0, 13, 1f / 60f);
        }
    }

    public void PreDraw(SpriteBatch spriteBatch, bool isSelected)
    {
        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_renderTarget);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        {
            MainGame.Graphics.GraphicsDevice.Clear(Color.Transparent);

            var col = Color.Lerp(Color.White, ColorUtil.CreateFromHex(0x94e089), _highlightValue);
            var offset = _highlightValue * 10;

            Vector2 pos = new(MathF.Round(offset), 0);

            spriteBatch.DrawString(
                MainGame.FontBold,
                ">",
                pos + new Vector2(-8, 0) + Vector2.One * 2,
                Color.Black
            );
            spriteBatch.DrawString(
                MainGame.FontBold,
                ">",
                pos + new Vector2(-8, 0) + Vector2.One,
                Color.Black
            );
            spriteBatch.DrawString(
                MainGame.FontBold,
                ">",
                pos + new Vector2(-8, 0),
                col
            );

            spriteBatch.DrawStringSpacesFix(
                MainGame.FontBold,
                LocalizationManager.Get(langToken),
                pos + Vector2.One * 2,
                Color.Black,
                6
            );
            spriteBatch.DrawStringSpacesFix(
                MainGame.FontBold,
                LocalizationManager.Get(langToken),
                pos + Vector2.One,
                Color.Black,
                6
            );
            spriteBatch.DrawStringSpacesFix(
                MainGame.FontBold,
                LocalizationManager.Get(langToken),
                pos,
                col,
                6
            );
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 pos)
    {
        spriteBatch.Draw(_renderTarget, position + Vector2.Floor(pos) + new Vector2(-10, 0) + _offset + Vector2.UnitY * -2, Color.White * _opacity);
    }
}
