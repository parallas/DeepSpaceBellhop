using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ElevatorGame.Source.Dialog;
using ElevatorGame.Source.Tickets;
using Engine;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Characters;

public class CharacterManager(Phone.Phone phone, TicketManager ticketManager, Dialog.Dialog dialog, Elevator.Elevator elevator)
{
    private readonly List<CharacterActor> _waitList = [];
    private readonly List<CharacterActor> _movingList = [];
    private readonly List<CharacterActor> _cabList = [];
    private readonly List<CharacterActor> _leavingList = [];
    private readonly List<string> _history = [];

    private bool _eventfulTurn = false;

    public List<CharacterActor> CacheCharactersInPlay = new List<CharacterActor>();
    public List<CharacterActor> CharactersInPlay => _waitList.Concat(_movingList).Concat(_cabList).ToList();

    public int CharactersFinished { get; private set; }

    public void LoadContent()
    {
        // foreach (var characterTableValue in CharacterRegistry.CharacterTable.Values)
        // {
        //     SpawnCharacter(characterTableValue, 2);
        // }
    }

    public void Init()
    {
        CharactersFinished = 0;
        _waitList.Clear();
        _movingList.Clear();
        _cabList.Clear();
        _leavingList.Clear();

        SpawnMultipleRandomCharacters(MainGame.StartCharacterCount);
    }

    public void Update(GameTime gameTime)
    {
        foreach (var characterActor in _leavingList)
        {
            characterActor.CanRandomlyTurnAround = false;
            characterActor.Update(gameTime);
        }
        foreach (var characterActor in _waitList)
        {
            characterActor.CanRandomlyTurnAround = false;
            characterActor.Update(gameTime);
        }
        foreach (var characterActor in _cabList)
        {
            var checkList = _cabList;
            var hitPerson = checkList.Find(actor =>
                actor != characterActor &&
                MathUtil.Approximately(actor.OffsetXTarget, characterActor.OffsetXTarget,
                    MathHelper.Lerp(32, 16, _cabList.Count / 10f)));
            if (hitPerson is not null)
            {
                var dir = Math.Sign(characterActor.OffsetXTarget - hitPerson.OffsetXTarget);
                if (dir == 0) dir = 1;
                var target = characterActor.OffsetXTarget + dir;
                characterActor.OffsetXTarget = Math.Clamp(target, -CharacterActor.StandingRoomSize,
                    CharacterActor.StandingRoomSize);
            }

            characterActor.Update(gameTime);
        }
        foreach (var characterActor in _movingList)
        {
            characterActor.CanRandomlyTurnAround = false;
            characterActor.Update(gameTime);
        }

        var cache = CharactersInPlay;
        if (!cache.ToHashSet().SetEquals(CacheCharactersInPlay))
        {
            CacheCharactersInPlay = cache;
        }

        StudioSystem.SetParameterValue("ActivityLevel", (float)CharactersInPlay.Count / MainGame.MaxCharacters);
    }

    public void DrawWaiting(SpriteBatch spriteBatch)
    {
        foreach (var characterActor in _waitList)
        {
            characterActor.Draw(spriteBatch);
        }
        foreach (var characterActor in _leavingList)
        {
            characterActor.Draw(spriteBatch);
        }
    }

    public void DrawMain(SpriteBatch spriteBatch)
    {
        for (var i = 0; i < _cabList.Count; i++)
        {
            var characterActor = _cabList[i];
            characterActor.Draw(spriteBatch, i);
        }

        for (var i = 0; i < _movingList.Count; i++)
        {
            var characterActor = _movingList[i];
            characterActor.Draw(spriteBatch, i);
        }
    }

