using System.Collections;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public class IntroSceneTest : IntroScene
{
    private RenderTarget2D _rt;

    private Matrix worldMatrix, viewMatrix, projectionMatrix;
    private VertexPositionColorTexture[] triangleVertices;
    private BasicEffect basicEffect;

    private SimpleModel model;

    public override void LoadContent()
    {
        _rt = new(MainGame.Graphics.GraphicsDevice, 240, 135);

        worldMatrix = Matrix.CreateTranslation(Vector3.One * -0.5f) * Matrix.CreateScale(10);

        viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 50), Vector3.Zero, Vector3.Up);

        projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            MainGame.Graphics.GraphicsDevice.Viewport.AspectRatio,
            1.0f, 300.0f
        );

        basicEffect = new(MainGame.Graphics.GraphicsDevice)
        {
            // primitive color
            AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f),
            DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f),
            SpecularColor = new Vector3(0.25f, 0.25f, 0.25f),
            SpecularPower = 5.0f,
            Alpha = 1.0f,

            // The following MUST be enabled if you want to color your vertices
            VertexColorEnabled = true
        };

        // Use the built in 3 lighting mode provided with BasicEffect            
        // basicEffect.EnableDefaultLighting();

        model = ContentLoader.Load<SimpleModel>("models/cube");

        // triangleVertices = [
        //     new(new Vector3(0f, 0f, 0f), Color.Cyan, Vector2.UnitY),
        //     new(new Vector3(10f, 10f, 0f), Color.Magenta, Vector2.UnitX),
        //     new(new Vector3(10f, 0f, -5f), Color.White, Vector2.One)
        // ];
    }

    public override IEnumerator GetEnumerator()
    {
        while(true)
        {
            yield return null;
        }
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_rt);
        MainGame.Graphics.GraphicsDevice.Clear(Color.Black);

        viewMatrix = Matrix.CreateLookAt(
            new Vector3(
                50 * MathF.Cos(MainGame.Frame * 0.05f),
                50 * MathF.Sin(MainGame.Frame * 0.05f),
                50 * MathF.Sin(MainGame.Frame * 0.05f)
            ),
            Vector3.Zero,
            Vector3.Up
        );

        basicEffect.World = worldMatrix;
        basicEffect.View = viewMatrix;
        basicEffect.Projection = projectionMatrix;

        RasterizerState rasterizerState = new()
        {
            CullMode = CullMode.CullClockwiseFace
        };
        MainGame.Graphics.GraphicsDevice.RasterizerState = rasterizerState;
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            model.Draw(MainGame.Graphics.GraphicsDevice);
        }

        MainGame.Graphics.GraphicsDevice.SetRenderTarget(null);
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_rt, Vector2.Zero, Color.White);
    }
}
