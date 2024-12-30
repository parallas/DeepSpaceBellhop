using System;
using System.IO;
using System.Text.Json;

namespace ElevatorGame;

public static class SaveManager
{
    public static SaveData SaveData { get; private set; }

    public static event Action<SaveData?> OnSave;
    public static event Action<SaveData?> OnLoad;

    public static void Load()
    {
        var filePath = Path.Combine(FileLocations.ProgramPath, "save.json");
        if(!File.Exists(filePath))
        {
            SaveData = new();
            return;
        }

        SaveData = JsonSerializer.Deserialize<SaveData>(
            File.OpenRead(filePath)
        );

        OnLoad?.Invoke(SaveData);
    }

    public static void Save()
    {
        OnSave?.Invoke(SaveData);

        File.WriteAllText(
            Path.Combine(FileLocations.ProgramPath, "save.json"),
            JsonSerializer.Serialize(SaveData)
        );
    }
}
