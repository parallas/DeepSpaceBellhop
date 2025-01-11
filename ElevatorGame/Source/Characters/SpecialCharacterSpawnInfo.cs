namespace ElevatorGame.Source.Characters;

public struct SpecialCharacterSpawnInfo()
{
    public required string CharacterName { get; set; }
    public int ChanceToSpawn { get; set; } = 100;
    public int MinFloor { get; set; } = 0;
    public int MaxFloor { get; set; } = 0;
    public int MinSpawnCompletionPercent { get; set; } = 0;
    public int MaxSpawnCompletionPercent { get; set; } = 0;
}
