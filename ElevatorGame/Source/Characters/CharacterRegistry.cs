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
                    new(LocalizationManager.Get("dialog.character.Blueulet.enter.0"))
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("dialog.character.Blueulet.exit.0"))
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("dialog.character.Blueulet.angry.0"))
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
                    new(LocalizationManager.Get("dialog.character.Greenulet.enter.0"))
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("dialog.character.Greenulet.exit.0"))
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("dialog.character.Greenulet.angry.0"))
                ],
                AngryIconPosition = new(-13, -69),
            },
            new CharacterDef
            {
                Name = "EmalynCat",
                SpritePath = "graphics/characters/EmalynCat",
                WalkSpeed = 6,
                EnterPhrases = [
                    new(LocalizationManager.Get("dialog.character.EmalynCat.enter.0")),
                    new(LocalizationManager.Get("dialog.character.EmalynCat.enter.1")),
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("dialog.character.EmalynCat.exit.0")),
                    new(LocalizationManager.Get("dialog.character.EmalynCat.exit.1")),
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("dialog.character.EmalynCat.angry.0")),
                    new(LocalizationManager.Get("dialog.character.EmalynCat.angry.1")),
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
                    new(LocalizationManager.Get("dialog.character.Robot.enter.0")),
                    new(LocalizationManager.Get("dialog.character.Robot.enter.1")),
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("dialog.character.Robot.exit.0")),
                    new(LocalizationManager.Get("dialog.character.Robot.exit.1")),
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("dialog.character.Robot.angry.0")),
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
            new CharacterDef
            {
                Name = "ShroomKing",
                SpritePath = "graphics/characters/ShroomKing",
                WalkSpeed = 6,
                EnterPhrases = [
                    new(LocalizationManager.Get("dialog.character.ShroomKing.enter.0")),
                    new(LocalizationManager.Get("dialog.character.ShroomKing.enter.1")),
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("dialog.character.ShroomKing.exit.0")),
                    new(LocalizationManager.Get("dialog.character.ShroomKing.exit.1")),
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("dialog.character.ShroomKing.angry.0")),
                    new(LocalizationManager.Get("dialog.character.ShroomKing.angry.1")),
                ],
                AngryIconPosition = new(-13, -74),
                Flags = CharacterDef.CharacterFlag.Toxic
            },
            new CharacterDef
            {
                Name = "Benbo",
                SpritePath = "graphics/characters/Benbo",
                WalkSpeed = 16,
                AngryIconPosition = new(-4, -12)
            },
            new CharacterDef
            {
                Name = "Beebo",
                SpritePath = "graphics/characters/Beebo",
                WalkSpeed = 6,
                EnterPhrases = [
                    new(LocalizationManager.Get("dialog.character.Beebo.enter.0")),
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("dialog.character.Beebo.exit.0")),
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("dialog.character.Beebo.angry.0")),
                ],
                AngryIconPosition = new(-13, -69),
            },
            new CharacterDef
            {
                Name = "Hourglass",
                SpritePath = "graphics/characters/Hourglass",
                WalkSpeed = 8,
                AngryIconPosition = new(-15, -76),
                Flags = CharacterDef.CharacterFlag.Psychedelic
            },
            new CharacterDef
            {
                Name = "JellyfishGirl",
                SpritePath = "graphics/characters/JellyfishGirl",
                WalkSpeed = 4,
                EnterPhrases = [
                    new(LocalizationManager.Get("dialog.character.JellyfishGirl.enter.0")),
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("dialog.character.JellyfishGirl.exit.0")),
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("dialog.character.JellyfishGirl.angry.0")),
                ],
                AngryIconPosition = new(-13, -64),
            },
            new CharacterDef
            {
                Name = "SeaDragon",
                SpritePath = "graphics/characters/SeaDragon",
                WalkSpeed = 6,
                AngryIconPosition = new(-12, -58),
            },
            new CharacterDef
            {
                Name = "Box",
                SpritePath = "graphics/characters/Box",
                WalkSpeed = 4,
                EnterPhrases = [
                    new("..."),
                ],
                ExitPhrases = [
                    new("..."),
                ],
                AngryPhrases = [
                    new("..."),
                ],
                AngryIconPosition = new(-11, -49),
            },
            new CharacterDef
            {
                Name = "Alieno",
                SpritePath = "graphics/characters/Alieno",
                WalkSpeed = 4,
                EnterPhrases = [
                    new(LocalizationManager.Get("dialog.character.Alieno.enter.0")),
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("dialog.character.Alieno.exit.0")),
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("dialog.character.Alieno.angry.0")),
                ],
                AngryIconPosition = new(-8, -46),
            },
            new CharacterDef
            {
                Name = "Flippy",
                SpritePath = "graphics/characters/Flippy",
                WalkSpeed = 4,
                EnterPhrases = [
                    new(LocalizationManager.Get("dialog.character.Flippy.enter.0")),
                ],
                ExitPhrases = [
                    new(LocalizationManager.Get("dialog.character.Flippy.exit.0")),
                ],
                AngryPhrases = [
                    new(LocalizationManager.Get("dialog.character.Flippy.angry.0")),
                ],
                AngryIconPosition = new(-9, -51),
                Flags = CharacterDef.CharacterFlag.Clumsy | CharacterDef.CharacterFlag.Flippy
            },
        ];

        foreach (var characterDef in _characterDefs)
        {
            CharacterTable.Add(characterDef.Name, characterDef);
        }
    }
}
