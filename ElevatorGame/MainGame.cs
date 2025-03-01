﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Processors;
using ElevatorGame.Source;
using ElevatorGame.Source.BG_Characters;
using ElevatorGame.Source.Characters;
using ElevatorGame.Source.Days;
using ElevatorGame.Source.GameOver;
using ElevatorGame.Source.Pause;
using ElevatorGame.Source.Rooms;
using ElevatorGame.Source.Tickets;
using Engine;
using Engine.Display;
using FMOD;
using FmodForFoxes;
using FmodForFoxes.Studio;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Aseprite;
using MonoGame.ImGuiNet;
using Elevator = ElevatorGame.Source.Elevator;
using Phone = ElevatorGame.Source.Phone;
using Dialog = ElevatorGame.Source.Dialog;
using ElevatorGame.Source.MainMenu;
using TinyTween;
using ElevatorGame.Source.Intro;
using ElevatorGame.Source.Dialog;

namespace ElevatorGame;

public class MainGame : Game
{
    private static MainGame _instance;
    public static GraphicsDeviceManager Graphics { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static ImGuiRenderer GuiRenderer { get; private set; }
    public bool ShowDebug { get; private set; }
    public static readonly Point RenderBufferSize = new Point(240, 135);

    public static long Step { get; private set; }
    public static long Frame { get; private set; }

    public static Texture2D PixelTexture { get; private set; }
    public static Texture2D OutlineTexture { get; private set; }

    public static Camera Camera { get; } = new()
    {
        RootOffset = Vector2.One * 8
    };

    public static Vector2 CameraPosition { get; set; }
    public static Vector2 CameraPositionTarget { get; set; }

    public static Vector2 ScreenPosition => Vector2.Round(Camera.Position) + Vector2.One * 8;

    public static CoroutineRunner Coroutines { get; set; } = new();

    public static int CurrentDay { get; private set; } = 0;
    public static int Turn { get; private set; } = 0;
    public static int FloorCount => DayRegistry.Days[CurrentDay].FloorCount;
    public static int CompletionRequirement => DayRegistry.Days[CurrentDay].CompletionRequirement;
    public static float SpawnChance => DayRegistry.Days[CurrentDay].OrderSpawnChancePerTurn;
    public static string[] CharacterIdsPool => DayRegistry.Days[CurrentDay].CharacterIds;
    public static int StartCharacterCount => DayRegistry.Days[CurrentDay].StartCharacterCount;
    public static int MaxCountPerSpawn => DayRegistry.Days[CurrentDay].MaxCountPerSpawn;
    public static int MaxCharacters => DayRegistry.Days[CurrentDay].MaxCharacters;
    public static bool PunishMistakes => DayRegistry.Days[CurrentDay].PunishMistakes;
    public static int CurrentFloor { get; set; } = 1;
    public static int CurrentHealth { get; set; } = 8;
    public static int HealthShield { get; set; } = 0;

    public static float GrayscaleCoeff { get; set; } = 1;
    public static float GrayscaleCoeffTarget { get; set; } = 1;
    public static float WobbleInfluence { get; set; } = 0;
    public static float HueShiftInfleunce { get; set; } = 0;
    public static float FlippyInfluence { get; set; } = 0;

    private static int _wobbleTurns = 0;
    private static int _hueShiftTurns = 0;
    private static int _flippyTurns = 0;

    public static Rectangle ScreenBounds { get; private set; }
    public static Cursor Cursor { get; private set; }

    public static SpriteFont Font { get; private set; }
    public static SpriteFont FontBold { get; private set; }
    public static SpriteFont FontItalic { get; private set; }
    public static SpriteFont FontIntro { get; private set; }

    public enum Menus
    {
        None,
        Phone,
        Dialog,
        Tickets,
        DayTransition,
        TurnTransition,
        MainMenu,
        Intro,
    }

    public static Menus CurrentMenu { get; set; } = Menus.Intro;

    public enum GameStates
    {
        Gameplay,
        MainMenu,
        Intro,
        GameOver,
    }

    public static GameStates GameState { get; set; } = GameStates.Intro;

    public bool EndOfDaySequence { get; private set; }

    private static Rectangle _actualWindowBounds;

    public static bool IsFullscreen { get; private set; }

    public static bool UseSteamworks { get; private set; }

    public static bool HasMadeMistake { get; set; }

    private Elevator.Elevator _elevator;
    private static Phone.Phone _phone;
    private Dialog.Dialog _dialog;

    private TicketManager _ticketManager;

    private static List<RoomDef> _roomDefs = [];
    private RoomRenderer _roomRenderer;

    private BgCharacterRenderer _bgCharacterRenderer;
    private int _comboCount;

    private Sprite _darkOverlaySprite;
    private float _darkOverlayOpacity = 1f;

    private AnimatedSprite _buttonHint;
    private float _buttonHintOpacity;
    private bool _buttonHintVisible;

    public static CharacterManager CharacterManager { get; private set; }

    private Effect _elevatorEffects;
    private Effect _postProcessingEffects;
    private EffectParameter _elevatorGameTime;
    private EffectParameter _elevatorGrayscaleIntensity;
    private EffectParameter _ppGameTime;
    private EffectParameter _ppWobbleInfluence;
    private EffectParameter _ppHueShiftInfluence;
    private EffectParameter _ppFlippyInfluence;

    public static readonly Rectangle GameBounds = new(8, 8, 240, 135);

    private static PauseManager _pauseManager = new();

    private static MainMenu? _mainMenu;
    private static GameOverScreen _gameOverScreen;

    private readonly DayTransition _dayTransition = new DayTransition();
    private static float _fadeoutProgress;
    private static TinyTween.Tween<float> _fadeoutTween = new FloatTween();

    private static Vector2 _lastMouseViewPos;
    private static Vector2 _lastMouseWorldPos;

    private bool _showDevTools;
    private bool _showCharacterList;
    private IntPtr _renderPipelineTextureId;

    private float _previousMasterVolume;

    private static bool _isUsingGamePad;

    public static bool IsUsingGamePad => _isUsingGamePad;

    public static bool SaveFileExists { get; private set; }

    public static event Action<Point> WindowResized;

    public static bool UseNativeCursor { get; set; } = false;

    public MainGame(bool useSteamworks)
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = UseNativeCursor;

        UseSteamworks = useSteamworks;

        _instance = this;
    }

