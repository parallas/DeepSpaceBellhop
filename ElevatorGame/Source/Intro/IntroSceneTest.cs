using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public class IntroSceneTest : IntroScene
{
    private RenderTarget2D _rt;

    private Matrix worldMatrix, viewMatrix, projectionMatrix;
    private VertexPositionColor[] triangleVertices;
    private BasicEffect basicEffect;

    public override void LoadContent()
    {
        _rt = new(MainGame.Graphics.GraphicsDevice, 240, 135);

        worldMatrix = Matrix.Identity;

        viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 50), Vector3.Zero, Vector3.Up);

        projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            MainGame.Graphics.GraphicsDevice.Viewport.AspectRatio,
            1.0f, 300.0f
        );

        basicEffect = new BasicEffect(MainGame.Graphics.GraphicsDevice);

        // primitive color
        basicEffect.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
        basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
        basicEffect.SpecularColor = new Vector3(0.25f, 0.25f, 0.25f);
        basicEffect.SpecularPower = 5.0f;
        basicEffect.Alpha = 1.0f;

        // The following MUST be enabled if you want to color your vertices
        basicEffect.VertexColorEnabled = true;

        // Use the built in 3 lighting mode provided with BasicEffect            
        // basicEffect.EnableDefaultLighting();

        triangleVertices = [
            new(new Vector3(0f, 0f, 0f), Color.Cyan),
            new(new Vector3(10f, 10f, 0f), Color.Magenta),
            new(new Vector3(10f, 0f, -5f), Color.White)
        ];
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
                0,
                50 * MathF.Sin(MainGame.Frame * 0.05f)
            ),
            Vector3.Zero,
            Vector3.Up
        );

        basicEffect.World = worldMatrix;
        basicEffect.View = viewMatrix;
        basicEffect.Projection = projectionMatrix;

        RasterizerState rasterizerState = new RasterizerState();
        rasterizerState.CullMode = CullMode.None;
        MainGame.Graphics.GraphicsDevice.RasterizerState = rasterizerState;
        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            MainGame.Graphics.GraphicsDevice.DrawUserPrimitives(
                PrimitiveType.TriangleList,
                triangleVertices,
                0,
                1,
                VertexPositionColor.VertexDeclaration
            );
        }

        MainGame.Graphics.GraphicsDevice.SetRenderTarget(null);
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_rt, Vector2.Zero, Color.White);
    }
}
