using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Smoke.Core;
using Smoke.GameObjects;
using Smoke.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Smoke;

public class SmokeGame : Game
{
    const int ScreenWidth = 2560;
    const int ScreenHeight = 1440;

    const int RenderWidth = 1280;
    const int RenderHeight = 720;
    //const int RenderWidth = 1600;
    //const int RenderHeight = 900;


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

    // Rocket
    private GameObjects.Rocket _rocket = new();

    // Smoke
    private Texture2D _smoke;
    private Vector2 _smokeEmitterMapPosition;
    private List<GameObjects.Smoke> _smokeParticles = new();

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
        // Ensure that the game speed is fixed at 60FPS
        TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0);
        IsFixedTimeStep = true;

        base.Initialize();
    }

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

        // Smoke
        _smoke = Content.Load<Texture2D>("Smoke");

        _debugText = Content.Load<SpriteFont>("Text");

    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var controller1 = GamePad.GetState(PlayerIndex.One);
        float delta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

        if (controller1.Buttons.Back == ButtonState.Pressed ||
            keyboardState.IsKeyDown(Keys.Escape))
            Exit();

        // Keyboard
        if (keyboardState.IsKeyDown(Keys.OemTilde)) _break = _break == false;
        if (_break) return;

        _rocket.Update(gameTime);

        _viewPortMapTopLeft = _rocket.MapPosition - new Vector2(RenderWidth / 2, RenderHeight / 2);
        _viewPort = new Rectangle(_viewPortMapTopLeft.ToPoint(), new Point(RenderWidth, RenderHeight));

        _tileOffset = new Vector2(_viewPortMapTopLeft.X % TileWidth, _viewPortMapTopLeft.Y % TileHeight);       // Tile offset adjusts the tile-map to keep things smooth.


        // Everything after this is just nice to have
        if (gameTime.IsRunningSlowly) return;

        // Smoke Emitter position
        Vector2 rocketMiddle = _smoke.Middle(_rocket.RocketSprite.Texture);
        var smokeDelta = _rocket.AngleVector * (_rocket.Height / 2);
        _smokeEmitterMapPosition = (_rocket.MapPosition - rocketMiddle) - smokeDelta;

        // Emit Smoke
        foreach (var smoke in _smokeParticles)
        {
            smoke.Update(delta);
            smoke.ScreenPosition = MapPositionToScreenPosition(smoke.MapPosition);
        }
        _smokeParticles.RemoveAll(x => x.MarkedForDestroy);
        _smokeParticles.Add(new GameObjects.Smoke(_smokeEmitterMapPosition));

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _scale = 1f / (((float)RenderHeight) / GraphicsDevice.Viewport.Height);
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);

        // == Draw to the Back-Buffer ==
        _spriteBatch.Begin();

        // Draw the background
        DrawTiles();
        DrawRocket(gameTime);
        DrawSmoke();
        DrawDebugText();

        _spriteBatch.End();

        DrawAndScaleFromBackBuffer();

        base.Draw(gameTime);
    }

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

    private void DrawDebugText()
    {
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

    private void DrawSmoke()
    {
        // Draw the Smoke      
        foreach (var smoke in _smokeParticles)
        {
            if (smoke.ScreenPosition is not null)
            {
                DrawRotatedAndScale(_spriteBatch, _smoke, smoke.ScreenPosition.Value, smoke.Angle, smoke.Scale * 0.8f, smoke.TintColor);
            }
        }
    }

    private void DrawRocket(GameTime gameTime)
    {
        _rocket.Draw(_spriteBatch, gameTime);
    }


    private void DrawTiles()
    {
        _mapData.DrawMap(_spriteBatch, _viewPort, _tileOffset);
    }

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
            return mapPosition - _viewPortMapTopLeft - _rocket.Middle();
        }
        return null;
    }


    #endregion
}