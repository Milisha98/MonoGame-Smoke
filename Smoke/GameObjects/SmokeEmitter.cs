using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Smoke.Core;
using Smoke.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Smoke.GameObjects;

internal class SmokeEmitter : IMonoGame
{
    const string TextureName = "Smoke";
    private Rocket _rocket;
    private List<Smoke> _smokeParticles = new();

    public SmokeEmitter(Rocket rocket)
    {
        _rocket = rocket;
    }

    //
    // Methods
    //
    public void LoadContent(ContentManager contentManager)
    {
        var texture = contentManager.Load<Texture2D>(TextureName);
        SmokeSprite = new SpriteFrame(TextureName,
                                      texture,
                                      texture.Bounds,
                                      new Vector2(texture.Bounds.Width, texture.Bounds.Height));
    }

    public void Update(GameTime gameTime)
    {
        // Smoke is just special effects. Not essential
        if (gameTime.IsRunningSlowly) return;

        float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        // Smoke Emitter
        Vector2 rocketMiddle = this.Middle() - _rocket.Middle();               // Update the Position
        var smokeDelta = _rocket.AngleVector * (_rocket.Height / 2);
        MapPosition = _rocket.MapPosition - rocketMiddle - smokeDelta;

        // Emit Smoke
        foreach (var smoke in _smokeParticles)
        {
            smoke.ScreenPosition = MapPositionToScreenPosition(smoke.MapPosition);
            smoke.Update(delta);            
        }
        _smokeParticles.RemoveAll(x => x.MarkedForDestroy);
        _smokeParticles.Add(new Smoke(MapPosition));
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        // Smoke is just special effects. Not essential
        if (gameTime.IsRunningSlowly) return;

        // Draw the Smoke      
        foreach (var smoke in _smokeParticles)
        {
            if (smoke.ScreenPosition is not null)
            {
                DrawFrame(spriteBatch, smoke);
            }
        }
    }
    private void DrawFrame(SpriteBatch spriteBatch, Smoke smoke)
    {
        if (smoke.ScreenPosition.HasValue == false) return;

        Color color = smoke.TintColor;
        var texture = SmokeSprite.Texture;
        var scale = smoke.Scale * 0.8f;

        var origin = new Vector2(texture.Width / 2, texture.Height / 2);
        var rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        var newPostion = smoke.ScreenPosition.Value + origin;

        spriteBatch.Draw(texture, newPostion, rectangle, color, smoke.Angle, origin, scale, SpriteEffects.None, 1);

    }

    //
    // Properties
    //
    public SpriteFrame SmokeSprite { get; set; }

    public Vector2 MapPosition { get; set; }

    public Vector2 Middle() => SmokeSprite.Texture.Middle();

    public Rectangle ViewPort { get; set; }


    private Vector2? MapPositionToScreenPosition(Vector2 mapPosition)
    {
        if (ViewPort.Contains(mapPosition))
        {
            var viewPortTopLeft = new Vector2(ViewPort.X, ViewPort.Y);
            return mapPosition - viewPortTopLeft - _rocket.Middle();
        }
        return null;
    }
}
