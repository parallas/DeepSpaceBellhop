using System.Collections.Generic;
using ElevatorGame.Source.Rooms;

namespace ElevatorGame;

public class SaveData
{
    public int Day { get; set; }

    public List<RoomDef> Rooms { get; set; } = [];

    public string LanguagePreference { get; set; } = "en-us";
}
