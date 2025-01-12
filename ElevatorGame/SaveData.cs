using System.Collections.Generic;
using System.Text.Json.Serialization;
using ElevatorGame.Source.Rooms;

namespace ElevatorGame;

public class SaveData()
{
    public int Day { get; set; }

    public string LanguagePreference { get; set; } = "en-us";

    public List<RoomDef> Rooms { get; set; } = [];
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
[JsonSerializable(typeof(SettingsData))]
// add additional serializable types here

internal partial class SaveDataSourceGenContext : JsonSerializerContext;
