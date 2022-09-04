using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Smoke.Core;
using Smoke.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Smoke.GameObjects;

internal class Rocket : IMonoGame
{
    const string RocketTextureName = "Rocket";
    const string ShadowTextureName = "Rocket-Shadow";

    public Rocket()
    {
        // Define Collision Points
        _relativeCollisionPoints = new List<Vector2>
        {
            new (11, 0),
            new (16, 22),
            new (23, 64),
            new (23, 120),
            new (6, 123),
            new (0, 116),
            new (0, 64),
            new (6, 21)
        };
    }

    //
    // Methods
    //
    public void LoadContent(ContentManager contentManager)
    {
        // Load the Rocket
        Texture2D rocketTexture = contentManager.Load<Texture2D>(RocketTextureName);
        RocketSprite = new SpriteFrame(RocketTextureName,
                                      rocketTexture,
                                      rocketTexture.Bounds,
                                      new Vector2(rocketTexture.Bounds.Width, rocketTexture.Bounds.Height));

        Texture2D smokeTexture = contentManager.Load<Texture2D>(ShadowTextureName);
        ShadowSprite = new SpriteFrame(RocketTextureName,
                                      smokeTexture,
                                      smokeTexture.Bounds,
                                      new Vector2(smokeTexture.Bounds.Width, smokeTexture.Bounds.Height));
    }

    public void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (keyboardState.IsKeyDown(Keys.Left) ||
            keyboardState.IsKeyDown(Keys.A))
            Angle -= (delta / 10 * Handling);

        if (keyboardState.IsKeyDown(Keys.Right) ||
            keyboardState.IsKeyDown(Keys.D))
            Angle += (delta / 10 * Handling);


        // Work out the Acceleration / Velocity
        if (Velocity < MaxVelocity)
        {
            Velocity += delta / Acceleration;
            if (Velocity > MaxVelocity) Velocity = MaxVelocity;
        }

        // Move the absolute position
        var sinTheta = (float)Math.Sin(Angle);
        var cosTheta = (float)Math.Cos(Angle);
        AngleVector = new Vector2(sinTheta, -cosTheta);
        MapPosition += (AngleVector * Velocity);


        // Set the Collision Points
        var centerAdjustment = (RocketSprite.Size - (RocketSprite.Size * Scale)) / 2;   // Because of scaling, we need to re-center
        var scaledVectors = _relativeCollisionPoints
                                .Select(vector => new Vector2                                       // Rotate
                                {
                                    X = cosTheta * (vector.X - Middle.X) -
                                        sinTheta * (vector.Y - Middle.Y) + Middle.X,
                                    Y = sinTheta * (vector.X - Middle.X) +
                                        cosTheta * (vector.Y - Middle.Y) + Middle.Y
                                })
                                .Select(vector => vector * Scale)                                   // Scale
                                .Select(vector => vector + MapPosition + centerAdjustment);         // Align Middle



        CollisionPoints = scaledVectors.Select(vector => new Rectangle(vector.ToPoint(), new Point(1, 1))).ToList();

    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Draw the Rocket
        var shadowOffset = new Vector2(25, 50);
        DrawFrame(spriteBatch, ShadowSprite, shadowOffset);
        DrawFrame(spriteBatch, RocketSprite);
    }

    private void DrawFrame(SpriteBatch spriteBatch, SpriteFrame spriteFrame, Vector2? offset = null)
    {
        Color color = Color.White;
        var texture = spriteFrame.Texture;

        var origin = new Vector2(texture.Width / 2, texture.Height / 2);
        var rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        var newPostion = ScreenPosition + (offset ?? Vector2.Zero) + origin;

        spriteBatch.Draw(texture, newPostion, rectangle, color, Angle, origin, Scale, SpriteEffects.None, 1);

    }

    //
    // Properties
    //

    public SpriteFrame RocketSprite { get; set; }
    public SpriteFrame ShadowSprite { get; set; }

    public Vector2 MapPosition { get; set; }
    public Vector2 ScreenPosition { get; set; } = Vector2.Zero;
    public float Acceleration { get; set; } = 10f;         // For now it can be linear
    public float MaxVelocity { get; set; } = 10;
    public float Velocity { get; set; } = 0;
    public float Handling { get; set; } = MathHelper.ToRadians(1);
    public float Angle { get; set; } = 0;
    public float Scale { get; set; } = 0.7f;
    public Vector2 AngleVector { get; set; } = Vector2.Zero;

    public Rectangle ClaytonsCollisionRectangle
    {
        get
        {
            var height = RocketSprite.Size.Y * Scale;
            var size = new Vector2(height, height);
            var location = new Vector2(MapPosition.X - (height / 2) + (Width / 2), MapPosition.Y - (height / 2) + (Height / 2));
            return new Rectangle(location.ToPoint(), size.ToPoint());
        }
    }

    private List<Vector2> _relativeCollisionPoints;
    public List<Rectangle> CollisionPoints { get; set; } = new();




    // Calculated
    public int Width { get => (int)RocketSprite.Size.X; }
    public int Height { get => (int)RocketSprite.Size.Y; }
    public Vector2 CurrentVector { get => AngleVector * Velocity; }
    public Vector2 Middle { get => RocketSprite.Texture.Middle(); }

}