    protected override void Initialize()
    {
        RenderPipeline.Init(RenderBufferSize);
        GuiRenderer = new ImGuiRenderer(this);
        #if DEBUG
        ShowDebug = true;
        IsMouseVisible = true;
        #endif

        Window.AllowUserResizing = true;

        Window.ClientSizeChanged += (object? sender, EventArgs args)
            => WindowResized?.Invoke(Window.ClientBounds.Size);

        Graphics.PreferredBackBufferWidth = 1920;
        Graphics.PreferredBackBufferHeight = 1080;

        Window.Position = new((GraphicsDevice.DisplayMode.Width - Graphics.PreferredBackBufferWidth) / 2, (GraphicsDevice.DisplayMode.Height - Graphics.PreferredBackBufferHeight) / 2);

        Graphics.ApplyChanges();

        if (OperatingSystem.IsWindows())
        {
            SetFullscreen(
                GraphicsDevice.DisplayMode.Width <= 1920 &&
                GraphicsDevice.DisplayMode.Height <= 1080
            );
        }

        SaveFileExists = SaveManager.SaveFileExists;

        SaveManager.OnLoad += OnSaveDataLoad;
        SaveManager.OnSave += OnSaveDataSave;
        SaveManager.OnLoadSettings += OnSettingsLoad;
        SaveManager.OnSaveSettings += OnSettingsSave;

        SaveManager.Load();

        if (UseSteamworks)
        {
            SteamManager.Initialize(3429210u);
        }

        Exiting += Game_Exiting;

        ContentLoader.Initialize(Content);

        FmodController.Init();

        DayRegistry.Init();

        BgCharacterRegistry.Init();

        CharacterRegistry.Init();

        base.Initialize();
    }

    private void OnSaveDataSave(ref SaveData data)
    {
        data.Day = CurrentDay;
        data.Rooms = [.. _roomDefs];
    }

    private void OnSaveDataLoad(ref SaveData data)
    {
        CurrentDay = data.Day;
        _roomDefs = [.. data.Rooms];
    }

    private void OnSettingsSave(ref SettingsData data)
    {
        data.AudioMasterVolume = StudioSystem.GetParameterTargetValue("VolumeMaster");
        data.AudioMusicVolume = StudioSystem.GetParameterTargetValue("VolumeMusic");
        data.AudioSFXVolume = StudioSystem.GetParameterTargetValue("VolumeSounds");

        data.LanguagePreference = LocalizationManager.CurrentLanguage ?? "en-us";

        data.LcdEffect = RenderPipeline.MaskBlend;
        data.FrameBlending = RenderPipeline.FrameBlend;

        data.UseNativeCursor = UseNativeCursor;
    }

    private void OnSettingsLoad(ref SettingsData data)
    {
        StudioSystem.SetParameterValue("VolumeMaster", data.AudioMasterVolume);
        StudioSystem.SetParameterValue("VolumeMusic", data.AudioMusicVolume);
        StudioSystem.SetParameterValue("VolumeSounds", data.AudioSFXVolume);

        LocalizationManager.CurrentLanguage = data.LanguagePreference;

        RenderPipeline.MaskBlend = data.LcdEffect;
        RenderPipeline.FrameBlend = data.FrameBlending;

        UseNativeCursor = data.UseNativeCursor;
    }

    protected override void LoadContent()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        GuiRenderer.RebuildFontAtlas();

        // NOTE: You HAVE TO init fmod in the Initialize().
        // Otherwise, it may not work on some platforms.
        FmodController.LoadContent("audio/banks/Desktop", true, ["Master", "SFX", "Music"], ["Master"]);
        MusicPlayer.RegisterEventGuid("MainMenuOld", "{3c9a1f7e-ccbd-4b36-b879-da149caab4c0}");
        MusicPlayer.RegisterEventGuid("MainMenu", "{7bbbc8c1-14ab-488e-9f28-b2854ec7fcd3}");
        MusicPlayer.RegisterEventGuid("Intro", "{e2958500-0c4e-4c30-bf7d-66afe77dd0f1}");
        MusicPlayer.RegisterEventGuid("Day1", "{6cb39cba-ca9b-459e-ba30-e19398c5536d}");
        MusicPlayer.RegisterEventGuid("Day2", "{1750806a-4d75-4285-8898-3c846fcbccf0}");
        MusicPlayer.RegisterEventGuid("Day3", "{bffc9aa0-3e77-4e1e-8c56-2ef5ade92602}");
        MusicPlayer.RegisterEventGuid("Day4", "{0db5b5ff-da7b-46a4-b2c6-8b39c311857f}");

        SaveManager.LoadSettings();
        CharacterRegistry.RefreshData();

        RenderPipeline.LoadContent(GraphicsDevice);

        PixelTexture = new(GraphicsDevice, 1, 1);
        PixelTexture.SetData([Color.White]);

        OutlineTexture = new(GraphicsDevice, 3, 3);
        OutlineTexture.SetData([
            Color.White,        Color.White,        Color.White,
            Color.White,        Color.Transparent,  Color.White,
            Color.White,        Color.White,        Color.White
        ]);

