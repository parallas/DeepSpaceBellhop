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
                AngryPhrases = [new("Actually, I think she might be on this floor...")],
                AngryIconPosition = new(-13, -72),
            },
            new()
            {
                Name = "Greenulet",
                SpritePath = "graphics/characters/GreenAxolotl",
                WalkSpeed = 6,
                EnterPhrases = [new("Hi, can you take me to floor $floorNumDest, please?")],
                ExitPhrases = [new("Bye bye!")],
                AngryPhrases = [new("At this rate I'll never find him. See ya...")],
                AngryIconPosition = new(-13, -69),
            },
            new()
            {
                Name = "EmalynCat",
                SpritePath = "graphics/characters/EmalynCat",
                WalkSpeed = 6,
                EnterPhrases = [
                    new("I want to see the world through the eyes of a normal girl!"),
                    new("Maow Maow Maow!")
                ],
                ExitPhrases = [
                    new("Thank You!"),
                    new("Haven't felt this happy in so long!")
                ],
                AngryPhrases = [
                    new("Did I scare you? Well I'm sorry. But I'm not sorry enough to stop!"),
                    new("I feel like I'm stuck between dangerous extremes...")
                ],
                AngryIconPosition = new(-10, -46),
            },
            new()
            {
                Name = "Kloob",
                SpritePath = "graphics/characters/Kloob",
                WalkSpeed = 2,
                AngryIconPosition = new(-10, -76),
            },
            new()
            {
                Name = "Robot",
                SpritePath = "graphics/characters/Robot",
                WalkSpeed = 6,
                EnterPhrases = [
                    new("var Greeting = \"HELLO HUMAN\", Destination = $floorNumDest;"),
                    new("InvalidCastException: [$floorNumDest] is not a string (Dialog.cs:74);"),
                ],
                ExitPhrases = [
                    new("[GRATITUDE];"),
                    new("The program '[23056] RobotDialog.dll' has exited with code 0 (0x0).")
                ],
                AngryPhrases = [new("Unhandled Exception in thread 'Main': Timed out;")],
                AngryIconPosition = new(-6, -39),
            },
        ];

        foreach (var characterDef in _characterDefs)
        {
            CharacterTable.Add(characterDef.Name, characterDef);
        }
    }
}
