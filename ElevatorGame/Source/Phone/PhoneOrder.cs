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
    public bool Highlighted { get; set; }
    public Vector2 Position => _position;
    
    private Vector2 _position;
    
    private AnimatedSprite _digitsSpriteAnim4x5;
    private Sprite _arrowSprite;
    private AnimatedSprite _moodsSpriteAnim;

    private Color _mainColor = ColorUtil.CreateFromHex(0x40318d);
    private Color _bgColor = ColorUtil.CreateFromHex(0x67b6bd);
    private Color _currentColor;

    public PhoneOrder()
    {
        // Digits
        _digitsSpriteAnim4x5 = ContentLoader.Load<AsepriteFile>("graphics/Digits4x5")!
            .CreateSpriteSheet(
                MainGame.Graphics.GraphicsDevice,
                true
            )
            .CreateAnimatedSprite("Tag");
        
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

        _currentColor = _mainColor;
    }

    public void Update(GameTime gameTime)
    {
        _position = MathUtil.ExpDecay(_position, TargetPosition, 5f, 1f / 60f);

        _currentColor = _mainColor;
        if (Highlighted)
        {
            _currentColor = MainGame.Step % 20 < 10 ? _mainColor : _bgColor;
        }
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 orderPos = Vector2.Round(Vector2.One + _position);

        if (_currentColor == _bgColor)
        {
            spriteBatch.Draw(MainGame.PixelTexture, new Rectangle((int)orderPos.X, (int)orderPos.Y, 26, 5), _mainColor);
        }
        
        _digitsSpriteAnim4x5.Color = _currentColor;
        if ((int)MathF.Round(FloorNumber) >= 10)
        {
            _digitsSpriteAnim4x5.SetFrame(FloorNumber / 10 % 10);
            _digitsSpriteAnim4x5.Draw(spriteBatch, orderPos);
        }
        _digitsSpriteAnim4x5.SetFrame(FloorNumber % 10);
        _digitsSpriteAnim4x5.Draw(spriteBatch, orderPos + Vector2.UnitX * 4);

        _arrowSprite.Color = _currentColor;
        _arrowSprite.FlipVertically = DestinationNumber < FloorNumber;
        _arrowSprite.Draw(spriteBatch, orderPos + new Vector2(10, 1));

        _moodsSpriteAnim.Color = _currentColor;
        _moodsSpriteAnim.SetFrame(Mood);
        _moodsSpriteAnim.Draw(spriteBatch, orderPos + Vector2.UnitX * 18);
    }
}
