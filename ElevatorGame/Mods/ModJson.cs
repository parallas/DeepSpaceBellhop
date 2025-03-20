namespace ElevatorGame.Mods;

public class ModJson
{
    public string Guid { get; set; }

    public string Name { get; set; }

    public string Version { get; set; }

    public ModJson_Dependencies? Dependencies { get; set; }

    public class ModJson_Dependencies
    {
        public IList<ModDependencyAttribute>? Required { get; set; }

        public IList<ModDependencyAttribute>? Optional { get; set; }

        public IList<ModDependencyAttribute>? Incompatible { get; set; }
    }
}
