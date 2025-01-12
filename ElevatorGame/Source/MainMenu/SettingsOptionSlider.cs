using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.MainMenu;

public class SettingsOptionSlider : SettingsOption<int>
{
    public int Min { get; }
    public int Max { get; }
    public int StepAmount { get; }

    private Rectangle _bounds;

    private Rectangle KnobBounds => new(
        ((_bounds.Location + new Point(5, 5)).ToVector2()
        + Vector2.UnitX * ((float)(_visualValue - Min) / (Max - Min)) * (_bounds.Width - 9)
        - Vector2.One * 5).ToPoint(),
        new(9)
    );

    private AnimatedSprite _knobSprite;
    private AnimatedSprite _sliderMiddleSprite;
    private AnimatedSprite _sliderLeftSprite;
    private AnimatedSprite _sliderRightSprite;

    private float _visualValue;

    private bool _mouseSelecting;

    private bool _usingMouse;

    private Point _lastMousePos;

    public SettingsOptionSlider(int index, int width, int minValue, int maxValue, int stepAmount)
    {
        Min = minValue;
        Max = maxValue;
        StepAmount = stepAmount;
        Index = index;
        _bounds = new(SettingsMenu.DividerX + 40, 2 + Index * 10, width, 9);
    }

    public override void LoadContent()
    {
        _knobSprite = ContentLoader.Load<AsepriteFile>("graphics/settings/SliderKnob")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
        _knobSprite.Origin = new(4);

        _sliderMiddleSprite = ContentLoader.Load<AsepriteFile>("graphics/settings/SliderBar")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Middle");
        _sliderMiddleSprite.Origin = new(1, 2);

        _sliderLeftSprite = ContentLoader.Load<AsepriteFile>("graphics/settings/SliderBar")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Left");
        _sliderLeftSprite.Origin = new(1, 2);

        _sliderRightSprite = ContentLoader.Load<AsepriteFile>("graphics/settings/SliderBar")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Right");
        _sliderRightSprite.Origin = new(1, 2);

        _visualValue = GetValue();
    }

    public override void Update(bool isSelected)
    {
        int inputDir =
        ((Keybindings.Right.Pressed || InputManager.GetPressed(Buttons.LeftThumbstickRight)) ? 1 : 0)
        - ((Keybindings.Left.Pressed || InputManager.GetPressed(Buttons.LeftThumbstickLeft)) ? 1 : 0);

        _visualValue = MathUtil.ExpDecay(_visualValue, GetValue(), _mouseSelecting ? 28 : 14, 1f / 60f);
        if (MathUtil.Approximately(_visualValue, GetValue(), 0.5f))
            _visualValue = GetValue();

        if (InputManager.GetAnyDown(InputType.Mouse) || InputManager.MousePosition != _lastMousePos)
        {
            _usingMouse = true;
        }

        var mouseInBounds = _bounds.Contains(MainGame.Cursor.ViewPosition);

        if (mouseInBounds || _mouseSelecting)
        {
            MainGame.Cursor.CursorSpriteOverride = Cursor.CursorSprites.Interact;
        }

        if (mouseInBounds && InputManager.GetPressed(MouseButtons.LeftButton))
        {
            _mouseSelecting = true;
        }
        if (InputManager.GetReleased(MouseButtons.LeftButton))
        {
            _mouseSelecting = false;
        }

        if (InputManager.GetAnyDown(InputType.Keyboard) || InputManager.GetAnyDown(InputType.GamePad))
        {
            _usingMouse = false;
        }

        if (_mouseSelecting)
        {
            if (!isSelected)
                SetSelected(Index);

            // I LOVE MATH !!!!!11!!1!
            int val = (int)MathHelper.Clamp(
                (MathHelper.Clamp(MainGame.Cursor.ViewPosition.X - (_bounds.X + 4), 0, _bounds.Width - 4) / (_bounds.Width - 8) * (Max - Min)) + Min,
                Min, Max
            );

            SetValue(val);

            if (Math.Abs(val - _visualValue) < 7)
                _visualValue = val;
        }
        else if (inputDir != 0 && isSelected)
        {
            int val = MathHelper.Clamp(GetValue() + StepAmount * inputDir, Min, Max);

            SetValue(val);
        }

        _lastMousePos = InputManager.MousePosition;
    }

