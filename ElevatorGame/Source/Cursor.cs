using AsepriteDotNet.Aseprite;
using Engine;
using Engine.Display;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source;

public class Cursor
{
    public enum CursorSprites
    {
        Default = 0,
        Dialog = 1,
        Wait = 2,
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

        var mousePos = RtScreen.ToScreenSpace(
            InputManager.MousePosition.ToVector2(),
            MainGame.RenderBufferSize,
            MainGame.Graphics.GraphicsDevice
        );

        _sprite.Draw(spriteBatch, Vector2.Round(MainGame.Camera.Position) + mousePos + Vector2.One * 8);
    }
}
