using System.Collections;
using Engine;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Intro;

public class IntroScenePostEnding : IntroScene
{
    private Vector2 _textPos;
    private float _textFade;
    private int _textDir;

    public override void LoadContent()
    {
        _textPos = MainGame.GameBounds.Size.ToVector2();
        var textSize = MainGame.FontBold.MeasureString(
            LocalizationManager.Get("credits.the_end").Replace(" ", "")
        );
        textSize.X += LocalizationManager.Get("credits.the_end")
            .Sum(c => c == ' ' ? 6 : 0);

        _textPos -= textSize;
        _textPos *= 0.5f;
    }

    public override IEnumerator GetEnumerator()
    {
        MainGame.FadeFromBlackInstant();
        MusicPlayer.StopMusic(false, false);

        yield return 30;

        using var audioHuzzah = StudioSystem.GetEvent("event:/SFX/CharactersBg/Huzzah").CreateInstance();
        audioHuzzah.Start();

        _textDir = 1;
        while (_textFade < 1)
        {
            _textFade += 1f / 15f;
            yield return null;
        }

        yield return 180;

        _textDir = -1;
        while (_textFade > 0)
        {
            _textFade -= 1f / 30f;
            yield return null;
        }

        yield return 60;
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.DrawStringSpacesFix(
            MainGame.FontBold,
            LocalizationManager.Get("credits.the_end"),
            _textPos + (Vector2.UnitY * -_textDir * MathHelper.Lerp(8, 0, MathF.Pow(_textFade, 0.5f))),
            Color.White * _textFade,
            6
        );
    }
}
