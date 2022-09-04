using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Smoke.Core;

internal static class Helper
{
    internal static Vector2 Middle(this Texture2D t1) =>
        new Vector2(t1.Width / 2, t1.Height / 2);

    internal static Vector2 Middle(this Texture2D t1, Texture2D t2) =>
        new Vector2(t1.Width / 2, t1.Height / 2) -
        new Vector2(t2.Width / 2, t2.Height / 2);

    internal static Vector2 LocationToVector(this Rectangle rectangle) =>
        new Vector2(rectangle.Left, rectangle.Top);

    internal static Rectangle Move(this Rectangle rectangle, Vector2 move) =>
        new Rectangle((rectangle.Location.ToVector2() + move).ToPoint(), rectangle.Size);

    internal static Vector2 RotatePoint(this Vector2 pointToRotate, float angleInRadians, Vector2? centerPoint = null)
    {
        centerPoint ??= Vector2.Zero;
        double cosTheta = Math.Cos(angleInRadians);
        double sinTheta = Math.Sin(angleInRadians);
        return new Vector2
        {
            X =
                (float)
                (cosTheta * (pointToRotate.X - centerPoint.Value.X) -
                 sinTheta * (pointToRotate.Y - centerPoint.Value.Y) + centerPoint.Value.X),
            Y =
                (float)
                (sinTheta * (pointToRotate.X - centerPoint.Value.X) +
                 cosTheta * (pointToRotate.Y - centerPoint.Value.Y) + centerPoint.Value.Y)
        };
    }
}
