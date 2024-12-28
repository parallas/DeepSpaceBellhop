using System;
using System.Collections;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Utils;

namespace ElevatorGame.Source.Days;

public class DayTransition
{
    private Sprite _transitionBaseSprite;
    private AnimatedSprite _digits5x7AnimSprite;
    private Sprite _arrowSprite;
    private AnimatedSprite _floorDotAnimSprite;

    private Rectangle _numberRect;
    private Rectangle _arrowRect;
    private Rectangle _dotRect;

    private bool _showTransition;
    private int _targetFloorNumber;

    private RenderTarget2D _transitionRenderTarget;
    private float _transitionAlpha;
    private int _transitionDirection = 1;

    public void LoadContent()
    {
        var baseFile = ContentLoader.Load<AsepriteFile>("graphics/day_transition/Base")!;
        _transitionBaseSprite = baseFile
            .CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        _numberRect = baseFile.GetSlice("Number").Keys[0].Bounds.ToXnaRectangle();
        _arrowRect = baseFile.GetSlice("Arrow").Keys[0].Bounds.ToXnaRectangle();
        _dotRect = baseFile.GetSlice("Dot").Keys[0].Bounds.ToXnaRectangle();

        var digitsFile = ContentLoader.Load<AsepriteFile>("graphics/Digits5x7")!;
        _digits5x7AnimSprite = digitsFile
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");

        _arrowSprite = ContentLoader.Load<AsepriteFile>("graphics/day_transition/Arrow")!
            .CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);

        var floorDotFile = ContentLoader.Load<AsepriteFile>("graphics/day_transition/DotMove")!;
        _floorDotAnimSprite = floorDotFile
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");

        _transitionRenderTarget =
            new RenderTarget2D(MainGame.Graphics.GraphicsDevice, baseFile.CanvasWidth, baseFile.CanvasHeight);
    }

    public void Update(GameTime gameTime)
    {
        _floorDotAnimSprite.Update(1f / 60f);
    }

    public void PreDraw(SpriteBatch spriteBatch)
    {
        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_transitionRenderTarget);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        {
            DrawTransition(spriteBatch);
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    private void DrawTransition(SpriteBatch spriteBatch)
    {
        if (!_showTransition)
            return;

        _transitionBaseSprite.Draw(spriteBatch, Vector2.Zero);
        _digits5x7AnimSprite.Draw(spriteBatch, _numberRect.Location.ToVector2());
        _arrowSprite.Draw(spriteBatch, _arrowRect.Location.ToVector2());

        Vector2 floorDotOffset = Vector2.UnitX * ((_targetFloorNumber - 2) * 3);
        _floorDotAnimSprite.Draw(spriteBatch, _dotRect.Location.ToVector2() + floorDotOffset);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_transitionRenderTarget,
            Vector2.UnitY * -_transitionDirection * MathHelper.Lerp(16, 0, MathF.Pow(_transitionAlpha, 0.5f)),
            Color.White * _transitionAlpha);
    }

    public IEnumerator TransitionToNextDay(int newLevelNumber)
    {
        _targetFloorNumber = newLevelNumber;
        _showTransition = true;

        _digits5x7AnimSprite.SetFrame(newLevelNumber - 1);

        _arrowSprite.Transparency = 0;

        // Fade in
        _transitionDirection = 1;
        while (_transitionAlpha < 1)
        {
            _transitionAlpha += 1f / 30f;
            yield return null;
        }

        yield return 30;
        ;
        // Start moving dot
        _arrowSprite.Transparency = 1;
        _floorDotAnimSprite.Play(1);

        // Wait for dot "half way" point
        yield return 3;

        // Update floor number
        _digits5x7AnimSprite.SetFrame(newLevelNumber);

        yield return 120;

        // Fade out
        _transitionDirection = -1;
        while (_transitionAlpha > 0)
        {
            _transitionAlpha -= 1f / 30f;
            yield return null;
        }

        _floorDotAnimSprite.Reset();
        yield return 60;

        _showTransition = false;
    }
}
