using AsepriteDotNet.Aseprite;
using ElevatorGame.Source.Pause;
using Engine;
using Engine.Display;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace ElevatorGame.Source;

public class Cursor()
{
    public enum CursorSprites
    {
        Default,
        Dialog,
        FastForward,
        Wait,
        Interact,
        OpenPhone,
        ClosePhone,
        OpenTickets,
        CloseTickets,
    }

    public CursorSprites CursorSprite { get; set; }

    public CursorSprites CursorSpriteOverride { get; set; }

    public Vector2 ViewPosition => RtScreen.ToScreenSpace(
        InputManager.MousePosition.ToVector2(),
        MainGame.RenderBufferSize,
        MainGame.ScreenBounds
    );

    public Vector2 WorldPosition => ViewPosition + MainGame.ScreenPosition;
    public Vector2 TiltOffset => (
        Vector2.Clamp(
            ViewPosition,
            Vector2.Zero,
            MainGame.GameBounds.Size.ToVector2()
        ) - MainGame.GameBounds.Size.ToVector2() / 2f
    ) * (8 / 120f);

    private AsepriteFile _file;
    private AnimatedSprite _sprite;
    private RenderTarget2D _rt;
    private Point _lastScaledSize;
    private static Vector2 CursorScale => new(
        (float)RtScreen.LastScaledWidth / RenderPipeline.RenderBufferSize.X,
        (float)RtScreen.LastScaledHeight / RenderPipeline.RenderBufferSize.Y
    );

    public void LoadContent()
    {
        _file = ContentLoader.Load<AsepriteFile>("graphics/Cursor");
        _sprite = _file.CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false).CreateAnimatedSprite("Tag");

        _sprite.Origin = new(4);

        _rt = new(MainGame.Graphics.GraphicsDevice, 16, 16);
        _lastScaledSize = new(RtScreen.LastScaledWidth, RtScreen.LastScaledHeight);
    }

    public void Update()
    {
        CursorSpriteOverride = CursorSprite;
        if(_lastScaledSize.X != RtScreen.LastScaledWidth || _lastScaledSize.Y != RtScreen.LastScaledHeight)
        {
            _rt = new(MainGame.Graphics.GraphicsDevice, MathUtil.CeilToInt(16 * CursorScale.X), MathUtil.CeilToInt(16 * CursorScale.Y));
        }
        _lastScaledSize = new(RtScreen.LastScaledWidth, RtScreen.LastScaledHeight);
    }

    public void PreDraw(SpriteBatch spriteBatch)
    {
        _sprite.SetFrame((int)CursorSpriteOverride);

        switch(CursorSpriteOverride)
        {
            case CursorSprites.Default:
            {
                _sprite.SetFrame(0);
                break;
            }
            case CursorSprites.Dialog:
            {
                _sprite.SetFrame(1);
                break;
            }
            case CursorSprites.FastForward:
            {
                _sprite.SetFrame(5);
                break;
            }
            case CursorSprites.Wait:
            {
                _sprite.SetFrame(2);
                break;
            }
            case CursorSprites.Interact:
            {
                _sprite.SetFrame(InputManager.GetDown(MouseButtons.LeftButton) ? 4 : 3);
                break;
            }
            case CursorSprites.OpenPhone:
            {
                _sprite.SetFrame(6);
                break;
            }
            case CursorSprites.ClosePhone:
            {
                _sprite.SetFrame(7);
                break;
            }
            case CursorSprites.OpenTickets:
            {
                _sprite.SetFrame(8);
                break;
            }
            case CursorSprites.CloseTickets:
            {
                _sprite.SetFrame(9);
                break;
            }
        }

        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_rt);
        MainGame.Graphics.GraphicsDevice.Clear(Color.Transparent);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        {
            spriteBatch.Draw(
                _sprite.CurrentFrame.TextureRegion,
                Vector2.Zero,
                Color.White,
                0, Vector2.Zero,
                CursorScale,
                SpriteEffects.None,
                0
            );
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if(MainGame.UseNativeCursor)
        {
            SetNativeCursor();
            return;
        }

        _sprite.Draw(spriteBatch, Vector2.Floor(ViewPosition));
    }

    private void SetNativeCursor()
    {
        Mouse.SetCursor(
            MouseCursor.FromTexture2D(
                _rt,
                (int)(_sprite.OriginX * CursorScale.X),
                (int)(_sprite.OriginY * CursorScale.Y)
            )
        );
    }
}
