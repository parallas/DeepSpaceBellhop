using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

using Semver;
using Microsoft.Xna.Framework;

namespace ElevatorGame.Mods;

public static class ModLoader
{
    private static int loadNum;

    internal static readonly List<LoadedMod> loadedMods = [];

    internal static readonly HashSet<string> loadedPaths = [];
    internal static readonly HashSet<string> resolvedGuids = [];

    internal static readonly Dictionary<string, ModJson> nonStandardMods = [];

    public static JsonSerializerOptions SerializerOptions { get; } = new() {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = {
            new JsonStringEnumConverter<ModDependencyKind>(),
        },
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    private static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
    {
        return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
    }

    private static bool CheckIsWorkshop(string basePath)
    {
        return FileLocations.WorkshopMods is not null && basePath.StartsWith(FileLocations.WorkshopMods);
    }

    private static void Log(object? message) => Console.WriteLine($"[Modloader/INFO]: {message}");
    private static void LogWarning(object? message) => Console.WriteLine($"[Modloader/WARN]: {message}");
    private static void LogError(object? message) => Console.Error.WriteLine($"[Modloader/ERROR]: {message}");

    internal static void DoBeforeRun()
    {
        AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;

        Log($"Finding mod assemblies...");
        bool workshopAvailable = FileLocations.WorkshopMods is not null && Directory.Exists(FileLocations.WorkshopMods);

        Directory.CreateDirectory(FileLocations.LocalMods);

        IEnumerable<string> workshopFileQuery = workshopAvailable ? Directory.EnumerateDirectories(FileLocations.WorkshopMods) : [];
        IEnumerable<string> localFileQuery = Directory.EnumerateDirectories(FileLocations.LocalMods);

        foreach(var modFolder in Query(workshopFileQuery, localFileQuery))
        {
            // load top-level assemblies
            // can be standard and non-standard
            foreach(var fullPath in Directory.EnumerateFiles(modFolder, "*.dll", SearchOption.TopDirectoryOnly))
            {
                Log($"  Found assembly {Path.GetFileName(fullPath)} ({fullPath})");

                ReadAssembly(fullPath, modFolder, true);

                loadedPaths.Add(modFolder);
            }

            // load lib assemblies (recursive!)
            // only loads non-standard
            var libPath = Path.Combine(modFolder, "lib");
            if(Directory.Exists(libPath))
            {
                foreach(var fullPath in Directory.EnumerateFiles(libPath, "*.dll", SearchOption.AllDirectories))
                {
                    Log($"  Found assembly {Path.GetFileName(fullPath)} ({fullPath})");

                    ReadAssembly(fullPath, modFolder, false);

                    loadedPaths.Add(modFolder);
                }
            }
        }

        if(loadedMods.Count > 0)
        {
            StringBuilder modListLog = new("Mods to load:");
            for(int i = 0; i < loadedMods.Count; i++)
            {
                var mod = loadedMods[i];

                modListLog.Append("\n  - ");
                modListLog.Append(loadedMods[i].DisplayName);
                if(mod.DisplayName != mod.Guid)
                {
                    modListLog.Append(" (");
                    modListLog.Append(loadedMods[i].Guid);
                    modListLog.Append(')');
                }
            }
            modListLog.Append("\nAssemblies:");
            for(int i = 0; i < loadedMods.Count; i++)
            {
                var mod = loadedMods[i];
                if(mod.Assembly is null)
                    continue;

                modListLog.Append("\n  - ");
                modListLog.Append(loadedMods[i].Assembly.FullName);
            }
            Log(modListLog);
        }

        resolvedGuids.Clear();

        for(int i = 0; i < loadedMods.Count; i++)
        {
            LoadedMod mod = loadedMods[i];

            Log($"Resolving dependencies for {mod.Guid}...");

            ResolveDependencies(mod);

            if(mod.failedLoad)
            {
                LogWarning($"Skipped loading {mod.Guid}");
                loadedMods.RemoveAt(i);
                i--;
                continue;
            }
        }

        loadedMods.Sort((a, b) => a.loadIndex - b.loadIndex);

        // post import

        if(loadedMods.Count == 0)
        {
            Log("Looks like there's nothing to do!");
            return;
        }
        else
        {
            StringBuilder modListLog = new StringBuilder("Successfully loaded ").Append(loadedMods.Count).Append(" mods:");
            for(int i = 0; i < loadedMods.Count; i++)
            {
                var mod = loadedMods[i];

                modListLog.Append("\n  - ");
                modListLog.Append(mod.loadIndex);
                modListLog.Append(": ");
                modListLog.Append(loadedMods[i].DisplayName);
                if(mod.DisplayName != mod.Guid)
                {
                    modListLog.Append(" (");
                    modListLog.Append(loadedMods[i].Guid);
                    modListLog.Append(')');
                }
            }
            Log(modListLog);
        }

        MergeContent();

        // dispatch BeforeRun
        foreach(var mod in loadedMods)
        {
            if(!mod.isNonStandard)
            {
                try
                {
                    mod.Instance = (Mod)mod.MainClass.GetConstructor(Type.EmptyTypes).Invoke(null);
                }
                catch(Exception e)
                {
                    LogError($"Failed to create mod instance for {mod.Guid}: {e}");
                    continue;
                }

                Dispatch(() => mod.Instance?.OnBeforeRun(), mod, nameof(DoBeforeRun));
            }
        }
    }

    private static IEnumerable<string> Query(params IEnumerable<string>[] enumerables)
    {
        foreach (var e in enumerables)
            foreach (var fullPath in e)
                yield return fullPath;
    }

    private static void ReadAssembly(string path, string basePath, bool standard)
    {
        Assembly assembly = null;

        try
        {
            assembly = Assembly.Load(File.ReadAllBytes(path));

            if(!standard)
            {
                try
                {
                    ImportLibrary(path, basePath, assembly);
                }
                catch (Exception e)
                {
                    LogError(new ModLoadException($"Failed to load assembly {assembly.FullName} ({path})", e));
                }
                return;
            }
        }
        catch(Exception e)
        {
            var name = assembly?.FullName ?? Path.GetFileNameWithoutExtension(path);
            LogError(new ModLoadException($"Failed to load assembly {name} ({path})", e));
            return;
        }

        foreach(var type in assembly.GetTypes())
        {
            try
            {
                if(type.IsSubclassOf(typeof(Mod)))
                {
                    if(!type.IsPublic)
                    {
                        throw new TypeAccessException("Mod class must be public and cannot be a nested type");
                    }

                    if(!type.GetConstructor(Type.EmptyTypes).IsPublic)
                    {
                        throw new MethodAccessException("Mod class's constructor must be public");
                    }

                    LoadedMod mod = new()
                    {
                        Assembly = assembly,
                        BasePath = basePath,
                        MainClass = type,
                        isWorkshop = CheckIsWorkshop(basePath),
                    };

                    ImportMod(mod);
                    return;
                }
            }
            catch(Exception e)
            {
                LogError(new ModLoadException($"Failed to load assembly {assembly.FullName} ({path})", e));
                return;
            }
        }

        // fallback if no Mod class found
        try
        {
            ImportLibrary(path, basePath, assembly);
        }
        catch (Exception e)
        {
            LogError(new ModLoadException($"Failed to load assembly {assembly.FullName} ({path})", e));
        }
    }

    private static void ImportLibrary(string path, string basePath, Assembly assembly)
    {
        // fallback for assemblies that lack a Mod class
        // in this case the mod *must* have a mod.json file at its root

        if(loadedPaths.Contains(basePath)) return;

        var jsonPath = Path.Combine(basePath, "mod.json");
        if(!File.Exists(jsonPath))
        {
            throw new FileNotFoundException("The mod is library-only but does not contain the required mod.json file", jsonPath);
        }

        ModJson modJson = JsonSerializer.Deserialize<ModJson>(File.ReadAllText(jsonPath), SerializerOptions);

        LoadedMod mod = new()
        {
            isNonStandard = true,
            Assembly = assembly,
            Guid = modJson.Guid,
            DisplayName = modJson.Name ?? modJson.Guid,
            Version = SemVersion.Parse(modJson.Version, SemVersionStyles.OptionalPatch),
            isWorkshop = CheckIsWorkshop(basePath),
        };

        CheckDuplicate(mod);

        nonStandardMods.Add(mod.Guid, modJson);

        if(modJson.Name == null)
            LogWarning($"{mod.Guid} is missing a display name");

        loadedMods.Add(mod);
    }

    private static void ImportMod(LoadedMod mod)
    {
        if(mod.MainClass.GetCustomAttribute<ModInfoAttribute>() is not ModInfoAttribute modInfo)
        {
            throw new ModLoadException($"Mod class is missing required ModInfo attribute");
        }

        mod.Guid = modInfo.Guid;
        mod.DisplayName = modInfo.DisplayName ?? mod.Guid;
        mod.Version = modInfo.Version;

        CheckDuplicate(mod);

        if(modInfo.DisplayName == null)
            LogWarning($"{mod.Guid} is missing a display name");

        loadedMods.Add(mod);
    }

    private static void CheckDuplicate(LoadedMod mod)
    {
        int duplicateIndex = loadedMods.FindIndex(m => m.Guid == mod.Guid && !m.failedLoad);
        if(duplicateIndex != -1)
        {
            var orig = loadedMods[duplicateIndex];
            throw new InvalidOperationException(string.Join('\n', [
                $"A mod with the GUID {mod.Guid} is already present",
                $"  Baseline: {orig.DisplayName} ({orig.Guid}) v{orig.Version}, from: {orig.BasePath}",
                $"  Incoming: {mod.DisplayName} ({mod.Guid}) v{mod.Version}, from: {mod.BasePath}"
            ]));
        }
    }

    private static void ResolveDependencies(LoadedMod mod)
    {
        if(resolvedGuids.Contains(mod.Guid)) return;

        try
        {
            List<Exception> errors = [];

            if(!mod.isNonStandard)
            {
                foreach(var attr in Attribute.GetCustomAttributes(mod.MainClass, typeof(ModDependencyAttribute), false))
                {
                    var dep = (ModDependencyAttribute)attr;
                    ReadDependency(mod, errors, dep);
                }
            }
            else
            {
                var jsonMod = nonStandardMods[mod.Guid];
                HashSet<ModDependencyAttribute> dependencyList = new(
                    [
                        ..jsonMod.Dependencies?.Incompatible ?? [],
                        ..jsonMod.Dependencies?.Required ?? [],
                        ..jsonMod.Dependencies?.Optional ?? []
                    ],
                    ModDependencyAttribute.GetGuidEqualityComparer()
                );

                foreach(var dep in dependencyList)
                {
                    ReadDependency(mod, errors, dep);
                }
            }

            if (errors.Count > 0)
            {
                LogError(new AggregateException($"Failed to resolve dependencies for {mod.Guid}", errors));
                mod.failedLoad = true;
                resolvedGuids.Add(mod.Guid);
                return;
            }
        }
        catch(Exception e)
        {
            LogError(e);
            mod.failedLoad = true;
            resolvedGuids.Add(mod.Guid);
            return;
        }

        resolvedGuids.Add(mod.Guid);
        mod.loadIndex = loadNum++;
    }

    private static void ReadDependency(LoadedMod mod, List<Exception> errors, ModDependencyAttribute dep)
    {
        bool depIsAvailable = false;

        var depMod = loadedMods.FirstOrDefault(m => m.Guid == dep.Guid, null);
        if(depMod != null)
        {
            string dependencyKindText = "requires a";
            if (dep.Kind == ModDependencyKind.Incompatible)
                dependencyKindText = "is incompatible with any";

            try
            {
                var versionRangeOptions = SemVersionRangeOptions.OptionalPatch | SemVersionRangeOptions.IncludeAllPrerelease;

                bool versionMatches = SemVersionRange.Parse(dep.VersionRange, versionRangeOptions)
                    .Contains(loadedMods.First(m => m.Guid == dep.Guid).Version);

                // min and max
                if ((dep.Kind == ModDependencyKind.Incompatible ^ versionMatches)
                    || (dep.Kind == ModDependencyKind.Optional && versionMatches))
                {
                    ResolveDependencies(depMod);
                    depIsAvailable = true;
                }
                else
                    throw new ModLoadException($"{mod.Guid} {dependencyKindText} version of {dep.Guid} that falls within the range {dep.VersionRange}, but {depMod.Version} was found");
            }
            catch(Exception e)
            {
                errors.Add(e);
            }

            if(dep.Kind != ModDependencyKind.Incompatible)
            {
                mod.depAvailability[dep.Guid] = depIsAvailable;
            }

            switch(dep.Kind)
            {
                case ModDependencyKind.Required:
                    if(depIsAvailable)
                        Log($"  {dep.Guid} {dep.VersionRange}: Required dependency met");
                    else
                        LogError($"  {dep.Guid} {dep.VersionRange}: Required dependency was not met!");
                    break;
                case ModDependencyKind.Optional:
                    if(depIsAvailable)
                        Log($"  {dep.Guid} {dep.VersionRange}: Optional dependency met");
                    else
                        LogWarning($"  {dep.Guid} {dep.VersionRange}: Optional dependency was not met!");
                    break;
            }

            return;
        }

        if (dep.Kind == ModDependencyKind.Optional)
        {
            mod.depAvailability[dep.Guid] = false;
        }

        if (dep.Kind == ModDependencyKind.Required)
        {
            errors.Add(new ModLoadException($"{mod.Guid} requires a version of {dep.Guid} that falls within the range {dep.VersionRange}, but {dep.Guid} is missing"));
        }

        switch(dep.Kind)
        {
            case ModDependencyKind.Required:
                if(depIsAvailable)
                    Log($"  {dep.Guid} {dep.VersionRange}: Required dependency met");
                else
                    LogError($"  {dep.Guid} {dep.VersionRange}: Required dependency was not met!");
                break;
            case ModDependencyKind.Optional:
                if(depIsAvailable)
                    Log($"  {dep.Guid} {dep.VersionRange}: Optional dependency met");
                else
                    LogError($"  {dep.Guid} {dep.VersionRange}: Optional dependency was not met!");
                break;
        }
    }

    private static void MergeContent()
    {
        Directory.CreateDirectory(FileLocations.ModsCache);

        foreach(var mod in loadedMods)
        {
            //
        }
    }

    internal static void DoInitialize()
    {
        foreach (var mod in loadedMods)
        {
            Dispatch(() => mod.Instance?.OnInitialize(), mod, nameof(DoInitialize));
        }
    }

    internal static void DoRegistriesInit()
    {
        foreach (var mod in loadedMods)
        {
            Dispatch(() => mod.Instance?.OnRegistriesInit(), mod, nameof(DoRegistriesInit));
        }
    }

    internal static void DoLoadContent()
    {
        foreach (var mod in loadedMods)
        {
            Dispatch(() => mod.Instance?.OnLoadContent(), mod, nameof(DoLoadContent));
        }
    }

    internal static void DoUpdate(GameTime gameTime)
    {
        foreach (var mod in loadedMods)
        {
            Dispatch(() => mod.Instance?.OnUpdate(gameTime), mod, nameof(DoUpdate));
        }
    }

    internal static void DoGameStateChanged(MainGame.GameStates state)
    {
        foreach (var mod in loadedMods)
        {
            Dispatch(() => mod.Instance?.OnGameStateChanged(state), mod, nameof(DoGameStateChanged));
        }
    }

    internal static void DoEndRun()
    {
        foreach (var mod in loadedMods)
        {
            Dispatch(() => mod.Instance?.OnEndRun(), mod, nameof(DoEndRun));
        }
    }

    private static void Dispatch(Action method, LoadedMod mod, string eventName)
    {
        if(mod.isNonStandard)
            return;

        if(mod.Instance is null)
        {
            LogWarning($"Could not dispatch {eventName} for {mod.Guid}: missing mod instance");
            return;
        }

        try
        {
            method();
        }
        catch(Exception e)
        {
            LogError($"Failed to dispatch {eventName} for {mod.Guid}: {e}");
            throw;
        }
    }
}

public class ModLoadException(string? message, string? guid, Exception? innerException)
    : Exception(message ?? "Mod loading failed" + (guid != null ? $" (Failed to load mod {guid})" : ""), innerException)
{
    public string Guid => guid;

    public ModLoadException(string? message, Exception? innerException)
        : this(message, null, innerException) {}

    public ModLoadException(string? message)
        : this(message, null, null) {}

    public ModLoadException()
        : this(null, null, null) {}
}
