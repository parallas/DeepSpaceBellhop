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
    private AnimatedSprite _batterySpriteAnim;
    private AnimatedSprite _emoticonRowSpriteAnim;

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
    
    private float _nudgeOffset = 0;
    public float ScrollTarget { get; set; }
    private float _scrollOffset;

    private int _simulatedBatteryValue;

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

        // Battery
        _batterySpriteAnim = ContentLoader.Load<AsepriteFile>("graphics/phone/PhoneBattery")!
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true,
                innerPadding: 2
            )
            .CreateAnimatedSprite("Tag");

        // Emoticon row
        _emoticonRowSpriteAnim = ContentLoader.Load<AsepriteFile>("graphics/phone/EmoticonRow")!
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Tag");
        
        _phonePosition = new Vector2(202, 77);
        _simulatedBatteryValue = MainGame.CurrentHealth;

        _screenRenderTarget = new RenderTarget2D(MainGame.Graphics.GraphicsDevice, 26, 35);
    }
    
    public void UnloadContent()
    {
        _screenRenderTarget.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        _dockedPhonePos = new Vector2(202, 77);
        _openPhonePos = new Vector2(202 - _offset, 8);

        _nudgeOffset = MathUtil.ExpDecay(_nudgeOffset, 0, 8f, 1f / 60f);
        _scrollOffset = MathUtil.ExpDecay(_scrollOffset, ScrollTarget, 16f, 1f / 60f);

        int bottomPaddingRows = 4;
        var scrollTargetMax = MathHelper.Max(0, (_orders.Count + (_orders.Count > 4 ? 1 : 0) - bottomPaddingRows) * 6);
        if (ScrollTarget > scrollTargetMax)
        {
            PlayEmoticonReaction();
        }
        if (ScrollTarget < 0)
        {
            PlayFaceReaction();
        }
        ScrollTarget = MathHelper.Clamp(ScrollTarget, 0, scrollTargetMax);

        if(MainGame.CurrentMenu == MainGame.Menus.None || MainGame.CurrentMenu == MainGame.Menus.Phone)
        {
            bool rightPressed = !_isOpen && Keybindings.Right.Pressed && elevator.State == Elevator.Elevator.ElevatorStates.Stopped;
            bool leftPressed = _isOpen && Keybindings.Left.Pressed;

            if (_isOpen)
            {
                int scrollInput = (Keybindings.Down.Pressed ? 1 : 0) + (Keybindings.Up.Pressed ? -1 : 0) - InputManager.GetScrollDelta();
                if (scrollInput != 0)
                {
                    Scroll(Math.Sign(scrollInput));
                    PlayButtonPress(scrollInput);
                }
            }

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
                MainGame.Cursor.CursorSpriteOverride = Cursor.CursorSprites.ClosePhone;
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

        _batterySpriteAnim.SetFrame(MainGame.CurrentHealth);
        if (_simulatedBatteryValue != MainGame.CurrentHealth)
        {
            bool changeUp = _simulatedBatteryValue > MainGame.CurrentHealth;
            SetFace(changeUp ? 5 : 3);
            int stepTime = changeUp ? 6 : 10;
            bool showChangedFrame = MainGame.Step % stepTime < stepTime * 0.5;
            _batterySpriteAnim.SetFrame(showChangedFrame ? _simulatedBatteryValue : MainGame.CurrentHealth);
        }

        float blend = _offset / 32f;
        Vector2 blendedPhonePos = Vector2.Lerp(_dockedPhonePos, _openPhonePos, blend);
        Vector2 phonePos = MainGame.GetCursorParallaxValue(blendedPhonePos, 25);
        _phonePosition = MathUtil.ExpDecay(_phonePosition, phonePos, 13f, 1f / 60f);

        // if (InputManager.GetPressed(Keys.T))
        // {
        //     // _orders.Sort((a, b) => Math.Abs(MainGame.CurrentFloor - a.FloorNumber).CompareTo(Math.Abs(MainGame.CurrentFloor - b.FloorNumber)));
        //     // _orders.Sort((a, b) => -(a.DestinationNumber > a.FloorNumber).CompareTo(b.DestinationNumber > b.FloorNumber));
        //     _orders.Sort((a, b) => a.FloorNumber.CompareTo(b.FloorNumber));
        //     for (var i = 0; i < _orders.Count; i++)
        //     {
        //         _orders[i].TargetPosition = new Vector2(0, i * 6);
        //     }
        // }

        foreach (var order in _orders)
        {
            order.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 phonePos = Vector2.Round(_phonePosition + Vector2.UnitY * _nudgeOffset);
        _phoneSprite.Draw(spriteBatch, phonePos);
        _faceSpriteAnim.Draw(spriteBatch, phonePos + _faceSliceKey.Location.ToVector2());
        _buttonsSpriteAnim.Draw(spriteBatch, phonePos + _buttonsSliceKey.Location.ToVector2());
        
        Vector2 screenPos = phonePos + _screenSliceKey.Location.ToVector2() + Vector2.One;
        spriteBatch.Draw(_screenRenderTarget, screenPos + Vector2.One, Color.Black * 0.1f);
        spriteBatch.Draw(_screenRenderTarget, screenPos, Color.White);
    }

    public void PreRenderScreen(SpriteBatch spriteBatch)
    {
        Matrix matrix =
            Matrix.CreateTranslation(Vector3.Round((Vector3.UnitY * 6) - Vector3.UnitY * (_scrollOffset - 3)));
        matrix *= Matrix.CreateTranslation(-1, -1, 0);
        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_screenRenderTarget);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp,
            transformMatrix: matrix);
        {
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            Color mainColor = ColorUtil.CreateFromHex(0x40318d);

            _batterySpriteAnim.Color = mainColor;
            _batterySpriteAnim.Draw(spriteBatch, new Vector2(2, -6));

            foreach (var order in _orders)
            {
                order.Draw(spriteBatch);
            }
            if(_orders.Count > 4)
            {
                _emoticonRowSpriteAnim.Color = mainColor;
                _emoticonRowSpriteAnim.Draw(spriteBatch, new Vector2(1, 8 + _orders[^1].Position.Y));
            }
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public void SetFace(int faceIndex)
    {
        _faceSpriteAnim.SetFrame(faceIndex);
    }

    private void PlayFaceReaction()
    {
        MainGame.Coroutines.Stop("phone_face_reaction");
        MainGame.Coroutines.TryRun("phone_face_reaction", FaceReactionSequence(), 0, out _);
    }

    private void PlayEmoticonReaction()
    {
        MainGame.Coroutines.Stop("phone_emoticon_reaction");
        MainGame.Coroutines.TryRun("phone_emoticon_reaction", EmoticonReactionSequence(), 0, out _);
    }

    private IEnumerator FaceReactionSequence()
    {
        _faceSpriteAnim.SetFrame(1);

        yield return 60;

        _faceSpriteAnim.SetFrame(0);
    }

    private IEnumerator EmoticonReactionSequence()
    {
        int randomChance = Random.Shared.Next(0, 100);
        _emoticonRowSpriteAnim.SetFrame(1);
        if (randomChance == 0)
        {
            _emoticonRowSpriteAnim.SetFrame(2);
        }

        yield return 60;

        _emoticonRowSpriteAnim.SetFrame(0);
    }

    private void PlayButtonPress(int dir)
    {
        MainGame.Coroutines.Stop("phone_button_press");
        MainGame.Coroutines.TryRun("phone_button_press", ButtonPressSequence(dir), 0, out _);
    }

    private IEnumerator ButtonPressSequence(int dir)
    {
        _buttonsSpriteAnim.SetFrame(dir switch {
            1 => 2,
            -1 => 1,
            _ => 0
        });

        yield return 15;

        _buttonsSpriteAnim.SetFrame(0);
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
        SetFace(0); // Reset
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

        foreach (var o in _orders) o.MarkAsViewed();
    }

    public void AddOrder(CharacterActor characterActor)
    {
        PhoneOrder newOrder = new PhoneOrder(characterActor.CharacterId)
        {
            FloorNumber = characterActor.FloorNumberCurrent,
            DestinationNumber = characterActor.FloorNumberTarget,
            Mood = 0,
            TargetPosition = new Vector2(0, _orders.Count * 6)
        };
        _orders.Add(newOrder);

        _orders.Sort((a, b) => a.FloorNumber.CompareTo(b.FloorNumber));
        for (var i = 0; i < _orders.Count; i++)
        {
            _orders[i].TargetPosition = new Vector2(0, i * 6);
            _orders[i].SnapToTarget();
        }
    }
    
    public void HighlightOrder(CharacterActor characterActor)
    {
        SetFace(4); // Lucky Cat

        var index = _orders.FindIndex(order =>
            order.FloorNumber == characterActor.FloorNumberCurrent &&
            order.DestinationNumber == characterActor.FloorNumberTarget
        );
        var order = _orders[index];

        ScrollTo(index);

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

    public void SetOrderMood(Guid characterId, int mood)
    {
        _orders.Find(order => order.CharacterId == characterId).Mood = mood;
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

    public IEnumerator CancelOrder(Guid characterId)
    {
        var order = _orders.Find(order => order.CharacterId == characterId);
        var index = _orders.IndexOf(order);

        ScrollTo(index);

        yield return 20; // plinky HERE

        yield return order.DeleteSequence();

        _orders.Remove(order);

        for (int i = 0; i < _orders.Count; i++)
        {
            _orders[i].TargetPosition = new Vector2(0, i * 6);
            yield return 5;
        }
    }

    public void SimulateBatteryChange(int change)
    {
        SimulateBatteryValue(_simulatedBatteryValue + change);
    }

    public void SimulateBatteryValue(int newValue)
    {
        _simulatedBatteryValue = Math.Clamp(newValue, 0, 8);
    }

    public void Scroll(int direction)
    {
        _nudgeOffset = direction;
        ScrollTarget += direction * 6;
        _scrollOffset += direction * 2;
    }

    public void ScrollTo(int index)
    {
        ScrollTarget = (index + 1) * 6;
    }

    public void ScrollToTop()
    {
        ScrollTarget = 0;
    }
}
