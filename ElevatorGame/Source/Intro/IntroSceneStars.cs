using System.Collections;
using ElevatorGame.Source.Backgrounds;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public class IntroSceneStars : IntroScene
{
    private BackgroundStars _backgroundStars;

    public override void LoadContent()
    {
        _backgroundStars = new BackgroundStars(MainGame.Graphics.GraphicsDevice);
    }

    public override IEnumerator GetEnumerator()
    {
        while (true)
        {
            _backgroundStars.Update();
            yield return null;
        }
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        _backgroundStars.PreDraw(spriteBatch);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _backgroundStars.Draw(spriteBatch);
    }
}
