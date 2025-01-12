using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElevatorGame;

public static class SaveManager
{
    public static SaveData SaveData => saveData;

    public static bool SaveFileExists => File.Exists(_saveFilePath);

    public static SettingsData Settings => settings;

    public static bool SettingsFileExists => File.Exists(_settingsFilePath);

    public delegate void SaveDataEvent(ref SaveData? saveData);
    public delegate void SettingsDataEvent(ref SettingsData? settings);

    public static event SaveDataEvent OnSave;
    public static event SaveDataEvent OnLoad;

    public static event SettingsDataEvent OnSaveSettings;
    public static event SettingsDataEvent OnLoadSettings;

    private static readonly string _saveFilePath = Path.Combine(FileLocations.ProgramPath, "save.json");
    private static SaveData saveData;

    private static readonly string _settingsFilePath = Path.Combine(FileLocations.ProgramPath, "settings.json");
    private static SettingsData settings;

    public static JsonSerializerOptions SerializerOptions => new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
#if DEBUG
        WriteIndented = true,
#else
        WriteIndented = false,
#endif
        TypeInfoResolver = SaveDataSourceGenContext.Default,
    };

    public static void DeleteSaveFile()
    {
        if (SaveFileExists)
        {
            File.Delete(_saveFilePath);
        }
    }

    public static void DeleteSettingsFile()
    {
        if (SettingsFileExists)
        {
            File.Delete(_settingsFilePath);
        }
    }

    public static void Load()
    {
        if (!SaveFileExists)
        {
            saveData = new();
            OnLoad?.Invoke(ref saveData);

            File.WriteAllText(
                _saveFilePath,
                JsonSerializer.Serialize(
                    SaveData,
                    SerializerOptions
                )
            );
            return;
        }

        saveData = JsonSerializer.Deserialize<SaveData>(
            File.ReadAllText(_saveFilePath),
            SerializerOptions
        );

        OnLoad?.Invoke(ref saveData);
    }

    public static void Save()
    {
        OnSave?.Invoke(ref saveData);

        File.WriteAllText(
            _saveFilePath,
            JsonSerializer.Serialize(
                SaveData,
                SerializerOptions
            )
        );
    }

    public static void LoadSettings()
    {
        if (!SettingsFileExists)
        {
            settings = new();
            OnLoadSettings?.Invoke(ref settings);

            File.WriteAllText(
                _settingsFilePath,
                JsonSerializer.Serialize(
                    Settings,
                    SerializerOptions
                )
            );
            return;
        }

        settings = JsonSerializer.Deserialize<SettingsData>(
            File.ReadAllText(_settingsFilePath),
            SerializerOptions
        );

        OnLoadSettings?.Invoke(ref settings);
    }

    public static void SaveSettings()
    {
        OnSaveSettings?.Invoke(ref settings);

        File.WriteAllText(
            _settingsFilePath,
            JsonSerializer.Serialize(
                Settings,
                SerializerOptions
            )
        );
    }
}
