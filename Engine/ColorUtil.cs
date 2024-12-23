using Microsoft.Xna.Framework;

namespace Engine;

public static class ColorUtil
{
    public static Color CreateFromHex(uint value, int alpha = 0xFF) => new(
        (int)((value & 0xFF0000) >> 0x10),
        (int)((value & 0x00FF00) >> 0x08),
        (int)(value & 0x0000FF),
        alpha
    );
}
