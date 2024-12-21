// Modified from source: https://github.com/IrishBruse/LDtkMonogame/blob/main/LDtk.LevelViewer/Camera.cs
// License: MIT
// Licensed to: Ethan Conneely - IrishBruse

using System;
using Microsoft.Xna.Framework;

namespace Engine.Display;

public class Camera
{
    float currentShake;
    float shakeMagnitude;
    int shakeTime;

    public Vector2 Position { get; set; } = Vector2.Zero;

    public Vector2 RootOffset { get; set; } = Vector2.Zero;
    
    public Vector2 VisualPosition { get; private set; }

    public float Zoom { get; set; } = 1;

    public Matrix Transform { get; private set; } = new();

    public void SetShake(float shakeMagnitude, int shakeTime)
    {
        if(Math.Abs(shakeMagnitude) >= this.currentShake)
        {
            this.shakeMagnitude = Math.Abs(shakeMagnitude);
            this.currentShake = Math.Abs(shakeMagnitude);
            this.shakeTime = Math.Abs(shakeTime);
        }
    }

    public void AddShake(float shakeMagnitude, int shakeTime)
    {
        this.shakeMagnitude = Math.Abs(shakeMagnitude);
        this.currentShake += Math.Abs(shakeMagnitude);
        this.shakeTime = Math.Abs(shakeTime);
    }

    public void Update()
    {
        Vector2 basePosition = Position + RootOffset;

        Vector2 shakePosition = basePosition - new Vector2(
            (Random.Shared.NextSingle() - 0.5f) * 2 * currentShake,
            (Random.Shared.NextSingle() - 0.5f) * 2 * currentShake
        );

        if(shakeTime > 0)
            currentShake = MathHelper.Max(0, currentShake - ((1f / shakeTime) * shakeMagnitude));
        else
            currentShake = 0;

        if(currentShake == 0)
            shakeTime = 0;

        Vector2 finalPosition = Vector2.Round(shakePosition);
        VisualPosition = finalPosition - RootOffset;

        Transform = Matrix.CreateTranslation(new Vector3(-finalPosition, 0)) * Matrix.CreateScale(Zoom);
    }

    public Vector2 GetParallaxPosition(Vector2 position, float distance)
    {
        return position + Vector2.Round(VisualPosition * (MathUtil.InverseLerp01(0, 100, distance)));
    }
}
