using AsepriteDotNet.Aseprite;
using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;

namespace ElevatorGame.Source.MainMenu;

public class SettingsOptionCheckbox : SettingsOption<bool>
{
    public bool Inverted { get; set; }

    private Rectangle _bounds;
    private Rectangle _boxBounds;

    private AnimatedSprite _checkboxSprite;

    private bool _usingMouse;

    private Point _lastMousePos;

    public SettingsOptionCheckbox(int index)
    {
        Index = index;
        _bounds = new(SettingsMenu.DividerX, 4 + Index * 10, 140, 9);
        _boxBounds = new Rectangle(_bounds.Location + new Point(140 - 9, 0), new(9));
    }

    public override void LoadContent()
    {
        _checkboxSprite = ContentLoader.Load<AsepriteFile>("graphics/settings/Checkbox")
            .CreateSpriteSheet(MainGame.Graphics.GraphicsDevice, true)
            .CreateAnimatedSprite("Tag");
        _checkboxSprite.Origin = new(1);
    }

    public override void Update(bool isSelected)
    {
        if (InputManager.GetAnyDown(InputType.Mouse) || InputManager.MousePosition != _lastMousePos)
        {
            _usingMouse = true;
        }

        var mouseInBounds = _bounds.Contains(MainGame.Cursor.ViewPosition);

        if (InputManager.GetAnyDown(InputType.Keyboard) || InputManager.GetAnyDown(InputType.GamePad))
        {
            _usingMouse = false;
        }

        if (mouseInBounds && InputManager.GetPressed(MouseButtons.LeftButton))
        {
            if (!isSelected)
                SetSelected(Index);

            SetValue(!GetValue());
        }
        else if (!InputManager.GetPressed(MouseButtons.LeftButton) && isSelected && Keybindings.Confirm.Pressed)
        {
            SetValue(!GetValue());
        }

        _lastMousePos = InputManager.MousePosition;
    }

    public override void Draw(SpriteBatch spriteBatch, bool isSelected)
    {
        bool highlighted = (_usingMouse && _bounds.Contains(MainGame.Cursor.ViewPosition)) || (isSelected && !_usingMouse);
        bool boxHighlighted = (_usingMouse && _boxBounds.Contains(MainGame.Cursor.ViewPosition)) || (isSelected && !_usingMouse);

        spriteBatch.DrawStringSpacesFix(
            MainGame.Font,
            LocalizationManager.Get(LangToken),
            _bounds.Location.ToVector2() - Vector2.UnitY * 2,
            highlighted ? Color.White : ColorUtil.CreateFromHex(0xc0c0c0),
            6
        );

        int frame = Inverted ? (GetValue() ? 0 : 1) : (GetValue() ? 1 : 0);
        if (highlighted)
            frame += 2;
        if (boxHighlighted)
            frame += 2;

        _checkboxSprite.SetFrame(frame);
        _checkboxSprite.Draw(spriteBatch, _boxBounds.Location.ToVector2());

        // spriteBatch.Draw(
        //     MainGame.PixelTexture,
        //     _bounds,
        //     null,
        //     Color.Red * 0.5f
        // );

        // spriteBatch.DrawNineSlice(
        //     MainGame.OutlineTexture,
        //     _boxBounds,
        //     null,
        //     new Point(1),
        //     new Point(1),
        //     Color.Yellow * 0.5f
        // );
    }
}
