using System.Text;
using ElevatorGame;

bool steam = false;
#if STEAM
steam = !args.Contains("--no-steam");
#endif

using var game = new MainGame(steam);

FileStream logFile = null;
StreamWriter logWriter = null;

if(!Console.IsOutputRedirected)
{
    string logPath = Path.Combine(FileLocations.ProgramPath, "latest.log");

    if(File.Exists(logPath + ".old")) // remove previous-previous log
        File.Delete(logPath + ".old");
    if(File.Exists(logPath)) // backup previous log
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

#if DEBUG

game.Run();

#else

try
{
    game.Run();
}
catch(Exception e)
{
    Console.Error.WriteLine($"Game Crashed!!!\n  at {DateTime.Now.ToShortTimeString()}, {DateTime.Now.ToShortDateString()}");
    Console.Error.WriteLine($"Fatal Error: {e}\n");
}

#endif

logWriter?.Flush();
logWriter?.Close();
