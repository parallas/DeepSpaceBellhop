using System.Collections;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public static class Intro
{
    private static int _currentScene = -1;

    private static IntroScene[] _scenes;

    public static void LoadContent()
    {
        _scenes = [
            new IntroSceneParallas(),
            new IntroSceneSimpleImage("graphics/intro/FmodLogo"),

            // animated intro
        ];

        foreach (var scene in _scenes)
        {
            scene.LoadContent();
        }
    }

    public static IEnumerator RunSequence()
    {
        yield return 10;

        for(int i = 0; i < _scenes.Length; i++)
        {
            _currentScene = i;
            yield return _scenes[i].GetEnumerator();
        }
    }

    public static void PreDraw(SpriteBatch spriteBatch)
    {
        if(_currentScene == -1)
            return;

        _scenes[_currentScene].PreDraw(spriteBatch);
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        if(_currentScene == -1)
            return;

        spriteBatch.GraphicsDevice.Clear(Color.Black);

        _scenes[_currentScene].Draw(spriteBatch);
    }
}
