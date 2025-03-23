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
                OrderSpawnChancePerTurn = 20,
                MaxCountPerSpawn = 2,
                MaxCharacters = 4,
                CompletionRequirement = 10,
                CharacterIds = ["Blueulet", "Greenulet", "Kloob", "Beebo", "EmalynCat", "JellyfishGirl"],
                StartCharacterCount = 1,
                StartDialog = new("dialog.phone.tutorial.intro"),
                PunishMistakes = false
            },
            new DayDef
            {
                FloorCount = 20,
                OrderSpawnChancePerTurn = 30,
                MaxCountPerSpawn = 2,
                MaxCharacters = 6,
                CompletionRequirement = 20,
                CharacterIds =
                [
                    "Blueulet", "Greenulet", "Kloob", "Beebo", "EmalynCat", "JellyfishGirl", "Robot", "Slime",
                    "ShroomKing", "Hourglass", "SeaDragon"
                ],
                StartCharacterCount = 2,
                StartDialog = new("dialog.phone.tutorial.character_effects"),
                PunishMistakes = false
            },
            new DayDef
            {
                FloorCount = 30,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                MaxCharacters = 8,
                CompletionRequirement = 20,
                CharacterIds =
                [
                    "Blueulet", "Greenulet", "Kloob", "Beebo", "EmalynCat", "JellyfishGirl", "Robot", "Slime",
                    "ShroomKing", "Hourglass", "SeaDragon", "Box", "Alieno", "Flippy", "Birthday"
                ],
                SpecialCharacters = [
                    new() {CharacterName = "Benbo", ChanceToSpawn = 20, MinSpawnCompletionPercent = 20, MaxSpawnCompletionPercent = 60},
                ],
                StartCharacterCount = 3,
                StartDialog = new("dialog.phone.tutorial.punish"),
                PunishMistakes = true
            },
            new DayDef
            {
                FloorCount = 40,
                OrderSpawnChancePerTurn = 40,
                MaxCountPerSpawn = 2,
                MaxCharacters = 10,
                CompletionRequirement = 30,
                CharacterIds =
                [
                    "Blueulet", "Greenulet", "Kloob", "Beebo", "EmalynCat", "JellyfishGirl", "Robot", "Slime",
                    "ShroomKing", "Hourglass", "SeaDragon", "Box", "Alieno", "Flippy", "Birthday"
                ],
                SpecialCharacters = [
                    new() {CharacterName = "Benbo", ChanceToSpawn = 20, MinSpawnCompletionPercent = 20, MaxSpawnCompletionPercent = 60},
                    new() {CharacterName = "EggBuddy", ChanceToSpawn = 20, MinSpawnCompletionPercent = 20, MaxSpawnCompletionPercent = 80},
                ],
                StartCharacterCount = 3,
                PunishMistakes = true
            },
        ];
    }
}
