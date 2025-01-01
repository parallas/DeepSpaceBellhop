using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElevatorGame;

public static class SaveManager
{
    public static SaveData SaveData => saveData;

    public delegate void SaveDataEvent(ref SaveData? saveData);

    public static event SaveDataEvent OnSave;
    public static event SaveDataEvent OnLoad;

    private static string _filePath = Path.Combine(FileLocations.ProgramPath, "save.json");
    private static SaveData saveData;

    public static JsonSerializerOptions SerializerOptions => new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
#if DEBUG
        WriteIndented = true,
#else
        WriteIndented = false,
#endif
    };

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
            saveData = new();
            OnLoad?.Invoke(ref saveData);
            return;
        }

        saveData = JsonSerializer.Deserialize<SaveData>(
            File.ReadAllText(_filePath),
            SerializerOptions
        );

        OnLoad?.Invoke(ref saveData);
    }

    public static void Save()
    {
        OnSave?.Invoke(ref saveData);

        File.WriteAllText(
            _filePath,
            JsonSerializer.Serialize(
                SaveData,
                SerializerOptions
            )
        );
    }
}
