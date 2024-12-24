using System;
using System.Runtime.InteropServices;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Processors;
using ElevatorGame.Source;
using ElevatorGame.Source.Rooms;
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
using Dialog = ElevatorGame.Source.Dialog;

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

    private Elevator.Elevator _elevator;
    private Phone.Phone _phone;
    private Dialog.Dialog _dialog;
    
    private RoomRenderer _roomRenderer;
    
    private Sprite _yetiIdle;
    private Sprite _yetiPeace;

    private Effect _elevatorEffects;
    private Effect _postProcessingEffects;
    private EffectParameter _elevatorGameTime;
    private EffectParameter _elevatorGrayscaleIntensity;
    private EffectParameter _ppGameTime;
    private EffectParameter _ppWobbleInfluence;
    
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
        FmodController.LoadContent("audio/banks/Desktop", true, ["Master", "SFX", "Music"], ["Master"]);
        
        RenderPipeline.LoadContent(GraphicsDevice);
        
        PixelTexture = new(GraphicsDevice, 1, 1);
        PixelTexture.SetData([Color.White]);
        
        _elevator = new(OnChangeFloorNumber);
        _elevator.LoadContent();

        _phone = new(_elevator);

        _dialog = new();
        _dialog.LoadContent();

        _roomRenderer = new RoomRenderer();
        
        var yetiSpriteFile = ContentLoader.Load<AsepriteFile>("graphics/characters/Yeti")!;
        _yetiIdle = yetiSpriteFile.CreateSprite(GraphicsDevice, 0, true);
        _yetiPeace = yetiSpriteFile.CreateSprite(GraphicsDevice, 1, true);
        
        _elevatorEffects =
            Content.Load<Effect>("shaders/elevatoreffects")!;
        _elevatorGrayscaleIntensity = _elevatorEffects.Parameters["GrayscaleIntensity"];
        _elevatorGameTime = _elevatorEffects.Parameters["GameTime"];
        
        _postProcessingEffects =
            Content.Load<Effect>("shaders/postprocessing")!;
        _ppWobbleInfluence = _postProcessingEffects.Parameters["WobbleInfluence"];
        _ppGameTime = _postProcessingEffects.Parameters["GameTime"];
    }
    
    protected override void UnloadContent()
    {
        FmodManager.Unload();
        
        _elevator.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        FmodManager.Update();
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

        if(InputManager.GetPressed(Keys.D))
        {
            if(!Coroutines.IsRunning("dialog"))
            {
                Coroutines.Run("dialog", _dialog.Display(
                    [
                        new() {
                            Content = "Hello everybody, my name is Markiplier and welcome to Five Nights at Freddy's, an indie horror game that you guys suggested,"
                        },
                        new() {
                            Content = "en masse,"
                        },
                        new() {
                            Content = "and I saw that Yamimash played it and he said it was really really good..."
                        },
                        new() {
                            Content = "So I'm very eager to see what is up."
                        }
                    ],
                    Dialog.Dialog.DisplayMethod.Alien
                ), 0);
            }
        }

        // TODO: Add your update logic here

        base.Update(gameTime);
        
        Step++;
    }

    protected override void Draw(GameTime gameTime)
    {
        _elevatorGrayscaleIntensity.SetValue(GrayscaleCoeff);
        // _elevatorGameTime.SetValue(Frame / 60f);
        _ppWobbleInfluence.SetValue(0);
        _ppGameTime.SetValue(Frame / 60f);
        
        _roomRenderer.PreRender(SpriteBatch);
        
        RenderPipeline.DrawBeforeUI(SpriteBatch, GraphicsDevice, _elevatorEffects, () =>
        {
            GraphicsDevice.Clear(new Color(new Vector3(120, 105, 196)));
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform);
            {
                DrawScene(gameTime);
            }
            SpriteBatch.End();
        });
        
        RenderPipeline.DrawUI(SpriteBatch, GraphicsDevice, () =>
        {
            GraphicsDevice.Clear(Color.Transparent);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform);
            {
                _phone.Draw(SpriteBatch);
            }
            SpriteBatch.End();
        });
        
        RenderPipeline.DrawPostProcess(SpriteBatch, GraphicsDevice, _postProcessingEffects);
        RenderPipeline.DrawFinish(SpriteBatch, Graphics);

        base.Draw(gameTime);
        
        Frame++;
    }

    private void DrawScene(GameTime gameTime)
    {
        _roomRenderer.Draw(SpriteBatch);
        // _yetiIdle.Draw(SpriteBatch, Camera.GetParallaxPosition(new(80, 40), 50));
        _elevator.Draw(SpriteBatch);
        _dialog.Draw(SpriteBatch);
    }
    
    private void OnChangeFloorNumber(int floorNumber)
    {
        _roomRenderer.Randomize();
    }
}
