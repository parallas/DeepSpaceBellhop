using Engine;

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
                CompletionRequirement = 10,
                CharacterIds = ["Benbo"],
                StartCharacterCount = 1,
                StartDialog = new(LocalizationManager.Get("dialog.phone.tutorial.intro"))
            },
            new DayDef
            {
                FloorCount = 20,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 25,
                CharacterIds = ["Blueulet", "Greenulet", "Kloob", "Robot", "Slime"],
                StartCharacterCount = 2
            },
            new DayDef
            {
                FloorCount = 30,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 30,
                CharacterIds = ["Blueulet", "Greenulet", "Kloob", "Robot", "Slime", "EmalynCat"],
                StartCharacterCount = 3
            },
            new DayDef
            {
                FloorCount = 40,
                OrderSpawnChancePerTurn = 60,
                MaxCountPerSpawn = 2,
                CompletionRequirement = 30,
                CharacterIds = ["Blueulet", "Greenulet", "Kloob", "Robot", "Slime", "EmalynCat"],
                StartCharacterCount = 3
            },
        ];
    }
}
