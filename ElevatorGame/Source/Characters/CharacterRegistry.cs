using System.Collections.Generic;
using Engine;

namespace ElevatorGame.Source.Characters;

public static class CharacterRegistry
{
    private static CharacterDef[] _characterDefs;
    public static Dictionary<string, CharacterDef> CharacterTable { get; } = [];

    public static void Init()
    {
        CharacterTable.Clear();

        _characterDefs =
        [
            new CharacterDef
            {
                Name = "Blueulet",
                SpritePath = "graphics/characters/BlueAxolotl",
                WalkSpeed = 10,
                EnterPhrases = [
                    new(LocalizationManager.Get("character.dialog.Blueulet.enter.0"))
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("character.dialog.Blueulet.exit.0"))
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("character.dialog.Blueulet.angry.0"))
                ],
                AngryIconPosition = new(-13, -72),
                Flags = CharacterDef.CharacterFlag.Clumsy
            },
            new CharacterDef
            {
                Name = "Greenulet",
                SpritePath = "graphics/characters/GreenAxolotl",
                WalkSpeed = 6,
                EnterPhrases = [
                    new(LocalizationManager.Get("character.dialog.Greenulet.enter.0"))
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("character.dialog.Greenulet.exit.0"))
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("character.dialog.Greenulet.angry.0"))
                ],
                AngryIconPosition = new(-13, -69),
            },
            new CharacterDef
            {
                Name = "EmalynCat",
                SpritePath = "graphics/characters/EmalynCat",
                WalkSpeed = 6,
                EnterPhrases = [
                    new(LocalizationManager.Get("character.dialog.EmalynCat.enter.0")),
                    new(LocalizationManager.Get("character.dialog.EmalynCat.enter.1")),
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("character.dialog.EmalynCat.exit.0")),
                    new(LocalizationManager.Get("character.dialog.EmalynCat.exit.1")),
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("character.dialog.EmalynCat.angry.0")),
                    new(LocalizationManager.Get("character.dialog.EmalynCat.angry.1")),
                ],
                AngryIconPosition = new(-10, -46),
            },
            new CharacterDef
            {
                Name = "Kloob",
                SpritePath = "graphics/characters/Kloob",
                WalkSpeed = 2,
                AngryIconPosition = new(-10, -76),
                Flags = CharacterDef.CharacterFlag.Clumsy
            },
            new CharacterDef
            {
                Name = "Robot",
                SpritePath = "graphics/characters/Robot",
                WalkSpeed = 6,
                EnterPhrases = [
                    new(LocalizationManager.Get("character.dialog.Robot.enter.0")),
                    new(LocalizationManager.Get("character.dialog.Robot.enter.1")),
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("character.dialog.Robot.exit.0")),
                    new(LocalizationManager.Get("character.dialog.Robot.exit.1")),
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("character.dialog.Robot.angry.0")),
                ],
                AngryIconPosition = new(-6, -39),
            },
            new CharacterDef
            {
                Name = "Slime",
                SpritePath = "graphics/characters/Slime",
                WalkSpeed = 4,
                AngryIconPosition = new(-13, -40),
                Flags = CharacterDef.CharacterFlag.Slimy
            },
        ];

        foreach (var characterDef in _characterDefs)
        {
            CharacterTable.Add(characterDef.Name, characterDef);
        }
    }
}