        // _elevator = new(OnChangeFloorNumber, EndOfTurnSequence, ElevatorCrashed);
        // _elevator.LoadContent();

        // _phone = new(_elevator);
        // _phone.LoadContent();

        // _ticketManager = new TicketManager(_elevator);
        // _ticketManager.LoadContent();

        // _dialog = new();
        // _dialog.LoadContent();

        // CharacterManager = new CharacterManager(_phone, _ticketManager, _dialog, _elevator);
        // // CharacterManager.Init();
        // CharacterManager.LoadContent();

        var darkOverlayFile = ContentLoader.Load<AsepriteFile>("graphics/ElevatorDarkOverlay");
        _darkOverlaySprite = darkOverlayFile!.CreateSprite(GraphicsDevice, 0, true);
        _darkOverlaySprite.OriginX = 64;

        _buttonHint = ContentLoader.Load<AsepriteFile>("graphics/ButtonHint")
            .CreateSpriteSheet(GraphicsDevice, false)
            .CreateAnimatedSprite("MoveElevator");
        _buttonHint.Origin = new(19, 5);

        Cursor = new();
        Cursor.LoadContent();

        _elevatorEffects =
            Content.Load<Effect>("shaders/elevatoreffects")!;
        _elevatorGrayscaleIntensity = _elevatorEffects.Parameters["GrayscaleIntensity"];
        _elevatorGameTime = _elevatorEffects.Parameters["GameTime"];

        _postProcessingEffects =
            Content.Load<Effect>("shaders/postprocessing")!;
        _ppWobbleInfluence = _postProcessingEffects.Parameters["WobbleInfluence"];
        _ppHueShiftInfluence = _postProcessingEffects.Parameters["HueShiftInfluence"];
        _ppFlippyInfluence = _postProcessingEffects.Parameters["FlippyInfluence"];
        _ppGameTime = _postProcessingEffects.Parameters["GameTime"];

        _dayTransition.LoadContent();

        _pauseManager.ExitGame = Exit;
        _pauseManager.OpenMainMenu = CreateMainMenu;
        _pauseManager.LoadContent();

        _gameOverScreen = new GameOverScreen();
        _gameOverScreen.LoadContent(GraphicsDevice);

        Font = ContentLoader.Load<SpriteFont>("fonts/default");
        FontBold = ContentLoader.Load<SpriteFont>("fonts/defaultBold");
        FontItalic = ContentLoader.Load<SpriteFont>("fonts/defaultItalic");
        FontIntro = ContentLoader.Load<SpriteFont>("fonts/intro");

        _roomRenderer = new RoomRenderer();
        _roomRenderer.LoadContent();

        BgCharacterRegistry.LoadContent();
        _bgCharacterRenderer = new BgCharacterRenderer();

        Intro.LoadContent();
        MusicPlayer.PlayMusic("MainMenu");

