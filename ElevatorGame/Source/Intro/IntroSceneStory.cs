using System.Collections;
using AsepriteDotNet.Aseprite;
using ElevatorGame.Source.Backgrounds;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Intro;

public class IntroSceneStory(int startingStarSpeed = 0) : IntroScene
{
    private RenderTarget2D _renderTarget;
    private Texture2D _elevatorSheet;
    private Texture2D _towerTex;
    private Texture2D _moonSurfaceTex;
    private Texture2D _planetTex;
    private Texture2D _starsTex;
    private AnimatedSprite _signalSprite;
    private Texture2D _ufoTex;
    private Texture2D _canonTex;

    private BackgroundStars _backgroundStars;

    private float _landPos = 135 * 3;
    private bool _showText = false;
    private float _elevatorPos = 30;
    private bool _elevatorFlamesActive = false;

    private Vector2 _ufoPos = new Vector2(-50, 14);
    private float _canonPos = 0;
    private float _signalPos = 140;
    private bool _signalMove = false;
    private bool _ufoShake = false;
    private bool _ufoHover = false;
    private bool _elevatorHover = false;

    private bool _doScreenShake = false;

    private string _introText;

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
        _canonTex = ContentLoader.Load<Texture2D>("graphics/intro/UfoLauncher");

        _backgroundStars = new BackgroundStars(MainGame.Graphics.GraphicsDevice, 0f)
            { HandleVelocity = false, Speed = startingStarSpeed };

        _introText = """
                     Mission Briefing:
                     Your job is to pick
                     up and drop off 
                     guests in a timely
                     manner. 
                     
                     Occupancy will grow
                     over the next four
                     days. 
                     
                     Precision and tact
                     is of utmost
                     importance to client.
                     """;
    }

    public IEnumerator PreTitleIntro()
    {
        _landPos = 135 * 3;
        _signalSprite.Play();
        _signalPos = 135;
        _signalMove = false;
        _ufoHover = true;
        _ufoShake = false;
        _backgroundStars.Speed = 0f;

        yield return 60;

        while (_ufoPos.X < 142)
        {
            _ufoPos.X = MathUtil.Approach(_ufoPos.X, 142, 1);

            if (_ufoPos.X > 142 - (135 - 26) / 2f)
            {
                _signalMove = true;
            }

            yield return null;
        }

        _ufoHover = false;

        _ufoShake = true;
        yield return 15;
        _ufoShake = false;

        yield return 40;

        while (_canonPos < 10)
        {
            _canonPos = MathUtil.Approach(_canonPos, 10, 0.1f);
            yield return null;
        }

        yield return 120;

        _elevatorPos = 30;
        while (_elevatorPos < 135)
        {
            _elevatorPos += 3;
            yield return null;
        }

        yield return 60;

        while (_backgroundStars.Speed < 4f)
        {
            _backgroundStars.Speed = MathUtil.Approach(_backgroundStars.Speed, 4f, 0.1f);
            _ufoPos.Y = MathUtil.Approach(_ufoPos.Y, -24, 4f);
            yield return null;
        }

        yield return 60;
    }

    public override IEnumerator GetEnumerator()
    {
        _backgroundStars.Speed = 4f;
        _elevatorPos = 135;

        _elevatorHover = false;

        yield return 30;

        while (!MathUtil.Approximately(_elevatorPos, 64, 1))
        {
            _elevatorPos = MathUtil.ExpDecay(_elevatorPos, 64, 2, 1f / 60f);
            yield return null;
        }

        _elevatorHover = true;
        _showText = true;

        yield return 60 * 12;

        _showText = false;

        yield return 30;

        _elevatorHover = false;
        _elevatorFlamesActive = true;

        while (!MathUtil.Approximately(_landPos, 0f, 1f))
        {
            var exponentialLandPos = MathF.Pow(_landPos / (135 * 3), 1.5f);
            _backgroundStars.Speed = exponentialLandPos * 4f;
            _landPos = MathUtil.ExpDecay(_landPos, 0f, 1f, 1f / 60f);
            yield return null;
        }
        _landPos = 0f;
        _backgroundStars.Speed = 0;

        int counter = 0;
        while (counter < 30)
        {
            counter++;

            _elevatorFlamesActive = counter % 4 < 2;
            yield return null;
        }
        _elevatorFlamesActive = false;

        yield return 30;

        float velocity = 0f;
        while (_elevatorPos < 125)
        {
            velocity += 0.1f;
            _elevatorPos += velocity;
            yield return null;
        }

        _doScreenShake = true;
        yield return 20;
        _doScreenShake = false;

        yield return 60;
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        var exponentialLandPos = MathF.Pow(_landPos / (135 * 3), 1.5f);
        _backgroundStars.Update();
        _backgroundStars.PreDraw(spriteBatch);

        _signalSprite.Update(1f / 60f);
        if (_signalMove) _signalPos += -2;

        // Screen Shake
        Vector2 screenShake = Vector2.Zero;
        if (_doScreenShake) screenShake = Vector2.UnitX * (MainGame.Frame % 4 < 2 ? -1 : 1);

        spriteBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
        spriteBatch.Begin();
        {
            var landPosFloor = MathUtil.FloorToInt(_landPos);
            var exponentialLandPosFloor = MathUtil.FloorToInt(exponentialLandPos * 135 * 3);
            _backgroundStars.Draw(spriteBatch);

            Point ufoPosInt = _ufoPos.ToPoint();
            int canonPosInt = MathUtil.FloorToInt(_canonPos);

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
            if (_elevatorHover) elevatorPosInt += MathUtil.RoundToInt(MathF.Sin(MainGame.Frame / 16f));
            if (elevatorPosInt > 30)
            {
                spriteBatch.Draw(_elevatorSheet, new Vector2(160, elevatorPosInt),
                    new Rectangle(_elevatorFlamesActive ? 7 : 0, 0, 7, 15), Color.White);
            }

            // Draw UFO Canon
            if (canonPosInt > 0)
                spriteBatch.Draw(_canonTex, ufoPosInt.ToVector2() + Vector2.UnitY * canonPosInt, Color.White);

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

            if (_showText)
            {
                var split = _introText.Split(["\r\n", "\n"], StringSplitOptions.TrimEntries);
                for (var i = 0; i < split.Length; i++)
                {
                    spriteBatch.DrawStringSpacesFix(
                        MainGame.FontIntro,
                        split[i],
                        new Vector2(8, 8 + i * 8),
                        Color.Lime,
                        6
                    );
                }
            }
        }
        spriteBatch.End();
        spriteBatch.GraphicsDevice.Reset();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
    }
}
