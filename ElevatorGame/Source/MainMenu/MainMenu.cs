using AsepriteDotNet.Aseprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.MainMenu;

public class MainMenu
{
    private readonly List<MainMenuButton> _titleButtons = [];

    private int _selectedButton;

    private AnimatedSprite _bgSprite;
    private AnimatedSprite _logoSprite;

    public Action ExitGame { get; set; }
    public Action StartGame { get; set; }
    public Action<bool> OnChangeFullscreen { get; set; }

    private enum State
    {
        Title,
        Settings
    }

    private State _state = State.Title;

    private SettingsMenu _settings;

    public void LoadContent()
    {
        _titleButtons.AddRange([
            new(
                position: new(MainGame.GameBounds.Width / 3, MainGame.GameBounds.Height - 16 - 30),
                langToken: "ui.main_menu.continue",
                index: 0,
                setSelectedButton: SetSelectedButton,
                onClick: OnButtonContinue
            ),
            new(
                position: new(MainGame.GameBounds.Width / 3, MainGame.GameBounds.Height - 16 - 20),
                langToken: "ui.main_menu.new_game",
                index: 1,
                setSelectedButton: SetSelectedButton,
                onClick: OnButtonNewGame
            ),
            new(
                position: new(MainGame.GameBounds.Width / 3, MainGame.GameBounds.Height - 16 - 10),
                langToken: "ui.main_menu.settings",
                index: 2,
                setSelectedButton: SetSelectedButton,
                onClick: OnButtonOpenSettings
            ),
            new(
                position: new(MainGame.GameBounds.Width / 3, MainGame.GameBounds.Height - 16),
                langToken: "ui.main_menu.quit",
                index: 3,
                setSelectedButton: SetSelectedButton,
                onClick: OnButtonQuit
            ),
        ]);

        if (!SaveManager.SaveFileExists)
        {
            RemoveButton(0);
        }

        foreach (var button in _titleButtons)
        {
            button.LoadContent();
        }

        _bgSprite = ContentLoader.Load<AsepriteFile>("graphics/main_menu/Background")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");

        _bgSprite.Color = Color.Gray;

        _logoSprite = ContentLoader.Load<AsepriteFile>("graphics/main_menu/Logo")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
        _logoSprite.OriginX = _logoSprite.Width / 2;
    }

    public void Update()
    {
        MainGame.Cursor.CursorSpriteOverride = Cursor.CursorSprites.Default;

        _bgSprite.Update(1f / 60f);
        _logoSprite.Update(1f / 60f);

        if (_state == State.Title)
        {
            for (int i = 0; i < _titleButtons.Count; i++)
            {
                _titleButtons[i].Update(i == _selectedButton);
            }

            int inputDir = (Keybindings.Down.Pressed ? 1 : 0) - (Keybindings.Up.Pressed ? 1 : 0);
            _selectedButton = (_selectedButton + inputDir) % _titleButtons.Count;
            if (_selectedButton < 0) _selectedButton = _titleButtons.Count - 1;
        }

        _settings?.Update();
    }

    public void PreDraw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < _titleButtons.Count; i++)
        {
            _titleButtons[i].PreDraw(spriteBatch, i == _selectedButton);
        }

        _settings?.PreDraw(spriteBatch);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        MainGame.Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

        _bgSprite.Draw(spriteBatch, Vector2.Zero);
        _logoSprite.Draw(spriteBatch, new((MainGame.GameBounds.Width / 3) + 12, -12));

        foreach (var b in _titleButtons)
        {
            b.Draw(spriteBatch);
        }

        _settings?.Draw(spriteBatch);
    }

    private void RemoveButton(int index)
    {
        _titleButtons.RemoveAt(index);
        for (int i = 0; i < _titleButtons.Count; i++)
            _titleButtons[i].Index = i;
    }

    private void SetSelectedButton(int buttonIndex)
    {
        _selectedButton = buttonIndex;
    }

    private void OnButtonContinue()
    {
        SaveManager.Load();
        MainGame.CloseMainMenu();
        MainGame.GameState = MainGame.GameStates.Gameplay;
        StartGame?.Invoke();
    }

    private void OnButtonNewGame()
    {
        SaveManager.DeleteSaveFile();
        SaveManager.Load();
        MainGame.CloseMainMenu();
        // MainGame.GameState = MainGame.GameStates.Intro;
        MainGame.GameState = MainGame.GameStates.Gameplay;
        StartGame?.Invoke();
    }

    private void OnButtonOpenSettings()
    {
        _state = State.Settings;
        _settings = new()
        {
            OnClose = OnSettingsClose,
            OnChangeFullscreen = this.OnChangeFullscreen,
        };
        _settings.LoadContent();
    }

    private void OnButtonQuit()
    {
        ExitGame?.Invoke();
    }

    private void OnSettingsClose()
    {
        _state = State.Title;
        SaveManager.SaveSettings();
    }
}
