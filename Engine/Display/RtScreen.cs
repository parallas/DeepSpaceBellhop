using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Display;

public static class RtScreen
{
    public static void DrawWithRtOnScreen(RenderTarget2D renderTarget2D, GraphicsDeviceManager graphics, SpriteBatch spriteBatch, Effect postProcessingEffect, Color color, Action drawCode)
    {
        GraphicsDevice graphicsDevice = graphics.GraphicsDevice;

        Rectangle bounds = graphicsDevice.PresentationParameters.Bounds;
        int screenWidth = bounds.Width;
        int screenHeight = bounds.Height;
        int rtWidth = renderTarget2D.Width;
        int rtHeight = renderTarget2D.Height;
        
        graphicsDevice.SetRenderTarget(renderTarget2D);
        drawCode?.Invoke();

        int nearestScale = (int)Math.Floor((decimal)screenHeight / rtHeight);
        RenderTarget2D scaledRt = new RenderTarget2D(graphicsDevice, rtWidth * nearestScale, rtHeight * nearestScale);
        graphicsDevice.SetRenderTarget(scaledRt);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: postProcessingEffect);
        {
            spriteBatch.Draw(renderTarget2D, new Rectangle(0, 0, scaledRt.Width, scaledRt.Height), color);
        }
        spriteBatch.End();
        graphicsDevice.Reset();

        float aspectRatio = (float)rtWidth / (float)rtHeight;
        int newWidth = (int)(screenHeight * aspectRatio);
        int newHeight = screenHeight;
        
        int xOffset = screenWidth / 2 - newWidth / 2;
        int yOffset = screenHeight / 2 - newHeight / 2;
        spriteBatch.Begin(samplerState: SamplerState.AnisotropicClamp);
        {
            spriteBatch.Draw(scaledRt, new Rectangle(xOffset, yOffset, newWidth, newHeight), Color.White);
        }
        spriteBatch.End();
        
        scaledRt.Dispose();
    }
}