    public override void Draw(SpriteBatch spriteBatch, bool isSelected)
    {
        var boundsShifted = _bounds.Shift(-40, 0).Contains(MainGame.Cursor.ViewPosition);
        bool highlighted = (_usingMouse && (_bounds.Contains(MainGame.Cursor.ViewPosition) || boundsShifted || _mouseSelecting)) || (isSelected && !_usingMouse);
        bool knobHighlighted = (_usingMouse && (KnobBounds.Contains(MainGame.Cursor.ViewPosition) || _mouseSelecting)) || (isSelected && !_usingMouse);
        bool barHighlighted = isSelected && !_usingMouse;

        spriteBatch.DrawStringSpacesFix(
            MainGame.Font,
            LocalizationManager.Get(LangToken),
            _bounds.Location.ToVector2() - Vector2.UnitX * 40 - Vector2.UnitY * 2,
            highlighted ? Color.White : ColorUtil.CreateFromHex(0xc0c0c0),
            6
        );

        float knobX = (float)(_visualValue - Min) / (Max - Min);

        // base bar
        _sliderMiddleSprite.SetFrame(barHighlighted ? 1 : 0);
        _sliderMiddleSprite.ScaleX = _bounds.Width - 8;
        _sliderMiddleSprite.Color = Color.White;
        _sliderMiddleSprite.Draw(spriteBatch, (_bounds.Location + new Point(4, 4)).ToVector2());

        // amount overlay
        _sliderMiddleSprite.SetFrame(2);
        _sliderMiddleSprite.ScaleX = knobX * (_bounds.Width - 8);
        _sliderMiddleSprite.Color = highlighted ? ColorUtil.CreateFromHex(0x87d6dd) : ColorUtil.CreateFromHex(0x67b6bd);
        _sliderMiddleSprite.Draw(spriteBatch, (_bounds.Location + new Point(4, 4)).ToVector2());

        _sliderLeftSprite.SetFrame(barHighlighted ? 1 : 0);
        _sliderLeftSprite.Draw(spriteBatch, (_bounds.Location + new Point(3, 4)).ToVector2());

        _sliderRightSprite.SetFrame(barHighlighted ? 1 : 0);
        _sliderRightSprite.Draw(spriteBatch, (_bounds.Location + new Point(4 + (_bounds.Width - 8), 4)).ToVector2());

        _knobSprite.SetFrame(knobHighlighted ? 2 : (highlighted ? 1 : 0));
        _knobSprite.Draw(spriteBatch, (_bounds.Location + new Point(4 + MathUtil.FloorToInt(knobX * (_bounds.Width - 9)), 4)).ToVector2());

        int displayVal = MathUtil.RoundToInt(GetValue());
        var valueString = $"{displayVal}";

        if (Min == 0 && Max == 100)
            valueString = $"{displayVal}%";

        spriteBatch.DrawStringSpacesFix(
            MainGame.Font,
            valueString,
            new Vector2(_bounds.Right + 24 - MainGame.Font.MeasureString(valueString).X, _bounds.Top - 2),
            highlighted ? Color.White : ColorUtil.CreateFromHex(0xc0c0c0),
            6,
            0, Vector2.Zero
        );

        // spriteBatch.Draw(
        //     MainGame.PixelTexture,
        //     _bounds,
        //     null,
        //     Color.Red * 0.5f
        // );

        // spriteBatch.DrawNineSlice(
        //     MainGame.OutlineTexture,
        //     KnobBounds,
        //     null,
        //     new Point(1),
        //     new Point(1),
        //     Color.Yellow * 0.5f
        // );
    }
}
