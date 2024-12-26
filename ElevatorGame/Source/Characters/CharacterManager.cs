using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ElevatorGame.Source.Tickets;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Characters;

public class CharacterManager(Phone.Phone phone, TicketManager ticketManager, Dialog.Dialog dialog)
{
    private readonly List<CharacterActor> _waitList = [];
    private readonly List<CharacterActor> _movingList = [];
    private readonly List<CharacterActor> _cabList = [];

    public void LoadContent()
    {
        foreach (var characterTableValue in CharacterRegistry.CharacterTable.Values)
        {
            SpawnCharacter(characterTableValue, 2);
        }
    }

    public void Update(GameTime gameTime)
    {
        foreach (var characterActor in _waitList)
        {
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
            characterActor.Update(gameTime);
        }
    }

    public void DrawWaiting(SpriteBatch spriteBatch)
    {
        foreach (var characterActor in _waitList)
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

    public static Dialog.Dialog.Page[] ParsePages(Dialog.Dialog.Page[] pages, CharacterActor characterActor)
    {
        Dialog.Dialog.Page[] newPages = new Dialog.Dialog.Page[pages.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            var page = pages[i];
            var content = page.Content;
            var parsedContent = content.Replace("$floorNumDest", characterActor.FloorNumberTarget.ToString());
            newPages[i] = new Dialog.Dialog.Page() { Content = parsedContent, CharInterval = page.CharInterval };
        }
        return newPages;
    }

    public IEnumerator EndOfTurnSequence()
    {
        yield return LeaveAtFloorSequence();
        yield return GetOnAtFloorSequence();
    }

    private IEnumerator LeaveAtFloorSequence()
    {
        for (int index = 0; index < _cabList.Count; index++)
        {
            var characterActor = _cabList[index];
            if (characterActor.FloorNumberTarget != MainGame.CurrentFloor) continue;

            _cabList.Remove(characterActor);
            index--;
            _movingList.Add(characterActor);
            yield return characterActor.GetOffElevatorBegin();
            MainGame.Coroutines.Stop("ticket_remove");
            MainGame.Coroutines.TryRun("ticket_remove", ticketManager.RemoveTicket(characterActor.FloorNumberTarget), out _);

            Dialog.Dialog.Page[] rawPages;
            Dialog.Dialog.DisplayMethod displayMethod = Dialog.Dialog.DisplayMethod.Human;
            if (characterActor.Def.ExitPhrases.Length == 0)
            {
                int randomCharCout = Random.Shared.Next(3, 30);
                string randomString = new string(Enumerable.Range(0, randomCharCout)
                    .Select(_ => (char)Random.Shared.Next('a', 'z' + 1)).ToArray());
                rawPages = [new Dialog.Dialog.Page() { Content = randomString }];
                displayMethod = Dialog.Dialog.DisplayMethod.Alien;
            }
            else
            {
                rawPages = characterActor.Def.ExitPhrases[Random.Shared.Next(characterActor.Def.ExitPhrases.Length)]
                    .Pages;
            }
            var parsesPages = ParsePages(rawPages, characterActor);
            yield return dialog.Display(parsesPages, displayMethod);

            _movingList.Remove(characterActor);
            _waitList.Add(characterActor);
            yield return characterActor.GetOffElevatorEnd();

            _waitList.Remove(characterActor);
        }
    }

    public IEnumerator GetOnAtFloorSequence()
    {
        for (var index = _waitList.Count - 1; index >= 0; index--)
        {
            if (_cabList.Count >= 10) break;

            var characterActor = _waitList[index];
            if (characterActor.FloorNumberCurrent != MainGame.CurrentFloor) continue;

            MainGame.Coroutines.TryRun("phone_show", phone.Open(false, false), out _);
            phone.CanOpen = false;
            // _cabList.ForEach(actor => actor.MoveOutOfTheWay());
            yield return characterActor.GetInElevatorBegin();
            _waitList.Remove(characterActor);
            _movingList.Add(characterActor);
            phone.HighlightOrder(characterActor);
            ticketManager.AddTicket(characterActor.FloorNumberTarget);

            Dialog.Dialog.Page[] rawPages;
            Dialog.Dialog.DisplayMethod displayMethod = Dialog.Dialog.DisplayMethod.Human;
            if (characterActor.Def.EnterPhrases.Length == 0)
            {
                int randomCharCout = Random.Shared.Next(3, 30);
                string randomString = new string(Enumerable.Range(0, randomCharCout)
                    .Select(_ => (char)Random.Shared.Next('a', 'z' + 1)).ToArray());
                rawPages = [new Dialog.Dialog.Page() { Content = randomString }];
                displayMethod = Dialog.Dialog.DisplayMethod.Alien;
            }
            else
            {
                rawPages = characterActor.Def.EnterPhrases[Random.Shared.Next(characterActor.Def.EnterPhrases.Length)]
                    .Pages;
            }
            var parsesPages = ParsePages(rawPages, characterActor);
            yield return dialog.Display(parsesPages, displayMethod);
            yield return phone.RemoveOrder(characterActor);

            yield return characterActor.GetInElevatorEnd();
            _movingList.Remove(characterActor);
            _cabList.Add(characterActor);
        }
    }

    public void SpawnCharacter(CharacterDef characterDef, int minFloor = 1)
    {
        var newCharacter = new CharacterActor
        {
            Def = characterDef,
            FloorNumberCurrent = Random.Shared.Next(minFloor, Elevator.Elevator.MaxFloors + 1),
            Patience = 5,
            OffsetXTarget = Random.Shared.Next(-48, 49)
        };
        SpawnCharacter(newCharacter);
    }

    public void SpawnCharacter(CharacterActor characterActor)
    {
        do
        {
            characterActor.FloorNumberTarget = Random.Shared.Next(1, Elevator.Elevator.MaxFloors + 1);
        } while (characterActor.FloorNumberTarget == characterActor.FloorNumberCurrent);

        phone.AddOrder(characterActor);

        Console.WriteLine(
            $"{characterActor.Def.Name} is going from {characterActor.FloorNumberCurrent} to {characterActor.FloorNumberTarget}");
        characterActor.LoadContent();
        _waitList.Add(characterActor);
    }
}
