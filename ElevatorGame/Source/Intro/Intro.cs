using System.Collections;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public static class Intro
{
    private static List<Texture2D> _logos;

    private static int _currentLogo;
    private static float _currentLogoOpacity = 1;

    private static IntroScene[] _scenes;

    public static void LoadContent()
    {
        _logos = [
            ContentLoader.Load<Texture2D>("graphics/intro/ParallasLogo"),
            ContentLoader.Load<Texture2D>("graphics/intro/FmodLogo"),
        ];

        _scenes = [
            
        ];
    }

    public static IEnumerator RunSequence()
    {
        for(int i = 0; i < _logos.Count; i++)
        {
            _currentLogo = i;
            yield return 2 * 60;
        }
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(Color.Black);

        spriteBatch.Draw(_logos[_currentLogo], Vector2.Zero, Color.White * _currentLogoOpacity);
    }
}
