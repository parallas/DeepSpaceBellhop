using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AsepriteDotNet.Aseprite;
using ElevatorGame.Source.Backgrounds;
using Engine;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Intro;

public class IntroSceneEnding : IntroScene, IDisposable
{
    private RenderTarget2D _renderTarget;
    private Texture2D _elevatorSheet;
    private Texture2D _towerTex;
    private Texture2D _moonSurfaceTex;
    private Texture2D _planetTex;
    private Texture2D _starsTex;
    private AnimatedSprite _signalSprite;
    private Texture2D _ufoTex;
    private Texture2D _cannonTex;
    private Texture2D _laserGlowTex;

    private Regex _langTokenRegex = _Regex.InlineLangToken();

    private BackgroundStars _backgroundStars;

    private float _landPos = 0;
    private float _elevatorPos = 125;
    private bool _elevatorFlamesActive = false;

    private Vector2 _ufoPos = new Vector2(-50, 14);
    private float _cannonPos = 0;
    private bool _showLaserGlow = false;
    private float _signalPos = 140;
    private bool _signalMove = false;
    private bool _ufoShake = false;
    private bool _ufoHover = false;
    private bool _elevatorHover = false;

    private CreditsData _data = null;
    private bool _rollCredits = false;
    private float _creditsPos = 0;
    private int _creditsHeight = 0;

    EventInstance? _ufoHoverSound;
    EventInstance? _cannonOpenSound;

    private bool _doScreenShake = false;

    public override void LoadContent()
    {
        _renderTarget = new RenderTarget2D(MainGame.Graphics.GraphicsDevice, 240, 135);
        _elevatorSheet = ContentLoader.Load<Texture2D>("graphics/intro/Elevator");
        _towerTex = ContentLoader.Load<Texture2D>("graphics/intro/Tower");
        _moonSurfaceTex = ContentLoader.Load<Texture2D>("graphics/intro/MoonSurface");
        _planetTex = ContentLoader.Load<Texture2D>("graphics/intro/Planet");
        _starsTex = ContentLoader.Load<Texture2D>("graphics/intro/Stars");
        _signalSprite = ContentLoader.Load<AsepriteFile>("graphics/intro/Signal")!
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Loop");
        _ufoTex = ContentLoader.Load<Texture2D>("graphics/intro/Ufo");
        _cannonTex = ContentLoader.Load<Texture2D>("graphics/intro/UfoLauncher");
        _laserGlowTex = ContentLoader.Load<Texture2D>("graphics/intro/LaserGlow");

        _data = JsonSerializer.Deserialize<CreditsData>(
            File.ReadAllText(Path.Combine(
                FileLocations.ProgramPath, "Content", "data", "credits.json"
            )),
            CreditsData.SerializerOptions
        );

        int y = 0;
        foreach(var section in _data.Sections)
        {
            foreach(var item in section.Value)
                y += 14;
            y += 20;
        }
        _creditsHeight = y + 135;

        _backgroundStars = new BackgroundStars(MainGame.Graphics.GraphicsDevice, 0f)
            { HandleVelocity = false, Speed = 0 };
    }

