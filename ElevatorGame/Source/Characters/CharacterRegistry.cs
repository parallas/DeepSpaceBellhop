using System.Collections.Generic;

namespace ElevatorGame.Source.Characters;

public static class CharacterRegistry
{
    private static CharacterDef[] _characterDefs;
    public static Dictionary<string, CharacterDef> CharacterTable { get; } = new();

    public static void Init()
    {
        _characterDefs =
        [
            new()
            {
                Name = "Blueulet",
                SpritePath = "graphics/characters/BlueAxolotl",
                WalkSpeed = 10,
                EnterPhrases = [new("C-can you take me to this floor, please?")],
                ExitPhrases = [new("Thank you.")],
                AngryPhrases = [new("Actually, I think she might be on this floor...")]
            },
            new()
            {
                Name = "Greenulet",
                SpritePath = "graphics/characters/GreenAxolotl",
                WalkSpeed = 6,
                EnterPhrases = [new("Hi, can you take me to floor $floorNumDest, please?")],
                ExitPhrases = [new("Bye bye!")],
                AngryPhrases = [new("At this rate I'll never find him. See ya!")]
            },
        ];

        foreach (var characterDef in _characterDefs)
        {
            CharacterTable.Add(characterDef.Name, characterDef);
        }
    }
}
