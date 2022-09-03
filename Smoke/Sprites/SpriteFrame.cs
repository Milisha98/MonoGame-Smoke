using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smoke.Sprites;


public record SpriteFrame
{
    public SpriteFrame(string name, Texture2D texture, Rectangle sourceRect, Vector2 size)
    {
        this.Name = name;
        this.Texture = texture;
        this.SourceRectangle = sourceRect;
        this.Size = size;
    }

    public string Name { get; init; }

    public Texture2D Texture { get; init; }

    public Rectangle SourceRectangle { get; init; }

    public Vector2 Size { get; init; }

}

