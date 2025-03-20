using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

using Semver;

namespace ElevatorGame;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ModInfoAttribute([DisallowNull] string guid, string displayName, [DisallowNull] string versionString) : Attribute
{
    public string Guid => guid ?? throw new NullReferenceException("ModInfo GUID cannot be null");
    public string DisplayName => displayName;

    public SemVersion Version { get; } = SemVersion.Parse(versionString ?? throw new NullReferenceException("ModInfo Version cannot be null"), SemVersionStyles.OptionalPatch);
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public sealed class ModDependencyAttribute(
    [DisallowNull] string guid,
    ModDependencyKind dependencyKind,
    [DisallowNull] string versionRange
) : Attribute, IEquatable<ModDependencyAttribute>
{
    public string Guid => guid;
    public string VersionRange => versionRange;

    [JsonPropertyName("type")]
    public ModDependencyKind Kind => dependencyKind;

    public bool Equals(ModDependencyAttribute other)
    {
        return other.Guid == Guid
            && other.Kind == Kind
            && other.VersionRange == VersionRange;
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj) || this.Equals(obj as ModDependencyAttribute);
    }

    public static IEqualityComparer<ModDependencyAttribute> GetGuidEqualityComparer() => new GuidEqualityComparer();

    public class GuidEqualityComparer : IEqualityComparer<ModDependencyAttribute>
    {
        public bool Equals(ModDependencyAttribute x, ModDependencyAttribute y)
        {
            return x.Guid == y.Guid;
        }

        public int GetHashCode([DisallowNull] ModDependencyAttribute obj)
        {
            return obj.Guid.GetHashCode();
        }
    }
}

public enum ModDependencyKind
{
    /// <summary>
    /// States that the specified mod guid and version range is required by this mod.
    /// This mod will also fail to load if the dependency is present but the installed version of the dependency doesn't match the range.
    /// </summary>
    Required,

    /// <summary>
    /// States that the specified mod guid and version range is compatible with this mod, but not essential for it to work properly.
    /// This mod will load normally regardless of the installed version of the dependency.
    /// </summary>
    Optional,

    /// <summary>
    /// States that the specified mod guid and version range is mutually exclusive with this mod.
    /// This mod will still load normally if the installed version of the dependency doesn't match the range.
    /// This mod will fail to load otherwise.
    /// </summary>
    Incompatible
}
