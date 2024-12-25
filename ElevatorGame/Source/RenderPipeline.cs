using System;
using Engine.Display;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source;

public static class RenderPipeline
{
    public static Point RenderBufferSize { get; private set; }
    private static RenderTarget2D _gameSceneRt;
    private static RenderTarget2D _beforeUiRt;
    private static RenderTarget2D _uiRt;
    private static RenderTarget2D _gameWithUiRt;
    private static RenderTarget2D _postProcessRt;
    private static RenderTarget2D _renderTarget;

    private static GraphicsDevice _graphics;

    public static void Init(Point size)
    {
        RenderBufferSize = size;
    }

    public static void LoadContent(GraphicsDevice graphicsDevice)
    {
        _graphics = graphicsDevice;
        _renderTarget = new RenderTarget2D(graphicsDevice, RenderBufferSize.X, RenderBufferSize.Y);
        _gameSceneRt = new RenderTarget2D(graphicsDevice, RenderBufferSize.X, RenderBufferSize.Y);
        _beforeUiRt = new RenderTarget2D(graphicsDevice, RenderBufferSize.X, RenderBufferSize.Y);
        _uiRt = new RenderTarget2D(graphicsDevice, RenderBufferSize.X, RenderBufferSize.Y);
        _gameWithUiRt = new RenderTarget2D(graphicsDevice, RenderBufferSize.X, RenderBufferSize.Y);
        _postProcessRt = new RenderTarget2D(graphicsDevice, RenderBufferSize.X, RenderBufferSize.Y);
    }

    public static void DrawBeforeUI(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Effect effect, Action drawAction)
    {
        graphicsDevice.SetRenderTarget(_gameSceneRt);
        drawAction?.Invoke();
        graphicsDevice.Reset();

        graphicsDevice.SetRenderTarget(_beforeUiRt);
        graphicsDevice.Clear(new Color(new Vector3(120, 105, 196)));
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: effect);
        {
            spriteBatch.Draw(_gameSceneRt, Vector2.Zero, Color.White);
        }
        spriteBatch.End();
    }

    public static void DrawUI(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Action drawAction)
    {
        graphicsDevice.SetRenderTarget(_uiRt);
        drawAction?.Invoke();
        graphicsDevice.Reset();
    }

    public static void DrawPostProcess(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Effect effect)
    {
        graphicsDevice.SetRenderTarget(_gameWithUiRt);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        {
            spriteBatch.Draw(_beforeUiRt, Vector2.Zero, Color.White);
            spriteBatch.Draw(_uiRt, Vector2.Zero, Color.White);
        }
        spriteBatch.End();

        graphicsDevice.SetRenderTarget(_postProcessRt);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: effect);
        {
            spriteBatch.Draw(_gameWithUiRt, Vector2.Zero, Color.White);
        }
        spriteBatch.End();
        graphicsDevice.Reset();
    }

    public static void DrawFinish(SpriteBatch spriteBatch, GraphicsDeviceManager graphicsDeviceManager)
    {
        var graphicsDevice = graphicsDeviceManager.GraphicsDevice;

        RtScreen.DrawWithRtOnScreen(_renderTarget, graphicsDeviceManager, spriteBatch, null, Color.White, () =>
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            {
                spriteBatch.Draw(_postProcessRt, Vector2.Zero, Color.White);
            }
            spriteBatch.End();
        });
    }
}
