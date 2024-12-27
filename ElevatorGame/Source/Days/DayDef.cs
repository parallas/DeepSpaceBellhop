namespace ElevatorGame.Source.Days;

public struct DayDef
{
    public required int FloorCount { get; set; }
    public required int OrderSpawnChancePerTurn { get; set; }
    public required int MaxCountPerSpawn { get; set; }
    public required int CompletionRequirement { get; set; }
    public required string[] CharacterIDs { get; set; }
}
