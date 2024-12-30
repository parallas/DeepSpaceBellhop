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
                CharacterIds = ["Blueulet", "Greenulet", "Kloob"],
                StartCharacterCount = 1,
                StartDialog = new(
                    "Your job is to pick up passengers and take them to their desired floor.",
                    "Hold up or down to continuously move through floors.",
                    "My screen will show you everybody who has called the elevator.",
                    "You can see their floor number, what direction they want to go, and how strong the connection is to them.",
                    "If the connection is blinking low, that means they're going to give up if you don't pick them up right away.",
                    "If you miss a passenger, they'll get angry and I'll lose battery.",
                    "Once a passenger is picked up, they'll give you a ticket showing what floor they want to go to.",
                    "If you take too many turns to get them to their floor, they'll leave in a fit, and I'll lose a lot of battery!",
                    "You can tell a passenger is about to leave if you see an angry pulsing blood vein on their head.",
                    "If you deliver a passenger to the correct floor, I'll gain some battery.",
                    "After a certain number of people come and go, the day will end and you can clock out.",
                    "Make sure to keep an eye on the battery meter. If it runs out, I'll shut down and you're out of a job!",
                    "GLHF, TTYL!"
                )
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
