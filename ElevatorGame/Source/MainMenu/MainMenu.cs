using System.Collections;
using AsepriteDotNet.Aseprite;
using ElevatorGame.Source.Backgrounds;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.MainMenu;

public class MainMenu
{
    private readonly List<MainMenuButton> _titleButtons = [];

    private int _selectedButton;

    private AnimatedSprite _logoSprite;
    private Sprite _buttonPanelSprite;

    private float _offset;

    public Action ExitGame { get; set; }
    public Action StartGame { get; set; }
    public Action<bool> OnChangeFullscreen { get; set; }

    private float _hideProgress = 1;

    private enum State
    {
        None,
        Title,
        Settings,
    }

    private State _state = State.None;

    private SettingsMenu _settings;

    private Intro.IntroSceneStory _intro;
    private static bool _started = false;
    private bool _instanceStarted = false;

    public void LoadContent()
    {
        _intro = new();
        _intro.LoadContent();

        _titleButtons.AddRange([
            new(
                position: new(MainGame.GameBounds.Width / 2, MainGame.GameBounds.Height - 16 - 30),
                langToken: "ui.main_menu.continue",
                index: 0,
                setSelectedButton: SetSelectedButton,
                onClick: OnButtonContinue
            ),
            new(
                position: new(MainGame.GameBounds.Width / 2, MainGame.GameBounds.Height - 16 - 20),
                langToken: "ui.main_menu.new_game",
                index: 1,
                setSelectedButton: SetSelectedButton,
                onClick: OnButtonNewGame
            ),
            new(
                position: new(MainGame.GameBounds.Width / 2, MainGame.GameBounds.Height - 16 - 10),
                langToken: "ui.main_menu.settings",
                index: 2,
                setSelectedButton: SetSelectedButton,
                onClick: OnButtonOpenSettings
            ),
            new(
                position: new(MainGame.GameBounds.Width / 2, MainGame.GameBounds.Height - 16),
                langToken: "ui.main_menu.quit",
                index: 3,
                setSelectedButton: SetSelectedButton,
                onClick: OnButtonQuit
            ),
        ]);

        if (!MainGame.SaveFileExists)
        {
            RemoveButton(0);
        }

        foreach (var button in _titleButtons)
        {
            button.LoadContent();
        }

        _logoSprite = ContentLoader.Load<AsepriteFile>("graphics/main_menu/Logo")!
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
        _logoSprite.OriginX = _logoSprite.Width / 2;

        _buttonPanelSprite = ContentLoader.Load<AsepriteFile>("graphics/CardTray")!
            .CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        _buttonPanelSprite.OriginX = _buttonPanelSprite.Width / 2;
    }

    public void Update()
    {
        if(!_started)
        {
            _started = true;
            _instanceStarted = true;
            MainGame.Coroutines.Stop("menu_pre_title");
            MainGame.Coroutines.Run("menu_pre_title", PreTitle());
        }
        else if(!_instanceStarted)
        {
            _instanceStarted = true;
            _state = State.Title;
        }

        // _backgroundStars.Update();
        MainGame.Cursor.CursorSpriteOverride = Cursor.CursorSprites.Default;

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

            _offset = MathUtil.ExpDecay(_offset, 0, 5, 1f / 60f);
            _hideProgress = MathUtil.ExpDecay(_hideProgress, 0, 3, 1f / 60f);
        }
        else if (_state == State.Settings)
        {
            _offset = MathUtil.ExpDecay(_offset, -200, 5, 1f / 60f);
            _hideProgress = MathUtil.ExpDecay(_hideProgress, 0, 3, 1f / 60f);
        }
        else
        {
            _offset = MathUtil.ExpDecay(_offset, 0, 5, 1f / 60f);
            _hideProgress = MathUtil.ExpDecay(_hideProgress, 1, 3, 1f / 60f);

            if(MainGame.Coroutines.IsRunning("menu_pre_title") && (Keybindings.Confirm.Pressed || Keybindings.GoBack.Pressed))
            {
                MainGame.Coroutines.Stop("menu_pre_title");
                _intro.Dispose();
                _intro = new(4);
                _intro.LoadContent();
                _state = State.Title;
            }
        }

        _settings?.Update();
    }

    public void PreDraw(SpriteBatch spriteBatch)
    {
        _intro.PreDraw(spriteBatch);

        for (int i = 0; i < _titleButtons.Count; i++)
        {
            _titleButtons[i].PreDraw(spriteBatch, i == _selectedButton);
        }

        _settings?.PreDraw(spriteBatch);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        MainGame.Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

        _intro.Draw(spriteBatch);

        Vector2 logoPos = new(MathF.Truncate((MainGame.GameBounds.Width / 2f) + _offset), -12);
        logoPos.Y += MathF.Sin(MainGame.Frame / 60f) * 2f;
        logoPos.Y -= _hideProgress * 120;
        logoPos = Vector2.Floor(logoPos);
        _logoSprite.Color = Color.Black;
        _logoSprite.Draw(spriteBatch, logoPos + Vector2.One * 2f);
        _logoSprite.Color = Color.White;
        _logoSprite.Draw(spriteBatch, logoPos);

        _buttonPanelSprite.Draw(
            spriteBatch,
            new(
                MathUtil.FloorToInt((MainGame.GameBounds.Width / 2) + _offset),
                MathUtil.FloorToInt(MainGame.GameBounds.Height - 16 - (_titleButtons.Count * 10) + (100 * _hideProgress))
            )
        );

        foreach (var b in _titleButtons)
        {
            b.Draw(spriteBatch, new(_offset, 120 * _hideProgress));
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
        MainGame.Coroutines.TryRun("transition_to_game", TransitionToGame(skipIntro: true), out _);
    }

    private void OnButtonNewGame()
    {
        SaveManager.DeleteSaveFile();
        SaveManager.Load();
        MainGame.Coroutines.TryRun("transition_to_game", TransitionToGame(skipIntro: false), out _);
    }

    private IEnumerator PreTitle()
    {
        yield return _intro.PreTitleIntro();
        _state = State.Title;
    }

    private IEnumerator TransitionToGame(bool skipIntro)
    {
        if(!skipIntro)
        {
            MusicPlayer.PlayMusic("Intro");
            _state = State.None;
            yield return _intro.GetEnumerator();
        }

        yield return MainGame.CloseMainMenu();
        // MainGame.GameState = MainGame.GameStates.Intro;
        MainGame.GameState = MainGame.GameStates.Gameplay;
        StartGame?.Invoke();
        // yield return MainGame.FadeFromBlack();
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
