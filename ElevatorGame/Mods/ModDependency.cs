namespace ElevatorGame.Mods;

public sealed class ModDependency(string guid, bool available)
{
    public string Guid => guid;
    public bool Available => available;
}
