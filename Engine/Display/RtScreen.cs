using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Display;

public static class RtScreen
{
    private static RenderTarget2D _scaledRt;
    private static RenderTarget2D _screenSpaceEffectRt;
    private static RenderTarget2D _lastFrameRt;

    public static void Init()
    {

    }

    public static void UnloadContent()
    {
        _scaledRt?.Dispose();
        _screenSpaceEffectRt?.Dispose();
        _lastFrameRt?.Dispose();
    }

    public static void DrawWithRtOnScreen(RenderTarget2D renderTarget2D, GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Effect postProcessingEffect, Effect screenSpaceEffect, Color color, Action drawCode)
    {
        GraphicsDevice graphicsDevice = graphics.GraphicsDevice;

        Rectangle bounds = graphicsDevice.PresentationParameters.Bounds;
        int screenWidth = bounds.Width;
        int screenHeight = bounds.Height;
        int rtWidth = renderTarget2D.Width;
        int rtHeight = renderTarget2D.Height;

        float widthRatio = (float)screenWidth / rtWidth;
        float heightRatio = (float)screenHeight / rtHeight;

        int longestGameSize = MathHelper.Max(rtWidth, rtHeight);
        int longestScreenSize = MathHelper.Max(screenWidth, screenHeight);

        graphicsDevice.SetRenderTarget(renderTarget2D);
        drawCode?.Invoke();

        int nearestScale = (int)Math.Floor((decimal)MathHelper.Min(widthRatio, heightRatio));
        int scaledRtWidth = rtWidth * nearestScale;
        int scaledRtHeight = rtHeight * nearestScale;
        if (_scaledRt is null || _screenSpaceEffectRt is null ||
            _scaledRt.Width != scaledRtWidth || _scaledRt.Height != scaledRtHeight)
        {
            _scaledRt?.Dispose();
            _scaledRt = new RenderTarget2D(graphicsDevice, scaledRtWidth, scaledRtHeight);

            _screenSpaceEffectRt?.Dispose();
            _screenSpaceEffectRt = new RenderTarget2D(graphicsDevice, scaledRtWidth, scaledRtHeight);

            _lastFrameRt?.Dispose();
            _lastFrameRt = new RenderTarget2D(graphicsDevice, scaledRtWidth, scaledRtHeight);

            graphicsDevice.SetRenderTarget(_lastFrameRt);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            {
                spriteBatch.Draw(_scaledRt, new Rectangle(0, 0, _scaledRt.Width, _scaledRt.Height), color);
            }
            spriteBatch.End();
            graphicsDevice.Reset();
        }
        graphicsDevice.SetRenderTarget(_scaledRt);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: postProcessingEffect);
        {
            spriteBatch.Draw(renderTarget2D, new Rectangle(0, 0, _scaledRt.Width, _scaledRt.Height), color);
        }
        spriteBatch.End();
        graphicsDevice.Reset();

        screenSpaceEffect?.Parameters["GameTime"]?.SetValue(DateTime.Now.Ticks);
        screenSpaceEffect?.Parameters["BackBufferResolution"]?.SetValue(new Vector4(rtWidth, rtHeight,
            (float)rtWidth / rtHeight, (float)rtHeight / rtWidth));
        screenSpaceEffect?.Parameters["LastFrameTexture"]?.SetValue(_lastFrameRt);
        graphicsDevice.SetRenderTarget(_screenSpaceEffectRt);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: screenSpaceEffect);
        {
            spriteBatch.Draw(_scaledRt, new Rectangle(0, 0, _scaledRt.Width, _scaledRt.Height), color);
        }
        spriteBatch.End();
        graphicsDevice.Reset();

        graphicsDevice.SetRenderTarget(_lastFrameRt);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        {
            spriteBatch.Draw(_screenSpaceEffectRt, new Rectangle(0, 0, _scaledRt.Width, _scaledRt.Height), color);
        }
        spriteBatch.End();
        graphicsDevice.Reset();

        float screenRatio = (float)screenWidth / screenHeight;
        float gameRatio = (float)rtWidth / rtHeight;

        // assume scale game up by its width
        float aspectRatio = (float)rtWidth / (float)rtHeight;
        int newWidth = (int)(screenHeight * aspectRatio);
        int newHeight = screenHeight;
        if (screenRatio < gameRatio)
        {
            // scale game up by its height
            aspectRatio = (float)rtHeight / (float)rtWidth;
            newHeight = (int)(screenWidth * aspectRatio);
            newWidth = screenWidth;
        }

        int xOffset = screenWidth / 2 - newWidth / 2;
        int yOffset = screenHeight / 2 - newHeight / 2;

        spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
        {
            spriteBatch.Draw(_screenSpaceEffectRt, new Rectangle(xOffset, yOffset, newWidth, newHeight), Color.White);
        }
        spriteBatch.End();
    }

    public static Vector2 ToScreenSpace(Vector2 position, Point renderBufferSize, Rectangle screenBounds)
    {
        // NEVER EVER TOUCH THIS CODE AGAIN! WE DON'T UNDERSTAND IT!
        int screenWidth = screenBounds.Width;
        int screenHeight = screenBounds.Height;
        int rtWidth = renderBufferSize.X;
        int rtHeight = renderBufferSize.Y;

        float screenRatio = (float)screenWidth / screenHeight;
        float gameRatio = (float)rtWidth / rtHeight;
        float aspectRatio = (float)rtWidth / (float)rtHeight;
        int newWidth = (int)(screenHeight * aspectRatio);
        int newHeight = screenHeight;
        
        // assume scale game up by its width
        float finalScale = (float)screenHeight / rtHeight;
        if (screenRatio < gameRatio)
        {
            // scale game up by its height
            aspectRatio = (float)rtHeight / (float)rtWidth;
            newHeight = (int)(screenWidth * aspectRatio);
            newWidth = screenWidth;
            
            finalScale = (float)screenWidth / rtWidth;
            float yOffset = ((float)screenHeight / 2 - (float)newHeight / 2) / finalScale;
            return new Vector2(position.X / finalScale, position.Y / finalScale - yOffset);
        }
        
        float xOffset = ((float)screenWidth / 2 - (float)newWidth / 2) / finalScale;
        return new Vector2(position.X / finalScale - xOffset, position.Y / finalScale);
    }
}
