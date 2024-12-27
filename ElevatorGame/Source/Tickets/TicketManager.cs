using System.Collections;
using System.Collections.Generic;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Tickets;

public class TicketManager(Elevator.Elevator elevator)
{
    public const float MaxOffset = 32f;

    private readonly List<TicketActor> _tickets = [];
    private readonly List<TicketActor> _toRemove = [];

    private bool _isOpen;
    private float _offset;
    private float _targetOffset;

    private CoroutineHandle _easeOffsetHandle;

    private Sprite _cardTraySprite;

    public void LoadContent()
    {
        _cardTraySprite = ContentLoader.Load<AsepriteFile>("graphics/CardTray")!
            .CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);
    }

    public void Update(GameTime gameTime)
    {
        for (int i = 0; i < _tickets.Count; i++)
        {
            var ticket = _tickets[i];

            if (!_isOpen && !_toRemove.Contains(ticket))
            {
                // if (i < _tickets.Count - 3) continue;
                
                // Stack tickets in the bottom left corner
                // Newest on top, show top three
                ticket.TargetPosition =
                    new(2 + i * 3, MainGame.GameBounds.Height - 2);
            }
            ticket.Update(gameTime);
        }

        bool mouseOver = new Rectangle(
            new(0, _isOpen
                ? MathUtil.RoundToInt(MainGame.GameBounds.Height + 1 - (MathHelper.Max(1, MathUtil.CeilToInt(_tickets.Count / 5f)) * 22 + 5))
                : MainGame.GameBounds.Height - 22
            ),
            new(
                5 * 16 + (_isOpen ? 6 : -32),
                200
            )
        ).Contains(MainGame.Cursor.ViewPosition);

        if(MainGame.CurrentMenu == MainGame.Menus.None || MainGame.CurrentMenu == MainGame.Menus.Tickets)
        {
            if(mouseOver)
            {
                if(!_isOpen)
                    MainGame.Cursor.CursorSpriteOverride = Cursor.CursorSprites.OpenTickets;
            }
            else if(_isOpen)
            {
                MainGame.Cursor.CursorSpriteOverride = Cursor.CursorSprites.CloseTickets;
            }
        }

        if(!_isOpen && (Keybindings.Left.Pressed || (mouseOver && InputManager.GetPressed(MouseButtons.LeftButton))) && MainGame.CurrentMenu == MainGame.Menus.None && elevator.State == Elevator.Elevator.ElevatorStates.Stopped)
        {
            MainGame.CurrentMenu = MainGame.Menus.Tickets;
            elevator.SetState(Elevator.Elevator.ElevatorStates.Other);
            MainGame.Coroutines.Stop("ticket_hide");
            MainGame.Coroutines.TryRun("ticket_show", Open(), out _);

            _targetOffset = MaxOffset;
            MainGame.Coroutines.Stop("ticket_ease_offset");
            MainGame.Coroutines.TryRun("ticket_ease_offset", EaseOffset(), out _easeOffsetHandle);
        }
        else if(_isOpen && (Keybindings.Right.Pressed || (!mouseOver && InputManager.GetPressed(MouseButtons.LeftButton))) && MainGame.CurrentMenu == MainGame.Menus.Tickets)
        {
            elevator.SetState(Elevator.Elevator.ElevatorStates.Stopped);
            MainGame.Coroutines.Stop("ticket_show");
            MainGame.Coroutines.TryRun("ticket_hide", Close(), out _);

            _targetOffset = 0;
            MainGame.Coroutines.Stop("ticket_ease_offset");
            MainGame.Coroutines.TryRun("ticket_ease_offset", EaseOffset(), out _easeOffsetHandle);
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Vector2 drawerPosition = MainGame.GetCursorParallaxValue(new(
            MathHelper.Lerp(-40, 1, (_offset / MaxOffset)),
            MainGame.GameBounds.Height + 1 - (MathHelper.Max(1, MathUtil.CeilToInt(_tickets.Count / 5f)) * 22 * (_offset / MaxOffset) + 17)
        ), 45);

        _cardTraySprite.Draw(spriteBatch, drawerPosition);
        // spriteBatch.Draw(
        //     MainGame.PixelTexture, new Rectangle(
        //         MathUtil.RoundToInt(drawerPosition.X),
        //         MathUtil.RoundToInt(drawerPosition.Y),
        //         5 * 16 + 1,
        //         MathHelper.Max(1, _tickets.Count / 5) * 22
        //     ),
        //     Color.Black * 0.5f * (_offset / MaxOffset)
        // );

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

        _toRemove.Add(ticket);

        ticket.TargetPosition = new(ticket.TargetPosition.X, MainGame.GameBounds.Height + 20);
        while(ticket.Position.Y < MainGame.GameBounds.Height + 20 - 1)
        {
            yield return null;
        }
        yield return 10;

        _tickets.Remove(ticket);
        _toRemove.Remove(ticket);

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

            ticketActor.TargetPosition = new(((i % 5)) * 16 + 9, MainGame.GameBounds.Height - 4 - ((i / 5) * 22));
            yield return 4;
        }

        yield return _easeOffsetHandle?.Wait();
    }

    public IEnumerator Close()
    {
        for (int i = 0; i < _tickets.Count; i++)
        {
            var ticketActor = _tickets[i];

            var oldTarget = ticketActor.TargetPosition;

            ticketActor.TargetPosition =
                new(2 + i * 3, MainGame.GameBounds.Height - 2);

            // should make interrupting the open animation be less delayed
            if(oldTarget != ticketActor.TargetPosition)
                yield return 2;
        }

        yield return _easeOffsetHandle?.Wait();

        MainGame.CurrentMenu = MainGame.Menus.None;
        _isOpen = false;
    }

    private IEnumerator EaseOffset()
    {
        while(!MathUtil.Approximately(_offset, _targetOffset, 1))
        {
            _offset = MathUtil.ExpDecay(
                _offset,
                _targetOffset,
                8,
                1/60f
            );

            var camPos = MainGame.CameraPosition;
            camPos.X = -_offset;
            MainGame.CameraPosition = camPos;
            MainGame.GrayscaleCoeff = 1-(_offset / MaxOffset);

            yield return null;
        }
        _offset = _targetOffset;
    }
}
