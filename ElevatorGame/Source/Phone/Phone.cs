using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using ElevatorGame.Source.Characters;
using Engine;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Utils;

namespace ElevatorGame.Source.Phone;

public class Phone(Elevator.Elevator elevator) : IDisposable
{
    public const float MaxOffset = 32f;

    private bool _isOpen;
    private float _offset;

    private bool _isTalking;

    private AsepriteFile _phoneFile;
    private Sprite _phoneSprite;
    private AnimatedSprite _faceSpriteAnim;
    private AnimatedSprite _buttonsSpriteAnim;
    private AnimatedSprite _batterySpriteAnim;
    private AnimatedSprite _emoticonRowSpriteAnim;
    private AnimatedSprite _idleSpriteAnim;

    private RenderTarget2D _screenRenderTarget;
    
    private Rectangle _faceSliceKey;
    private Rectangle _buttonsSliceKey;
    private Rectangle _screenSliceKey;
    private Rectangle _dotSliceKey;

    private AnimatedSprite _dotSpriteAnim;
    private AnimatedSprite _dotNormalSpriteAnim;
    private AnimatedSprite _dotStarSpriteAnim;
    private AnimatedSprite _dotTransitionSpriteAnim;

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
    
    private readonly List<PhoneOrder> _orders = [];

    private EventInstance _audioOpen;
    private EventInstance _audioClose;
    private EventInstance _audioNotification;
    private EventInstance _audioBump;
    private EventInstance _audioJingle;
    private EventInstance _audioHurt;
    private EventInstance _audioHeal;
    private EventInstance _audioCorrect;

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

        // Idle
        _idleSpriteAnim = ContentLoader.Load<AsepriteFile>("graphics/phone/ScreenSprites")!
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Idle");

        // blinking light
        var dotFile = ContentLoader.Load<AsepriteFile>("graphics/phone/Dot")!;
        _dotNormalSpriteAnim = dotFile
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Normal");

        _dotStarSpriteAnim = dotFile
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Star");

        _dotTransitionSpriteAnim = dotFile
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Transition");

        _dotNormalSpriteAnim.Origin = new(3);
        _dotStarSpriteAnim.Origin = new(3);
        _dotTransitionSpriteAnim.Origin = new(3);

        SetDot(_dotNormalSpriteAnim);

        _phonePosition = new Vector2(202, 77);
        _simulatedBatteryValue = MainGame.CurrentHealth;

        _screenRenderTarget = new RenderTarget2D(MainGame.Graphics.GraphicsDevice, 26, 35);

