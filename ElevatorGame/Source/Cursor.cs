using AsepriteDotNet.Aseprite;
using Engine;
using Engine.Display;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace ElevatorGame.Source;

public class Cursor
{
    public enum CursorSprites
    {
        Default,
        Dialog,
        FastForward,
        Wait,
        Interact
    }

    public CursorSprites CursorSprite { get; set; }

    private AsepriteFile _file;
    private AnimatedSprite _sprite;

    public void LoadContent()
    {
        _file = ContentLoader.Load<AsepriteFile>("graphics/Cursor");
        _sprite = _file.CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false).CreateAnimatedSprite("Tag");

        _sprite.Origin = new(4);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _sprite.SetFrame((int)CursorSprite);

        switch(CursorSprite)
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
        }

        var mousePos = RtScreen.ToScreenSpace(
            InputManager.MousePosition.ToVector2(),
            MainGame.RenderBufferSize,
            MainGame.Graphics.GraphicsDevice
        );

        _sprite.Draw(spriteBatch, Vector2.Floor(mousePos));
    }
}
