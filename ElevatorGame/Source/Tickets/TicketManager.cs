using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.Tickets;

public class TicketManager
{
    private readonly List<TicketActor> _tickets = [];
    private bool _isOpen;

    public void LoadContent()
    {
        
    }

    public void Update(GameTime gameTime)
    {
        for (int i = 0; i < _tickets.Count; i++)
        {
            var ticket = _tickets[i];

            if (!_isOpen)
            {
                // if (i < _tickets.Count - 3) continue;
                
                // Stack tickets in the bottom left corner
                // Newest on top, show top three
                ticket.TargetPosition =
                    new(2 + i * 3, MainGame.GameBounds.Height - 2);
            }
            ticket.Update(gameTime);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < _tickets.Count; i++)
        {
            var ticket = _tickets[i];
            ticket.Draw(spriteBatch);
        }
    }

    public void AddTicket(int floorNumber, string[] flags = null)
    {
        flags ??= [];

        var newTicket = new TicketActor()
        {
            FloorNumber = floorNumber,
            TargetPosition = new Vector2(MainGame.GameBounds.Center.X, MainGame.GameBounds.Bottom - 32)
        };
        newTicket.LoadContent();
        
        _tickets.Add(newTicket);
    }

    public IEnumerator RemoveTicket(int floorNumber)
    {
        var ticket = _tickets.Find(t => t.FloorNumber == floorNumber);
        _tickets.Remove(ticket);

        for (int i = 0; i < _tickets.Count; i++)
        {
            var ticketActor = _tickets[i];

            ticketActor.TargetPosition =
                new(2 + i * 3, MainGame.GameBounds.Height - 2);
            yield return 10;
        }
    }

    public IEnumerator Open()
    {
        _isOpen = true;
        for (int i = _tickets.Count - 1; i >= 0; i--)
        {
            var ticketActor = _tickets[i];

            ticketActor.TargetPosition = new((5 - (i % 5)) * 16, ((i / 5) * 16) - (16 * 5));
            yield return 10;
        }
    }

    public void Close()
    {
        _isOpen = false;
    }
}
