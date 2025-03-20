using System.Reflection;

using Engine;

namespace ElevatorGame;

public static class FileLocations
{
    private static readonly Assembly assembly = Assembly.GetEntryAssembly();

    public static string ProgramPath => Path.GetDirectoryName(assembly.Location);

    public static string LocalModsPath => Path.Combine(ProgramPath, "mods");

    public static string? WorkshopModsPath => SteamManager.GetWorkshopModsPath();
}
