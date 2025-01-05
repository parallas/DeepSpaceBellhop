using System;
using System.Collections;
using System.Diagnostics;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using Engine;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Utils;

namespace ElevatorGame.Source.Elevator;

public class Elevator(Action<int> onChangeFloorNumber, Func<IEnumerator> endOfTurnSequence) : IDisposable
{
    public const int ParallaxDoors = 35;
    public const int ParallaxWalls = 25;

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

    private float _floorNumber = 1;
    private int _targetFloorNumber = 1;
    private float _acceleration = 0.0005f;
    private float _velocity;
    private float _lastMaxUnsignedVelocity;
    private float _maxSpeed = 0.16f;

    private int _comboDirection = 0;
    private int _dir;

    private bool _stopping;
    private float _velocityParallax;

    public bool CanMove { get; set; } = true;

    private Doors _doors;
    private FloorNumberDisplay _floorNumbers;

    // FMOD
    private EventInstance _audioElevatorMove;
    private EventDescription _audioWhooshEventDescription;
    private EventInstance _audioBellUpEvent;
    private EventInstance _audioBellDownEvent;

    public void LoadContent()
    {
        // Load the elevator interior sprite
        var elevatorInteriorFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorInterior");
        _elevatorInteriorSprite = elevatorInteriorFile!.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);

        // Initialize the doors
        _doors = new Doors(this, elevatorInteriorFile);

        _floorNumbers = new FloorNumberDisplay();
        _floorNumbers.LoadContent(this, elevatorInteriorFile);

        _audioElevatorMove = StudioSystem.GetEvent("event:/SFX/Elevator/Move").CreateInstance();
        _audioElevatorMove.Start();
        _audioWhooshEventDescription = StudioSystem.GetEvent("event:/SFX/Elevator/Whoosh");
        
        _audioBellUpEvent = StudioSystem.GetEvent("event:/SFX/Elevator/Bell/Up").CreateInstance();
        _audioBellDownEvent = StudioSystem.GetEvent("event:/SFX/Elevator/Bell/Down").CreateInstance();

