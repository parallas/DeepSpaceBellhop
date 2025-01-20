using System.Collections;
using AsepriteDotNet.Aseprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Intro;

public class IntroSceneAnimatedSprite(string path, string tag) : IntroScene
{
    private AnimatedSprite _sprite;

    public override void LoadContent()
    {
        _sprite = ContentLoader.Load<AsepriteFile>(path)
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite(tag);
    }

    public override IEnumerator GetEnumerator()
    {
        _sprite.Play();
        yield return 120;
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        _sprite.Update(1f/60f);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _sprite.Draw(spriteBatch, Vector2.Zero);
    }
}
