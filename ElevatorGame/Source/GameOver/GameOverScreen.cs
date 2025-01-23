using System.Collections;
using AsepriteDotNet.Aseprite;
using Engine;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.GameOver;

public class GameOverScreen
{
    private AnimatedSprite _phoneSprite;
    private Vector2 _phonePos;
    private Vector2 _phoneTargetPos;

    private bool _showOhNoText;
    private bool _showPromptForResults;

    private enum GameOverStates
    {
        Crash,
        Center,
        Results
    }
    private GameOverStates _state = GameOverStates.Crash;

    private Action _onContinue;

    public void Init(Vector2 phoneStartPos, Action onContinue)
    {
        _phonePos = _phoneTargetPos = phoneStartPos;
        _phoneSprite.Stop();
        _phoneSprite.Play(startingFrame: 0);

        MainGame.Coroutines.StopAll();

        var instance = StudioSystem.GetEvent("event:/SFX/GameOver/Break").CreateInstance();
        instance.Start();
        instance.Dispose();

        MainGame.Camera.SetShake(10, 20);

        MainGame.Coroutines.TryRun("game_over_sequence", GameOverSequence(), out _);

        _state = GameOverStates.Crash;
        _showOhNoText = false;
        _showPromptForResults = false;

        _onContinue = onContinue;
    }

    public void LoadContent(GraphicsDevice graphicsDevice)
    {
        _phoneSprite = ContentLoader.Load<AsepriteFile>("graphics/phone/Phone")!
            .CreateSpriteSheet(graphicsDevice, true)
            .CreateAnimatedSprite("GameOver");
    }

    public void Update()
    {
        _phoneSprite.Update(1f / 60f);

        _phonePos = MathUtil.ExpDecay(_phonePos, _phoneTargetPos, 13f, 1f / 60f);

        if (_showPromptForResults && Keybindings.Confirm.Pressed)
        {
            MainGame.Coroutines.TryRun("continue_return_to_main_menu", ReturnToMainMenu(), out _);
            _showPromptForResults = false;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 uiCamOffset = MainGame.Camera.GetParallaxPosition(Vector2.Zero, 100);
        spriteBatch.Draw(MainGame.PixelTexture,
            new Rectangle(
                MainGame.Camera.GetParallaxPosition(MainGame.GameBounds.Location.ToVector2(), 100).ToPoint(),
                MainGame.GameBounds.Size
            ), Color.Black);

        _phoneSprite.Draw(spriteBatch, MainGame.Camera.GetParallaxPosition(_phonePos, Phone.Phone.ParallaxDepth));

        if (_showOhNoText)
        {
            Vector2 ohNoTextPos = new Vector2(
                MainGame.GameBounds.Center.X - MainGame.FontBold.MeasureString("OH NO!").X * 0.5f,
                MainGame.GameBounds.Top + 4
            ) + uiCamOffset;
            spriteBatch.DrawStringSpacesFix(MainGame.FontBold, "OH NO!", ohNoTextPos, Color.White, 6);
        }

        if (_showPromptForResults)
        {
            Vector2 promptPos = new Vector2(
                MainGame.GameBounds.Center.X - MainGame.Font.MeasureString("continue").X * 0.5f,
                MainGame.GameBounds.Top + 14
            ) + uiCamOffset;
            spriteBatch.DrawStringSpacesFix(MainGame.Font, "continue", promptPos, Color.White, 6);
        }

    }

    private IEnumerator GameOverSequence()
    {
        yield return 60;

        _phoneTargetPos = new Vector2(MainGame.GameBounds.Center.X - _phoneSprite.Width * 0.5f,
            MathUtil.FloorToInt(MainGame.GameBounds.Bottom - _phoneSprite.Height * 0.5f - 32));

        yield return 30;

        _state = GameOverStates.Center;
        _showOhNoText = true;

        yield return 30;

        _showPromptForResults = true;
    }

    private IEnumerator ReturnToMainMenu()
    {
        _phoneTargetPos.Y = MainGame.GameBounds.Bottom + 16;
        _showOhNoText = false;
        _showPromptForResults = false;
        yield return 15;
        _onContinue?.Invoke();
    }
}
