using ElevatorGame.Source.Characters;
using ElevatorGame.Source.Dialog;

namespace ElevatorGame.Source.Days;

public struct DayDef()
{
    public required int FloorCount { get; set; }
    public required int OrderSpawnChancePerTurn { get; set; }
    public required int MaxCountPerSpawn { get; set; }
    public required int CompletionRequirement { get; set; }
    public required string[] CharacterIds { get; set; }
    public SpecialCharacterSpawnInfo[] SpecialCharacters { get; set; }
    public required int StartCharacterCount { get; set; }
    public DialogDef StartDialog { get; set; }
    public bool PunishMistakes { get; set; }
}
