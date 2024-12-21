using AsepriteDotNet.Aseprite.Types;
using Microsoft.Xna.Framework;

namespace ElevatorGame;

static class Extensions
{
    public static Vector2 GetLocation(this AsepriteSliceKey key)
    {
        return key.Bounds.Location.ToVector2();
    }
}