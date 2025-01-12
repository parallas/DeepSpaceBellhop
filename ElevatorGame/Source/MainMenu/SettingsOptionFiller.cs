using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElevatorGame.Source.MainMenu;

public class SettingsOptionFiller : SettingsOption<object>
{
    public static SettingsOptionFiller Create(int index, string? langToken = null)
    {
        return new()
        {
            Index = index,
            LangToken = langToken,
            SetValue = null,
            GetValue = null,
        };
    }

    public override void Draw(SpriteBatch spriteBatch, bool isSelected)
    {
        float textWidth = 0;

        if (LangToken is not null)
        {
            textWidth = MainGame.FontBold.MeasureString(LocalizationManager.Get(LangToken)).X - 4;

            spriteBatch.DrawStringSpacesFix(
                MainGame.FontBold,
                LocalizationManager.Get(LangToken),
                Vector2.UnitX * (SettingsMenu.DividerX - 4) + Vector2.UnitY * (4 + (Index * 10) - 2),
                Color.White,
                6
            );
        }

        spriteBatch.Draw(
            MainGame.PixelTexture,
            new Rectangle(SettingsMenu.DividerX + 2 + MathUtil.RoundToInt(textWidth), 4 + 4 + Index * 10, 160 - MathUtil.RoundToInt(textWidth), 1),
            Color.White
        );
    }

    public override void LoadContent() { }

    public override void Update(bool isSelected) { }
}
