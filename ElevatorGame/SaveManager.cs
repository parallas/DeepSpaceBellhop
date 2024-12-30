using System;
using System.IO;
using System.Text.Json;

namespace ElevatorGame;

public static class SaveManager
{
    public static SaveData SaveData { get; private set; }

    public static event Action<SaveData?> OnSave;
    public static event Action<SaveData?> OnLoad;

    private static string _filePath = Path.Combine(FileLocations.ProgramPath, "save.json");

    public static void DeleteFile()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }

    public static void Load()
    {
        if (!File.Exists(_filePath))
        {
            SaveData = new();
            return;
        }

        SaveData = JsonSerializer.Deserialize<SaveData>(
            File.ReadAllText(_filePath)
        );

        OnLoad?.Invoke(SaveData);
    }

    public static void Save()
    {
        OnSave?.Invoke(SaveData);

        File.WriteAllText(
            _filePath,
            JsonSerializer.Serialize(SaveData)
        );
    }
}
