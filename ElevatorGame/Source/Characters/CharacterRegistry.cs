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
                EnterPhrases = [new("Can you take me to this floor?")],
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
            new()
            {
                Name = "EmalynCat",
                SpritePath = "graphics/characters/EmalynCat",
                WalkSpeed = 6,
                EnterPhrases = [new("I want to see the world through the eyes of a normal girl!")],
                ExitPhrases = [new("Thank You!")],
                AngryPhrases = [
                    new("Did I scare you? Well I'm sorry. But I'm not sorry enough to stop!"),
                    new("I feel like I'm stuck between dangerous extremes...")
                ]
            },
            new()
            {
                Name = "Kloob",
                SpritePath = "graphics/characters/Kloob",
                WalkSpeed = 2,
            },
        ];

        foreach (var characterDef in _characterDefs)
        {
            CharacterTable.Add(characterDef.Name, characterDef);
        }
    }
}
