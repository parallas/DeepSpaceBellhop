using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.Dialog;

public class Dialog()
{
    public enum DisplayMethod
    {
        Human,
        Alien
    }

    public class Page
    {
        public string Content { get; set; }
        public int CharInterval { get; set; } = 4;
    }

    public const int Padding = 2;
    public const int FastScrollSpeed = 1;
    public const int LineHeight = 10;

    private AnimatedSprite _glyphSprite;

    private int _targetOffsetY = -LineHeight * 3 - Padding * 2;
    private float _offsetY = -LineHeight * 3 - Padding * 2;

    private bool _awaitingConfirmation;

    private readonly List<char> _charBuffer = [];
    private readonly List<int> _glyphBuffer = [];

    public void LoadContent()
    {
        _glyphSprite = ContentLoader.Load<AsepriteFile>("graphics/Glyphs")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false)
            .CreateAnimatedSprite("Tag");
        _glyphSprite.Speed = 0;
    }

    public IEnumerator Display(Page[] pages, DisplayMethod displayMethod)
    {
        var lastMenu = MainGame.CurrentMenu;
        MainGame.CurrentMenu = MainGame.Menus.Dialog;
        MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Wait;
        _targetOffsetY = 0;
        switch (displayMethod)
        {
            case DisplayMethod.Human:
                yield return DisplayHuman(pages);
                break;
            case DisplayMethod.Alien:
                yield return DisplayAlien(pages);
                break;
        }
        MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Default;
        MainGame.CurrentMenu = lastMenu;
        _targetOffsetY = -LineHeight * 3 - Padding * 2;
    }

    private IEnumerator DisplayHuman(Page[] pages)
    {
        _charBuffer.Clear();
        foreach (var page in pages)
        {
            StringReader reader = new(page.Content);

            int ch = -1;
            while ((ch = reader.Read()) != -1)
            {
                _charBuffer.Add((char)ch);

                // Console.WriteLine($"dialog buffer: {string.Join("", _charBuffer)}");

                if(ch != ' ')
                {
                    if(Keybindings.Confirm.IsDown)
                    {
                        MainGame.Cursor.CursorSprite = Cursor.CursorSprites.FastForward;
                        yield return FastScrollSpeed;
                    }
                    else
                    {
                        MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Wait;
                        yield return page.CharInterval;
                    }
                }
            }

            _awaitingConfirmation = true;
            MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Dialog;
            while (!Keybindings.Confirm.Pressed)
            {
                yield return null;
            }
            _awaitingConfirmation = false;

            _charBuffer.Clear();
        }
    }

    private IEnumerator DisplayAlien(Page[] pages)
    {
        _glyphBuffer.Clear();
        foreach (var page in pages)
        {
            StringReader reader = new(page.Content);

            int ch = -1;
            while ((ch = reader.Read()) != -1)
            {
                _glyphBuffer.Add(Random.Shared.Next(_glyphSprite.FrameCount));

                if(Keybindings.Confirm.IsDown)
                {
                    MainGame.Cursor.CursorSprite = Cursor.CursorSprites.FastForward;
                    yield return FastScrollSpeed;
                }
                else
                {
                    MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Wait;
                    yield return page.CharInterval;
                }
            }

            _awaitingConfirmation = true;
            MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Dialog;
            while (!Keybindings.Confirm.Pressed)
            {
                yield return null;
            }
            _awaitingConfirmation = false;

            _glyphBuffer.Clear();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _offsetY = MathUtil.ExpDecay(_offsetY, _targetOffsetY, 10, 1f/60f);
        spriteBatch.Draw(
            MainGame.PixelTexture,
            MainGame.GameBounds with {
                X = 0,
                Y = MathUtil.RoundToInt(_offsetY),
                Height = LineHeight * 3 + Padding * 2
            },
            Color.Black
        );
        spriteBatch.Draw(
            MainGame.PixelTexture,
            MainGame.GameBounds with {
                X = 0,
                Y = MathUtil.RoundToInt(_offsetY),
                Height = LineHeight * 3 + Padding * 2 - 1
            },
            Color.White
        );

        if(_charBuffer.Count != 0)
        {
            StringBuilder builder = new();
            int x = 0;
            int y = 0;
            var str = string.Join("", _charBuffer);
            var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach(var word in words)
            {
                var w = (int)MainGame.Font.MeasureString(word).X + 6;
                if(x + w > 240 - (Padding * 2))
                {
                    builder = new();
                    x = 0;
                    y += LineHeight;
                }

                builder.Append(word + " ");
                x += w;

                spriteBatch.DrawStringSpacesFix(
                    MainGame.Font,
                    builder.ToString(),
                    new Vector2(Padding, Padding + y - 2 + _offsetY),
                    _awaitingConfirmation ? Color.Blue : Color.Black,
                    6
                );
            }
        }

        if(_glyphBuffer.Count != 0)
        {
            int x = 0;
            int y = 0;
            foreach(var ind in _glyphBuffer)
            {
                var w = 9;
                if(x + w > 240 - (Padding * 2))
                {
                    x = 0;
                    y += LineHeight;
                }

                _glyphSprite.SetFrame(ind);
                if(_awaitingConfirmation)
                    _glyphSprite.Color = Color.Blue;
                else
                    _glyphSprite.Color = Color.Black;

                _glyphSprite.Draw(spriteBatch, new(Padding + x, Padding + y - 4 + _offsetY));

                x += w;
            }
        }
    }
}
