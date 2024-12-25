using System;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Phone;

public class PhoneOrder
{
    public int FloorNumber { get; set; }
    public int DestinationNumber { get; set; }
    public int Mood { get; set; }
    public Vector2 TargetPosition { get; set; }
    public Vector2 Position => _position;
    
    private Vector2 _position;
    
    private AnimatedSprite _digitsSpriteAnim4x5;
    private Sprite _arrowSprite;
    private AnimatedSprite _moodsSpriteAnim;

    public PhoneOrder()
    {
        // Digits
        _digitsSpriteAnim4x5 = ContentLoader.Load<AsepriteFile>("graphics/Digits4x5")!
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Tag");
        _digitsSpriteAnim4x5.Color = ColorUtil.CreateFromHex(0x40318d);
        
        // Arrow
        _arrowSprite = ContentLoader.Load<AsepriteFile>("graphics/phone/Arrow")!
            .CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
        
        // Moods
        _moodsSpriteAnim = ContentLoader.Load<AsepriteFile>("graphics/phone/Moods")!
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Tag");
    }

    public void Update(GameTime gameTime)
    {
        _position = MathUtil.ExpDecay(_position, TargetPosition, 5f, 1f / 60f);
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 orderPos = Vector2.One + _position;
        
        if ((int)MathF.Round(FloorNumber) >= 10)
        {
            _digitsSpriteAnim4x5.SetFrame(FloorNumber / 10 % 10);
            _digitsSpriteAnim4x5.Draw(spriteBatch, orderPos);
        }
        _digitsSpriteAnim4x5.SetFrame(FloorNumber % 10);
        _digitsSpriteAnim4x5.Draw(spriteBatch, orderPos + Vector2.UnitX * 4);

        _arrowSprite.FlipVertically = DestinationNumber < FloorNumber;
        _arrowSprite.Draw(spriteBatch, orderPos + new Vector2(10, 1));
        
        _moodsSpriteAnim.SetFrame(Mood);
        _moodsSpriteAnim.Draw(spriteBatch, orderPos + Vector2.UnitX * 18);
    }
}
