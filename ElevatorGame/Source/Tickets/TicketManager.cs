using System.Collections;
using System.Collections.Generic;
using AsepriteDotNet.Aseprite;
using Engine;
using FmodForFoxes.Studio;
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

    private EventDescription _audioSlideEventDescription;

    public void LoadContent()
    {
        _cardTraySprite = ContentLoader.Load<AsepriteFile>("graphics/CardTray")!
            .CreateSprite(MainGame.Graphics.GraphicsDevice, 0, true);

        _audioSlideEventDescription = StudioSystem.GetEvent("event:/SFX/UI/Tickets/Slide");
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

    public void AddTicket(int floorNumber, TicketActor.TicketFlags flags)
    {
        var newTicket = new TicketActor()
        {
            FloorNumber = floorNumber,
            TargetPosition = new Vector2(MainGame.GameBounds.Center.X, MainGame.GameBounds.Bottom - 32),
            Flags = flags,
        };
        newTicket.LoadContent();

        _tickets.Add(newTicket);
    }

    public IEnumerator RemoveTicket(int floorNumber)
    {
        var ticket = _tickets.Find(t => t.FloorNumber == floorNumber);

        _toRemove.Add(ticket);

        ticket.TargetPosition = new(ticket.TargetPosition.X, MainGame.GameBounds.Height + 32);
        while(ticket.Position.Y < ticket.TargetPosition.Y - 1)
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
        MainGame.CurrentMenu = MainGame.Menus.Tickets;

        for (int i = _tickets.Count - 1; i >= 0; i--)
        {
            var ticketActor = _tickets[i];

            PlaySlideSound((float)(_tickets.Count - i) / 10);

            ticketActor.TargetPosition = new(((i % 5)) * 16 + 9, MainGame.GameBounds.Height - 4 - ((i / 5) * 22));
            yield return 4;
        }

        yield return _easeOffsetHandle?.Wait();
    }

    public IEnumerator Close()
    {
        _isOpen = false;
        MainGame.CurrentMenu = MainGame.Menus.None;
        for (int i = 0; i < _tickets.Count; i++)
        {
            var ticketActor = _tickets[i];
            var oldTarget = ticketActor.TargetPosition;
            ticketActor.TargetPosition =
                new(2 + i * 3, MainGame.GameBounds.Height - 2);

            // should make interrupting the open animation be less delayed
            if (oldTarget == ticketActor.TargetPosition) continue;

            PlaySlideSound((float)(_tickets.Count - i) / 10);
            yield return 2;
        }

        yield return _easeOffsetHandle?.Wait();
    }

    private IEnumerator EaseOffset()
    {
        MainGame.CameraPositionTarget = MainGame.CameraPositionTarget with { X = -_targetOffset };
        MainGame.GrayscaleCoeffTarget = 1-_targetOffset/MaxOffset;
        while(!MathUtil.Approximately(_offset, _targetOffset, 1))
        {
            _offset = MathUtil.ExpDecay(
                _offset,
                _targetOffset,
                8,
                1/60f
            );

            yield return null;
        }
        _offset = _targetOffset;
    }

    private void PlaySlideSound(float pitchPercent)
    {
        var eventInstance = _audioSlideEventDescription.CreateInstance();
        eventInstance.SetParameterValue("PitchPercent", pitchPercent);
        eventInstance.Start();
        eventInstance.Dispose();
    }
}
