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

    private SpriteFont _font;
    private AnimatedSprite _glyphSprite;

    private bool _awaitingConfirmation;

    private readonly List<char> _charBuffer = [];
    private readonly List<int> _glyphBuffer = [];

    public void LoadContent()
    {
        _font = ContentLoader.Load<SpriteFont>("fonts/default");

        _glyphSprite = ContentLoader.Load<AsepriteFile>("graphics/Glyphs")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, false)
            .CreateAnimatedSprite("Tag");
        _glyphSprite.Speed = 0;
    }

    public IEnumerator Display(Page[] pages, DisplayMethod displayMethod)
    {
        MainGame.Cursor.CursorSprite = Cursor.CursorSprites.Wait;
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
    }

    private static bool CheckConfirmDown() => InputManager.GetDown(Keys.Enter) || InputManager.GetDown(Keys.Space) || InputManager.GetDown(MouseButtons.LeftButton);
    private static bool CheckConfirmPressed() => InputManager.GetPressed(Keys.Enter) || InputManager.GetPressed(Keys.Space) || InputManager.GetPressed(MouseButtons.LeftButton);

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
                    if(CheckConfirmDown())
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
            while (!CheckConfirmPressed())
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

                if(CheckConfirmDown())
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
            while (!CheckConfirmPressed())
            {
                yield return null;
            }
            _awaitingConfirmation = false;

            _glyphBuffer.Clear();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if(_charBuffer.Count != 0)
        {
            StringBuilder builder = new();
            int x = 0;
            int y = 0;
            var str = string.Join("", _charBuffer);
            var words = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach(var word in words)
            {
                var w = (int)_font.MeasureString(word).X + 6;
                if(x + w > 240 - (Padding * 2))
                {
                    builder = new();
                    x = 0;
                    y += LineHeight;
                }

                builder.Append(word + " ");
                x += w;

                spriteBatch.DrawStringSpacesFix(
                    _font,
                    builder.ToString(),
                    new Vector2(Padding, Padding + y - 2),
                    _awaitingConfirmation ? Color.Yellow : Color.White,
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
                    _glyphSprite.Color = Color.Yellow;
                else
                    _glyphSprite.Color = Color.White;

                _glyphSprite.Draw(spriteBatch, new(Padding + x, Padding + y - 4));

                x += w;
            }
        }
    }
}
