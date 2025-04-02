using System.Collections;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public static class Intro
{
    public static event Action OnComplete;
    private static int _currentScene = -1;

    private static IntroScene[] _scenes;

    public static void DoIntro()
    {
        _scenes = [
            // new IntroSceneTest(),
            new IntroSceneParallas(),
            new IntroSceneSimpleImage("graphics/intro/FmodLogo"),

            // animated intro
        ];

        foreach (var scene in _scenes)
        {
            scene.LoadContent();
        }
    }

    public static void DoOutro()
    {
        _scenes = [
            new IntroSceneEnding(),
            new IntroScenePostEnding(),
        ];

        foreach (var scene in _scenes)
        {
            scene.LoadContent();
        }
    }

    public static IEnumerator RunSequence()
    {
        _currentScene = -1;

        yield return 10;

        for(int i = 0; i < _scenes.Length; i++)
        {
            _currentScene = i;
            yield return _scenes[i].GetEnumerator();
        }

        OnComplete?.Invoke();
    }

    public static void Update()
    {
        // if (Keybindings.Confirm.Pressed || Keybindings.GoBack.Pressed)
        // {
        //     MainGame.Coroutines.Stop("main_intro");
        //     OnComplete?.Invoke();
        // }
    }

    public static void PreDraw(SpriteBatch spriteBatch)
    {
        if(_currentScene == -1)
            return;

        _scenes[_currentScene].PreDraw(spriteBatch);
    }

    public static void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.GraphicsDevice.Clear(Color.Black);

        if(_currentScene == -1)
            return;

        _scenes[_currentScene].Draw(spriteBatch);
    }
}