    public override IEnumerator GetEnumerator()
    {
        MusicPlayer.PlayMusic($"Day1");

        yield return 60;

        _doScreenShake = true;
        yield return 90;

        while (!MathUtil.Approximately(_landPos, 135 * 3, 1f))
        {
            var exponentialLandPos = MathF.Pow(_landPos / (135 * 3), 1.5f);
            _backgroundStars.Speed = -exponentialLandPos * 5f;
            _landPos = MathUtil.ExpDecay(_landPos, 135 * 3, 1.2f, 1f / 60f);
            _elevatorPos = MathUtil.ExpDecay(_elevatorPos, 64, 2, 1f / 60f);
            _elevatorFlamesActive = true;
            if(_landPos > 135 * 1.8f)
            {
                _doScreenShake = false;
                _elevatorFlamesActive = false;
            }
            yield return null;
        }

        _rollCredits = true;
        _elevatorHover = true;

        while(_creditsPos < _creditsHeight)
        {
            _creditsPos = MathUtil.Approach(_creditsPos, _creditsHeight, 0.25f);
            yield return null;
        }

        yield return 60;

        yield return MainGame.FadeToBlack();

        // _backgroundStars.Speed = 4f;
        // _elevatorPos = 135;

        // _elevatorHover = false;

        // yield return 30;

        // _elevatorHover = true;

        // while(!MathUtil.Approximately(_elevatorPos, 64, 0.2f))
        //     _elevatorPos = MathUtil.ExpDecay(_elevatorPos, 64, 2, 1f / 60f);

        // yield return 30;

        // _elevatorHover = false;
        // _elevatorFlamesActive = true;

        // while (!MathUtil.Approximately(_landPos, 135 * 3, 1f))
        // {
        //     var exponentialLandPos = MathF.Pow(_landPos / (135 * 3), 1.5f);
        //     _backgroundStars.Speed = exponentialLandPos * 4f;
        //     _landPos = MathUtil.ExpDecay(_landPos, 135 * 3, 1.2f, 1f / 60f);
        //     yield return null;
        // }
        // _landPos = 0f;
        // _backgroundStars.Speed = 0;

        // int counter = 0;
        // while (counter < 30)
        // {
        //     counter++;

        //     _elevatorFlamesActive = counter % 4 < 2;
        //     yield return null;
        // }
        // _elevatorFlamesActive = false;

        // yield return 30;

        // float velocity = 0f;
        // while (_elevatorPos < 125)
        // {
        //     velocity += 0.1f;
        //     _elevatorPos += velocity;
        //     yield return null;
        // }

        // _doScreenShake = true;
        // yield return 20;
        // _doScreenShake = false;

        // yield return 120;
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        var exponentialLandPos = MathF.Pow(_landPos / (135 * 3), 1.5f);
        _backgroundStars.Update();
        if(Keybindings.Confirm.IsDown)
            _backgroundStars.Update();
        _backgroundStars.PreDraw(spriteBatch);

        _signalSprite.Update(1f / 60f);
        if (_signalMove) _signalPos += -2;

        // Screen Shake
        Vector2 screenShake = Vector2.Zero;
        if (_doScreenShake) screenShake = Vector2.UnitX * (MainGame.Frame % 4 < 2 ? -1 : 1) * (((135 * 1.5f) - _landPos) / (135 * 1.5f));

        spriteBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
        spriteBatch.Begin();
        {
            var landPosFloor = MathUtil.FloorToInt(_landPos);
            var exponentialLandPosFloor = MathUtil.FloorToInt(exponentialLandPos * 135 * 3);
            _backgroundStars.Draw(spriteBatch);

            Point ufoPosInt = _ufoPos.ToPoint();
            int cannonPosInt = MathUtil.FloorToInt(_cannonPos);

            // Draw Stars
            int starsPos = MathUtil.FloorToInt(exponentialLandPosFloor * 0.8f);
            spriteBatch.Draw(_starsTex, new Vector2(121, 7 + starsPos), Color.White);

            // Draw Planet
            int planetPos = MathUtil.FloorToInt(exponentialLandPosFloor * 0.9f);
            spriteBatch.Draw(_planetTex, Vector2.UnitY * (135 - _planetTex.Height + planetPos), Color.White);

            // Draw Signal
            _signalSprite.Draw(spriteBatch, new Vector2(158, MathUtil.FloorToInt(_signalPos)));

            // Draw Elevator
            int elevatorPosInt = MathUtil.FloorToInt(_elevatorPos);
            if (_elevatorHover) elevatorPosInt += MathUtil.RoundToInt(MathF.Sin(MainGame.Frame / 32f));
            if (elevatorPosInt > 30)
            {
                spriteBatch.Draw(_elevatorSheet, new Vector2(160, elevatorPosInt),
                    new Rectangle(_elevatorFlamesActive ? 7 : 0, 0, 7, 15), Color.White);
            }

            // Draw Laser Glow
            if (_showLaserGlow && MainGame.Frame % 4 < 2)
                spriteBatch.Draw(_laserGlowTex,
                    ufoPosInt.ToVector2() + Vector2.UnitY * cannonPosInt + new Vector2(15, 9), Color.White);

            // Draw UFO Canon
            if (cannonPosInt > 0)
                spriteBatch.Draw(_cannonTex, ufoPosInt.ToVector2() + Vector2.UnitY * cannonPosInt, Color.White);

            // Draw UFO
            Vector2 randomShake = Vector2.Zero;
            if (_ufoHover) randomShake = new Vector2(0, MathUtil.FloorToInt(MathF.Sin(ufoPosInt.X / 8f) * 2));
            if (_ufoShake) randomShake += new Vector2(Random.Shared.Next(-1, 2), Random.Shared.Next(-1, 2));
            spriteBatch.Draw(_ufoTex, ufoPosInt.ToVector2() + randomShake, Color.White);

            // Draw Moon Surface
            spriteBatch.Draw(_moonSurfaceTex,
                new Vector2(240 - _moonSurfaceTex.Width + 8, 135 - _moonSurfaceTex.Height + landPosFloor)
                    + Vector2.One * 2f
                    + screenShake,
                Color.White
            );

            // Draw Tower
            spriteBatch.Draw(_towerTex, new Vector2(149 + 8, 130 - _towerTex.Height + landPosFloor) + screenShake,
                Color.White);

            float y = -_creditsHeight * ((_creditsHeight - _creditsPos) / _creditsHeight) + 135;
            foreach(var section in _data.Sections.Reverse())
            {
                foreach(var item in section.Value.Reverse<string>())
                {
                    string replaced = _langTokenRegex.Replace(item, m => LocalizationManager.Get(m.Value[1..^1]));
                    spriteBatch.DrawStringSpacesFix(
                        MainGame.FontIntro,
                        replaced,
                        new Vector2(10, MathUtil.FloorToInt(y) + 1),
                        Color.Black * 0.5f,
                        5
                    );
                    spriteBatch.DrawStringSpacesFix(
                        MainGame.FontIntro,
                        replaced,
                        new Vector2(10, MathUtil.FloorToInt(y)),
                        Color.White,
                        5
                    );
                    y += 14;
                }

                spriteBatch.DrawStringSpacesFix(
                    MainGame.FontIntro,
                    LocalizationManager.Get($"credits.section.{section.Key}"),
                    new Vector2(6, MathUtil.FloorToInt(y) + 1),
                    Color.Black * 0.5f,
                    5
                );
                spriteBatch.DrawStringSpacesFix(
                    MainGame.FontIntro,
                    LocalizationManager.Get($"credits.section.{section.Key}"),
                    new Vector2(6, MathUtil.FloorToInt(y)),
                    Color.Cyan,
                    5
                );
                y += 20;
            }
        }
        spriteBatch.End();
        spriteBatch.GraphicsDevice.Reset();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
    }

    public void Dispose()
    {
        _cannonTex = null;
        _elevatorSheet = null;
        _laserGlowTex = null;
        _moonSurfaceTex = null;
        _planetTex = null;
        _ufoTex = null;
        _starsTex = null;
        _towerTex = null;

        _ufoHoverSound?.Stop();
        _ufoHoverSound?.Dispose();
        _cannonOpenSound?.Stop();
        _cannonOpenSound?.Dispose();

        GC.SuppressFinalize(this);
    }
}
