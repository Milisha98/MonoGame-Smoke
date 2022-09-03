using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Smoke.Sprites;
using System;
using System.Collections.Generic;

namespace Smoke;

public class SmokeGame : Game
{ 
    const int ScreenWidth = 2560;
    const int ScreenHeight = 1440;
    const int TileWidth = 32, TileHeight = 32;

    private RenderTarget2D _renderTarget;
    float _scale = 1f;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Tiles
    private Map.Map _mapData;
    private SpriteSheet _tileSet;
    private Vector2 _tileOffset;

    // Rocket
    private Texture2D _rocket, _rocketShadow, _smoke;
    private Vector2 _rocketMapPosition, _rocketScreenPosition;
    private float _rocketAcceleration = 10f;         // For now it can be linear
    private float _rocketMaxVelocity = 10;
    private float _rocketVelocity = 0;
    private float _rocketHandling = MathHelper.ToRadians(1);
    private float _rocketAngle = 0;

    // Smoke
    private Vector2 _smokeEmitterMapPosition, _smokeEmitterScreenPosition;
    private List<Smoke> _smokeParticles;

    private Vector2 _viewPortMapTopLeft;
    private Rectangle _viewPort;

    // Debugging    
    private SpriteFont _debugText;
    private bool _break = false;


    public SmokeGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _smokeParticles = new List<Smoke>();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _renderTarget = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight);
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Tiles
        var loader = new TexturePackLoader(Content);
        _tileSet = loader.Load("TileSet/Tile-Map");
        
        _mapData = new Map.Map(_tileSet); 

        // Rocket
        _rocket = Content.Load<Texture2D>("Rocket-Wireframe");
        _rocketShadow = Content.Load<Texture2D>("Rocket-Shadow");
        _smoke = Content.Load<Texture2D>("Smoke");

        _debugText = Content.Load<SpriteFont>("Text");

        _rocketScreenPosition = new Vector2(640 - (_rocket.Width / 2), 360 - (_rocket.Height / 2));

        _rocketMapPosition = new Vector2((_mapData.Rows / 2) * TileWidth, (_mapData.Columns / 2) * TileHeight);

        _smokeEmitterMapPosition = new Vector2(_smoke.Width / 2, _smoke.Height / 2) + 
                                   new Vector2(_rocket.Width / 2, _rocket.Height);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var controller1 = GamePad.GetState(PlayerIndex.One);
        float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000;

        if (controller1.Buttons.Back == ButtonState.Pressed || 
            keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        // Keyboard
        if (keyboardState.IsKeyDown(Keys.OemTilde)) _break = _break == false;
        if (_break) return;

        if (keyboardState.IsKeyDown(Keys.Left) || 
            keyboardState.IsKeyDown(Keys.A))
            _rocketAngle -= (delta * 100 * _rocketHandling);
        
        if (keyboardState.IsKeyDown(Keys.Right) || 
            keyboardState.IsKeyDown(Keys.D))
            _rocketAngle += (delta * 100 * _rocketHandling);


        // Work out the Acceleration / Velocity
        if (_rocketVelocity < _rocketMaxVelocity)
        {
            _rocketVelocity += delta * _rocketAcceleration;
            if (_rocketVelocity > _rocketMaxVelocity) _rocketVelocity = _rocketMaxVelocity;
        }

        // Move the absolute position
        var deltaX = (float)Math.Sin(_rocketAngle);
        var deltaY = (float)-Math.Cos(_rocketAngle);
        var relativePos = new Vector2(deltaX * _rocketVelocity, deltaY * _rocketVelocity);
        _rocketMapPosition += relativePos;

        _viewPortMapTopLeft = _rocketMapPosition - new Vector2(640, 360);
        _viewPort = new Rectangle(_viewPortMapTopLeft.ToPoint(), new Point(1280, 720));

        _tileOffset = new Vector2(_viewPortMapTopLeft.X % TileWidth, _viewPortMapTopLeft.Y % TileHeight);       // Tile offset adjusts the tile-map to keep things smooth.


        // Everything after this is just nice to have
        if (gameTime.IsRunningSlowly) return;

        // Smoke Emitter position
        float rocketHalfHeight = _rocket.Height / 2;
        var smokeDelta = new Vector2(_smoke.Width / 2, _smoke.Height / 2) -     // Smoke is centered on the Rockets (0, 0)
                         new Vector2(_rocket.Width / 2, rocketHalfHeight);      // Center of Rocket

        rocketHalfHeight += 20;                                                 // Move it off the end of the tail
        smokeDelta += new Vector2(deltaX * rocketHalfHeight, deltaY * rocketHalfHeight);

        _smokeEmitterMapPosition = _rocketMapPosition - smokeDelta;
        _smokeEmitterScreenPosition = _rocketScreenPosition - smokeDelta;

        // Emit Smoke
        foreach (var smoke in _smokeParticles)
        {
            smoke.Update();
            smoke.ScreenPosition = MapPositionToScreenPosition(smoke.MapPosition);
        }
        _smokeParticles.RemoveAll(x => x.MarkedForDestroy);
        _smokeParticles.Add(new Smoke(_smokeEmitterMapPosition));

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _scale = 1f / (720f / GraphicsDevice.Viewport.Height);
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);
        
        // == Draw to the Back-Buffer ==
        _spriteBatch.Begin();

        // Draw the background
        DrawTiles();

        // Draw the Rocket
        var shadowOffset = new Vector2(25, 50);
        DrawRotatedAndScale(_spriteBatch, _rocketShadow, _rocketScreenPosition + shadowOffset, _rocketAngle, 0.7f, Color.White);
        DrawRotatedAndScale(_spriteBatch, _rocket, _rocketScreenPosition, _rocketAngle, 0.7f, Color.White);

        // Draw the Smoke
        var position = MapPositionToScreenPosition(_smokeEmitterMapPosition);        
        foreach (var smoke in _smokeParticles)
        {
            if (smoke.ScreenPosition == null) continue;
            DrawRotatedAndScale(_spriteBatch, _smoke, smoke.ScreenPosition.Value, smoke.Angle, smoke.Scale * 0.8f, smoke.TintColor);
        }

        // Debug
        //_spriteBatch.DrawString(_debugText, $"{offsetString}", new Vector2(0, 0), Color.White);

        _spriteBatch.End();


        // Display the Back-Buffer to the screen with a scale
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();
        _spriteBatch.Draw(_renderTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawTiles()
    {
        _mapData.DrawMap(_spriteBatch, _viewPort, _tileOffset);
    }

    private void DrawRotated(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, float angle)
        => DrawRotatedAndScale(spriteBatch, texture, position, angle, 1.0f, Color.White);

    private void DrawRotatedAndScale(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, float angle, float scale, Color color)
    {
        var origin = new Vector2(texture.Width / 2, texture.Height / 2);
        var rectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        var newPostion = position + origin;

        spriteBatch.Draw(texture, newPostion, rectangle, color, angle, origin, scale, SpriteEffects.None, 1);

    }

    private Vector2? MapPositionToScreenPosition(Vector2 mapPosition)
    {
        if (_viewPort.Contains(mapPosition))
        {
            return mapPosition - _viewPortMapTopLeft - new Vector2(_rocket.Width / 2, _rocket.Height / 2);
        }
        return null;
    }
}