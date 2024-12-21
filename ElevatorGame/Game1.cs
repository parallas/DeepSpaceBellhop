using Engine.Display;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ElevatorGame;

public class Game1 : Game
{
    public static GraphicsDeviceManager Graphics;
    public static SpriteBatch SpriteBatch;
    
    private RenderTarget2D _renderTarget;

    public Game1()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);

        Window.AllowUserResizing = true;

        _renderTarget = new RenderTarget2D(GraphicsDevice, 240, 135);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {

        RtScreen.DrawWithRtOnScreen(_renderTarget, Graphics, SpriteBatch, () =>
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            {
                SpriteBatch.Draw(
                    Content.Load<Texture2D>("graphics/ElevatorGameMockup"), Vector2.Zero, Color.White
                );
            }
            SpriteBatch.End();
        });


        base.Draw(gameTime);
    }
}
