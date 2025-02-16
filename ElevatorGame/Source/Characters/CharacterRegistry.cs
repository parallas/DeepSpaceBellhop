using System.Collections.Generic;
using ElevatorGame.Source.Dialog;
using Engine;

namespace ElevatorGame.Source.Characters;

public static class CharacterRegistry
{
    private static CharacterDef[] _characterDefs;
    public static Dictionary<string, CharacterDef> CharacterTable { get; } = [];

    public static void Init()
    {
        _characterDefs =
        [
            new CharacterDef
            {
                Name = "Blueulet",
                SpritePath = "graphics/characters/BlueAxolotl",
                WalkSpeed = 10,
                AngryIconPosition = new(-13, -72),
                Flags = CharacterDef.CharacterFlag.Clumsy
            },
            new CharacterDef
            {
                Name = "Greenulet",
                SpritePath = "graphics/characters/GreenAxolotl",
                WalkSpeed = 6,
                AngryIconPosition = new(-13, -69),
            },
            new CharacterDef
            {
                Name = "EmalynCat",
                SpritePath = "graphics/characters/EmalynCat",
                WalkSpeed = 6,
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
                AngryIconPosition = new(-11, -49),
            },
            new CharacterDef
            {
                Name = "Alieno",
                SpritePath = "graphics/characters/Alieno",
                WalkSpeed = 4,
                AngryIconPosition = new(-8, -46),
            },
            new CharacterDef
            {
                Name = "Flippy",
                SpritePath = "graphics/characters/Flippy",
                WalkSpeed = 4,
                AngryIconPosition = new(-9, -51),
                Flags = CharacterDef.CharacterFlag.Clumsy | CharacterDef.CharacterFlag.Flippy
            },
        ];

        LocalizationManager.LocalizationDataReloaded += RefreshData;
    }

    public static void RefreshData()
    {
        CharacterTable.Clear();

        foreach (var characterDef in _characterDefs)
        {
            characterDef.EnterPhrases = [..
                from t in GetTokens(characterDef.Name, "enter")
                select new DialogDef(t)
            ];
            characterDef.ExitPhrases = [..
                from t in GetTokens(characterDef.Name, "exit")
                select new DialogDef(t)
            ];
            characterDef.AngryPhrases = [..
                from t in GetTokens(characterDef.Name, "angry")
                select new DialogDef(t)
            ];

            CharacterTable.Add(characterDef.Name, characterDef);
        }
    }

    private static List<string> GetTokens(string name, string category)
    {
        int i = 0;
        string token;
        List<string> tokens = [];

        while (LocalizationManager.TokenExists(token = $"dialog.character.{name}.{category}.{i}"))
        {
            tokens.Add(token);
            i++;
        }

        return tokens;
    }
}
