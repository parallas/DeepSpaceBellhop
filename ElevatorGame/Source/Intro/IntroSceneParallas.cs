using System.Collections;
using AsepriteDotNet.Aseprite;
using Engine;
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
    private RenderTarget2D _rtEffect;

    private float _fadeAmount = 1;

    public override void LoadContent()
    {
        _sprite = ContentLoader.Load<AsepriteFile>("graphics/intro/ParallasLogo")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
        _ditherShader = ContentLoader.Load<Effect>("shaders/dither_overlay");
        _ditherShaderIntensity = _ditherShader.Parameters["DitherIntensity"];

        _rt = new(MainGame.Graphics.GraphicsDevice, 240, 135);
        _rtEffect = new(MainGame.Graphics.GraphicsDevice, 240, 135);
    }

    public override IEnumerator GetEnumerator()
    {
        while(_fadeAmount > 0)
        {
            _fadeAmount = MathUtil.Approach(_fadeAmount, 0, 1f/20f);
            yield return null;
        }

        yield return 10;

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

        yield return 20;

        while(_fadeAmount < 1)
        {
            _fadeAmount = MathUtil.Approach(_fadeAmount, 1, 1f/20f);
            yield return null;
        }

        yield return 20;
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        _ditherShader.Parameters["ScreenDimensions"].SetValue(new Vector2(
            240,
            135
        ));

        _ditherShaderIntensity?.SetValue(1-_fadeAmount);

        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_rt);
        MainGame.Graphics.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(samplerState: SamplerState.PointWrap);
        {
            _sprite.Draw(spriteBatch, Vector2.Zero);
        }
        spriteBatch.End();

        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_rtEffect);
        MainGame.Graphics.GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(samplerState: SamplerState.PointWrap, effect: _ditherShader);
        {
            spriteBatch.Draw(_rt, Vector2.Zero, Color.White);
        }
        spriteBatch.End();

        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_rtEffect, Vector2.Zero, Color.White);
    }
}
