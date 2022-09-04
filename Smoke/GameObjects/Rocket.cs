using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Smoke.Core;
using Smoke.Sprites;
using System;
using System.Net.Sockets;

namespace Smoke.GameObjects;

internal class Rocket : IMonoGame
{
    const string RocketTextureName = "Rocket-Wireframe";
    const string ShadowTextureName = "Rocket-Shadow";

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
        var deltaX = (float)Math.Sin(Angle);
        var deltaY = (float)-Math.Cos(Angle);
        AngleVector = new Vector2(deltaX, deltaY);
        MapPosition += (AngleVector * Velocity);
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

    // Calculated
    public int Width { get => (int)RocketSprite.Size.X; }
    public int Height { get => (int)RocketSprite.Size.Y; }
    public Vector2 CurrentVector { get => AngleVector * Velocity; }
    public Vector2 Middle() => RocketSprite.Texture.Middle();

}
