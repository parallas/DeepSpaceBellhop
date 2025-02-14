using System.Collections;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public abstract class IntroScene
{
    public bool Skippable { get; set; } = true;
    public bool AutoSkippable { get; set; } = true;
    public abstract void LoadContent();
    public abstract IEnumerator GetEnumerator();
    public abstract void PreDraw(SpriteBatch spriteBatch);
    public abstract void Draw(SpriteBatch spriteBatch);
}
