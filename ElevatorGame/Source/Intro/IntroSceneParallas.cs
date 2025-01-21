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

    public override void LoadContent()
    {
        _sprite = ContentLoader.Load<AsepriteFile>("graphics/intro/ParallasLogo")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
    }

    public override IEnumerator GetEnumerator()
    {
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
    }

    public override void PreDraw(SpriteBatch spriteBatch)
    {
        
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _sprite.Draw(spriteBatch, Vector2.Zero);
    }
}
