using System;
using ElevatorGame.Source.Dialog;
using Microsoft.Xna.Framework;

namespace ElevatorGame.Source.Characters;

public struct CharacterDef()
{
    [Flags]
    public enum CharacterFlag
    {
        None = 0,
        Slimy = 1,
        Clumsy = 2,
        Toxic = 4,
        Psychedelic = 8,
    }

    public required string Name { get; set; }
    public required string SpritePath { get; set; }
    public int WalkSpeed { get; set; } = 8;
    public DialogDef[] EnterPhrases { get; set; } = [];
    public DialogDef[] ExitPhrases { get; set; } = [];
    public DialogDef[] AngryPhrases { get; set; } = [];
    public required Vector2 AngryIconPosition { get; set; }
    public CharacterFlag Flags { get; set; } = CharacterFlag.None;
}
