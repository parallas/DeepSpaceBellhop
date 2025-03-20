using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ElevatorGame.Mods;

public class ModDependencyCollection : ICollection<ModDependency>, IEnumerable<ModDependency>, IEnumerable
{
    int ICollection<ModDependency>.Count => dict.Count;
    bool ICollection<ModDependency>.IsReadOnly => true;

    internal readonly Dictionary<string, ModDependency> dict = [];

    public bool Contains(string guid)
    {
        return dict.ContainsKey(guid);
    }

    public bool IsAvailable(string guid)
    {
        return dict.TryGetValue(guid, out var value) && value.Available;
    }

    public void CopyTo(ModDependency[] array, int arrayIndex)
    {
        dict.Values.CopyTo(array, arrayIndex);
    }

    internal void Add(ModDependency item)
    {
        dict.Add(item.Guid, item);
    }

    bool ICollection<ModDependency>.Contains(ModDependency item)
    {
        return dict.ContainsValue(item);
    }

    void ICollection<ModDependency>.Clear()
    {
        dict.Clear();
    }

    void ICollection<ModDependency>.Add(ModDependency item)
    {
        Add(item);
    }

    bool ICollection<ModDependency>.Remove(ModDependency item)
    {
        return dict.Remove(item.Guid);
    }

    public IEnumerator<ModDependency> GetEnumerator()
    {
        return dict.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}