    public IEnumerator EndOfTurnSequence()
    {
        _eventfulTurn = false;

        yield return LeaveAtFloorSequence();

        foreach (var characterActor in _cabList)
        {
            characterActor.Patience--;
            if(characterActor.Patience <= 0)
                characterActor.DrawAngryIcon = true;
        }

        yield return GetOnAtFloorSequence();

        yield return SubtractPatienceOfWaiting();

        yield return SpawnMoreCharacters();

        if (MainGame.PunishMistakes && !_eventfulTurn)
        {
            MainGame.HasMadeMistake = true;

            phone.SimulateBatteryChange(-1);
            yield return 20;
            MainGame.
                ChangeHealth(-1);
        }

        foreach (CharacterActor characterActor in _cabList)
        {
            if (characterActor.FloorTargetDirection == elevator.GetComboDirection()) continue;
            bool appliedEffect = false;
            if (characterActor.Def.Flags.HasFlag(CharacterDef.CharacterFlag.Toxic))
            {
                MainGame.StartEffectWobble();
                appliedEffect = true;
            }

            if (characterActor.Def.Flags.HasFlag(CharacterDef.CharacterFlag.Psychedelic))
            {
                MainGame.StartEffectHueShift();
                appliedEffect = true;
            }

            if (characterActor.Def.Flags.HasFlag(CharacterDef.CharacterFlag.Flippy))
            {
                MainGame.StartEffectFlippy();
                appliedEffect = true;
            }

            if (!appliedEffect) continue;
            characterActor.ForceDrawAngryAnim = true;
            characterActor.CanRandomlyTurnAround = false;
        }
    }

    private IEnumerator LeaveAtFloorSequence()
    {
        for (int index = 0; index < _cabList.Count; index++)
        {
            var characterActor = _cabList[index];
            var isTargetFloor = characterActor.FloorNumberTarget == MainGame.CurrentFloor;
            var isPatienceOut = characterActor.Patience <= 0 && !isTargetFloor;
            var doTurn = isTargetFloor || isPatienceOut;
            if (!doTurn) continue;
            if(isTargetFloor)
            {
                characterActor.DrawAngryIcon = false;
                characterActor.Patience = 1;
            }

            characterActor.ForceDrawAngryAnim = false;

            MainGame.Coroutines.Stop($"character_get_in_elevator_{characterActor.CharacterId}");
            _cabList.Remove(characterActor);
            index--;
            _movingList.Add(characterActor);
            yield return characterActor.GetOffElevatorBegin(isPatienceOut);
            MainGame.Coroutines.Stop("ticket_remove");
            MainGame.Coroutines.TryRun("ticket_remove", ticketManager.RemoveTicket(characterActor.FloorNumberTarget), out _);

            // Pretend to reduce health (and show phone)
            if (isPatienceOut)
            {
                phone.SimulateBatteryChange(-3);
            }
            else if (MainGame.CurrentHealth < 8) // this check prevents the unecessary opening from happening at full health
            {
                phone.SimulateBatteryChange(+1);
            }

            // Use angry phrases if patience is <= 0
            var phrases =
                isPatienceOut
                    ? characterActor.Def.AngryPhrases
                    : characterActor.Def.ExitPhrases;
            var pages = DialogParser.GetRandomDialog(phrases, out var displayMethod);
            yield return dialog.Display(DialogParser.ParseCharacterDialog(pages, characterActor), displayMethod);

            _movingList.Remove(characterActor);
            _leavingList.Add(characterActor);

            // Reduce health for real now
            if (isPatienceOut)
            {
                MainGame.ChangeHealth(-3);
                yield return 60;
            }
            else if (MainGame.CurrentHealth < 8)
            {
                MainGame.ChangeHealth(+1);
                yield return 60;
            }

            MainGame.Coroutines.TryRun(
                $"character_get_off_elevator_{characterActor.CharacterId}",
                characterActor.GetOffElevatorEnd(() =>
                {
                    _leavingList.Remove(characterActor);
                }), out _);

            CharactersFinished++;
            _eventfulTurn = true;
        }
    }

