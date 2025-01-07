using System.Collections.Generic;
using System.Text.Json.Serialization;
using ElevatorGame.Source.Rooms;

namespace ElevatorGame;

public class SaveData()
{
    public int Day { get; set; }

    public string LanguagePreference { get; set; } = "en-us";

    public uint Achievements { get; set; }

    public List<RoomDef> Rooms { get; set; } = [];

    [Flags]
    public enum AchievementFlags : uint
    {
        None = 0b0,
        Efficient = 0b1,
    }
}

// SOURCE GENERATION
[JsonSourceGenerationOptions(
#if DEBUG
    WriteIndented = true
#endif
)]

[JsonSerializable(typeof(SaveData))]
[JsonSerializable(typeof(RoomDef))]
[JsonSerializable(typeof(List<RoomDef>))]
// add additional serializable types here

internal partial class SaveDataSourceGenContext : JsonSerializerContext;
