using AsepriteDotNet.Aseprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

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

    private int _selectedButton;

    public void LoadContent()
    {
        var pausedFile = ContentLoader.Load<AsepriteFile>("graphics/pause/Base")!;

        var hoverTextSpriteFile = ContentLoader.Load<AsepriteFile>("graphics/pause/HoverText")!;
        _hoverResumeSprite = hoverTextSpriteFile.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        _hoverMainMenuSprite = hoverTextSpriteFile.CreateSprite(MainGame.Graphics.GraphicsDevice, 1, true);
        _hoverQuitSprite = hoverTextSpriteFile.CreateSprite(MainGame.Graphics.GraphicsDevice, 2, true);
        _hoverResumeSprite.Origin = new Vector2(_hoverResumeSprite.Width * 0.5f, _hoverResumeSprite.Height);
        _hoverMainMenuSprite.Origin = new Vector2(_hoverMainMenuSprite.Width * 0.5f, _hoverMainMenuSprite.Height);
        _hoverQuitSprite.Origin = new Vector2(_hoverQuitSprite.Width * 0.5f, _hoverQuitSprite.Height);

        _resumeButton = new PauseButton(
            "graphics/pause/Resume",
            new Vector2(0,
                0),
            _hoverResumeSprite,
            0,
            SetSelectedButton,
            () => { }
        );
        _resumeButton.LoadContent();
        
        _mainMenuButton = new PauseButton(
            "graphics/pause/MainMenu",
            new Vector2(0,
                0),
            _hoverMainMenuSprite,
            1,
            SetSelectedButton,
            () => { }
        );
        _mainMenuButton.LoadContent();

        _quitButton = new PauseButton(
            "graphics/pause/QuitGame",
            new Vector2(0,
                0),
            _hoverQuitSprite,
            2,
            SetSelectedButton,
            () => { }
        );
        _quitButton.LoadContent();
    }

    public void Update(GameTime gameTime)
    {
        _resumeButton.Update(gameTime, _selectedButton == 0);
        _mainMenuButton.Update(gameTime, _selectedButton == 1);
        _quitButton.Update(gameTime, _selectedButton == 2);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _pausedTextSprite.Draw(spriteBatch, new Vector2(0, 0));
        _resumeButton.Draw(spriteBatch, _selectedButton == 0);
        _mainMenuButton.Draw(spriteBatch, _selectedButton == 1);
        _quitButton.Draw(spriteBatch, _selectedButton == 2);
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
