using System;
using System.Diagnostics;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Rooms;

public class RoomRenderer
{
    private RenderTarget2D _roomRenderTarget;
    private Sprite _roomSprite;
    
    private Effect _roomEffects;

    private uint[] _colorsC64 =
    [
        0x000000,
        0xffffff,
        0x883932,
        0x67b6bd,
        0x8b3f96,
        0x55a049,
        0x40318d,
        0xbfce72,
        0x8b5429,
        0x574200,
        0xb86962,
        0x505050,
        0x787878,
        0x94e089,
        0x7869c4,
        0x9f9f9f
    ];

    private Color _randomColor1;
    private Color _randomColor2;

    private EffectParameter _effectColor1Param;
    private EffectParameter _effectColor2Param;

    public RoomRenderer()
    {
        Randomize();
    }

    public void Randomize()
    {
        int colorIndex1 = Random.Shared.Next(_colorsC64.Length);

        int colorIndex2 = colorIndex1;
        while (colorIndex2 == colorIndex1)
        {
            colorIndex2 = Random.Shared.Next(_colorsC64.Length);
        }

        _randomColor1 = ColorUtil.CreateFromHex(_colorsC64[colorIndex1]);
        _randomColor2 = ColorUtil.CreateFromHex(_colorsC64[colorIndex2]);
    }

    public void PreRender(SpriteBatch spriteBatch)
    {
        _roomRenderTarget ??= new RenderTarget2D(MainGame.Graphics.GraphicsDevice, 128, 128);
        _roomSprite ??=
            ContentLoader.Load<AsepriteFile>("graphics/concepting/RoomTest")!.CreateSprite(
                MainGame.Graphics.GraphicsDevice, 0, true);
        _roomEffects ??= ContentLoader.Load<Effect>("shaders/roomrender");
        
        Debug.Assert(_roomEffects != null, nameof(_roomEffects) + " != null");
        _effectColor1Param ??= _roomEffects.Parameters["Color1"];
        _effectColor2Param ??= _roomEffects.Parameters["Color2"];
        
        _effectColor1Param.SetValue(_randomColor1.ToVector3());
        _effectColor2Param.SetValue(_randomColor2.ToVector3());
        
        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_roomRenderTarget);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _roomEffects);
        {
            _roomSprite.Draw(spriteBatch, MainGame.Camera.GetParallaxPosition(Vector2.Zero, 70));
        }
        spriteBatch.End();
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_roomRenderTarget, new Vector2(64, 32), Color.White);
    }
}