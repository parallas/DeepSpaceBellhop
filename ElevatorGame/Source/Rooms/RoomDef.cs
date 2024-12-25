using System;
using System.Text.Json;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using Engine;

namespace ElevatorGame.Source.Rooms;

public struct RoomDef
{
    public string SpritePath { get; set; }
    public int FrameNumber { get; set; }
    public int ColorIndex1 { get; set; }
    public int ColorIndex2 { get; set; }

    public static RoomDef MakeRandom(string filePath)
    {
        int colorIndex1 = Random.Shared.Next(RoomRenderer.ColorsC64.Length);

        int colorIndex2 = colorIndex1;
        while (colorIndex2 == colorIndex1)
        {
            colorIndex2 = Random.Shared.Next(RoomRenderer.ColorsC64.Length);
        }
        
        var spriteFile = ContentLoader.Load<AsepriteFile>(filePath)!;
        int randomFrameIndex = Random.Shared.Next(spriteFile.FrameCount);

        return new RoomDef()
        {
            ColorIndex1 = colorIndex1,
            ColorIndex2 = colorIndex2,
            FrameNumber = randomFrameIndex,
            SpritePath = filePath
        };
    }
}