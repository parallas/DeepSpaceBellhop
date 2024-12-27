namespace ElevatorGame.Source.Days;

public static class DayRegistry
{
    public static DayDef[] Days { get; private set; }

    public static void Init()
    {
        Days =
        [
            new DayDef
            {
                FloorCount = 10,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 20,
                CharacterIDs = ["Blueulet", "Greenulet", "Kloob"]
            },
            new DayDef
            {
                FloorCount = 25,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 30,
                CharacterIDs = ["Blueulet", "Greenulet", "Kloob", "Robot", "Slime"]
            },
            new DayDef
            {
                FloorCount = 50,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 30,
                CharacterIDs = ["Blueulet", "Greenulet", "Kloob", "Robot", "Slime", "EmalynCat"]
            },
            new DayDef
            {
                FloorCount = 99,
                OrderSpawnChancePerTurn = 60,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 30,
                CharacterIDs = ["Blueulet", "Greenulet", "Kloob", "Robot", "Slime", "EmalynCat"]
            },
        ];
    }
}
