using AsepriteDotNet.Aseprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.MainMenu;

public class MainMenu
{
    private readonly List<MainMenuButton> _buttons = [];

    private int _selectedButton;

    private AnimatedSprite _bgSprite;
    private AnimatedSprite _logoSprite;

    public void LoadContent()
    {
        _buttons.AddRange([
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
                onClick: OnButtonSettings
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
            _buttons.RemoveAt(0);
            for (int i = 0; i < _buttons.Count; i++)
                _buttons[i].Index = i;
        }

        foreach (var button in _buttons)
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

        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].Update(i == _selectedButton);
        }

        int inputDir = (Keybindings.Down.Pressed ? 1 : 0) - (Keybindings.Up.Pressed ? 1 : 0);
        if (_selectedButton < 0) _selectedButton = _buttons.Count - 1;
        _selectedButton = (_selectedButton + inputDir) % _buttons.Count;
    }

    public void PreDraw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            _buttons[i].PreDraw(spriteBatch, i == _selectedButton);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        MainGame.Graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

        _bgSprite.Draw(spriteBatch, Vector2.Zero);
        _logoSprite.Draw(spriteBatch, new((MainGame.GameBounds.Width / 3) + 12, -12));

        foreach (var b in _buttons)
        {
            b.Draw(spriteBatch);
        }
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
    }

    private void OnButtonNewGame()
    {
        SaveManager.DeleteFile();
        SaveManager.Load();
        MainGame.CloseMainMenu();
        // MainGame.GameState = MainGame.GameStates.Intro;
        MainGame.GameState = MainGame.GameStates.Gameplay;
    }

    private void OnButtonSettings()
    {

    }

    private void OnButtonQuit()
    {

    }
}
