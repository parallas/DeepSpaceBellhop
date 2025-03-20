var steam = !args.Contains("--no-steam");

using var game = new ElevatorGame.MainGame(steam);

ElevatorGame.Mods.ModLoader.DoBeforeRun();

game.Run();

ElevatorGame.Mods.ModLoader.DoEndRun();
