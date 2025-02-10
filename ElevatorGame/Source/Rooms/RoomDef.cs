using System;
using System.Linq;
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

    public static RoomDef MakeRandom(string filePath, int minIndex = 0, int maxIndex = Int32.MaxValue)
    {
        int colorIndex1 = 0;
        int colorIndex2 = 0;
        do
        {
            colorIndex1 = Random.Shared.Next(RoomRenderer.ColorsC64Dark.Length);
            colorIndex2 = Random.Shared.Next(RoomRenderer.ColorsC64Light.Length);
        } while (RoomRenderer.BannedColorCombos.Any(tuple => 
                     (RoomRenderer.ColorsC64Dark[colorIndex1] == tuple.Item1 && 
                      RoomRenderer.ColorsC64Light[colorIndex2] == tuple.Item2) || 
                     (RoomRenderer.ColorsC64Dark[colorIndex1] == tuple.Item2 && 
                      RoomRenderer.ColorsC64Light[colorIndex2] == tuple.Item1)
                )
        );
        
        var spriteFile = ContentLoader.Load<AsepriteFile>(filePath)!;
        Console.WriteLine(Math.Min(spriteFile.FrameCount, maxIndex + 1));
        int randomFrameIndex = Random.Shared.Next(minIndex, Math.Min(spriteFile.FrameCount, maxIndex + 1));

        return new RoomDef()
        {
            ColorIndex1 = colorIndex1,
            ColorIndex2 = colorIndex2,
            FrameNumber = randomFrameIndex,
            SpritePath = filePath
        };
    }
}
