using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Processors;
using ElevatorGame.Source;
using ElevatorGame.Source.Characters;
using ElevatorGame.Source.Rooms;
using ElevatorGame.Source.Tickets;
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

    public enum Menus
    {
        None,
        Phone,
        Dialog,
        Tickets,
        DayTransition,
        TurnTransition,
    }

    public static Menus CurrentMenu { get; set; }

    private static Point _actualWindowSize;
    private static bool _isFullscreen;

    private static Elevator.Elevator _elevator;
    private Phone.Phone _phone;
    private Dialog.Dialog _dialog;

    private TicketManager _ticketManager;

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

    private float _fadeoutProgress;

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

        _ticketManager = new TicketManager(_elevator);
        _ticketManager.LoadContent();

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
        for (int i = 0; i < 5; i++)
        {
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

    }

    protected override void UnloadContent()
    {
        FmodManager.Unload();

        _elevator.UnloadContent();
        _phone.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        FmodManager.Update();
        InputManager.InputDisabled = !IsActive;

        InputManager.RefreshKeyboardState();
        InputManager.RefreshMouseState();
        InputManager.RefreshGamePadState();

        InputManager.UpdateTypingInput(gameTime);

        Cursor.Update();

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

        if(InputManager.GetPressed(Keys.Y))
        {
            Coroutines.Stop("main_day_advance");
            Coroutines.TryRun("main_day_advance", AdvanceDay(), out _);
        }

        _elevator.Update(gameTime);
        _ticketManager.Update(gameTime);
        _phone.Update(gameTime);

        foreach (var characterActor in _waitList)
        {
            characterActor.Update(gameTime);
        }
        foreach (var characterActor in _cabList)
        {
            if (_cabList.Count <= 10)
            {
                var hitPerson = _cabList.Find((actor =>
                    actor != characterActor &&
                    MathUtil.Approximately(actor.OffsetXTarget, characterActor.OffsetXTarget,
                        MathHelper.Lerp(32, 16, _cabList.Count / 10f))));
                if (hitPerson is not null)
                {
                    var dir = Math.Sign(characterActor.OffsetXTarget - hitPerson.OffsetXTarget);
                    if (dir == 0) dir = 1;
                    var target = characterActor.OffsetXTarget + dir;
                    characterActor.OffsetXTarget = Math.Clamp(target, -CharacterActor.StandingRoomSize,
                        CharacterActor.StandingRoomSize);
                }
            }
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

        RenderPipeline.DrawUI(SpriteBatch, GraphicsDevice, () =>
        {
            GraphicsDevice.Clear(Color.Transparent);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            {
                _phone.Draw(SpriteBatch);

                _ticketManager.Draw(SpriteBatch);

                _dialog.Draw(SpriteBatch);

                Cursor.Draw(SpriteBatch);

                SpriteBatch.Draw(PixelTexture, GameBounds with { X = 0, Y = 0 }, Color.Black * _fadeoutProgress);
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

        for (var i = 0; i < _cabList.Count; i++)
        {
            var characterActor = _cabList[i];
            characterActor.Draw(SpriteBatch, i);
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

        CurrentMenu = Menus.TurnTransition;

        for (int index = 0; index < _cabList.Count; index++)
        {
            var characterActor = _cabList[index];
            if (characterActor.FloorNumberTarget == CurrentFloor)
            {
                _cabList.Remove(characterActor);
                _cabList.Add(characterActor);
                yield return characterActor.GetOffElevatorBegin();
                Coroutines.Stop("ticket_remove");
                Coroutines.TryRun("ticket_remove", _ticketManager.RemoveTicket(characterActor.FloorNumberTarget), out _);
                yield return _dialog.Display(characterActor.Def.ExitPhrases[0].Pages,
                    Dialog.Dialog.DisplayMethod.Human);

                _cabList.Remove(characterActor);
                index--;
                _waitList.Add(characterActor);
                yield return characterActor.GetOffElevatorEnd();

                _waitList.Remove(characterActor);
            }
        }

        for (var index = _waitList.Count - 1; index >= 0; index--)
        {
            var characterActor = _waitList[index];
            if (characterActor.FloorNumberCurrent == CurrentFloor)
            {
                Coroutines.TryRun("phone_show", _phone.Open(false, false), out _);
                _phone.CanOpen = false;
                // _cabList.ForEach(actor => actor.MoveOutOfTheWay());
                yield return characterActor.GetInElevatorBegin();
                _waitList.Remove(characterActor);
                _phone.HighlightOrder(characterActor);
                _ticketManager.AddTicket(characterActor.FloorNumberTarget);
                _cabList.Add(characterActor);
                yield return _dialog.Display(characterActor.Def.EnterPhrases[0].Pages,
                    Dialog.Dialog.DisplayMethod.Human);
                yield return _phone.RemoveOrder(characterActor);

                yield return characterActor.GetInElevatorEnd();
            }
        }

        _phone.CanOpen = true;
        Coroutines.Stop("phone_show");
        Coroutines.TryRun("phone_hide", _phone.Close(false), out _);

        yield return null;

        CurrentMenu = Menus.None;
    }

    private IEnumerator AdvanceDay()
    {
        yield return FadeToBlack();

        yield return 10;

        // the rest of the cleanup process

        yield return FadeFromBlack();
    }

    private IEnumerator FadeToBlack()
    {
        _fadeoutProgress = 0;
        while(!MathUtil.Approximately(_fadeoutProgress, 1, 0.03f))
        {
            _fadeoutProgress = MathUtil.ExpDecay(_fadeoutProgress, 1, 5, 1f/60f);
            yield return null;
        }
        _fadeoutProgress = 1;
    }

    private IEnumerator FadeFromBlack()
    {
        _fadeoutProgress = 1;
        while(!MathUtil.Approximately(_fadeoutProgress, 0, 0.03f))
        {
            _fadeoutProgress = MathUtil.ExpDecay(_fadeoutProgress, 0, 5, 1f/60f);
            yield return null;
        }
        _fadeoutProgress = 0;
    }
    
    public static Vector2 GetCursorParallaxValue(Vector2 position,  float distance)
    {
        var mousePos =
            Vector2.Floor(
                RtScreen.ToScreenSpace(
                    InputManager.MousePosition.ToVector2(),
                    RenderBufferSize,
                    Graphics.GraphicsDevice
                ) 
            );

        Vector2 checkPos = (
            Vector2.Clamp(
                mousePos,
                Vector2.Zero,
                new(240, 135)
            ) - new Vector2(240, 135) / 2f
        ) * (8 / 120f);
        return position + Vector2.Round(checkPos * MathUtil.InverseLerp01(0, 100, distance));
    }
}
