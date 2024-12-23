using System;
using System.Runtime.InteropServices;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Processors;
using Engine;
using Engine.Display;
using FMOD;
using FmodForFoxes;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using Elevator = ElevatorGame.Source.Elevator;
using Phone = ElevatorGame.Source.Phone;

namespace ElevatorGame;

public class MainGame : Game
{
    public static GraphicsDeviceManager Graphics { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    
    public static long Step { get; private set; }
    public static long Frame { get; private set; }

    public static Texture2D PixelTexture { get; private set; }

    public static Camera Camera { get; } = new()
    {
        RootOffset = Vector2.One * 8
    };

    public static CoroutineRunner Coroutines { get; set; } = new();

    public static float GrayscaleCoeff { get; set; } = 1;

    private static Point _actualWindowSize;
    private static bool _isFullscreen;
    
    private RenderTarget2D _gameSceneRt;
    private RenderTarget2D _uiRt;
    private RenderTarget2D _renderTarget;

    private Elevator.Elevator _elevator;
    private Phone.Phone _phone;

    private Sprite _yetiTestSprite;

    private Effect _grayscaleEffect;
    private EffectParameter _grayscaleIntensity;
    
    public MainGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Window.AllowUserResizing = true;

        Graphics.PreferredBackBufferWidth = 1920;
        Graphics.PreferredBackBufferHeight = 1080;

        Window.Position = new((GraphicsDevice.DisplayMode.Width - Graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - Graphics.PreferredBackBufferHeight) / 2);

        Graphics.ApplyChanges();

        _actualWindowSize = new(
            Graphics.PreferredBackBufferWidth,
            Graphics.PreferredBackBufferHeight
        );

        ContentLoader.Initialize(Content);
        
        FmodController.Init();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        
        // NOTE: You HAVE TO init fmod in the Initialize().
        // Otherwise, it may not work on some platforms.
        FmodController.LoadContent("audio/banks/Desktop", true, ["Master"], ["Master"]);
        
        PixelTexture = new(GraphicsDevice, 1, 1);
        PixelTexture.SetData([Color.White]);

        _renderTarget = new RenderTarget2D(GraphicsDevice, 240, 135);
        _gameSceneRt = new RenderTarget2D(GraphicsDevice, 240, 135);
        _uiRt = new RenderTarget2D(GraphicsDevice, 240, 135);
        
        _elevator = new();
        _elevator.LoadContent();

        _phone = new(_elevator);

        _yetiTestSprite =
            ContentLoader.Load<AsepriteFile>("graphics/concepting/YetiRoom")!.CreateSprite(GraphicsDevice, 0, true);
        
        _grayscaleEffect =
            Content.Load<Effect>("shaders/grayscale")!;
        _grayscaleIntensity = _grayscaleEffect.Parameters["GrayscaleIntensity"];
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.InputDisabled = !IsActive;

        InputManager.RefreshKeyboardState();
        InputManager.RefreshMouseState();
        InputManager.RefreshGamePadState();

        InputManager.UpdateTypingInput(gameTime);

        if (InputManager.GetPressed(Keys.P))
        {
            var guid = Guid.Parse("d0e4a213-d503-4267-bff8-a624210d5868");
            var audioInstance = StudioSystem.GetEvent("event:/SFX/Elevator/Bell/Double").CreateInstance();
            audioInstance.Start();
            audioInstance.Volume = 1f;
            audioInstance.Dispose();
        }

        if(InputManager.GetPressed(Buttons.Start) || InputManager.GetPressed(Keys.Escape))
            Exit();

        if(InputManager.GetPressed(Keys.F11) && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if(_isFullscreen)
            {
                Graphics.PreferredBackBufferWidth = _actualWindowSize.X;
                Graphics.PreferredBackBufferHeight = _actualWindowSize.Y;
                Window.Position = new((GraphicsDevice.DisplayMode.Width - Graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - Graphics.PreferredBackBufferHeight) / 2);
                Window.IsBorderless = false;
                Graphics.ApplyChanges();
            }
            else
            {
                _actualWindowSize.X = Graphics.PreferredBackBufferWidth;
                _actualWindowSize.Y = Graphics.PreferredBackBufferHeight;

                Graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
                Graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
                Window.IsBorderless = true;
                Graphics.ApplyChanges();
            }

            _isFullscreen = !_isFullscreen;
        }

        Coroutines.Update();

        Camera.Update();

        _elevator.Update(gameTime);
        _phone.Update(gameTime);

        // TODO: Add your update logic here

        base.Update(gameTime);
        
        Step++;
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_gameSceneRt);
        GraphicsDevice.Clear(new Color(new Vector3(120, 105, 196)));
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform);
        {
            _yetiTestSprite.Draw(SpriteBatch, Camera.GetParallaxPosition(Vector2.Zero, 50));
            _elevator.Draw(SpriteBatch);
        }
        SpriteBatch.End();
        
        GraphicsDevice.SetRenderTarget(_uiRt);
        GraphicsDevice.Clear(Color.Transparent);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform);
        {
            _phone.Draw(SpriteBatch);
        }
        SpriteBatch.End();
        GraphicsDevice.Reset();
        
        RtScreen.DrawWithRtOnScreen(_renderTarget, Graphics, SpriteBatch, () =>
        {
            _grayscaleIntensity.SetValue(GrayscaleCoeff);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, effect: _grayscaleEffect);
            {
                SpriteBatch.Draw(_gameSceneRt, Vector2.Zero, Color.White);
            }
            SpriteBatch.End();
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            {
                SpriteBatch.Draw(_uiRt, Vector2.Zero, Color.White);
            }
            SpriteBatch.End();
        });

        base.Draw(gameTime);
        
        Frame++;
    }
}