        Coroutines.TryRun("main_intro", DoIntro(), out _);
    }

    private IEnumerator DoIntro()
    {
        yield return Intro.RunSequence();

        CreateMainMenu();
    }

    protected override void UnloadContent()
    {
        FmodManager.Unload();
        MusicPlayer.UnloadContent();

        _elevator?.UnloadContent();
        _phone?.UnloadContent();

        RtScreen.UnloadContent();
    }

    private void Game_Exiting(object sender, ExitingEventArgs e)
    {
        if (GameState == GameStates.Gameplay)
            SaveManager.Save();

        if (UseSteamworks)
            SteamManager.Cleanup();
    }

    protected override void Update(GameTime gameTime)
    {
        // Static Properties
        ScreenBounds = GraphicsDevice.PresentationParameters.Bounds;

        // Update Systems
        FmodManager.Update();

        // Update Input
        UpdateInput(gameTime);

        HandleToggleFullscreen();

        DebugUpdate();

        IsMouseVisible = UseNativeCursor || ShowDebug;

        if (UseSteamworks)
            SteamManager.Update();

        Coroutines.Update();
        if (_fadeoutTween.State == TweenState.Running)
        {
            _fadeoutTween.Update(1f / 60f);
            _fadeoutProgress = _fadeoutTween.CurrentValue;
        }

        if (GameState == GameStates.MainMenu)
        {
            _mainMenu?.Update();
            base.Update(gameTime);
            return;
        }
        else if (GameState == GameStates.Intro)
        {
            Intro.Update();
            if(Keybindings.Confirm.Pressed || Keybindings.GoBack.Pressed)
            {
                Coroutines.StopAll();
                CreateMainMenu();
                return;
            }
            else
            {
                base.Update(gameTime);
                return;
            }
        }
        else if (GameState == GameStates.GameOver)
        {
            _gameOverScreen.Update();

            Camera.Position =
                CameraPosition + Cursor.TiltOffset;

            CameraPosition = MathUtil.ExpDecay(CameraPosition, CameraPositionTarget, 8f, 1f / 60f);
            Camera.Update();

            return;
        }

        if (Keybindings.Pause.Pressed && CurrentMenu != Menus.DayTransition && GameState == GameStates.Gameplay)
        {
            _pauseManager.Pause();
        }
        _pauseManager.Update(gameTime);

        if (_pauseManager.IsPaused)
        {
            base.Update(gameTime);
            return;
        }

        // Tilt camera towards cursor (should be an option to disable)
        Camera.Position =
            CameraPosition + Cursor.TiltOffset;

        _elevator.Update(gameTime);
        _ticketManager.Update(gameTime);
        _phone.Update(gameTime);

        _bgCharacterRenderer.Update(gameTime);

        CharacterManager.Update(gameTime);

        _dayTransition.Update(gameTime);

        CameraPosition = MathUtil.ExpDecay(CameraPosition, CameraPositionTarget, 8f, 1f / 60f);
        Camera.Update();

        float darkOverlayOpacityTarget = 0.25f;
        if(EndOfDaySequence)
        {
            darkOverlayOpacityTarget = 1f;
        }
        else if ((float)(CharacterManager.CharactersFinished + CharacterManager.CacheCharactersInPlay.Count) / CompletionRequirement >= 0.8f)
        {
            darkOverlayOpacityTarget = 0.5f;
        }
        _darkOverlayOpacity = MathUtil.ExpDecay(_darkOverlayOpacity, darkOverlayOpacityTarget, 2f, 1f/60f);
        _darkOverlaySprite.Color = Color.White * _darkOverlayOpacity;

        if (CurrentDay == 0)
        {
            if(Turn >= 2)
                _buttonHintVisible = false;

            _buttonHintOpacity = MathUtil.Approach(_buttonHintOpacity, _buttonHintVisible ? 1 : 0, 0.1f);
        }
        _buttonHint.Color = Color.White * _buttonHintOpacity;
        _buttonHint.Update(1f / 60f);

        base.Update(gameTime);

        Step++;
    }

    protected override void Draw(GameTime gameTime)
    {
        UpdateShaderProperties();

        _roomRenderer?.PreRender(SpriteBatch);
        _phone?.PreRenderScreen(SpriteBatch);
        _dayTransition?.PreDraw(SpriteBatch);
        _pauseManager?.PreDraw(SpriteBatch);
        _mainMenu?.PreDraw(SpriteBatch);
        Cursor.PreDraw(SpriteBatch);

        if(GameState == GameStates.Intro)
        {
            Intro.PreDraw(SpriteBatch);
        }

        RenderPipeline.DrawBeforeUI(SpriteBatch, GraphicsDevice, _elevatorEffects, () =>
        {
            GraphicsDevice.Clear(new Color(new Vector3(120, 105, 196)));
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Camera.Transform);
            {
                switch (GameState)
                {
                    case GameStates.Gameplay:
                        DrawScene(SpriteBatch);
                        break;
                    case GameStates.MainMenu:
                        break;
                    case GameStates.Intro:
                        break;
                    case GameStates.GameOver:
                        _gameOverScreen.Draw(SpriteBatch);
                        break;
                }
            }
            SpriteBatch.End();
        });

        RenderPipeline.DrawUI(SpriteBatch, GraphicsDevice, () =>
        {
            GraphicsDevice.Clear(Color.Transparent);
            SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            {
                switch (GameState)
                {
                    case GameStates.Gameplay:
                        DrawUI(SpriteBatch);
                        break;
                    case GameStates.MainMenu:
                        _mainMenu?.Draw(SpriteBatch);
                        DrawScreenTransition(SpriteBatch);
                        if (!_isUsingGamePad && !ShowDebug)
                            Cursor.Draw(SpriteBatch);
                        break;
                    case GameStates.Intro:
                        Intro.Draw(SpriteBatch);
                        break;
                }
            }
            SpriteBatch.End();
        });

        RenderPipeline.DrawPostProcess(SpriteBatch, GraphicsDevice, _postProcessingEffects);
        RenderPipeline.DrawFinish(SpriteBatch, Graphics);

        base.Draw(gameTime);

        if (ShowDebug) DrawImGui(gameTime);

        Frame++;
    }

    private void DrawScene(SpriteBatch spriteBatch)
    {
        _roomRenderer.Draw(spriteBatch);

        _bgCharacterRenderer.Draw(spriteBatch);

        CharacterManager.DrawWaiting(spriteBatch);

        _elevator.Draw(spriteBatch);

        CharacterManager.DrawMain(spriteBatch);

        _darkOverlaySprite.Draw(spriteBatch, Camera.GetParallaxPosition(Vector2.Zero, 0));
    }

    private void DrawUI(SpriteBatch spriteBatch)
    {
        _phone?.Draw(spriteBatch);

        _ticketManager?.Draw(spriteBatch);

        spriteBatch.DrawStringSpacesFix(
            FontIntro,
            string.Format(LocalizationManager.Get("ui.gameplay.turn_counter"), Turn + 1),
            Vector2.UnitX * 2,
            Color.White,
            6
        );

        _dialog?.Draw(spriteBatch);

        _buttonHint?.Draw(spriteBatch, new(GameBounds.Width / 2, GameBounds.Height - 20 - (_buttonHintOpacity * 4)));

        _pauseManager?.Draw(spriteBatch);

        DrawScreenTransition(spriteBatch);

        if (!_isUsingGamePad && !ShowDebug)
            Cursor.Draw(spriteBatch);
    }

    private void DrawImGui(GameTime gameTime)
    {
        _renderPipelineTextureId = GuiRenderer.BindTexture(RenderPipeline.RenderTarget);

        GuiRenderer.BeginLayout(gameTime);
        {
            ImGui.BeginMainMenuBar();
            {
                if (ImGui.Button("Dev Tools"))
                {
                    _showDevTools = !_showDevTools;
                }

                if (ImGui.BeginMenu("Load Day", enabled: GameState == GameStates.Gameplay))
                {
                    for (int i = 0; i < DayRegistry.Days.Length; i++)
                    {
                        if (ImGui.MenuItem($"Day {i + 1}"))
                        {
                            Coroutines.TryRun("main_day_advance", SetDay(i), out _);
                        }
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Gameplay", enabled: GameState == GameStates.Gameplay))
                {
                    if (ImGui.MenuItem("kills u"))
                    {
                        ChangeHealth(-int.MaxValue);
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Characters", enabled: GameState == GameStates.Gameplay))
                {
                    ImGui.Checkbox("Show List", ref _showCharacterList);
                    if (ImGui.Button("Clear Characters"))
                    {
                        CharacterManager.ClearCharacters();
                    }
                    if (ImGui.Button("Spawn Random"))
                    {
                        CharacterManager.SpawnMultipleRandomCharacters(1);
                    }
                    if (ImGui.BeginMenu("Spawn Character"))
                    {
                        foreach (var characterDef in CharacterRegistry.CharacterTable)
                        {
                            if (ImGui.MenuItem(characterDef.Key, "", false, CharacterManager.CacheCharactersInPlay.All(c => c.Def.Name != characterDef.Key)))
                            {
                                CharacterManager.SpawnCharacter(characterDef.Value);
                            }
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }

                if (UseSteamworks && SteamManager.IsSteamRunning)
                {
                    if (ImGui.BeginMenu("Steam"))
                    {
                        if (ImGui.BeginMenu("Achievements"))
                        {
                            var achievements = SteamManager.GetAchievements();
                            foreach (var achievement in achievements)
                            {
                                bool enabled = achievement.Item2;
                                if (ImGui.Checkbox(achievement.Item1, ref enabled))
                                {
                                    if(enabled)
                                        SteamManager.UnlockAchievement(achievement.Item1);
                                    else
                                        SteamManager.ClearAchievement(achievement.Item1);
                                }
                            }
                            ImGui.EndMenu();
                        }
                        ImGui.EndMenu();
                    }
                }
            }
            ImGui.EndMainMenuBar();

            if (_showCharacterList)
            {
                ImGui.Begin("Character List", ImGuiWindowFlags.NoFocusOnAppearing);
                {
                    ImGui.BeginTable("CharactersTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable);
                    {
                        ImGui.TableSetupColumn("Name");
                        ImGui.TableSetupColumn("Current Floor");
                        ImGui.TableSetupColumn("Target Floor");
                        ImGui.TableSetupColumn("Initial Patience");
                        ImGui.TableSetupColumn("Patience");
                        ImGui.TableHeadersRow();

                        var sortSpecs = ImGui.TableGetSortSpecs();
                        if (sortSpecs is {})
                        {
                            int sortDir = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending ? 1 : -1;
                            var specs = sortSpecs.Specs;
                            switch (specs.ColumnIndex)
                            {
                                case 0:
                                    CharacterManager.CacheCharactersInPlay.Sort((a, b) =>
                                        sortDir * string.Compare(a.Def.Name, b.Def.Name,
                                            StringComparison.InvariantCultureIgnoreCase));
                                    break;
                                case 1:
                                    CharacterManager.CacheCharactersInPlay.Sort((a, b) =>
                                        sortDir * a.FloorNumberCurrent.CompareTo(b.FloorNumberCurrent));
                                    break;
                                case 2:
                                    CharacterManager.CacheCharactersInPlay.Sort((a, b) =>
                                        sortDir * a.FloorNumberTarget.CompareTo(b.FloorNumberTarget));
                                    break;
                                case 3:
                                    CharacterManager.CacheCharactersInPlay.Sort((a, b) =>
                                        sortDir * a.InitialPatience.CompareTo(b.InitialPatience));
                                    break;
                                case 4:
                                    CharacterManager.CacheCharactersInPlay.Sort((a, b) =>
                                        sortDir * a.Patience.CompareTo(b.Patience));
                                    break;
                            }
                            sortSpecs.SpecsDirty = false;
                        }

                        foreach (var character in CharacterManager.CacheCharactersInPlay)
                        {
                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text(character.Def.Name);
                            ImGui.TableNextColumn();
                            ImGui.Text($"{character.FloorNumberCurrent}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{character.FloorNumberTarget}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{character.InitialPatience}");
                            ImGui.TableNextColumn();
                            ImGui.Text($"{character.Patience}");
                        }
                    }
                    ImGui.EndTable();
                }
                ImGui.End();
            }

            if (_showDevTools)
            {
                // var viewport = ImGui.GetMainViewport();
                // ImGui.SetNextWindowPos(viewport.WorkPos);
                // ImGui.SetNextWindowSize(viewport.WorkSize);
                // ImGui.SetNextWindowViewport(viewport.ID);
                // ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f); // No corner rounding on the window
                // ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f); // No border around the window
                // ImGui.Begin("Dev Tools", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove);
                ImGui.Begin("Dev Tools");
                {
                    if (ImGui.CollapsingHeader("Effects"))
                    {
                        bool allEffects = _wobbleTurns > 0 && _hueShiftTurns > 0 && _flippyTurns > 0;
                        bool wobbleEffect = _wobbleTurns > 0;
                        bool hueShiftEffect = _hueShiftTurns > 0;
                        bool flippyEffect = _flippyTurns > 0;

                        if (ImGui.Checkbox("All Effects", ref allEffects))
                        {
                            wobbleEffect = allEffects;
                            hueShiftEffect = allEffects;
                            flippyEffect = allEffects;
                            StartEffectWobble();
                            StartEffectHueShift();
                            StartEffectFlippy();
                        }

                        ImGui.Separator();

                        if (ImGui.Checkbox("Wobble", ref wobbleEffect) && wobbleEffect)
                        {
                            StartEffectWobble();
                        }
                        if (!wobbleEffect && _wobbleTurns > 0)
                        {
                            _wobbleTurns = 0;
                        }

                        if (ImGui.Checkbox("Hue Shift", ref hueShiftEffect) && hueShiftEffect)
                        {
                            StartEffectHueShift();
                        }
                        if (!hueShiftEffect && _hueShiftTurns > 0)
                        {
                            _hueShiftTurns = 0;
                        }

                        if (ImGui.Checkbox("Flippy", ref flippyEffect) && flippyEffect)
                        {
                            StartEffectFlippy();
                        }
                        if (!flippyEffect && _flippyTurns > 0)
                        {
                            _flippyTurns = 0;
                        }
                    }
                    if (ImGui.CollapsingHeader("Preview"))
                    {
                        ImGui.Image(_renderPipelineTextureId,
                            new System.Numerics.Vector2(RenderPipeline.RenderTarget.Width,
                                RenderPipeline.RenderTarget.Height));
                    }
                }
                ImGui.End();
            }
        }
        GuiRenderer.EndLayout();
    }

    private void DrawScreenTransition(SpriteBatch spriteBatch)
    {
        // left
        spriteBatch.Draw(
            PixelTexture,
            GameBounds with {
                X = 0,
                Y = 0,
                Width = MathUtil.CeilToInt(_fadeoutProgress * GameBounds.Width / 2)
            },
            Color.Black
        );

        // right
        spriteBatch.Draw(
            PixelTexture,
            GameBounds with {
                X = GameBounds.Width - MathUtil.CeilToInt(_fadeoutProgress * GameBounds.Width / 2),
                Y = 0,
                Width = MathUtil.CeilToInt(_fadeoutProgress * GameBounds.Width / 2)
            },
            Color.Black
        );

        _dayTransition.Draw(spriteBatch);
    }

    protected override void OnDeactivated(object sender, EventArgs args)
    {
        if (SaveManager.Settings?.AudioMuteWhenUnfocused ?? false)
        {
            _previousMasterVolume = StudioSystem.GetParameterTargetValue("VolumeMaster");
            StudioSystem.SetParameterValue("VolumeMaster", 0, true);
        }
    }

    protected override void OnActivated(object sender, EventArgs args)
    {
        if (SaveManager.Settings?.AudioMuteWhenUnfocused ?? false)
        {
            StudioSystem.SetParameterValue("VolumeMaster", _previousMasterVolume, true);
        }
    }

    public static IEnumerator CloseMainMenu()
    {
        if (GameState != GameStates.MainMenu)
            yield break;

        yield return FadeToBlack();
        yield return 30;

        _mainMenu = null;
        CurrentMenu = Menus.None;

        if (SaveManager.SaveData.Rooms.Count == 0)
        {
            for (int i = 0; i < 99; i++)
            {
                int max = int.MaxValue - 1;
                (int, int) roomRange = (0, max);
                if (i < 10)
                    roomRange = (0, 2);
                else if (i < 20)
                    roomRange = (3, 5);
                else if (i < 30)
                    roomRange = (6, max);
                var newRoomDef = RoomDef.MakeRandom("graphics/RoomsGeneric", roomRange.Item1, roomRange.Item2);
                _roomDefs.Add(newRoomDef);
            }
        }
    }

    private void CreateMainMenu()
    {
        _mainMenu = new()
        {
            ExitGame = Exit,
            StartGame = OnMainMenuStartGame,
            OnChangeFullscreen = SetFullscreen
        };
        _mainMenu.LoadContent();
        CurrentMenu = Menus.MainMenu;
        GameState = GameStates.MainMenu;

        SaveFileExists = SaveManager.SaveFileExists;

        SaveManager.Save();

        CurrentDay = 0;
        CleanupAndReinitialize();
        Coroutines.StopAll();

        MusicPlayer.PlayMusic("MainMenu");
    }

    private IEnumerator ReturnToMainMenuSequence(bool fadeOut = true, bool fadeIn = true)
    {
        if (fadeOut) yield return FadeToBlack(); else _fadeoutProgress = 1;
        yield return 10;
        CreateMainMenu();
        StudioSystem.SetParameterValue("IsPaused", 0f);
        if (fadeOut || fadeIn) yield return FadeFromBlack();
    }

    private void ReturnToMainMenu(bool fadeOut = true, bool fadeIn = true)
    {
        Coroutines.TryRun("main_return_to_main_menu", ReturnToMainMenuSequence(fadeOut, fadeIn), out _);
    }

    private void ReturnToMainMenuFromResultsScreen()
    {
        ReturnToMainMenu(false, true);
    }

    private void OnMainMenuStartGame()
    {
        Coroutines.StopAll();
        Coroutines.TryRun("main_day_advance", SetDay(CurrentDay), out _);
    }

    private void OnChangeFloorNumber(int floorNumber)
    {
        CurrentFloor = floorNumber;
        _roomRenderer.SetDefinition(_roomDefs[floorNumber - 1]);

        bool isProductiveTurn = CharacterManager.IsCharacterWaitingOnFloor(CurrentFloor) ||
                                CharacterManager.IsCharacterWaitingToGoToFloor(CurrentFloor);

        if (!isProductiveTurn)
        {
            int countClamped = MathUtil.ClampToInt(_comboCount, 0, 9);
            double chanceToSpawn = Math.Pow(MathUtil.InverseLerp01(0, 10, countClamped), 2);
            double roll = Random.Shared.NextDouble();
            if (roll < chanceToSpawn)
            {
                _bgCharacterRenderer.SetCharacterDef(BgCharacterRegistry.GetRandomCharacter());
            }
            else
            {
                _bgCharacterRenderer.SetCharacterDef(null);
            }
        }
        else
        {
            _comboCount++;
            _bgCharacterRenderer.SetCharacterDef(null);
        }
    }

    private IEnumerator EndOfTurnSequence()
    {
        // If anyone is going to this floor, they leave one at a time
        // Subtract patience from remaining passengers
        // Any passengers with patience <= 0 leave
        // Any passengers getting on this floor get on

        var waitingDir = CharacterManager.WaitingDirectionOnFloor(CurrentFloor);
        if (waitingDir != 0) _elevator.SetComboDirection(waitingDir);
        CurrentMenu = Menus.TurnTransition;
        Cursor.CursorSprite = Cursor.CursorSprites.Wait;

        if (CharacterManager.CharactersFinished < DayRegistry.Days[CurrentDay].CompletionRequirement)
        {
            yield return CharacterManager.EndOfTurnSequence();

            _wobbleTurns--;
            _hueShiftTurns--;
            _flippyTurns--;

            _phone.CanOpen = true;
            Coroutines.Stop("phone_show");
            Coroutines.TryRun("phone_hide", _phone.Close(false), out _);

            yield return null;
        }

        if (CharacterManager.CharactersFinished >= DayRegistry.Days[CurrentDay].CompletionRequirement)
        {
            // Advance to the next day

            _wobbleTurns = 0;
            _hueShiftTurns = 0;

            if(!EndOfDaySequence)
            {
                EndOfDaySequence = true;

                _phone.PlayJingle();

                if (CurrentDay == 0)
                {
                    yield return _phone.Open(false, false);
                    _phone.StartTalking();

                    yield return _dialog.Display(
                        new Dialog.DialogDef(
                            LocalizationManager.Get("dialog.phone.tutorial.end_of_day")
                        ).Pages,
                        Dialog.Dialog.DisplayMethod.Human
                    );

                    _phone.StopTalking();
                    yield return _phone.Close(false, true);
                }
            }

            if (CurrentFloor == 1)
            {
                Coroutines.TryRun("main_day_advance", AdvanceDay(), out _);
            }
        }

        CurrentMenu = Menus.None;
        Cursor.CursorSprite = Cursor.CursorSprites.Default;

        if (CharacterManager.CharactersInPlay.Count == 0)
            CharacterManager.SpawnMultipleRandomCharacters(MaxCountPerSpawn);

        HealthShield = 0;

        Turn++;
    }

    private IEnumerator ElevatorCrashed()
    {
        if (PunishMistakes)
        {
            HasMadeMistake = true;

            _phone.SimulateBatteryChange(-1);
            yield return 20;
            ChangeHealth(-1);
        }
    }

    public static void ChangeHealth(int change)
    {
        if (change < 0)
        {
            change += HealthShield;
            if (change >= 0) return;
        }
        CurrentHealth = Math.Clamp(CurrentHealth + change, 0, 8);

        if (CurrentHealth > 0) return;
        StartGameOver();
    }

    private static void StartGameOver()
    {
        // kills u
        GameState = GameStates.GameOver;
        Coroutines.StopAll();
        ResetShaderProperties();
        _gameOverScreen.Init(_phone.PhonePosition, _instance.ReturnToMainMenuFromResultsScreen);
        MusicPlayer.StopMusic(true, false);
    }

    private IEnumerator AdvanceDay()
    {
        if (UseSteamworks && !HasMadeMistake && CurrentDay == 2)
        {
            SteamManager.UnlockAchievement("DAY_3_FLAWLESS");
        }
        yield return SetDay(CurrentDay + 1);
    }

    private IEnumerator SetDay(int day, bool skipTransition = false)
    {
        ResetShaderProperties();
        CurrentMenu = Menus.DayTransition;

        MusicPlayer.StopMusic(false, true);

        if (!skipTransition)
        {
            yield return FadeToBlack();
        }

        EndOfDaySequence = false;
        _darkOverlayOpacity = 0;

        Console.WriteLine("TRANSITION");

        HasMadeMistake = false;

        if (!skipTransition)
        {
            yield return 60;
            yield return _dayTransition.TransitionToNextDay(day + 1);
        }

        if (day >= DayRegistry.Days.Length || day < 0)
        {
            // End of the game
            if (!skipTransition)
            {
                yield return FadeFromBlack();
            }
            yield break;
        }

        // the rest of the cleanup process
        CurrentDay = day;

        SaveManager.Save();

        CleanupAndReinitialize();

        _roomRenderer.SetDefinition(_roomDefs[0]);
        _roomRenderer.PreRender(SpriteBatch);

        _bgCharacterRenderer.SetCharacterDef(null);

        _comboCount = 0;
        CurrentHealth = 8;
        HealthShield = 0;

        if (!skipTransition)
        {
            yield return FadeFromBlack();
        }

        CurrentMenu = Menus.None;
        Coroutines.StopAll();

        MusicPlayer.PlayMusic($"Day{day + 1}");

        yield return StartDay(day);
    }

    private void CleanupAndReinitialize()
    {
        _pauseManager.UnloadContent();
        _pauseManager = new()
        {
            ExitGame = Exit,
            OpenMainMenu = CreateMainMenu
        };
        _pauseManager.LoadContent();

        _elevator?.Dispose();
        _phone?.Dispose();
        _dialog?.StopImmediately();
        // _dialog.UnloadContent();
        // _ticketManager.UnloadContent();
        // CharacterManager.UnloadContent();

        CurrentHealth = 8;
        HealthShield = 0;
        _comboCount = 0;

        _dialog = new();
        _elevator = new(OnChangeFloorNumber, EndOfTurnSequence, ElevatorCrashed);
        _phone = new(_elevator);
        _ticketManager = new(_elevator);
        CharacterManager = new(_phone, _ticketManager, _dialog, _elevator);

        CurrentFloor = 1;
        Turn = 0;
        _dialog.LoadContent();
        _elevator.LoadContent();
        _phone.LoadContent();
        _ticketManager.LoadContent();
        CharacterManager.LoadContent();

        CameraPosition = Vector2.Zero;
        CameraPositionTarget = Vector2.Zero;
        Camera.Position = Vector2.Zero;
        ResetShaderProperties();

        GC.Collect();
    }

    private IEnumerator StartDay(int dayIndex)
    {
        bool displayDialog = DayRegistry.Days[dayIndex].StartDialog.Pages.Length != 0;
        if (displayDialog)
        {
            yield return _phone.Open(false, false);
            _phone.StartTalking();
            yield return _dialog.Display(
                DayRegistry.Days[dayIndex].StartDialog.Pages,
                Dialog.Dialog.DisplayMethod.Human
            );
            _phone.StopTalking();
        }

        if (dayIndex == 0)
        {
            _buttonHint.Play();
            _buttonHintVisible = true;
        }

        if (displayDialog)
            yield return _phone.Close(false, true);

        // calling Init here prevents characters from spawning during start of day dialog
        CharacterManager.Init();
    }

    public static IEnumerator FadeToBlack()
    {
        ResetShaderProperties();

        _fadeoutTween.Start(_fadeoutProgress, 1f, 0.5f, TinyTween.ScaleFuncs.QuadraticEaseIn);
        while(!MathUtil.Approximately(_fadeoutProgress, 1, 0.01f))
        {
            yield return null;
        }
        _fadeoutTween.Stop(stopBehavior: StopBehavior.ForceComplete);

        _fadeoutProgress = 1;
    }

    public static IEnumerator FadeFromBlack()
    {
        while(!MathUtil.Approximately(_fadeoutProgress, 0, 0.01f))
        {
            _fadeoutProgress = MathUtil.ExpDecay(_fadeoutProgress, 0, 5, 1f/60f);
            yield return null;
        }
        _fadeoutProgress = 0;
    }

    public void UpdateInput(GameTime gameTime)
    {
        InputManager.InputDisabled = !IsActive;
        InputManager.RefreshKeyboardState();
        InputManager.RefreshMouseState();
        InputManager.RefreshGamePadState();
        InputManager.UpdateTypingInput(gameTime);
        Cursor.Update();
        if (InputManager.GetAnyPressed(InputType.GamePad))
        {
            _isUsingGamePad = true;
        }
        if (_lastMouseViewPos != Cursor.ViewPosition || InputManager.GetAnyPressed(InputType.Keyboard) || InputManager.GetAnyPressed(InputType.Mouse))
        {
            _isUsingGamePad = false;
        }
        _lastMouseViewPos = Cursor.ViewPosition;
        _lastMouseWorldPos = Cursor.WorldPosition;
    }

    public static Vector2 GetCursorParallaxValue(Vector2 position, float distance)
    {
        Vector2 checkPos = (
            Vector2.Clamp(
                _pauseManager.IsPaused
                    ? _lastMouseViewPos
                    : Cursor.ViewPosition,
                Vector2.Zero,
                new(240, 135)
            ) - new Vector2(240, 135) / 2f
        ) * (8 / 120f);
        return position + Vector2.Round(checkPos * MathUtil.InverseLerp(0, 100, distance));
    }

    private static void ResetShaderProperties()
    {
        GrayscaleCoeffTarget = 1;
        // GrayscaleCoeff = 1;
        // WobbleInfluence = 0;
        // HueShiftInfleunce = 0;
        _wobbleTurns = 0;
        _hueShiftTurns = 0;
        _flippyTurns = 0;
    }

    private void UpdateShaderProperties()
    {
        GrayscaleCoeff = MathUtil.ExpDecay(GrayscaleCoeff, GrayscaleCoeffTarget, 8, 1f / 60f);
        WobbleInfluence = MathUtil.ExpDecay(WobbleInfluence, _wobbleTurns > 0 ? 1 : 0, 8, 1f / 60f);
        HueShiftInfleunce = MathUtil.ExpDecay(HueShiftInfleunce, _hueShiftTurns > 0 ? 1 : 0, 8, 1f / 60f);
        FlippyInfluence = MathUtil.ExpDecay(FlippyInfluence, _flippyTurns > 0 ? 1 : 0, 8, 1f / 60f);
        _elevatorGrayscaleIntensity?.SetValue(GrayscaleCoeff);
        _ppWobbleInfluence?.SetValue(WobbleInfluence);
        _ppHueShiftInfluence?.SetValue(HueShiftInfleunce);
        _ppFlippyInfluence?.SetValue(FlippyInfluence);
        _ppGameTime?.SetValue(Frame / 60f);
    }

    public static void StartEffectWobble()
    {
        _wobbleTurns = 5;
    }

    public static void StartEffectHueShift()
    {
        _hueShiftTurns = 5;
    }

    public static void StartEffectFlippy()
    {
        _flippyTurns = 3;
    }

    private void HandleToggleFullscreen()
    {
        if (InputManager.GetPressed(Keys.F11) && OperatingSystem.IsWindows())
        {
            SetFullscreen(!IsFullscreen);
        }
    }

    private void SetFullscreen(bool fullscreen)
    {
        if (IsFullscreen == fullscreen) return;

        if (!fullscreen)
        {
            Graphics.PreferredBackBufferWidth = _actualWindowBounds.Width;
            Graphics.PreferredBackBufferHeight = _actualWindowBounds.Height;
            Window.Position = _actualWindowBounds.Location;
            Window.IsBorderless = false;
            Graphics.ApplyChanges();
        }
        else
        {
            _actualWindowBounds = Window.ClientBounds;

            Window.Position = Point.Zero;

            Graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            Graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            Window.IsBorderless = true;
            Graphics.ApplyChanges();
        }

        IsFullscreen = fullscreen;
    }

    [Conditional("DEBUG")]
    private void DebugUpdate()
    {
        if (InputManager.GetPressed(Keys.F3))
        {
            ShowDebug = !ShowDebug;
            if(!UseNativeCursor)
                IsMouseVisible = ShowDebug;

            Mouse.SetCursor(MouseCursor.Arrow);
        }

        if (InputManager.GetPressed(Keys.Y))
        {
            Coroutines.Stop("main_day_advance");
            Coroutines.TryRun("main_day_advance", AdvanceDay(), out _);
        }

        if (InputManager.GetPressed(Keys.T))
        {
            CharacterManager.ForceCompleteDay();
        }
    }
}
