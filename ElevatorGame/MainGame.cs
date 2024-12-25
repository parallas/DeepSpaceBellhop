using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Processors;
using ElevatorGame.Source;
using ElevatorGame.Source.Characters;
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
    public static readonly Point RenderBufferSize = new Point(240, 135);

    public static long Step { get; private set; }
    public static long Frame { get; private set; }

    public static Texture2D PixelTexture { get; private set; }

    public static Camera Camera { get; } = new()
    {
        RootOffset = Vector2.One * 8
    };

    public static Vector2 CameraPosition { get; set; }

    public static Vector2 ScreenPosition => Vector2.Round(Camera.Position) + Vector2.One * 8;

    public static CoroutineRunner Coroutines { get; set; } = new();

    public static int CurrentFloor { get; set; } = 1;

    public static float GrayscaleCoeff { get; set; } = 1;

    public static Cursor Cursor { get; private set; }

    private static Point _actualWindowSize;
    private static bool _isFullscreen;

    private Elevator.Elevator _elevator;
    private Phone.Phone _phone;
    private Dialog.Dialog _dialog;

    private List<RoomDef> _roomDefs = [];
    private RoomRenderer _roomRenderer;

    private Sprite _yetiIdle;
    private Sprite _yetiPeace;

    private readonly List<CharacterActor> _waitList = [];
    private readonly List<CharacterActor> _cabList = [];

    private Effect _elevatorEffects;
    private Effect _postProcessingEffects;
    private EffectParameter _elevatorGameTime;
    private EffectParameter _elevatorGrayscaleIntensity;
    private EffectParameter _ppGameTime;
    private EffectParameter _ppWobbleInfluence;

    public static readonly Rectangle GameBounds = new(8, 8, 240, 135);

    public MainGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
    }

    protected override void Initialize()
    {
        RenderPipeline.Init(RenderBufferSize);

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

        _elevator = new(OnChangeFloorNumber, EndOfTurnSequence);
        _elevator.LoadContent();

        _phone = new(_elevator);
        _phone.LoadContent();

        _dialog = new();
        _dialog.LoadContent();

        for (int i = 0; i < Elevator.Elevator.MaxFloors; i++)
        {
            var newRoomDef = RoomDef.MakeRandom("graphics/RoomsGeneric");
            _roomDefs.Add(newRoomDef);
        }
        _roomRenderer = new RoomRenderer();
        _roomRenderer.LoadContent();
        _roomRenderer.SetDefinition(_roomDefs[0]);

        var yetiSpriteFile = ContentLoader.Load<AsepriteFile>("graphics/characters/Yeti")!;
        _yetiIdle = yetiSpriteFile.CreateSprite(GraphicsDevice, 0, true);
        _yetiPeace = yetiSpriteFile.CreateSprite(GraphicsDevice, 1, true);

        Cursor = new();
        Cursor.LoadContent();

        _elevatorEffects =
            Content.Load<Effect>("shaders/elevatoreffects")!;
        _elevatorGrayscaleIntensity = _elevatorEffects.Parameters["GrayscaleIntensity"];
        _elevatorGameTime = _elevatorEffects.Parameters["GameTime"];

        _postProcessingEffects =
            Content.Load<Effect>("shaders/postprocessing")!;
        _ppWobbleInfluence = _postProcessingEffects.Parameters["WobbleInfluence"];
        _ppGameTime = _postProcessingEffects.Parameters["GameTime"];

        CharacterRegistry.Init();
        foreach (var characterTableValue in CharacterRegistry.CharacterTable.Values)
        {
            var newCharacter = new CharacterActor
            {
                Def = characterTableValue,
                FloorNumberCurrent = Random.Shared.Next(2, Elevator.Elevator.MaxFloors + 1),
                Patience = 5,
                OffsetXTarget = Random.Shared.Next(-48, 49)
            };
            do
            {
                newCharacter.FloorNumberTarget = Random.Shared.Next(1, Elevator.Elevator.MaxFloors + 1);
            } while (newCharacter.FloorNumberTarget == newCharacter.FloorNumberCurrent);

            _phone.AddOrder(newCharacter);
            
            Console.WriteLine(
                $"{characterTableValue.Name} is going from {newCharacter.FloorNumberCurrent} to {newCharacter.FloorNumberTarget}");
            newCharacter.LoadContent();
            _waitList.Add(newCharacter);
        }
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

        if(Keybindings.Pause.Pressed)
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

        var mousePos =
            Vector2.Floor(
                RtScreen.ToScreenSpace(
                    InputManager.MousePosition.ToVector2(),
                    RenderBufferSize,
                    GraphicsDevice
                )
            );
        Camera.Position =
            CameraPosition + (
                Vector2.Clamp(
                    mousePos,
                    Vector2.Zero,
                    new(240, 135)
                ) - new Vector2(240, 135)/2f
            ) * (8/120f);

        Camera.Update();

        _elevator.Update(gameTime);
        _phone.Update(gameTime);

        foreach (var characterActor in _waitList)
        {
            characterActor.Update(gameTime);
        }
        foreach (var characterActor in _cabList)
        {
            characterActor.Update(gameTime);
        }

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
        _phone.PreRenderScreen(SpriteBatch);

        RenderPipeline.DrawBeforeUI(SpriteBatch, GraphicsDevice, _elevatorEffects, () =>
        {
            GraphicsDevice.Clear(new Color(new Vector3(120, 105, 196)));
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform);
            {
                DrawScene(gameTime);
            }
            SpriteBatch.End();
        });

        var mousePos =
            Vector2.Floor(
                RtScreen.ToScreenSpace(
                    InputManager.MousePosition.ToVector2(),
                    RenderBufferSize,
                    GraphicsDevice
                )
            );
        RenderPipeline.DrawUI(SpriteBatch, GraphicsDevice, () =>
        {
            GraphicsDevice.Clear(Color.Transparent);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            {
                _phone.Draw(SpriteBatch);

                _dialog.Draw(SpriteBatch);

                Cursor.Draw(SpriteBatch);
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

        foreach (var characterActor in _waitList)
        {
            characterActor.Draw(SpriteBatch);
        }

        _elevator.Draw(SpriteBatch);

        foreach (var characterActor in _cabList)
        {
            characterActor.Draw(SpriteBatch);
        }
    }

    private void OnChangeFloorNumber(int floorNumber)
    {
        CurrentFloor = floorNumber;
        _roomRenderer.SetDefinition(_roomDefs[floorNumber - 1]);
    }

    private IEnumerator EndOfTurnSequence()
    {
        // If anyone is going to this floor, they leave one at a time
        // Subtract patience from remaining passengers
        // Any passengers with patience <= 0 leave
        // Any passengers getting on this floor get on

        Coroutines.TryRun("phone_show", _phone.Open(false), out _);

        for (int index = 0; index < _cabList.Count; index++)
        {
            var characterActor = _cabList[index];
            if (characterActor.FloorNumberTarget == CurrentFloor)
            {
                yield return characterActor.GetOffElevatorBegin();
                _cabList.Remove(characterActor);
                index--;
                _waitList.Add(characterActor);
                yield return _dialog.Display(characterActor.Def.ExitPhrases[0].Pages,
                    Dialog.Dialog.DisplayMethod.Human);

                yield return characterActor.GetOffElevatorEnd();

                _waitList.Remove(characterActor);
            }
        }

        for (var index = _waitList.Count - 1; index >= 0; index--)
        {
            var characterActor = _waitList[index];
            if (characterActor.FloorNumberCurrent == CurrentFloor)
            {
                yield return characterActor.GetInElevatorBegin();
                _waitList.Remove(characterActor);
                _phone.RemoveOrder(characterActor);
                _cabList.Add(characterActor);
                yield return _dialog.Display(characterActor.Def.EnterPhrases[0].Pages,
                    Dialog.Dialog.DisplayMethod.Human);

                yield return characterActor.GetInElevatorEnd();
            }
        }
        
        Coroutines.TryRun("phone_hide", _phone.Close(false), out _);

        yield return null;
    }
}