        _audioOpen = StudioSystem.GetEvent("event:/SFX/UI/Phone/Open").CreateInstance();
        _audioClose = StudioSystem.GetEvent("event:/SFX/UI/Phone/Close").CreateInstance();
        _audioNotification = StudioSystem.GetEvent("event:/SFX/UI/Phone/Notification").CreateInstance();
        _audioBump = StudioSystem.GetEvent("event:/SFX/UI/Phone/Bump").CreateInstance();
        _audioJingle = StudioSystem.GetEvent("event:/SFX/UI/Phone/Jingle").CreateInstance();
        _audioHurt = StudioSystem.GetEvent("event:/SFX/UI/Phone/Hurt").CreateInstance();
        _audioHeal = StudioSystem.GetEvent("event:/SFX/UI/Phone/Heal").CreateInstance();
        _audioCorrect = StudioSystem.GetEvent("event:/SFX/UI/Phone/Correct").CreateInstance();
    }
    
    public void UnloadContent()
    {
        Dispose();
    }

    public void Dispose()
    {
        _screenRenderTarget?.Dispose();
        _audioOpen?.Dispose();
        _audioClose?.Dispose();
        _audioNotification?.Dispose();
        _audioBump?.Dispose();
        _audioJingle?.Dispose();
        _audioHurt?.Dispose();
        _audioHeal?.Dispose();
        _audioCorrect?.Dispose();
    }

    public void Update(GameTime gameTime)
    {
        _dockedPhonePos = new Vector2(190, 68);
        _openPhonePos = new Vector2(202 - _offset, 8);

        _nudgeOffset = MathUtil.ExpDecay(_nudgeOffset, 0, 8f, 1f / 60f);
        _scrollOffset = MathUtil.ExpDecay(_scrollOffset, ScrollTarget, 16f, 1f / 60f);

        int bottomPaddingRows = 4;
        var scrollTargetMax = MathHelper.Max(0, (_orders.Count + (_orders.Count > 4 ? 1 : 0) - bottomPaddingRows) * 6);
        if (_isOpen && CanOpen)
        {
            if (ScrollTarget > scrollTargetMax)
            {
                PlayEmoticonReaction();
                _audioBump.Start();
            }

            if (ScrollTarget < 0 && !_isTalking)
            {
                PlayFaceReaction();
                _audioBump.Start();
            }
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
                if (!_isOpen && elevator.State == Elevator.Elevator.ElevatorStates.Stopped)
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
                    _audioClose?.Stop();
                    _audioOpen?.Start();
                    MainGame.Coroutines.Stop("phone_hide");
                    MainGame.Coroutines.TryRun("phone_show", Open(true), 0, out _);
                    elevator.SetState(Elevator.Elevator.ElevatorStates.Other);
                    MainGame.CurrentMenu = MainGame.Menus.Phone;
                }
                else
                {
                    _audioOpen?.Stop();
                    _audioClose?.Start();
                    MainGame.Coroutines.Stop("phone_show");
                    MainGame.Coroutines.TryRun("phone_hide", Close(true, markAllViewed: true), 0, out _);
                    elevator.SetState(Elevator.Elevator.ElevatorStates.Stopped);
                    MainGame.CurrentMenu = MainGame.Menus.None;
                }
            }
        }

        _batterySpriteAnim.SetFrame(MainGame.CurrentHealth);
        if (_simulatedBatteryValue != MainGame.CurrentHealth)
        {
            bool changeUp = _simulatedBatteryValue > MainGame.CurrentHealth;
            SetFace(changeUp ? 4 : 3);
            int stepTime = changeUp ? 6 : 10;
            bool showChangedFrame = MainGame.Step % stepTime < stepTime * 0.5;
            _batterySpriteAnim.SetFrame(showChangedFrame ? _simulatedBatteryValue : MainGame.CurrentHealth);
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
        Vector2 phonePos = Vector2.Round(_phonePosition + Vector2.UnitY * _nudgeOffset);
        _phoneSprite.Draw(spriteBatch, phonePos);
        _faceSpriteAnim.Draw(spriteBatch, phonePos + _faceSliceKey.Location.ToVector2());
        _buttonsSpriteAnim.Draw(spriteBatch, phonePos + _buttonsSliceKey.Location.ToVector2());
        _dotSpriteAnim.Draw(spriteBatch, phonePos + _dotSliceKey.Location.ToVector2());

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
            if (_orders is { Count: <= 0 })
            {
                _idleSpriteAnim.Color = mainColor;
                _idleSpriteAnim.Draw(spriteBatch, new Vector2(0, -8));
            }
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public void SetFace(int faceIndex)
    {
        _faceSpriteAnim.SetFrame(faceIndex);
    }

    void SetDot(AnimatedSprite animatedSprite)
    {
        _dotSpriteAnim = animatedSprite;
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

    public void PlayDotBlink()
    {
        if (!MainGame.Coroutines.IsRunning("phone_dot_blink"))
            MainGame.Coroutines.TryRun("phone_dot_blink", DotBlinkSequence(), out _);
    }

    private IEnumerator DotBlinkSequence()
    {
        const int frameTime = 6;

        SetDot(_dotTransitionSpriteAnim);
        _dotSpriteAnim.SetFrame(0);
        yield return frameTime;

        SetDot(_dotStarSpriteAnim);
        _dotSpriteAnim.SetFrame(1);
        _audioNotification.Start();
        yield return frameTime;

        _dotSpriteAnim.SetFrame(0);
        yield return frameTime;

        _dotSpriteAnim.SetFrame(1);
        yield return frameTime;

        _dotSpriteAnim.SetFrame(0);
    }

    private IEnumerator DotRevertSequence()
    {
        const int frameTime = 6;

        SetDot(_dotTransitionSpriteAnim);
        _dotSpriteAnim.SetFrame(0);
        yield return frameTime;

        SetDot(_dotNormalSpriteAnim);
        _dotSpriteAnim.SetFrame(0);
    }

    public void StartTalking()
    {
        _isTalking = true;
        SetFace(1);
        MainGame.Coroutines.Stop("phone_talk");
        MainGame.Coroutines.TryRun("phone_talk", TalkSequence(), out _);
    }

    public void StopTalking()
    {
        _isTalking = false;
    }

    private IEnumerator TalkSequence()
    {
        // play start talking sound

        while (_isTalking)
        {
            yield return 5;
            SetFace(7); // eyes and mouth open
            yield return 5;
            SetFace(1); // eyes open
        }

        // play stop talking sound

        SetFace(0); // Reset
    }

    public IEnumerator Open(bool shiftCam, bool changeCanOpen = true)
    {
        _idleSpriteAnim.SetFrame(Random.Shared.Next(_idleSpriteAnim.FrameCount));
        if (_isOpen)
        {
            MainGame.Coroutines.Stop("phone_show");
            yield return null;
        };
        _isOpen = true;
        MainGame.Coroutines.Stop("phone_hide");
        CanOpen = false;
        if(changeCanOpen)
            CanOpen = true;
        _offset = 0;

        if (shiftCam)
        {
            MainGame.CameraPositionTarget = MainGame.CameraPositionTarget with { X = MaxOffset };
            MainGame.GrayscaleCoeffTarget = 0;
        }

        while(_offset < MaxOffset - 1)
        {
            _offset = MathUtil.ExpDecay(
                _offset,
                MaxOffset,
                8,
                1/60f
            );
            
            yield return null;
        }
        _offset = MaxOffset;
    }

    public IEnumerator Close(bool shiftCam, bool changeCanOpen = true, bool markAllViewed = false)
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
        if(changeCanOpen)
            CanOpen = true;
        _offset = MaxOffset;

        if (shiftCam)
        {
            MainGame.CameraPositionTarget = MainGame.CameraPositionTarget with { X = 0 };
            MainGame.GrayscaleCoeffTarget = 1;
        }

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

        if (_orders.Any(o => !o.Viewed) && markAllViewed)
        {
            foreach (var o in _orders) o.MarkAsViewed();

            MainGame.Coroutines.Stop("phone_dot_blink");
            MainGame.Coroutines.Stop("phone_dot_revert");
            MainGame.Coroutines.TryRun("phone_dot_revert", DotRevertSequence(), out _);
        }
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

        _orders.Sort((a, b) => -a.FloorNumber.CompareTo(b.FloorNumber));
        for (var i = 0; i < _orders.Count; i++)
        {
            _orders[i].TargetPosition = new Vector2(0, i * 6);
            _orders[i].SnapToTarget();
        }
    }
    
    public void HighlightOrder(CharacterActor characterActor)
    {
        SetFace(4); // Lucky Cat

        _audioCorrect.Start();

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
        if (change == 0) return;
        if (change < 0)
        {
            change += MainGame.HealthShield;
            if (change >= 0) return;

            PlayHurt();
        }
        else
        {
            PlayHeal();
        }

        SimulateBatteryValue(_simulatedBatteryValue + change);
    }

    public void SimulateBatteryValue(int newValue)
    {
        _simulatedBatteryValue = Math.Clamp(newValue, 0, 8);
    }

    public void PlayJingle()
    {
        _audioJingle.Start();
    }

    public void PlayHurt()
    {
        _audioHurt.Start();
    }

    public void PlayHeal()
    {
        _audioHeal.Start();
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

    public void ForceClearOrders()
    {
        _orders.Clear();
    }
}
