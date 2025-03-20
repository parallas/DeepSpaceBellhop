using System.Reflection;

using Semver;

namespace ElevatorGame.Mods;

public sealed class LoadedMod
{
    internal bool failedLoad;
    internal bool isNonStandard;
    internal int loadIndex;
    internal readonly Dictionary<string, bool> depAvailability = [];

    public Assembly Assembly { get; internal set; }

    public string BasePath { get; internal set; }

    public Type MainClass { get; internal set; }

    public Mod Instance { get; internal set; }

    public string Guid { get; internal set; }

    public string DisplayName { get; internal set; }

    public SemVersion Version { get; internal set; }
}
