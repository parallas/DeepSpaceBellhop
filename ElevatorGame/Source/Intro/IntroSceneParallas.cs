using System.Collections;
using AsepriteDotNet.Aseprite;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Intro;

public class IntroSceneParallas : IntroScene
{
    private AnimatedSprite _sprite;
    private Effect _ditherShader;
    private EffectParameter _ditherShaderIntensity;
    private RenderTarget2D _rt;

    private int _frame;

    public override void LoadContent()
    {
        _sprite = ContentLoader.Load<AsepriteFile>("graphics/intro/ParallasLogo")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
        _ditherShader = ContentLoader.Load<Effect>("shaders/dither_overlay");
        _ditherShaderIntensity = _ditherShader.Parameters["DitherIntensity"];

        _rt = new(MainGame.Graphics.GraphicsDevice, 240, 135);

        OnResize(new(1920, 1080));
    }

    public override IEnumerator GetEnumerator()
    {
        MainGame.WindowResized += OnResize;

        yield return 30;

        for(int i = 0; i < 107; i++)
        {
            _sprite.SetFrame(i);

            switch(i)
            {
                case 14: {
                    var audioDoorClose = StudioSystem.GetEvent("event:/SFX/Elevator/Doors/Open").CreateInstance();
                    audioDoorClose.Start();
                    audioDoorClose.Dispose();
                    yield return 2;
                    break;
                }
                case 16: {
                    yield return 8;
                    break;
                }
                case 29: {
                    var audioMove = StudioSystem.GetEvent("event:/SFX/UI/Transition/Move").CreateInstance();
                    audioMove.SetParameterValue("Velocity", 0.7f);
                    audioMove.Start();
                    audioMove.Dispose();
                    yield return 2;
                    break;
                }
                case 53: {
                    var audioDoorOpen = StudioSystem.GetEvent("event:/SFX/Elevator/Doors/Open").CreateInstance();
                    audioDoorOpen.Start();
                    audioDoorOpen.Dispose();
                    yield return 2;
                    break;
                }
                case 62: {
                    var audioMeow = StudioSystem.GetEvent("event:/SFX/CharactersBg/Meow").CreateInstance();
                    audioMeow.Start();
                    audioMeow.Dispose();
                    yield return 2;
                    break;
                }
                case 87: {
                    yield return 30;
                    var audioHuzzah = StudioSystem.GetEvent("event:/SFX/CharactersBg/Huzzah").CreateInstance();
                    audioHuzzah.Start();
                    audioHuzzah.Dispose();
                    break;
                }
                default:
                    yield return 2;
                    break;
            }
        }

        yield return 30;

        MainGame.WindowResized -= OnResize;
    }

    private void OnResize(Point point)
    {
        _ditherShader.Parameters["ScreenDimensions"].SetValue(new Vector2(
            point.X,
            point.Y
        ));
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        const int edge = 8;
        if(_frame <= edge)
        {
            _ditherShaderIntensity.SetValue(_frame / (float)edge);
        }
        else if(_frame > (107 * 2) + edge)
        {
            _ditherShaderIntensity.SetValue(1f - ((_frame - ((107 * 2) + edge)) / (float)edge));
        }

        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_rt);
        MainGame.Graphics.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(samplerState: SamplerState.PointWrap/*, effect: _ditherShader*/); // need to fix this
        {
            _sprite.Draw(spriteBatch, Vector2.Zero);
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();

        _frame++;
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_rt, Vector2.Zero, Color.White);
    }
}
