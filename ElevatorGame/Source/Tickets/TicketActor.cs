using System;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Tickets;

public class TicketActor
{
    public Vector2 Position { get; set; }
    
    public Vector2 TargetPosition { get; set; }
    
    public int FloorNumber { get; set; }

    private bool _isHighlighted;
    private bool _isUpsideDown;
    
    private AnimatedSprite _digitsSpriteAnim5x7;
    private AnimatedSprite _ticketsSpriteAnim;

    public void LoadContent()
    {
        var digitsFile = ContentLoader.Load<AsepriteFile>("graphics/Digits5x7")!;
        _digitsSpriteAnim5x7 = digitsFile
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
        
        var ticketsFile = ContentLoader.Load<AsepriteFile>("graphics/Tickets")!;
        _ticketsSpriteAnim = ticketsFile
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");

        Position = TargetPosition;
    }

    public void Update(GameTime gameTime)
    {
        Position = MathUtil.ExpDecay(Position, TargetPosition, 13, 1f / 60f);
    }
    
    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 renderedTicketPos = Position - Vector2.UnitY * (_ticketsSpriteAnim.Height - 1);
        renderedTicketPos = MainGame.GetCursorParallaxValue(renderedTicketPos, 25);
        if (_isUpsideDown)
        {
            _ticketsSpriteAnim.FlipVertically = true;
            _ticketsSpriteAnim.FlipHorizontally = true;
            _digitsSpriteAnim5x7.FlipVertically = true;
            _digitsSpriteAnim5x7.FlipHorizontally = true;
        }
        
        _ticketsSpriteAnim.Color = Color.Black;
        _ticketsSpriteAnim.Draw(spriteBatch, renderedTicketPos + new Vector2(-1, 1));
        _ticketsSpriteAnim.Color = Color.White;
        _ticketsSpriteAnim.Draw(spriteBatch, renderedTicketPos);

        Vector2 digitsStartPos = renderedTicketPos + new Vector2(4, 9);
        _digitsSpriteAnim5x7.Color = Color.Black;
        if (!_isUpsideDown)
        {
            // Draw tens then ones
            _digitsSpriteAnim5x7.SetFrame(FloorNumber / 10);
            _digitsSpriteAnim5x7.Draw(spriteBatch, digitsStartPos);
            _digitsSpriteAnim5x7.SetFrame(FloorNumber % 10);
            _digitsSpriteAnim5x7.Draw(spriteBatch, digitsStartPos + Vector2.UnitX * 5);
        }
        else
        {
            // Draw ones then tens
            _digitsSpriteAnim5x7.SetFrame(FloorNumber % 10);
            _digitsSpriteAnim5x7.Draw(spriteBatch, digitsStartPos);
            _digitsSpriteAnim5x7.SetFrame(FloorNumber / 10);
            _digitsSpriteAnim5x7.Draw(spriteBatch, digitsStartPos + Vector2.UnitX * 5);
        }
        
    }
}
