using System.Reflection;

using Engine;

namespace ElevatorGame;

public static class FileLocations
{
    public static string ProgramPath => AppDomain.CurrentDomain.BaseDirectory;

    public static string LocalMods => Path.Combine(ProgramPath, "mods");

    public static string? WorkshopMods => SteamManager.GetWorkshopModsPath();

    public static string ModsCache => Path.Combine(ProgramPath, ".mods_cache");
}
