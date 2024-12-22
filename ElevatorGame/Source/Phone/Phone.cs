using System;
using System.Collections;
using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ElevatorGame.Source.Phone;

public class Phone(Elevator.Elevator elevator)
{
    bool _isOpen;
    float _offset;

    public bool CanOpen { get; set; } = true;

    public void Update(GameTime gameTime)
    {
        if(CanOpen && ((!_isOpen && InputManager.GetPressed(Keys.Right) && elevator.State == Elevator.Elevator.ElevatorStates.Stopped) || (_isOpen && InputManager.GetPressed(Keys.Left))))
        {
            if((_isOpen = !_isOpen) == true)
            {
                MainGame.Coroutines.Stop("phone_hide");
                MainGame.Coroutines.TryRun("phone_show", Open(), 0, out _);
                elevator.SetState(Elevator.Elevator.ElevatorStates.Other);
            }
            else
            {
                MainGame.Coroutines.Stop("phone_show");
                MainGame.Coroutines.TryRun("phone_hide", Close(), 0, out _);
                elevator.SetState(Elevator.Elevator.ElevatorStates.Stopped);
            }
        }

        var camPos = MainGame.Camera.Position;
        camPos.X = _offset;
        MainGame.Camera.Position = camPos;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        
    }

    private IEnumerator Open()
    {
        CanOpen = false;
        _offset = 0;
        while(_offset < 31)
        {
            _offset = MathUtil.ExpDecay(
                _offset,
                32f,
                8,
                1/60f
            );
            yield return null;
        }
        _offset = 32;
        CanOpen = true;
    }

    private IEnumerator Close()
    {
        CanOpen = false;
        _offset = 32;
        while(_offset > 1)
        {
            _offset = MathUtil.ExpDecay(
                _offset,
                0,
                10,
                1/60f
            );
            yield return null;
        }
        _offset = 0;
        CanOpen = true;
    }
}
