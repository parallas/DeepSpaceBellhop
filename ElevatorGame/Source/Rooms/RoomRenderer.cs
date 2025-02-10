using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Rooms;

public class RoomRenderer
{
    public RoomDef Def { get; set; }
    
    private RenderTarget2D _roomRenderTarget;
    private AsepriteFile _spriteFile;
    private Sprite[] _layerSprites = [];

    private Effect _roomEffects;

    public static readonly uint[] ColorsC64 =
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

    public static readonly uint[] ColorsC64Dark =
    [
        0x000000,
        0x883932,
        0x8b3f96,
        0x55a049,
        0x40318d,
        0x8b5429,
        // 0x574200,
    ];
    
    public static readonly uint[] ColorsC64Light =
    [
        0xffffff,
        0x67b6bd,
        0xbfce72,
        0xb86962,
        0x94e089,
        0x7869c4,
    ];

    public static readonly (uint, uint)[] BannedColorCombos =
    [
        (0x67b6bd, 0x94e089),
        (0x67b6bd, 0x55a049),
        (0x55a049, 0x7869c4),
        (0x67b6bd, 0x7869c4),
        (0x94e089, 0xbfce72),
        (0x8b3f96, 0x7869c4),
        (0x8b5429, 0x7869c4),
        (0xb86962, 0x55a049),
        (0x883932, 0x7869c4),
        (0x8b3f96, 0xb86962),
        (0x8b5429, 0xb86962),
    ];

    private Color _randomColor1;
    private Color _randomColor2;

    private EffectParameter _effectColor1Param;
    private EffectParameter _effectColor2Param;

    private struct RoomSpriteUserData
    {
        public int depth { get; set; } = 70;

        public RoomSpriteUserData(int depth = 70)
        {
            this.depth = depth;
        }
    }
    private RoomSpriteUserData[] _layerUserDatas;

    public void LoadContent()
    {
        _roomEffects = ContentLoader.Load<Effect>("shaders/roomrender");
        Debug.Assert(_roomEffects != null, nameof(_roomEffects) + " != null");
        _effectColor1Param = _roomEffects.Parameters["Color1"];
        _effectColor2Param = _roomEffects.Parameters["Color2"];
    }

    public void SetDefinition(RoomDef def)
    {
        Def = def;
        
        _randomColor1 = ColorUtil.CreateFromHex(ColorsC64Dark[Def.ColorIndex1]);
        _randomColor2 = ColorUtil.CreateFromHex(ColorsC64Light[Def.ColorIndex2]);
        _effectColor1Param.SetValue(_randomColor1.ToVector3());
        _effectColor2Param.SetValue(_randomColor2.ToVector3());
        
        _spriteFile = ContentLoader.Load<AsepriteFile>(Def.SpritePath)!;
        _layerSprites = _spriteFile.Layers.ToArray()
            .Select(layer => _spriteFile.CreateSprite(MainGame.Graphics.GraphicsDevice, def.FrameNumber, [layer.Name]))
            .ToArray();

        _layerUserDatas = new RoomSpriteUserData[_layerSprites.Length];
        for (var index = 0; index < _layerSprites.Length; index++)
        {
            AsepriteUserData userData = _spriteFile.Layers[index].UserData;
            if (!userData.HasText) continue;

            string userDataText = userData.Text;
            try
            {
                var userDataObject = JsonSerializer.Deserialize<RoomSpriteUserData>(userDataText);
                _layerUserDatas[index] = userDataObject;
            }
            catch
            {
                Console.Error.WriteLine($"Failed to parse user data for layer {index}");
            }
        }
    }

    public void PreRender(SpriteBatch spriteBatch)
    {
        _roomRenderTarget ??= new RenderTarget2D(MainGame.Graphics.GraphicsDevice, 256, 128);

        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_roomRenderTarget);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _roomEffects);
        {
            for (var index = 0; index < _layerSprites.Length; index++)
            {
                var sprite = _layerSprites[index];
                int parallaxOffset = 70;
                parallaxOffset = _layerUserDatas[index].depth;

                sprite.Draw(spriteBatch, MainGame.Camera.GetParallaxPosition(Vector2.Zero, parallaxOffset));
            }
        }
        spriteBatch.End();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_roomRenderTarget, MainGame.Camera.GetParallaxPosition(new Vector2(0, 32), 0), Color.White);
    }
}
