using System.Collections;
using ElevatorGame.Source.Backgrounds;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public class IntroSceneStory : IntroScene
{
    private RenderTarget2D _renderTarget;
    private Texture2D _elevatorSheet;
    private Texture2D _towerTex;
    private Texture2D _moonSurfaceTex;
    private Texture2D _planetTex;
    private Texture2D _starsTex;

    private BackgroundStars _backgroundStars;

    private float _landPos = 135 * 3;
    private bool _showText = false;
    private bool _elevatorFlamesActive = false;

    private string _introText;

    public override void LoadContent()
    {
        _renderTarget = new RenderTarget2D(MainGame.Graphics.GraphicsDevice, 240, 135);
        _elevatorSheet = ContentLoader.Load<Texture2D>("graphics/intro/Elevator");
        _towerTex = ContentLoader.Load<Texture2D>("graphics/intro/Tower");
        _moonSurfaceTex = ContentLoader.Load<Texture2D>("graphics/intro/MoonSurface");
        _planetTex = ContentLoader.Load<Texture2D>("graphics/intro/Planet");
        _starsTex = ContentLoader.Load<Texture2D>("graphics/intro/Stars");

        _backgroundStars = new BackgroundStars(MainGame.Graphics.GraphicsDevice, 0f) { HandleVelocity = false };

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
        _landPos = 0;

        yield return 60;

        _landPos = 135 * 3;
    }

    public override IEnumerator GetEnumerator()
    {
        yield return 60;

        _showText = true;

        yield return 60 * 12;

        _showText = false;

        yield return 30;

        _elevatorFlamesActive = true;

        while (!MathUtil.Approximately(_landPos, 0f, 1f))
        {
            _landPos = MathUtil.ExpDecay(_landPos, 0f, 1f, 1f / 60f);
            yield return null;
        }
        _landPos = 0f;

        while (true)
        {
            yield return null;
        }
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        var exponentialLandPos = MathF.Pow(_landPos / (135 * 3), 1.5f);
        _backgroundStars.Speed = exponentialLandPos * 4f;
        _backgroundStars.Update();
        _backgroundStars.PreDraw(spriteBatch);
        spriteBatch.GraphicsDevice.SetRenderTarget(_renderTarget);
        spriteBatch.Begin();
        {
            var landPosFloor = MathUtil.FloorToInt(_landPos);
            var exponentialLandPosFloor = MathUtil.FloorToInt(exponentialLandPos * 135 * 3);
            _backgroundStars.Draw(spriteBatch);

            // Draw Stars
            int starsPos = MathUtil.FloorToInt(exponentialLandPosFloor * 0.8f);
            spriteBatch.Draw(_starsTex, new Vector2(121, 7 + starsPos), Color.White);

            // Draw Planet
            int planetPos = MathUtil.FloorToInt(exponentialLandPosFloor * 0.9f);
            spriteBatch.Draw(_planetTex, Vector2.UnitY * (135 - _planetTex.Height + planetPos), Color.White);

            // Draw Moon Surface
            spriteBatch.Draw(_moonSurfaceTex,
                new Vector2(240 - _moonSurfaceTex.Width + 8, 135 - _moonSurfaceTex.Height + landPosFloor) + Vector2.One * 2f, Color.White);

            // Draw Tower
            spriteBatch.Draw(_towerTex, new Vector2(149 + 8, 130 - _towerTex.Height + landPosFloor), Color.White);

            spriteBatch.Draw(_elevatorSheet, new Vector2(160, 64),
                new Rectangle(_elevatorFlamesActive ? 7 : 0, 0, 7, 15), Color.White);

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
