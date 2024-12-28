using System.IO;
using System.Reflection;

namespace ElevatorGame;

public static class FileLocations
{
    private static readonly Assembly assembly = Assembly.GetEntryAssembly();

    public static string ProgramPath => Path.GetDirectoryName(assembly.Location);
}
