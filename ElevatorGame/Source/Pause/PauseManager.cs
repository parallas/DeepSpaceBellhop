using System;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Utils;

namespace ElevatorGame.Source.Pause;

public class PauseManager
{
    public bool IsPaused { get; private set; }

    private float _opacity;

    private Sprite _pausedTextSprite;
    private Sprite _hoverResumeSprite;
    private Sprite _hoverMainMenuSprite;
    private Sprite _hoverQuitSprite;

    private PauseButton _resumeButton;
    private PauseButton _mainMenuButton;
    private PauseButton _quitButton;

    private Rectangle _resumeButtonBounds;
    private Rectangle _mainMenuButtonBounds;
    private Rectangle _quitButtonBounds;

    private int _selectedButton;
    private float _transitionAlpha;

    private RenderTarget2D _renderTarget;

    public void LoadContent()
    {
        _renderTarget = new RenderTarget2D(MainGame.Graphics.GraphicsDevice, MainGame.GameBounds.Width,
            MainGame.GameBounds.Height);
        var pausedFile = ContentLoader.Load<AsepriteFile>("graphics/pause/Base")!;
        _pausedTextSprite = pausedFile.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, ["PausedText"]);

        _resumeButtonBounds = pausedFile.GetSlice("ButtonResume").Keys[0].Bounds.ToXnaRectangle();
        _mainMenuButtonBounds = pausedFile.GetSlice("ButtonMainMenu").Keys[0].Bounds.ToXnaRectangle();
        _quitButtonBounds = pausedFile.GetSlice("ButtonQuitGame").Keys[0].Bounds.ToXnaRectangle();

        var hoverTextSpriteFile = ContentLoader.Load<AsepriteFile>("graphics/pause/HoverText")!;
        _hoverResumeSprite = hoverTextSpriteFile.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        _hoverMainMenuSprite = hoverTextSpriteFile.CreateSprite(MainGame.Graphics.GraphicsDevice, 1, true);
        _hoverQuitSprite = hoverTextSpriteFile.CreateSprite(MainGame.Graphics.GraphicsDevice, 2, true);
        _hoverResumeSprite.Origin = new Vector2(_hoverResumeSprite.Width * 0.5f, _hoverResumeSprite.Height);
        _hoverMainMenuSprite.Origin = new Vector2(_hoverMainMenuSprite.Width * 0.5f, _hoverMainMenuSprite.Height);
        _hoverQuitSprite.Origin = new Vector2(_hoverQuitSprite.Width * 0.5f, _hoverQuitSprite.Height);

        _resumeButton = new PauseButton(
            "graphics/pause/Resume",
            _resumeButtonBounds.Location.ToVector2(),
            _hoverResumeSprite,
            0,
            SetSelectedButton,
            () => { }
        );
        _resumeButton.LoadContent();
        
        _mainMenuButton = new PauseButton(
            "graphics/pause/MainMenu",
            _mainMenuButtonBounds.Location.ToVector2(),
            _hoverMainMenuSprite,
            1,
            SetSelectedButton,
            () => { }
        );
        _mainMenuButton.LoadContent();

        _quitButton = new PauseButton(
            "graphics/pause/QuitGame",
            _quitButtonBounds.Location.ToVector2(),
            _hoverQuitSprite,
            2,
            SetSelectedButton,
            () => { }
        );
        _quitButton.LoadContent();
    }

    public void UnloadContent()
    {
        _renderTarget.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        _transitionAlpha = MathUtil.ExpDecay(_transitionAlpha, IsPaused ? 0.9f : 0f, 8f, 1f / 60f);

        if (!IsPaused)
            return;
        if (_transitionAlpha > 0.5f && Keybindings.Pause.Pressed)
        {
            Resume();
        }

        _resumeButton.Update(gameTime, _selectedButton == 0);
        _mainMenuButton.Update(gameTime, _selectedButton == 1);
        _quitButton.Update(gameTime, _selectedButton == 2);

        int inputDir = (Keybindings.Right.Pressed ? 1 : 0) - (Keybindings.Left.Pressed ? 1 : 0);
        _selectedButton = Math.Clamp(_selectedButton + inputDir, 0, 2);
    }

    public void PreDraw(SpriteBatch spriteBatch)
    {
        if (_transitionAlpha <= 0.001f) return;
        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_renderTarget);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        {

            spriteBatch.Draw(MainGame.PixelTexture, MainGame.ScreenBounds, Color.Black * 0.8f);
            _pausedTextSprite.Draw(spriteBatch, new Vector2(0, 0));
            _resumeButton.Draw(spriteBatch, _selectedButton == 0);
            _mainMenuButton.Draw(spriteBatch, _selectedButton == 1);
            _quitButton.Draw(spriteBatch, _selectedButton == 2);
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (_transitionAlpha <= 0.001f) return;

        spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White * _transitionAlpha);
    }

    private void SetSelectedButton(int buttonIndex)
    {
        _selectedButton = buttonIndex;
    }

    public void Pause()
    {
        IsPaused = true;
    }

    public void Resume()
    {
        IsPaused = false;
    }
}
