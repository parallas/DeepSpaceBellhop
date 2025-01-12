using Microsoft.Xna.Framework;

namespace Engine;

public static class Extensions
{
    public static Vector2 GetBottom(this Rectangle rectangle)
    {
        return new Vector2(rectangle.X, rectangle.Y + rectangle.Height - 1);
    }
    public static Vector2 GetTop(this Rectangle rectangle)
    {
        return new Vector2(rectangle.X, rectangle.Y);
    }
    public static Vector2 GetLeft(this Rectangle rectangle)
    {
        return new Vector2(rectangle.X, rectangle.Y);
    }
    public static Vector2 GetRight(this Rectangle rectangle)
    {
        return new Vector2(rectangle.X + rectangle.Width - 1, rectangle.Y);
    }

    public static Rectangle Shift(this Rectangle rectangle, int x, int y)
    {
        rectangle.X += x;
        rectangle.Y += y;
        return rectangle;
    }

    public static Rectangle Shift(this Rectangle rectangle, Point offset)
    {
        rectangle.X += offset.X;
        rectangle.Y += offset.Y;
        return rectangle;
    }
}
