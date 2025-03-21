using System.Reflection;

using Engine;

namespace ElevatorGame;

public static class FileLocations
{
    private static readonly string assemblyLocation = Assembly.GetEntryAssembly().Location;

    public static string ProgramPath => Path.GetDirectoryName(assemblyLocation);

    public static string LocalMods => Path.Combine(ProgramPath, "mods");

    public static string? WorkshopMods => SteamManager.GetWorkshopModsPath();

    public static string ModsCache => Path.Combine(ProgramPath, ".mods_cache");
}
