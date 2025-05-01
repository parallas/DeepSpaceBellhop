namespace ElevatorGame;

public static class GameMetadata
{
    public static string Version { get; }

    static GameMetadata()
    {
        var path = Path.Combine(FileLocations.ProgramPath, "VERSION.txt");

        if(File.Exists(path))
        {
            Version = File.ReadLines(path).First();
        }
    }
}
