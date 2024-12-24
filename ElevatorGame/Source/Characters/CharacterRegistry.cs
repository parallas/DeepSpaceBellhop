using System.Collections.Generic;

namespace ElevatorGame.Source.Characters;

public static class CharacterRegistry
{
    private static CharacterDef[] CharacterDefs;
    public static Dictionary<string, CharacterDef> CharacterTable { get; } = new();

    public static void Init()
    {
        CharacterDefs =
        [
            new()
            {
                Name = "Blueulet",
                SpritePath = "graphics/characters/BlueAxolotl",
                WalkSpeed = 10,
                EnterPhrases = [new("So, anyway, if you’re looking to date me, you need to meet my mother first.")],
                ExitPhrases = [new("Sorry, it’s a requirement. Do you want to go see her now?")]
            },
            new()
            {
                Name = "Greenulet",
                SpritePath = "graphics/characters/GreenAxolotl",
                WalkSpeed = 6,
                EnterPhrases = [new("Hi :3")],
                ExitPhrases = [new("Bye :3 *skips out of elevator*")]
            },
        ];

        foreach (var characterDef in CharacterDefs)
        {
            CharacterTable.Add(characterDef.Name, characterDef);
        }
    }
}
