using ElevatorGame.Source.Dialog;

namespace ElevatorGame.Source.Characters;

public struct CharacterDef
{
    public string Name { get; set; }
    public string SpritePath { get; set; }
    public DialogDef[] EnterPhrases { get; set; }
    public DialogDef[] ExitPhrases { get; set; }
}