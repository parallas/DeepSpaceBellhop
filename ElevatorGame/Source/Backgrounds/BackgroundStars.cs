using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Backgrounds;

public class BackgroundStars
{
    public bool DoParallax { get; set; }
    public bool DoInput { get; set; } = false;
    public int MaxSpeed { get; set; } = 8;
    public float IdleSpeed { get; set; }
    public bool HandleVelocity { get; set; } = true;
    public float Speed { get; set; }

    private readonly GraphicsDevice _graphicsDevice;
    private RenderTarget2D _renderTarget;
    private Texture2D _starTexture;
    private List<Star> _stars = new();

    private Color[] _colors = [Color.CornflowerBlue, ColorUtil.CreateFromHex(0x67b6bd), Color.Pink, Color.Lavender, Color.HotPink, ColorUtil.CreateFromHex(0x94e089)];

    public BackgroundStars(GraphicsDevice graphicsDevice, float idleSpeed = 1f)
    {
        _graphicsDevice = graphicsDevice;
        _renderTarget = new RenderTarget2D(graphicsDevice, 240, 135);

        _starTexture = ContentLoader.Load<Texture2D>("graphics/backgrounds/stars");

        IdleSpeed = idleSpeed;
        Speed = idleSpeed;

        for (var i = 0; i < 500; i++)
        {
            var pos = new Vector2(
                Random.Shared.Next(0, 240),
                Random.Shared.Next(0, 135)
            );
            _stars.Add(new Star
            {
                Position = pos,
                LastPosition = pos,
                Size = MathF.Pow(MathUtil.InverseLerp01(0, 500, i), 20f) * 3f,
                ColorIndex = Random.Shared.Next(0, _colors.Length)
            });
        }
    }

    private struct Star()
    {
        public required Vector2 Position;
        public required Vector2 LastPosition;
        public required float Size;
        public required int ColorIndex;
        public float Seed = Random.Shared.NextSingle();
    }

    public void Update()
    {
        for (var i = 0; i < _stars.Count; i++)
        {
            var star = _stars[i];
            star.LastPosition = star.Position;
            star.Position.Y -= MathHelper.Max(star.Size, star.Seed * 0.15f) * Speed;
            Vector2 speed = star.Position - star.LastPosition;

            bool reset = false;
            switch (star.Position.Y)
            {
                case < 0:
                    star.Position.Y = 135;
                    reset = true;
                    break;
                case > 135:
                    star.Position.Y = 0;
                    reset = true;
                    break;
            }
            if (reset)
            {
                star.Position.X = Random.Shared.Next(0, 240);
                star.LastPosition = star.Position - speed;
                star.ColorIndex = Random.Shared.Next(0, _colors.Length);
            }
            _stars[i] = star;
        }

        if (!HandleVelocity) return;
        int dir = Keybindings.Down.IsDown ? 1 : 0 - (Keybindings.Up.IsDown ? 1 : 0);
        if (!DoInput) dir = 0;

        Speed = MathUtil.ExpDecay(Speed, (dir * MaxSpeed) + IdleSpeed, 1f, 1f / 60f);
    }

    public void PreDraw(SpriteBatch spriteBatch)
    {
        _graphicsDevice.SetRenderTarget(_renderTarget);
        _graphicsDevice.Clear(ColorUtil.CreateFromHex(0x110928));
        spriteBatch.Begin(samplerState: SamplerState.LinearClamp);
        {
            foreach (Star star in _stars)
            {
                Vector2 speed = star.Position - star.LastPosition;
                float xScale = MathHelper.Clamp(2f / Math.Abs(speed.Y), 0f, 1f);
                float yScale = MathHelper.Max(Math.Abs(speed.Y) / 2f, 1f);

                int xIndex = MathUtil.FloorToInt(star.Size) * 5;
                int yIndex = (int)(4 - xScale * 4) * 5;
                Point srcPoint = new Point(xIndex, yIndex);

                Vector2 pos = star.Position;
                if (DoParallax)
                {
                    pos = MainGame.GetCursorParallaxValue(pos, MathUtil.InverseLerp01(0f, 3f, star.Size) * -100);
                }

                spriteBatch.Draw(
                    _starTexture,
                    Vector2.Round(pos),
                    new Rectangle(
                        srcPoint,
                        new Point(5, 5)
                    ),
                    ColorUtil.CreateFromHex(0x270b51),
                    0f,
                    Vector2.One * 2,
                    new Vector2(1, yScale),
                    SpriteEffects.None,
                    0f
                );

                spriteBatch.Draw(
                    _starTexture,
                    Vector2.Round(pos),
                    new Rectangle(
                        srcPoint,
                        new Point(5, 5)
                    ),
                    _colors[star.ColorIndex] * MathF.Pow(MathUtil.InverseLerp01(0f, 3f, star.Size), 0.2f),
                    0f,
                    Vector2.One * 2,
                    new Vector2(1, yScale),
                    SpriteEffects.None,
                    0f
                );
            }
        }
        spriteBatch.End();
        _graphicsDevice.Reset();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
    }
}
