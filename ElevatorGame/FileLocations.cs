namespace ElevatorGame;

public static class FileLocations
{
    public static string ProgramPath => AppDomain.CurrentDomain.BaseDirectory;

    public static string LocalData => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "parallas", "Deep Space Bellhop");
}