    public IEnumerator GetOnAtFloorSequence()
    {
        for (var index = _waitList.Count - 1; index >= 0; index--)
        {
            if (_cabList.Count >= 10) break;

            var characterActor = _waitList[index];
            if (characterActor.FloorNumberCurrent != MainGame.CurrentFloor) continue;

            characterActor.Patience = characterActor.InitialPatience;
            MainGame.Coroutines.TryRun("phone_show", phone.Open(false, false), out _);
            phone.CanOpen = false;
            // _cabList.ForEach(actor => actor.MoveOutOfTheWay());
            yield return characterActor.GetInElevatorBegin();
            _waitList.Remove(characterActor);
            _movingList.Add(characterActor);
            _history.Add(characterActor.Def.Name);
            phone.HighlightOrder(characterActor);

            if (_cabList.Count >= 9)
            {
                SteamManager.UnlockAchievement("10_PEOPLE");
            }

            TicketActor.TicketFlags flags = TicketActor.TicketFlags.None;
            if (characterActor.Def.Flags.HasFlag(CharacterDef.CharacterFlag.Slimy))
                flags |= TicketActor.TicketFlags.Slimy;
            if (characterActor.Def.Flags.HasFlag(CharacterDef.CharacterFlag.Clumsy)
                && Random.Shared.Next(0, 3) == 0
                && !(_history.Count == 1 && MainGame.CurrentDay == 0))
                flags |= TicketActor.TicketFlags.UpsideDown;

            ticketManager.AddTicket(characterActor.FloorNumberTarget, flags);

            var pages = DialogParser.GetRandomDialog(characterActor.Def.EnterPhrases, out var displayMethod);
            yield return dialog.Display(DialogParser.ParseCharacterDialog(pages, characterActor), displayMethod);
            yield return phone.RemoveOrder(characterActor);

            MainGame.Coroutines.TryRun($"character_get_in_elevator_{characterActor.CharacterId}",
                characterActor.GetInElevatorEnd(), out _);
            _movingList.Remove(characterActor);
            _cabList.Add(characterActor);

            _eventfulTurn = true;
        }
    }

    private IEnumerator SubtractPatienceOfWaiting()
    {
        int orderFailedCount = 0;
        for (int i = 0; i < _waitList.Count; i++)
        {
            var characterActor = _waitList[i];
            if (characterActor.Patience > 0) continue;

            MainGame.Coroutines.TryRun("phone_show", phone.Open(false, false), out _);

            phone.SimulateBatteryChange(-2);
            yield return phone.CancelOrder(characterActor.CharacterId);
            _waitList.Remove(characterActor);
            i--;
            orderFailedCount++;
        }

        if (orderFailedCount > 0)
        {
            phone.ScrollToTop();
            yield return 60;
            MainGame.ChangeHealth(-orderFailedCount * 2);

            CharactersFinished += orderFailedCount;
            yield return 30;
        }

        foreach (var characterActor in _waitList)
        {
            characterActor.Patience--;
            characterActor.Patience = Math.Max(0, characterActor.Patience);
            float patiencePercent = MathUtil.InverseLerp01(1, characterActor.InitialPatience, characterActor.Patience);
            int moodValue = MathUtil.RoundToInt(MathHelper.Lerp(2, 0, patiencePercent));
            if (characterActor.Patience <= 0) moodValue = 3;
            phone.SetOrderMood(characterActor.CharacterId, moodValue);
        }

        yield return null;
    }

    public int WaitingDirectionOnFloor(int floorNumber)
    {
         var firstWaiting = _waitList.FirstOrDefault(actor => actor.FloorNumberCurrent == floorNumber);
         if (firstWaiting is null) return 0;
         return Math.Sign(firstWaiting.FloorNumberTarget - firstWaiting.FloorNumberCurrent);
    }

    public bool IsCharacterWaitingOnFloor(int floorNumber)
    {
        return _waitList.Any(actor => actor.FloorNumberCurrent == floorNumber);
    }

    public bool IsCharacterWaitingToGoToFloor(int floorNumber)
    {
        return _cabList.Any(actor => actor.FloorNumberTarget == floorNumber);
    }

