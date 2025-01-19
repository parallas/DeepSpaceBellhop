using AsepriteDotNet.Aseprite;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.BG_Characters;

public class BgCharacterRenderer
{
    public BgCharacterDef CharacterDef { get; private set; }
    public AnimatedSprite CurrentSpriteAnim { get; private set; }
    public bool HasCharacter { get; private set; }

    private struct AnimationEventData
    {
        public AnimationEventData(int frameStart, int frameEnd, string eventName, string eventData)
        {
            FrameStart = frameStart;
            FrameEnd = frameEnd;
            EventName = eventName;
            EventData = eventData;
        }

        public int FrameStart { get; set; }
        public int FrameEnd { get; set; }
        public string EventName { get; set; }
        public string EventData { get; set; }
    }
    private List<AnimationEventData> AnimationEventDatas { get; set; } = new List<AnimationEventData>();

    private int _lastFrame = -1;

    public void SetCharacterDef(BgCharacterDef? character)
    {
        CurrentSpriteAnim?.Stop();

        HasCharacter = character != null;
        if (character == null) return;
        CharacterDef = character.Value;

        var file = ContentLoader.Load<AsepriteFile>(CharacterDef.SpritePath)!;
        CurrentSpriteAnim = file
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");

        AnimationEventDatas.Clear();
        foreach (var asepriteTag in file.Tags)
        {
            if (!asepriteTag.UserData.HasText) continue;
            switch (asepriteTag.Name)
            {
                case "SFX":
                    AnimationEventDatas.Add(new AnimationEventData(asepriteTag.From, asepriteTag.To, "SFX",
                        asepriteTag.UserData.Text));
                    break;
            }
        }

        _lastFrame = -1;
        CurrentSpriteAnim.Play();
    }

    public void Update(GameTime gameTime)
    {
        if (!HasCharacter) return;
        CurrentSpriteAnim.Update(1f / 60f);

        if (CurrentSpriteAnim.CurrentFrame.FrameIndex > _lastFrame)
        {
            foreach (var animationEventData in AnimationEventDatas
                         .Where(animationEventData => CurrentSpriteAnim.CurrentFrame.FrameIndex == animationEventData.FrameStart))
            {
                switch (animationEventData.EventName)
                {
                    case "SFX":
                        using (var soundEvent = StudioSystem.GetEvent($"event:/{animationEventData.EventData}").CreateInstance())
                        {
                            soundEvent.Start();
                        }
                        break;
                }
            }
        }

        _lastFrame = CurrentSpriteAnim.CurrentFrame.FrameIndex;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!HasCharacter) return;
        CurrentSpriteAnim.Draw(spriteBatch,
            MainGame.Camera.GetParallaxPosition(new Vector2(80, 55), Elevator.Elevator.ParallaxDoors + 10));
    }
}
