using System.Collections;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public abstract class IntroScene
{
    public abstract IEnumerator GetEnumerator();
    public abstract void Draw(SpriteBatch spriteBatch);
    public abstract void LoadContent();
}
