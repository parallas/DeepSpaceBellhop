using System.Linq;

using var game = new ElevatorGame.MainGame(
    !args.Contains("--no-steam")
);
game.Run();
