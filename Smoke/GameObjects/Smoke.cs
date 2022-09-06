using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smoke.GameObjects;

internal class Smoke
{
    public Smoke(Vector2 position)
    {
        var rnd = new Random();

        MapPosition = position;
        MaxDecay = 200;
        Scale = (float)(rnd.NextDouble() * 0.25 + 0.1);

        float velocity = (float)rnd.NextDouble();
        Angle = MathHelper.ToRadians(rnd.Next(0, 359));
        var deltaX = (float)Math.Sin(Angle) * velocity;
        var deltaY = (float)-Math.Cos(Angle) * velocity;
        Movement = new Vector2(deltaX, deltaY);

        Tint = rnd.Next(200, 255);
    }

    public void Update(float delta)
    {
        if (Decay < MaxDecay) Decay++;
        MapPosition += Movement * (delta / 100);
    }

    // IDecaying
    public int Decay { get; set; } = 0;
    public int MaxDecay { get; init; } = 200;
    public bool MarkedForDestroy { get => Decay >= MaxDecay; }
    public float Scale { get; init; }
    public float Angle { get; init; }
    public Vector2 Movement { get; init; }
    public Vector2 MapPosition { get; set; }
    public Vector2? ScreenPosition { get; set; }
    public int Tint { get; set; }
    public Color TintColor { get => new Color(Tint, Tint, Tint); }

}
