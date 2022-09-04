using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
}
