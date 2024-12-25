using System;
using System.Collections;
using System.Collections.Generic;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using ElevatorGame.Source.Characters;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Utils;

namespace ElevatorGame.Source.Phone;

public class Phone(Elevator.Elevator elevator)
{
    public const float MaxOffset = 32f;

    private bool _isOpen;
    private float _offset;

    private AsepriteFile _phoneFile;
    private Sprite _phoneSprite;
    private AnimatedSprite _faceSpriteAnim;
    private AnimatedSprite _buttonsSpriteAnim;
    
    private RenderTarget2D _screenRenderTarget;
    
    private Rectangle _faceSliceKey;
    private Rectangle _buttonsSliceKey;
    private Rectangle _screenSliceKey;
    private Rectangle _dotSliceKey;

    private Rectangle _mouseRegion = new(193, 75, 38, 200);

    // sprite origin: 202, 79
    // mouse region origin: 193, 75

    private Vector2 _phonePosition;
    private Vector2 _dockedPhonePos;
    private Vector2 _openPhonePos;

    public bool CanOpen { get; set; } = true;
    
    private List<PhoneOrder> _orders = new();

    public void LoadContent()
    {
        _phoneFile = ContentLoader.Load<AsepriteFile>("graphics/phone/Phone")!;
        _phoneSprite = _phoneFile.CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);

        _faceSliceKey = _phoneFile.GetSlice("Face").Keys[0].Bounds.ToXnaRectangle();
        _buttonsSliceKey = _phoneFile.GetSlice("Buttons").Keys[0].Bounds.ToXnaRectangle();
        _screenSliceKey = _phoneFile.GetSlice("Screen").Keys[0].Bounds.ToXnaRectangle();
        _dotSliceKey = _phoneFile.GetSlice("Dot").Keys[0].Bounds.ToXnaRectangle();
        
        // Face
        _faceSpriteAnim = ContentLoader.Load<AsepriteFile>("graphics/phone/Face")!
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Tag");
        
        // Buttons
        _buttonsSpriteAnim = ContentLoader.Load<AsepriteFile>("graphics/phone/PhoneButtons")!
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Tag");
        
        _phonePosition = new Vector2(202, 77);

