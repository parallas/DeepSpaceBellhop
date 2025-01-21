using AsepriteDotNet.Aseprite;

namespace ElevatorGame.Source.BG_Characters;

public static class BgCharacterRegistry
{
    public static Dictionary<string, BgCharacterDef> CharacterTable { get; private set; } = [];

    public static void RegisterCharacterDef(BgCharacterDef characterDefs)
    {
        CharacterTable.Add(characterDefs.Name, characterDefs);
    }

    public static void RegisterCharacterDefs(BgCharacterDef[] characterDefs)
    {
        foreach (var characterDef in characterDefs)
        {
            RegisterCharacterDef(characterDef);
        }
    }

    public static void Init()
    {
        CharacterTable.Clear();

        RegisterCharacterDefs([
            new() { Name = "Mimi", SpritePath = "graphics/characters_bg/Mimi" },
            new() { Name = "Yeti", SpritePath = "graphics/characters_bg/Yeti" },
        ]);
    }

    public static void LoadContent()
    {
        foreach (var characterDefsValue in CharacterTable.Values)
        {
            ContentLoader.Load<AsepriteFile>(characterDefsValue.SpritePath);
        }
    }

    public static BgCharacterDef GetRandomCharacter()
    {
        var random = new Random();
        var randomIndex = random.Next(0, CharacterTable.Count);
        return CharacterTable.Values.ElementAt(randomIndex);
    }
}
