using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public class IntroSceneSimpleImage(string path) : IntroScene
{
    private Texture2D _texture;

    public override void LoadContent()
    {
        _texture = ContentLoader.Load<Texture2D>(path);
    }

    public override IEnumerator GetEnumerator()
    {
        yield return 120;
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, Vector2.Zero, Color.White);
    }
}
