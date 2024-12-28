using System.IO;
using System.Text.Json;

namespace ElevatorGame;

public static class SaveManager
{
    public static SaveData SaveData { get; private set; }

    public static void Load()
    {
        var filePath = Path.Combine(FileLocations.ProgramPath, "save.json");
        if(!File.Exists(filePath))
        {
            SaveData = new();
            return;
        }

        SaveData = JsonSerializer.Deserialize<SaveData>(File.OpenRead(filePath));
    }

    public static void Save()
    {
        File.WriteAllText(
            Path.Combine(FileLocations.ProgramPath, "save.json"),
            JsonSerializer.Serialize(SaveData)
        );
    }
}
