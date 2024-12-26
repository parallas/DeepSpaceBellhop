using System;
using System.Collections;
using System.Collections.Generic;
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
        for (int i = 0; i < 5; i++)
        {
            foreach (var characterTableValue in CharacterRegistry.CharacterTable.Values)
            {
                SpawnCharacter(characterTableValue);
            }
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
            if (_cabList.Count <= 10)
            {
                var hitPerson = _cabList.Find((actor =>
                    actor != characterActor &&
                    MathUtil.Approximately(actor.OffsetXTarget, characterActor.OffsetXTarget,
                        MathHelper.Lerp(32, 16, _cabList.Count / 10f))));
                if (hitPerson is not null)
                {
                    var dir = Math.Sign(characterActor.OffsetXTarget - hitPerson.OffsetXTarget);
                    if (dir == 0) dir = 1;
                    var target = characterActor.OffsetXTarget + dir;
                    characterActor.OffsetXTarget = Math.Clamp(target, -CharacterActor.StandingRoomSize,
                        CharacterActor.StandingRoomSize);
                }
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

    public IEnumerator EndOfTurnSequence()
    {
        for (int index = 0; index < _cabList.Count; index++)
        {
            var characterActor = _cabList[index];
            if (characterActor.FloorNumberTarget == MainGame.CurrentFloor)
            {
                _cabList.Remove(characterActor);
                index--;
                _movingList.Add(characterActor);
                yield return characterActor.GetOffElevatorBegin();
                MainGame.Coroutines.Stop("ticket_remove");
                MainGame.Coroutines.TryRun("ticket_remove", ticketManager.RemoveTicket(characterActor.FloorNumberTarget), out _);
                yield return dialog.Display(characterActor.Def.ExitPhrases[0].Pages,
                    Dialog.Dialog.DisplayMethod.Human);

                _movingList.Remove(characterActor);
                _waitList.Add(characterActor);
                yield return characterActor.GetOffElevatorEnd();

                _waitList.Remove(characterActor);
            }
        }

        for (var index = _waitList.Count - 1; index >= 0; index--)
        {
            var characterActor = _waitList[index];
            if (characterActor.FloorNumberCurrent == MainGame.CurrentFloor)
            {
                MainGame.Coroutines.TryRun("phone_show", phone.Open(false, false), out _);
                phone.CanOpen = false;
                // _cabList.ForEach(actor => actor.MoveOutOfTheWay());
                yield return characterActor.GetInElevatorBegin();
                _waitList.Remove(characterActor);
                _movingList.Add(characterActor);
                phone.HighlightOrder(characterActor);
                ticketManager.AddTicket(characterActor.FloorNumberTarget);
                yield return dialog.Display(characterActor.Def.EnterPhrases[0].Pages,
                    Dialog.Dialog.DisplayMethod.Human);
                yield return phone.RemoveOrder(characterActor);

                yield return characterActor.GetInElevatorEnd();
                _movingList.Remove(characterActor);
                _cabList.Add(characterActor);
            }
        }
    }

    public void SpawnCharacter(CharacterDef characterDef)
    {
        var newCharacter = new CharacterActor
        {
            Def = characterDef,
            FloorNumberCurrent = Random.Shared.Next(1, Elevator.Elevator.MaxFloors + 1),
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
