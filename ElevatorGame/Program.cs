var steam = !args.Contains("--no-steam");

using var game = new ElevatorGame.MainGame(steam);
game.Run();
