using System;
using System.Linq;
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

    [Flags]
    public enum TicketFlags
    {
        None,
        UpsideDown,
        Slimy,
    }
    public TicketFlags Flags { get; set; }

    private AnimatedSprite _digitsSpriteAnim5x7;
    private AnimatedSprite _ticketsSpriteAnim;
    private AnimatedSprite _ticketSpriteOverlaysAnim;

    public void LoadContent()
    {
        var digitsFile = ContentLoader.Load<AsepriteFile>("graphics/Digits5x7")!;
        _digitsSpriteAnim5x7 = digitsFile
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
        
        var ticketsFile = ContentLoader.Load<AsepriteFile>("graphics/Tickets")!;
        _ticketsSpriteAnim = ticketsFile
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, ["Main"])
            .CreateAnimatedSprite("Tag");

        _ticketSpriteOverlaysAnim = ticketsFile
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, ["Overlays"])
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
        renderedTicketPos = Vector2.Round(MainGame.GetCursorParallaxValue(renderedTicketPos, 25));

        bool isUpsideDown = Flags.HasFlag(TicketFlags.UpsideDown);
        bool isSlimy = Flags.HasFlag(TicketFlags.Slimy);

        if (isUpsideDown)
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

        Vector2 digitsStartPos = renderedTicketPos + new Vector2(4 - (isUpsideDown ? 1 : 0), 9);
        _digitsSpriteAnim5x7.Color = Color.Black;
        if (!isUpsideDown)
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

        _ticketSpriteOverlaysAnim.SetFrame(0);
        if (Flags.HasFlag(TicketFlags.Slimy)) _ticketSpriteOverlaysAnim.SetFrame(1);
        _ticketSpriteOverlaysAnim.Color = Color.Black;
        _ticketSpriteOverlaysAnim.Draw(spriteBatch, renderedTicketPos + new Vector2(-1, 1));
        _ticketSpriteOverlaysAnim.Color = Color.White;
        _ticketSpriteOverlaysAnim.Draw(spriteBatch, renderedTicketPos);
    }
}
