using System.Text;

bool steam = false;
#if STEAM
steam = !args.Contains("--no-steam");
#endif

using var game = new ElevatorGame.MainGame(steam);

FileStream logFile = null;
StreamWriter logWriter = null;
if(!Console.IsOutputRedirected)
{
    string logPath = Path.Combine(ElevatorGame.FileLocations.ProgramPath, "latest.log");
    if(File.Exists(logPath + ".old"))
        File.Delete(logPath + ".old");
    if(File.Exists(logPath))
        File.Copy(logPath, logPath + ".old");

    logFile = File.Open(logPath, FileMode.Create, FileAccess.Write);
    logWriter = new StreamWriter(logFile, Encoding.UTF8)
    {
        AutoFlush = true,
        NewLine = "\n",
    };

    Console.SetOut(logWriter);

    if(!Console.IsErrorRedirected)
        Console.SetError(logWriter);
}

game.Run();

logWriter?.Flush();
logWriter?.Close();
