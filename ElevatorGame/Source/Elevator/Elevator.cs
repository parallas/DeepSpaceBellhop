using System;
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
    private Sprite _elevatorInteriorSprite;
    private Sprite _elevatorLeftDoorSprite;
    private Sprite _elevatorRightDoorSprite;
    private AnimatedSprite _elevatorNumbersAnimSprite;
    
    private AsepriteSliceKey _elevatorDoorLeftSlice;
    private AsepriteSliceKey _elevatorDoorRightSlice;
    private AsepriteSliceKey _elevatorNumberTensSlice;
    private AsepriteSliceKey _elevatorNumberOnesSlice;

    private Vector2 _doorLeftOrigin;
    private Vector2 _doorRightOrigin;

    private float _floorNumber;
    private float _acceleration = 0.0005f;
    private float _friction = 0.005f;
    private float _velocity;
    private float _lastMaxUnsignedVelocity;
    private float _maxSpeed = 0.16f;
    private float _targetFloorNumber;
    private float _distToStopTarget;

    private int _turns;
    private int _dir;

    private bool _stopping;

    public void LoadContent()
    {
        // Load the elevator interior sprite
        var elevatorInteriorFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorInterior");
        _elevatorInteriorSprite = elevatorInteriorFile!.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        
        // Get the slices
        _elevatorDoorLeftSlice = elevatorInteriorFile.GetSlice("DoorL").Keys[0];
        _elevatorDoorRightSlice = elevatorInteriorFile.GetSlice("DoorR").Keys[0];
        _elevatorNumberTensSlice = elevatorInteriorFile.GetSlice("DigitTens").Keys[0];
        _elevatorNumberOnesSlice = elevatorInteriorFile.GetSlice("DigitOnes").Keys[0];
        
        // Set the target positions for the doors when closed (based on slices)
        var leftDoorSliceBounds = _elevatorDoorLeftSlice.Bounds.ToXnaRectangle();
        var leftDoorTopRight = new Vector2(leftDoorSliceBounds.Right - 1, leftDoorSliceBounds.Y);
        _doorLeftOrigin = leftDoorTopRight;
        var rightDoorSliceBounds = _elevatorDoorRightSlice.Bounds.ToXnaRectangle();
        var rightDoorTopLeft = rightDoorSliceBounds.Location.ToVector2();
        _doorRightOrigin = rightDoorTopLeft;
        
        // Load the door sprites, and set their properties
        var elevatorDoorFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorDoor");
        _elevatorLeftDoorSprite = elevatorDoorFile!.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        _elevatorRightDoorSprite = elevatorDoorFile!.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        _elevatorLeftDoorSprite.Origin = new Vector2(_elevatorLeftDoorSprite.Width - 1, 0);
        _elevatorRightDoorSprite.FlipHorizontally = true;
        
        // Load the animated numbers sprite
        var elevatorNumbersAnimFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorNumbers");
        var elevatorNumbersSpriteSheet = elevatorNumbersAnimFile!.CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false);
        _elevatorNumbersAnimSprite = elevatorNumbersSpriteSheet.CreateAnimatedSprite("Tag");
        _elevatorNumbersAnimSprite.Speed = 0;
    }
    
    public void Update(GameTime gameTime)
    {
        if (_dir == 0 && !_stopping)
        {
            int inputDir = 0;
            if(InputManager.GetPressed(Keys.Up))
                inputDir += 1;
            if(InputManager.GetPressed(Keys.Down))
                inputDir -= 1;
            _dir = inputDir;
        }

        if((_dir == 1 && InputManager.GetReleased(Keys.Up)) || (_dir == -1 && InputManager.GetReleased(Keys.Down)) && !_stopping)
        {
            _stopping = true;

            var lastFloor = _targetFloorNumber;
            _targetFloorNumber = MathF.Round(_floorNumber);
            _distToStopTarget = Math.Abs(_targetFloorNumber - _floorNumber);

            if(Math.Abs(_velocity) > _maxSpeed * 0.5f || Math.Sign(_targetFloorNumber - _floorNumber) != _dir)
            {
                _targetFloorNumber += Math.Sign(_velocity);
            }

            if(_targetFloorNumber != lastFloor)
            {
                _turns++;
                Console.WriteLine($"_turns: {_turns}");
            }
        }

        if(!_stopping)
        {
            _velocity = MathUtil.Approach(_velocity, _dir * _maxSpeed, _acceleration);
            _lastMaxUnsignedVelocity = Math.Max(_lastMaxUnsignedVelocity, Math.Abs(_velocity));
        }
        else
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
                _stopping = false;
                _dir = 0;
            }
        }

        _floorNumber += _velocity;

        if(_floorNumber < 0 || _floorNumber > 40)
        {
            _floorNumber = MathHelper.Clamp(_floorNumber, 0, 40);
            _velocity = 0;
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        int floorTop = ((int)(_floorNumber * 140) % 140) - 5;

        spriteBatch.Draw(
            MainGame.PixelTexture,
            new Rectangle(
                _elevatorDoorLeftSlice.Bounds.X,
                0,
                _elevatorDoorLeftSlice.Bounds.Width + 2 + _elevatorDoorRightSlice.Bounds.Width,
                135
            ),
            Color.Black
        );

        DrawLight(spriteBatch, floorTop);

        _elevatorLeftDoorSprite.Draw(spriteBatch, _doorLeftOrigin);
        _elevatorRightDoorSprite.Draw(spriteBatch, _doorRightOrigin);
        _elevatorInteriorSprite.Draw(spriteBatch, Vector2.Zero);

        if (MathF.Round(_floorNumber) < 10)
            _elevatorNumbersAnimSprite.SetFrame(10);
        else
            _elevatorNumbersAnimSprite.SetFrame((int)MathF.Round(_floorNumber) / 10 % 10);
        _elevatorNumbersAnimSprite.Draw(spriteBatch, _elevatorNumberTensSlice.Bounds.Location.ToVector2());

        _elevatorNumbersAnimSprite.SetFrame((int)MathF.Round(_floorNumber) % 10);
        _elevatorNumbersAnimSprite.Draw(spriteBatch, _elevatorNumberOnesSlice.Bounds.Location.ToVector2());
    }

    private static void DrawLight(SpriteBatch spriteBatch, int floorTop)
    {
        int lightTop = floorTop + 40;

        // spriteBatch.Draw(MainGame.PixelTexture, new Vector2(119, floorTop), Color.White);
        spriteBatch.Draw(MainGame.PixelTexture, new Rectangle(119, lightTop, 2, 100), Color.White);
        spriteBatch.Draw(MainGame.PixelTexture, new Rectangle(119, lightTop - 140, 2, 100), Color.White);
    }
}
