using FMOD;
using FmodForFoxes;
using FmodForFoxes.Studio;

namespace ElevatorGame.Source;

public static class MusicPlayer
{
    private static readonly Dictionary<string, EventDescription> MusicEventDescriptions = new();
    private static EventInstance _currentMusic;
    private static string? _currentMusicName;

    public static void RegisterEventPath(string key, string path)
    {
        MusicEventDescriptions[key] = StudioSystem.GetEvent(path);
    }

    public static void RegisterEventGuid(string key, string guidString)
    {
        FMOD.Studio.Util.parseID(guidString, out var guid);
        MusicEventDescriptions[key] = StudioSystem.GetEvent(guid);
    }

    public static void UnloadContent()
    {
        _currentMusic?.Stop(true);
        _currentMusic?.Dispose();
    }

    public static void PlayMusic(string key)
    {
        if(key == _currentMusicName)
            return;

        _currentMusicName = key;

        StopMusic(false, true);

        if (!MusicEventDescriptions.TryGetValue(key, out var desc)) return;
        _currentMusic = desc.CreateInstance();
        _currentMusic.Start();
    }

    public static void StopMusic(bool immediate, bool release)
    {
        _currentMusic?.Stop(immediate);
        if (release)
        {
            _currentMusic?.Dispose();
            _currentMusic = null;
        }
    }
}
