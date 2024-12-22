using System;
using System.Collections;
using System.Diagnostics;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Utils;

namespace ElevatorGame.Source.Elevator;

public class Elevator
{
    public static readonly int ParallaxDoors = 25;
    public static readonly int ParallaxWalls = 15;
    
    public enum ElevatorStates
    {
        Stopped,
        Closing,
        Moving,
        Stopping,
        Opening,
        Waiting,
        Other,
    }
    public ElevatorStates State { get; private set; } = ElevatorStates.Stopped;
    public void SetState(ElevatorStates state) => State = state;
    
    private Sprite _elevatorInteriorSprite;
    private AnimatedSprite _elevatorNumbersAnimSprite;
    
    private AsepriteSliceKey _elevatorNumberTensSlice;
    private AsepriteSliceKey _elevatorNumberOnesSlice;

    private float _floorNumber = 1;
    private int _targetFloorNumber = 1;
    private float _acceleration = 0.0005f;
    private float _velocity;
    private float _lastMaxUnsignedVelocity;
    private float _maxSpeed = 0.16f;

    private int _turns;
    private int _dir;

    private bool _stopping;
    private float _velocityParallax;

    public bool CanMove { get; set; } = true;

    private Doors _doors;

    public void LoadContent()
    {
        // Load the elevator interior sprite
        var elevatorInteriorFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorInterior");
        _elevatorInteriorSprite = elevatorInteriorFile!.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        
        // Get the slices
        _elevatorNumberTensSlice = elevatorInteriorFile.GetSlice("DigitTens").Keys[0];
        _elevatorNumberOnesSlice = elevatorInteriorFile.GetSlice("DigitOnes").Keys[0];
        
        // Load the animated numbers sprite
        var elevatorNumbersAnimFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorNumbers");
        var elevatorNumbersSpriteSheet = elevatorNumbersAnimFile!.CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false);
        _elevatorNumbersAnimSprite = elevatorNumbersSpriteSheet.CreateAnimatedSprite("Tag");
        _elevatorNumbersAnimSprite.Speed = 0;

        // Initialize the doors
        _doors = new Doors(this, elevatorInteriorFile);
    }

    public void Update(GameTime gameTime)
    {
        switch (State)
        {
            case ElevatorStates.Stopped:
                UpdateStateStopped(gameTime);
                break;
            case ElevatorStates.Closing:
                // UpdateStateClosing(gameTime);
                break;
            case ElevatorStates.Moving:
                UpdateStateMoving(gameTime);
                break;
            case ElevatorStates.Stopping:
                UpdateStateStopping(gameTime);
                break;
            case ElevatorStates.Opening:
                // UpdateStateOpening(gameTime);
                break;
            case ElevatorStates.Waiting:
                // UpdateStateWaiting(gameTime);
                _turns++;
                Console.WriteLine($"Turns: {_turns}");
                SetState(ElevatorStates.Stopped);
                break;
            case ElevatorStates.Other:
                return;
                break;
        }

        float targetParallax = 4 * MathUtil.InverseLerp01(_maxSpeed * 0.5f, _maxSpeed, Math.Abs(_velocity)) * _dir;
        _velocityParallax = MathUtil.ExpDecay(_velocityParallax, targetParallax, 8,
            (float)gameTime.ElapsedGameTime.TotalSeconds);
        
        MainGame.Camera.Position = new(MainGame.Camera.Position.X, _velocityParallax);
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        int floorTop = ((int)(_floorNumber * 140) % 140) - 5 + 8;
        
        _doors.Draw(spriteBatch, floorTop);

        _elevatorInteriorSprite.Draw(spriteBatch, MainGame.Camera.GetParallaxPosition(Vector2.Zero, ParallaxWalls));

        DrawNumbers(spriteBatch);
    }

    private void UpdateStateStopped(GameTime gameTime)
    {
        int inputDir = 0;
        if(InputManager.GetPressed(Keys.Up) && (int)Math.Round(_floorNumber) != 40)
            inputDir += 1;
        if(InputManager.GetPressed(Keys.Down) && (int)Math.Round(_floorNumber) != 1)
            inputDir -= 1;
        _dir = inputDir;

        if(_dir != 0)
        {
            _doors.Close();
            State = ElevatorStates.Closing;
        }
    }

    private void UpdateStateMoving(GameTime gameTime)
    {
        _floorNumber += _velocity;
        
        if(_floorNumber < 1 || _floorNumber > 40)
        {
            _floorNumber = MathHelper.Clamp(_floorNumber, 1, 40);

            if(_velocity != 0)
                MainGame.Camera.SetShake(10 * _velocity, (int)(90 * _velocity));

            _velocity = 0;
            _velocityParallax *= 0.25f;
            _dir = 0;

            _targetFloorNumber = (int)_floorNumber;
            
            SetState(ElevatorStates.Other);
            MainGame.Coroutines.TryRun("elevator_crash", CrashSequence(), 0, out _);
            return;
        }
        
        if((_dir == 1 && !InputManager.GetDown(Keys.Up)) || (_dir == -1 && !InputManager.GetDown(Keys.Down)))
        {
            int lastFloor = _targetFloorNumber;
            _targetFloorNumber = (int)Math.Round(_floorNumber);

            if(Math.Abs(_velocity) > _maxSpeed * 0.5f || Math.Sign(_targetFloorNumber - _floorNumber) != _dir)
            {
                _targetFloorNumber += Math.Sign(_velocity);
            }

            _targetFloorNumber = MathHelper.Clamp(_targetFloorNumber, 1, 40);
            
            State = ElevatorStates.Stopping;
            return;
        }
        
        _velocity = MathUtil.Approach(_velocity, _dir * _maxSpeed, _acceleration);
        _lastMaxUnsignedVelocity = Math.Max(_lastMaxUnsignedVelocity, Math.Abs(_velocity));
    }

    private void UpdateStateStopping(GameTime gameTime)
    {
        _velocity = 0;
        _floorNumber = MathUtil.ExpDecay(
            _floorNumber, 
            _targetFloorNumber, 
            8,
            (float)gameTime.ElapsedGameTime.TotalSeconds
        );
        if(Math.Abs(_targetFloorNumber - _floorNumber) < 1/140f)
        {
            _floorNumber = _targetFloorNumber;
            _dir = 0;

            _doors.Open();
            State = ElevatorStates.Opening;
        }
    }

    private void DrawNumbers(SpriteBatch spriteBatch)
    {
        if ((int)MathF.Round(_floorNumber) < 10)
            _elevatorNumbersAnimSprite.SetFrame(10);
        else
            _elevatorNumbersAnimSprite.SetFrame((int)MathF.Round(_floorNumber) / 10 % 10);
        _elevatorNumbersAnimSprite.Draw(spriteBatch,
            MainGame.Camera.GetParallaxPosition(_elevatorNumberTensSlice.GetLocation(), ParallaxWalls));

        if((int)MathF.Round(_floorNumber) == 1)
            _elevatorNumbersAnimSprite.SetFrame(11);
        else
            _elevatorNumbersAnimSprite.SetFrame((int)MathF.Round(_floorNumber) % 10);
        _elevatorNumbersAnimSprite.Draw(spriteBatch,
            MainGame.Camera.GetParallaxPosition(_elevatorNumberOnesSlice.GetLocation(), ParallaxWalls));
    }
    
    private IEnumerator CrashSequence()
    {
        yield return 60;
        SetState(Elevator.ElevatorStates.Stopping);
    }
}
