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
                NumberOfFloors = 10,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 20,
                CharacterIDs = ["Blueulet", "Greenulet", "Kloob"]
            },
            new DayDef
            {
                NumberOfFloors = 25,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 30,
                CharacterIDs = ["Blueulet", "Greenulet", "Kloob", "Robot"]
            },
            new DayDef
            {
                NumberOfFloors = 50,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 30,
                CharacterIDs = ["Blueulet", "Greenulet", "Kloob", "Robot", "EmalynCat"]
            },
            new DayDef
            {
                NumberOfFloors = 99,
                OrderSpawnChancePerTurn = 60,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 20,
                CharacterIDs = ["Blueulet", "Greenulet", "Kloob", "Robot", "EmalynCat"]
            },
        ];
    }
}
