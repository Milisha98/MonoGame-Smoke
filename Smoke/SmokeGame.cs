using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Smoke.Core;
using Smoke.GameObjects;
using Smoke.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Smoke;

public class SmokeGame : Game
{
    const int ScreenWidth = 2560;
    const int ScreenHeight = 1440;

    const int RenderWidth = 1280;
    const int RenderHeight = 720;
    //const int RenderWidth = 1920;
    //const int RenderHeight = 1080;


    const int TileWidth = 32, TileHeight = 32;

    private RenderTarget2D _renderTarget;
    float _scale = 1f;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Tiles
    private TexturePackLoader _tileSetLoader = new();
    private Map.Map _mapData;
    private SpriteSheet _tileSet;
    private Vector2 _tileOffset;

    // Game Objects
    private Rocket _rocket = new();
    private SmokeEmitter _smoke;

    // View Port
    private Vector2 _viewPortMapTopLeft;
    private Rectangle _viewPort;

    // Debugging    
    private bool _break = false;
    private SpriteFont _debugText;
    private Texture2D _debugRectangle;



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
        // Ensure that the game speed is fixed at 60FPS
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
        IsFixedTimeStep = true;

        // Smoke Emitter needs knowledge of the rocket
        _smoke = new(_rocket);

        base.Initialize();
    }

    //
    // Load Content
    //
    protected override void LoadContent()
    {
        // Game
        _renderTarget = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight);
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Tiles
        _tileSet = _tileSetLoader.LoadContent(Content, "TileSet/Tile-Map");
        _mapData = new Map.Map(_tileSet);

        // Rocket
        _rocket.LoadContent(Content);
        _rocket.ScreenPosition = new Vector2((RenderWidth / 2) - (_rocket.Width / 2), (RenderHeight / 2) - (_rocket.Height / 2));
        _rocket.MapPosition = new Vector2((_mapData.Rows / 2) * TileWidth, (_mapData.Columns / 2) * TileHeight);

        // Smoke Emitter
        _smoke.LoadContent(Content);

        _debugText = Content.Load<SpriteFont>("Text");
        _debugRectangle = new Texture2D(GraphicsDevice, 1, 1);
        _debugRectangle.SetData(new Color[] { Color.White });
    }


    //
    // Update
    //
    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var controller1 = GamePad.GetState(PlayerIndex.One);

        if (controller1.Buttons.Back == ButtonState.Pressed ||
            keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        // Keyboard
        if (keyboardState.IsKeyDown(Keys.OemTilde)) _break = _break == false;
        if (_break) return;

        UpdateRocket(gameTime);
        UpdateCamera();
        UpdateSmoke(gameTime);

        bool collide = CheckCollisions();
        if (collide)
        {
            _break = true;
        }

        base.Update(gameTime);
    }

    //
    // Draw
    //
    protected override void Draw(GameTime gameTime)
    {
        _scale = 1f / (((float)RenderHeight) / GraphicsDevice.Viewport.Height);
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        // == Draw to the Back-Buffer ==
        _spriteBatch.Begin();

        // Draw the background
        DrawTiles();
        DrawDebug();
        DrawRocket(gameTime);
        DrawSmoke(gameTime);


        _spriteBatch.End();

        DrawAndScaleFromBackBuffer();

        base.Draw(gameTime);
    }

    #region Collisions

     private bool CheckCollisions()
    {
        List<Rectangle> collisionPoints = ClaytonsCollisionCheck();
        if (collisionPoints.Any() == false) return false;

        // Now, lets be more fine grained and check Rocket Collision Points 
        foreach (var point in collisionPoints)
        {
            if (_rocket.CollisionPoints.Any(x => x.Intersects(point)))
            {
                return true;
            }
        }
        return false;
    }

    private List<Rectangle> ClaytonsCollisionCheck()
    {
        // Do a generous check
        var rocketRectangle = _rocket.ClaytonsCollisionRectangle;

        // Get Tiles that Intersect
        var collisionPoints = _mapData.CollisionPoints
                                      .Where(tile => rocketRectangle.Intersects(tile))
                                      .ToList();
        return collisionPoints;
    }

    #endregion

    #region Update Methods

    private void UpdateSmoke(GameTime gameTime)
    {
        // Update Smoke
        _smoke.ViewPort = _viewPort;
        _smoke.Update(gameTime);
    }

    private void UpdateRocket(GameTime gameTime)
    {
        // Update the Rocket
        _rocket.Update(gameTime);
    }

    private void UpdateCamera()
    {
        // Update the ViewPort (Camera)
        _viewPortMapTopLeft = _rocket.MapPosition - new Vector2(RenderWidth / 2, RenderHeight / 2);
        _viewPort = new Rectangle(_viewPortMapTopLeft.ToPoint(), new Point(RenderWidth, RenderHeight));

        // Offset the Tiles for the Camera
        _tileOffset = new Vector2(_viewPortMapTopLeft.X % TileWidth, _viewPortMapTopLeft.Y % TileHeight);       // Tile offset adjusts the tile-map to keep things smooth.
    }

    #endregion

    #region Draw Methods

    private void DrawAndScaleFromBackBuffer()
    {
        // Display the Back-Buffer to the screen with a scale
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();
        _spriteBatch.Draw(_renderTarget, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);
        _spriteBatch.End();
    }

    private void DrawDebug()
    {

        DrawClaytonsCollisionRectangle();

        foreach (var r in _rocket.CollisionPoints)
        {
            var screenRectangle = MapPositionToScreenPosition(r.Location.ToVector2());
            if (screenRectangle is not null)
            {
                _spriteBatch.Draw(_debugRectangle, screenRectangle.Value, Color.Red);
            }
        }

        
        // Debug
        //        string debugText = $@"
        //Rocket Map Pos:         {_rocket.MapPosition - _rocket.Middle()}
        //Smoke Emitter Map Pos:  {_smokeEmitterMapPosition}
        //Rocket Screen Pos:      {_rocket.ScreenPosition - _rocket.Middle()}
        //Smoke Map -> Screen:    {MapPositionToScreenPosition(_smokeEmitterMapPosition)}
        //Angle Vector:           {_rocket.AngleVector}
        //";

        //        _spriteBatch.DrawString(_debugText, debugText, new Vector2(0, 0), Color.White);
    }

    private void DrawSmoke(GameTime gameTime)
    {
        _smoke.Draw(_spriteBatch, gameTime);
    }

    private void DrawRocket(GameTime gameTime)
    {
        _rocket.Draw(_spriteBatch, gameTime);
    }


    private void DrawTiles()
    {
        _mapData.DrawMap(_spriteBatch, _viewPort, _tileOffset);
    }

    private Vector2? MapPositionToScreenPosition(Vector2 mapPosition)
    {
        if (_viewPort.Contains(mapPosition))
        {
            return mapPosition - _viewPortMapTopLeft - _rocket.Middle;
        }
        return null;
    }

    private void DrawClaytonsCollisionRectangle()
    {
        var rect = _rocket.ClaytonsCollisionRectangle;
        var origin = MapPositionToScreenPosition(new Vector2(rect.X, rect.Y));
        if (origin == null) return;
        var screenRectangle = new Rectangle(origin.Value.ToPoint(), new Point(rect.Width, rect.Height));
        _spriteBatch.Draw(_debugRectangle, screenRectangle, Color.Beige);
    }

    #endregion
}