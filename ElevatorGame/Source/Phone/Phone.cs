using System;
using System.Collections;
using System.Collections.Generic;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.Aseprite.Utils;

namespace ElevatorGame.Source.Phone;

public class Phone(Elevator.Elevator elevator)
{
    private bool _isOpen;
    private float _offset;

    private AsepriteFile _phoneFile;
    private Sprite _phoneSprite;
    private AnimatedSprite _faceSpriteAnim;
    private AnimatedSprite _buttonsSpriteAnim;
    
    private AnimatedSprite _digitsSpriteAnim4x5;
    private Sprite _arrowSprite;
    private AnimatedSprite _moodsSpriteAnim;
    
    private RenderTarget2D _screenRenderTarget;
    
    private Rectangle _faceSliceKey;
    private Rectangle _buttonsSliceKey;
    private Rectangle _screenSliceKey;
    private Rectangle _dotSliceKey;

    // sprite origin: 202, 79
    // mouse region origin: 193, 75

    private Vector2 _phonePosition;

    public bool CanOpen { get; set; } = true;

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
        
        // Digits
        _digitsSpriteAnim4x5 = ContentLoader.Load<AsepriteFile>("graphics/Digits4x5")!
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Tag");
        _digitsSpriteAnim4x5.Color = ColorUtil.CreateFromHex(0x40318d);
        
        // Arrow
        _arrowSprite = ContentLoader.Load<AsepriteFile>("graphics/phone/Arrow")!
            .CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        
        // Moods
        _moodsSpriteAnim = ContentLoader.Load<AsepriteFile>("graphics/phone/Moods")!
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
        if(CanOpen && ((!_isOpen && Keybindings.Right.Pressed && elevator.State == Elevator.Elevator.ElevatorStates.Stopped) || (_isOpen && Keybindings.Left.Pressed)))
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

        var camPos = MainGame.CameraPosition;
        camPos.X = _offset;
        MainGame.CameraPosition = camPos;

        MainGame.GrayscaleCoeff = 1-(_offset / 32f);
        
        Vector2 dockedPhonePos = new Vector2(202, 77);
        Vector2 openPhonePos = new Vector2(202 - 16 - _offset, 8);
        float blend = _offset / 32f;
        Vector2 blendedPhonePos = Vector2.Lerp(dockedPhonePos, openPhonePos, blend);
        Vector2 phonePos = MainGame.Camera.GetParallaxPosition(blendedPhonePos, 25);
        _phonePosition = MathUtil.ExpDecay(_phonePosition, phonePos, 13f, 1f / 60f);
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
            DrawOrder(spriteBatch, 0);
            DrawOrder(spriteBatch, 1);
            DrawOrder(spriteBatch, 2);
            DrawOrder(spriteBatch, 3);
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }
    
    private void DrawOrder(SpriteBatch spriteBatch, int index)
    {
        Vector2 orderPos = Vector2.One + (Vector2.UnitY * index * 6);
        
        _digitsSpriteAnim4x5.Draw(spriteBatch, orderPos);
        _digitsSpriteAnim4x5.Draw(spriteBatch, orderPos + Vector2.UnitX * 4);
        _arrowSprite.Draw(spriteBatch, orderPos + new Vector2(10, 1));
        _moodsSpriteAnim.Draw(spriteBatch, orderPos + Vector2.UnitX * 18);
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
