using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.MainMenu;

public class SettingsOptionEnum : SettingsOption<string>
{
    private Rectangle _bounds;
    private Rectangle _rightArrowBounds;
    private Rectangle _leftArrowBounds;

    private AnimatedSprite _arrowSprite;

    private bool _usingMouse;

    private Point _lastMousePos;

    private List<string> _options;
    private List<string> _optionNames;

    private int _selected;

    public SettingsOptionEnum(int index, IEnumerable<(string, string)> options)
    {
        Index = index;
        var boundWidth = MainGame.GameBounds.Width - SettingsMenu.DividerX - 4;
        _bounds = new(SettingsMenu.DividerX, 2 + Index * 10, boundWidth, 9);
        _rightArrowBounds = new Rectangle(_bounds.Location + new Point(boundWidth - 9, 0), new(9));
        _leftArrowBounds = new Rectangle(_bounds.Location + new Point(boundWidth - 120, 0), new(9));
        _options = [..from pair in options select pair.Item1];
        _optionNames = [..from pair in options select pair.Item2];
    }

    public override void LoadContent()
    {
        _arrowSprite = ContentLoader.Load<AsepriteFile>("graphics/settings/Arrow")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
        _arrowSprite.Origin = new(1);

        _selected = MathHelper.Max(0, _options.IndexOf(GetValue()));
    }

    public override void Update(bool isSelected)
    {
        int inputDir =
        ((Keybindings.Right.Pressed || InputManager.GetPressed(Buttons.LeftThumbstickRight)) ? 1 : 0)
        - ((Keybindings.Left.Pressed || InputManager.GetPressed(Buttons.LeftThumbstickLeft)) ? 1 : 0);

        if (InputManager.GetAnyDown(InputType.Mouse) || InputManager.MousePosition != _lastMousePos)
        {
            _usingMouse = true;
        }

        var mouseInBounds = _bounds.Contains(MainGame.Cursor.ViewPosition);
        var mouseOverRightArrow = _rightArrowBounds.Contains(MainGame.Cursor.ViewPosition);
        var mouseOverLeftArrow = _leftArrowBounds.Contains(MainGame.Cursor.ViewPosition);

        if (InputManager.GetAnyDown(InputType.Keyboard) || InputManager.GetAnyDown(InputType.GamePad))
        {
            _usingMouse = false;
        }

        if (mouseOverRightArrow && InputManager.GetPressed(MouseButtons.LeftButton))
        {
            if (!isSelected)
                SetSelected(Index);

            int sel = _selected + 1;
            if(sel >= _options.Count) sel = 0;
            SetValue(_options[sel]);
        }
        else if (mouseOverLeftArrow && InputManager.GetPressed(MouseButtons.LeftButton))
        {
            if (!isSelected)
                SetSelected(Index);

            int sel = _selected - 1;
            if(sel < 0) sel = _options.Count - 1;
            SetValue(_options[sel]);
        }
        else if (!InputManager.GetPressed(MouseButtons.LeftButton) && isSelected && Keybindings.Confirm.Pressed)
        {
            int sel = _selected + inputDir;
            if(sel < 0) sel = _options.Count - 1;
            SetValue(_options[sel % _options.Count]);
        }

        _lastMousePos = InputManager.MousePosition;
    }

    public override void Draw(SpriteBatch spriteBatch, bool isSelected)
    {
        bool highlighted = (_usingMouse && _bounds.Contains(MainGame.Cursor.ViewPosition)) || (isSelected && !_usingMouse);
        bool rightArrowHighlighted = (_usingMouse && _rightArrowBounds.Contains(MainGame.Cursor.ViewPosition)) || (isSelected && !_usingMouse);
        bool leftArrowHighlighted = (_usingMouse && _leftArrowBounds.Contains(MainGame.Cursor.ViewPosition)) || (isSelected && !_usingMouse);

        spriteBatch.DrawStringSpacesFix(
            MainGame.Font,
            LocalizationManager.Get(LangToken),
            _bounds.Location.ToVector2() - Vector2.UnitY * 2,
            highlighted ? Color.White : ColorUtil.CreateFromHex(0xc0c0c0),
            6
        );

        _arrowSprite.FlipHorizontally = false;
        _arrowSprite.SetFrame(rightArrowHighlighted ? 2 : (highlighted ? 1 : 0));
        _arrowSprite.Draw(spriteBatch, _rightArrowBounds.Location.ToVector2());

        _arrowSprite.FlipHorizontally = true;
        _arrowSprite.SetFrame(leftArrowHighlighted ? 2 : (highlighted ? 1 : 0));
        _arrowSprite.Draw(spriteBatch, _leftArrowBounds.Location.ToVector2());

        float textWidth = MainGame.Font.MeasureString(_optionNames[_selected]).X;
        spriteBatch.DrawStringSpacesFix(
            MainGame.Font,
            _optionNames[_selected],
            new Vector2(((_leftArrowBounds.Right + _rightArrowBounds.Left) / 2f) - (textWidth / 2), _bounds.Y - 2),
            highlighted ? Color.White : ColorUtil.CreateFromHex(0xc0c0c0),
            6
        );

        // spriteBatch.Draw(
        //     MainGame.PixelTexture,
        //     _bounds,
        //     null,
        //     Color.Red * 0.5f
        // );

        // spriteBatch.DrawNineSlice(
        //     MainGame.OutlineTexture,
        //     _leftArrowBounds,
        //     null,
        //     new Point(1),
        //     new Point(1),
        //     Color.Yellow * 0.5f
        // );

        // spriteBatch.DrawNineSlice(
        //     MainGame.OutlineTexture,
        //     _rightArrowBounds,
        //     null,
        //     new Point(1),
        //     new Point(1),
        //     Color.Yellow * 0.5f
        // );
    }
}