        _screenRenderTarget = new RenderTarget2D(MainGame.Graphics.GraphicsDevice, 28, 37);
    }
    
    public void UnloadContent()
    {
        _screenRenderTarget.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        _dockedPhonePos = new Vector2(202, 77);
        _openPhonePos = new Vector2(202 - _offset, 8);


        if(MainGame.CurrentMenu == MainGame.Menus.None || MainGame.CurrentMenu == MainGame.Menus.Phone)
        {
            bool rightPressed = !_isOpen && Keybindings.Right.Pressed && elevator.State == Elevator.Elevator.ElevatorStates.Stopped;
            bool leftPressed = _isOpen && Keybindings.Left.Pressed;

            bool mouseOver = new Rectangle(
                _isOpen
                    ? Vector2.Round(_openPhonePos).ToPoint() + new Point(14, 44)
                    : Vector2.Round(_dockedPhonePos).ToPoint(),
                _mouseRegion.Size
            ).Contains(MainGame.Cursor.ViewPosition);

            bool mouseEnter = mouseOver && InputManager.GetPressed(MouseButtons.LeftButton) && !_isOpen && elevator.State == Elevator.Elevator.ElevatorStates.Stopped;
            bool mouseExit = !mouseOver && InputManager.GetPressed(MouseButtons.LeftButton) && _isOpen;

            if(mouseOver)
            {
                if(!_isOpen)
                    MainGame.Cursor.CursorSpriteOverride = Cursor.CursorSprites.OpenPhone;
            }
            else if(_isOpen)
            {
                MainGame.Cursor.CursorSpriteOverride = Cursor.CursorSprites.Close;
            }

            if(CanOpen && ((rightPressed || leftPressed) ^ (mouseEnter || mouseExit)))
            {
                if(!_isOpen)
                {
                    MainGame.Coroutines.Stop("phone_hide");
                    MainGame.Coroutines.TryRun("phone_show", Open(true), 0, out _);
                    elevator.SetState(Elevator.Elevator.ElevatorStates.Other);
                    MainGame.CurrentMenu = MainGame.Menus.Phone;
                }
                else
                {
                    MainGame.Coroutines.Stop("phone_show");
                    MainGame.Coroutines.TryRun("phone_hide", Close(true), 0, out _);
                    elevator.SetState(Elevator.Elevator.ElevatorStates.Stopped);
                    MainGame.CurrentMenu = MainGame.Menus.None;
                }
            }
        }

        float blend = _offset / 32f;
        Vector2 blendedPhonePos = Vector2.Lerp(_dockedPhonePos, _openPhonePos, blend);
        Vector2 phonePos = MainGame.GetCursorParallaxValue(blendedPhonePos, 25);
        _phonePosition = MathUtil.ExpDecay(_phonePosition, phonePos, 13f, 1f / 60f);

        foreach (var order in _orders)
        {
            order.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 phonePos = Vector2.Round(_phonePosition);
        _phoneSprite.Draw(spriteBatch, phonePos);
        _faceSpriteAnim.Draw(spriteBatch, phonePos + _faceSliceKey.Location.ToVector2());
        _buttonsSpriteAnim.Draw(spriteBatch, phonePos + _buttonsSliceKey.Location.ToVector2());
        
        Vector2 screenPos = phonePos + _screenSliceKey.Location.ToVector2();
        spriteBatch.Draw(_screenRenderTarget, screenPos + Vector2.One, Color.Black * 0.1f);
        spriteBatch.Draw(_screenRenderTarget, screenPos, Color.White);
    }

    public void PreRenderScreen(SpriteBatch spriteBatch)
    {
        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_screenRenderTarget);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        {
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            foreach (var order in _orders)
            {
                order.Draw(spriteBatch);
            }
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public IEnumerator Open(bool shiftCam, bool changeCanOpen = true)
    {
        if (_isOpen)
        {
            MainGame.Coroutines.Stop("phone_show");
            yield return null;
        };
        _isOpen = true;
        MainGame.Coroutines.Stop("phone_hide");
        CanOpen = false;
        _offset = 0;
        while(_offset < MaxOffset - 1)
        {
            _offset = MathUtil.ExpDecay(
                _offset,
                MaxOffset,
                8,
                1/60f
            );
            
            if (shiftCam)
            {
                var camPos = MainGame.CameraPosition;
                camPos.X = _offset;
                MainGame.CameraPosition = camPos;
                MainGame.GrayscaleCoeff = 1-(_offset / MaxOffset);
            }
            
            yield return null;
        }
        _offset = MaxOffset;
        if (shiftCam)
        {
            var camPos = MainGame.CameraPosition;
            camPos.X = _offset;
            MainGame.CameraPosition = camPos;
            MainGame.GrayscaleCoeff = 1-(_offset / MaxOffset);
        }
        if(changeCanOpen)
            CanOpen = true;
    }

    public IEnumerator Close(bool shiftCam, bool changeCanOpen = true)
    {
        if (!_isOpen)
        {
            MainGame.Coroutines.Stop("phone_hide");
            yield return null;
        };
        _isOpen = false;
        MainGame.Coroutines.Stop("phone_show");
        CanOpen = false;
        _offset = MaxOffset;
        while(_offset > 1)
        {
            _offset = MathUtil.ExpDecay(
                _offset,
                0,
                10,
                1/60f
            );
            
            if (shiftCam)
            {
                var camPos = MainGame.CameraPosition;
                camPos.X = _offset;
                MainGame.CameraPosition = camPos;
                MainGame.GrayscaleCoeff = 1-(_offset / MaxOffset);
            }
            
            yield return null;
        }
        _offset = 0;
        if (shiftCam)
        {
            var camPos = MainGame.CameraPosition;
            camPos.X = _offset;
            MainGame.CameraPosition = camPos;
            MainGame.GrayscaleCoeff = 1-(_offset / MaxOffset);
        }
        if(changeCanOpen)
            CanOpen = true;
    }

    public void AddOrder(CharacterActor characterActor)
    {
        PhoneOrder newOrder = new PhoneOrder()
        {
            FloorNumber = characterActor.FloorNumberCurrent,
            DestinationNumber = characterActor.FloorNumberTarget,
            Mood = 0,
            TargetPosition = new Vector2(0, _orders.Count * 6)
        };
        _orders.Add(newOrder);
    }
    
    public void HighlightOrder(CharacterActor characterActor)
    {
        var order = _orders[_orders.FindIndex(order =>
            order.FloorNumber == characterActor.FloorNumberCurrent &&
            order.DestinationNumber == characterActor.FloorNumberTarget
        )];

        order.Highlighted = true;
    }
    
    public void UnhighlightOrder(CharacterActor characterActor)
    {
        var order = _orders[_orders.FindIndex(order =>
            order.FloorNumber == characterActor.FloorNumberCurrent &&
            order.DestinationNumber == characterActor.FloorNumberTarget
        )];

        order.Highlighted = false;
    }

    public IEnumerator RemoveOrder(CharacterActor characterActor)
    {
        var order = _orders.Find(order =>
            order.FloorNumber == characterActor.FloorNumberCurrent &&
            order.DestinationNumber == characterActor.FloorNumberTarget
        );

        yield return 20; // plinky HERE

        order.TargetPosition = new(30, order.TargetPosition.Y);
        while(order.Position.X <= 28)
        {
            yield return null;
        }

        _orders.Remove(order);

        for (int i = 0; i < _orders.Count; i++)
        {
            _orders[i].TargetPosition = new Vector2(0, i * 6);
            yield return 5;
        }
    }
}