        _doors.Open();
    }

    public void UnloadContent()
    {
        Dispose();
    }

    public void Dispose()
    {
        _doors?.Dispose();
        _audioElevatorMove?.Stop();
        _audioElevatorMove?.Dispose();
        _audioBellUpEvent?.Stop();
        _audioBellUpEvent?.Dispose();
        _audioBellDownEvent?.Stop();
        _audioBellDownEvent?.Dispose();
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
                UpdateStateWaiting(gameTime);
                break;
            case ElevatorStates.Other:
                break;
        }

        if (State != ElevatorStates.Stopped && MainGame.CurrentMenu == MainGame.Menus.None)
        {
            MainGame.Cursor.CursorSpriteOverride = Cursor.CursorSprites.Wait;
        }

        _audioElevatorMove.SetParameterValue("Velocity", Math.Abs(_velocity) / _maxSpeed);

        float targetParallax = 4 * MathUtil.InverseLerp01(_maxSpeed * 0.6f, _maxSpeed, Math.Abs(_velocity)) * _dir;
        _velocityParallax = MathUtil.ExpDecay(_velocityParallax, targetParallax, 8,
            (float)gameTime.ElapsedGameTime.TotalSeconds);

        MainGame.CameraPosition = new(MainGame.CameraPosition.X, _velocityParallax);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        int floorTop = ((int)(_floorNumber * 140) % 140) - 5 + 8;

        _doors.Draw(spriteBatch, floorTop);

        Vector2 fillPos = MainGame.Camera.GetParallaxPosition(new Vector2(0, 0), ParallaxWalls);
        var elevatorColor = new Color(120, 105, 196, 255);
        spriteBatch.Draw(MainGame.PixelTexture, new Rectangle((int)fillPos.X - 100, (int)fillPos.Y, 100, 135 + 16),
            elevatorColor);
        spriteBatch.Draw(MainGame.PixelTexture, new Rectangle((int)fillPos.X + 240 + 16, (int)fillPos.Y, 100, 135 + 16),
            elevatorColor);
        spriteBatch.Draw(MainGame.PixelTexture, new Rectangle((int)fillPos.X, (int)fillPos.Y - 100, 240 + 16, 100),
            elevatorColor);
        spriteBatch.Draw(MainGame.PixelTexture, new Rectangle((int)fillPos.X, (int)fillPos.Y + 135 + 16, 240 + 16, 100),
            elevatorColor);
        _elevatorInteriorSprite.Draw(spriteBatch, MainGame.Camera.GetParallaxPosition(Vector2.Zero, ParallaxWalls));

        _floorNumbers.Draw(spriteBatch, _floorNumber, _comboDirection);

        Vector2 dotsCenter = Vector2.Round(MainGame.Camera.GetParallaxPosition(new((MainGame.GameBounds.Width / 2) + 8, 33), ParallaxWalls));
        var dotCount = MathHelper.Min(40, MainGame.FloorCount);

        for(int i = 0; i < dotCount; i++)
        {
            spriteBatch.Draw(
                MainGame.PixelTexture,
                dotsCenter + new Vector2(((i - (dotCount * 0.5f)) * 3) + 1, 0),
                Color.Black
            );

            if(MathUtil.RoundToInt(_floorNumber) == i + 1)
            {
                spriteBatch.Draw(
                    MainGame.PixelTexture,
                    dotsCenter + new Vector2(((i - (dotCount * 0.5f)) * 3) + 1, 0),
                    Color.Yellow * (1 - Math.Abs(MathUtil.RoundToInt(_floorNumber) - _floorNumber))
                );
            }
        }
    }

    private void UpdateStateStopped(GameTime gameTime)
    {
        if (MainGame.CurrentMenu != MainGame.Menus.None) return;
        int inputDir = 0;
        if(Keybindings.Up.Pressed)
            inputDir += 1;
        if(Keybindings.Down.Pressed)
            inputDir -= 1;

        if (inputDir > 0 && (int)Math.Round(_floorNumber) >= MainGame.FloorCount || inputDir < 0 && (int)Math.Round(_floorNumber) <= 1)
        {
            MainGame.Camera.SetShake(2, 15);
            return;
        }

        _dir = inputDir;

        if (_dir == 0) return;
        _comboDirection = _dir;
        _doors.Close();
        State = ElevatorStates.Closing;
    }

    private void UpdateStateMoving(GameTime gameTime)
    {
        int lastFloor = MathUtil.RoundToInt(_floorNumber);
        _floorNumber += _velocity;
        if (MathUtil.RoundToInt(_floorNumber) != lastFloor)
        {
            PlayWhoosh();
        }

        bool didSoftCrash = false;
        if(_floorNumber < 1 || _floorNumber > MainGame.FloorCount)
        {
            _floorNumber = MathHelper.Clamp(_floorNumber, 1, MainGame.FloorCount);

            _velocityParallax *= 0.25f;
            _dir = 0;

            _targetFloorNumber = (int)_floorNumber;

            if (Math.Abs(_velocity) < _maxSpeed * 0.5)
            {
                didSoftCrash = true;
            }
            else
            {
                MainGame.Camera.SetShake(4, 60);
                _velocity = 0;

                SetState(ElevatorStates.Other);
                MainGame.Coroutines.TryRun("elevator_crash", CrashSequence(), 0, out _);
                return;
            }
            _velocity = 0;
        }

        if((_dir == 1 && !Keybindings.Up.IsDown) || (_dir == -1 && !Keybindings.Down.IsDown) || didSoftCrash)
        {
            _targetFloorNumber = (int)Math.Round(_floorNumber);

            if (Math.Abs(_velocity) < _acceleration)
            {
                _audioElevatorMove.SetParameterValue("Velocity", 0.9f);
            }

            if(Math.Abs(_velocity) > _maxSpeed * 0.5f || Math.Sign(_targetFloorNumber - _floorNumber) != _dir)
            {
                int rolloverAmount = Math.Sign(_velocity);
                if (rolloverAmount == 0)
                    rolloverAmount = _dir;
                _targetFloorNumber += rolloverAmount;
            }

            _targetFloorNumber = MathHelper.Clamp(_targetFloorNumber, 1, MainGame.FloorCount);

            MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Wait;
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

            MainGame.Coroutines.TryRun("elevator_open", OpenSequence(), 0, out _);
        }
    }

    private IEnumerator OpenSequence()
    {
        PlayBell(_comboDirection);
        onChangeFloorNumber?.Invoke(_targetFloorNumber);
        State = ElevatorStates.Opening;
        MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Wait;
        var openHandle = _doors.Open();
        yield return openHandle.Wait();
        State = ElevatorStates.Waiting;
        MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Default;
        yield return endOfTurnSequence();
        State = ElevatorStates.Stopped;
    }

    private void UpdateStateWaiting(GameTime gameTime)
    {

    }

    public void SetFloor(int floorNumber)
    {
        _floorNumber = floorNumber;
    }

    public void Reset()
    {
        SetFloor(0);
        _comboDirection = 0;
        _dir = 0;
        _velocity = 0;
        _lastMaxUnsignedVelocity = 0;
        _stopping = false;
        _velocityParallax = 0;
        _doors.Open();
        State = ElevatorStates.Stopped;
    }

    private IEnumerator CrashSequence()
    {
        MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Wait;
        yield return 60;
        SetState(Elevator.ElevatorStates.Stopping);
    }

    public int GetComboDirection()
    {
        return _comboDirection;
    }

    public void SetComboDirection(int direction)
    {
        _comboDirection = Math.Sign(direction);
    }

    private void PlayWhoosh()
    {
        var whooshInstance = _audioWhooshEventDescription.CreateInstance();
        whooshInstance.Start();
        whooshInstance.SetParameterValue("Velocity", Math.Abs(_velocity) / _maxSpeed);
        whooshInstance.Dispose();
    }
    
    private void PlayBell(int direction)
    {
        if (direction == 1)
            _audioBellUpEvent.Start();
        else
            _audioBellDownEvent.Start();
    }
}