    public CharacterActor SpawnCharacter(CharacterDef characterDef, int minFloor = 1)
    {
        var spawnFloor = 0;
        do
        {
            spawnFloor = Random.Shared.Next(minFloor, MainGame.FloorCount + 1);
        }
        while (spawnFloor == MainGame.CurrentFloor);

        var newCharacter = new CharacterActor
        {
            Def = characterDef,
            FloorNumberCurrent = spawnFloor,
            Patience = Random.Shared.Next(5, 9),
            OffsetXTarget = Random.Shared.Next(-48, 49)
        };
        SpawnCharacter(newCharacter);
        return newCharacter;
    }

    public void SpawnCharacter(CharacterActor characterActor)
    {
        if (CharactersFinished + CharactersInPlay.Count >= MainGame.CompletionRequirement)
        {
            return; // Don't spawn anyone!!!!!!!!!!!!
        }
        do
        {
            int randomFloor = Random.Shared.Next(1, MainGame.FloorCount + 1);
            var other = _waitList.FirstOrDefault(c => c.FloorNumberCurrent == characterActor.FloorNumberCurrent);
            if (other is not null)
            {
                randomFloor = other.FloorTargetDirection > 0
                    ? Random.Shared.Next(other.FloorNumberCurrent + 1, MainGame.FloorCount + 1)
                    : Random.Shared.Next(1, other.FloorNumberCurrent);
            }
            characterActor.FloorNumberTarget = randomFloor;
        } while (
            characterActor.FloorNumberTarget == characterActor.FloorNumberCurrent
        );

        phone.AddOrder(characterActor);
        phone.PlayDotBlink();

        Console.WriteLine(
            $"{characterActor.Def.Name} is going from {characterActor.FloorNumberCurrent} to {characterActor.FloorNumberTarget}");
        characterActor.LoadContent();
        _waitList.Add(characterActor);
    }

    public CharacterActor[] SpawnMultipleRandomCharacters(int count)
    {
        List<CharacterActor> characters = new();
        for (int i = 0; i < count; i++)
        {
            if (!TryGetRandomValidCharacter(out var characterDef))
                break;
            var newCharacter = SpawnCharacter(characterDef);
            characters.Add(newCharacter);
        }

        return [.. characters];
    }

    private bool TryGetRandomValidCharacter(out CharacterDef characterDef)
    {
        characterDef = default;
        CharacterDef[] validCharactersToSpawn = [.. CharacterRegistry.CharacterTable.Values
            .Where(characterDef =>
                    MainGame.CharacterIdsPool.Contains(characterDef.Name) && // Get characters from the day's available characters
                    CharactersInPlay.Find(a => a.Def.Name == characterDef.Name) is null // Don't spawn the same character twice
            )
        ];

        if (validCharactersToSpawn.Length == 0)
            return false;

        characterDef = validCharactersToSpawn[Random.Shared.Next(validCharactersToSpawn.Length)];
        return true;
    }

    private IEnumerator SpawnMoreCharacters()
    {
        // The more people who are waiting (relative to the max characters for the day), the less chance more can spawn
        bool shouldSpawn = Random.Shared.Next(100) <
                           MainGame.SpawnChance * (1 - (float)_waitList.Count / MainGame.MaxCharacters);
        if (!shouldSpawn) yield break;

        int spawnAmount = Random.Shared.Next(MainGame.MaxCountPerSpawn) + 1;
        spawnAmount = Math.Clamp(spawnAmount, 0, MainGame.MaxCharacters - CharactersInPlay.Count);
        if (spawnAmount <= 0) yield break;

        SpawnMultipleRandomCharacters(spawnAmount);
    }

    public void ClearCharacters()
    {
        _waitList.Clear();
        _movingList.Clear();
        _cabList.Clear();
        _leavingList.Clear();
        phone.ForceClearOrders();
        ticketManager.ForceClearTickets();
    }

    public void ForceCompleteDay()
    {
        CharactersFinished = MainGame.CompletionRequirement;
        ClearCharacters();
    }
}
