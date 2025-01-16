using Engine;
using Engine.Localization;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ElevatorGame.Source.MainMenu;

public class SettingsMenu
{
    public SettingsTabs CurrentTab => _currentTab;

    public Action OnClose { get; set; }

    public Action<bool> OnChangeFullscreen { get; set; }

    public static int DividerX { get; private set; } = 72;

    public enum SettingsTabs : int
    {
        Game = 0,
        Audio = 1,
        Interface = 2,
        Graphics = 3
    }

    private SettingsTabs _currentTab = SettingsTabs.Game;

    private SettingsTab Tab => _tabs[(int)CurrentTab];

    private RenderTarget2D _renderTarget;

    private float _opacity;
    private int _opacityTarget = 1;

    private bool _closed;

    private List<SettingsTab> _tabs = [];
    private List<float> _tabOffsets = [];
    private List<float> _tabOffsetTargets = [];

    private bool _isDirty;

    public void LoadContent()
    {
        _isDirty = false;

        _renderTarget = new(MainGame.Graphics.GraphicsDevice, MainGame.GameBounds.Width, MainGame.GameBounds.Height);

        SaveManager.LoadSettings();

        _tabs = [
            new SettingsTab
            {
                TitleLangToken = GetTabLangToken("game"),
                Options = [
                    new SettingsOptionEnum(
                        index: 0,
                        options:
                            from langSettings in LocalizationManager.LoadedLanguages
                            select (langSettings.Identifier, langSettings.Name)
                    ) {
                        GetValue = () => LocalizationManager.CurrentLanguage,
                        SetValue = (id) =>
                        {
                            SaveManager.Settings.LanguagePreference = id;
                            LocalizationManager.CurrentLanguage = id;
                            _isDirty = true;
                        },
                        LangToken = GetOptionLangToken("game", "language"),
                        SetSelected = GetOptionSelectedAction(tab: SettingsTabs.Audio),
                    }
                ],
            },

            new SettingsTab
            {
                TitleLangToken = GetTabLangToken("audio"),
                Options = [
                    SettingsOptionFiller.Create(index: 0, langToken: GetSectionLangToken("audio", "volume")),

                    new SettingsOptionSlider(index: 1, width: 100, minValue: 0, maxValue: 100, stepAmount: 5)
                    {
                        GetValue = () => MathUtil.FloorToInt(StudioSystem.GetParameterTargetValue("VolumeMaster") * 100),
                        SetValue = (value) =>
                        {
                            StudioSystem.SetParameterValue("VolumeMaster", value / 100f);
                            _isDirty = true;
                        },
                        LangToken = GetOptionLangToken("audio", "master_volume"),
                        SetSelected = GetOptionSelectedAction(tab: SettingsTabs.Audio),
                    },

                    new SettingsOptionSlider(index: 2, width: 100, minValue: 0, maxValue: 100, stepAmount: 5)
                    {
                        GetValue = () => MathUtil.FloorToInt(StudioSystem.GetParameterTargetValue("VolumeMusic") * 100),
                        SetValue = (value) =>
                        {
                            StudioSystem.SetParameterValue("VolumeMusic", value / 100f);
                            _isDirty = true;
                        },
                        LangToken = GetOptionLangToken("audio", "music_volume"),
                        SetSelected = GetOptionSelectedAction(tab: SettingsTabs.Audio),
                    },

                    new SettingsOptionSlider(index: 3, width: 100, minValue: 0, maxValue: 100, stepAmount: 5)
                    {
                        GetValue = () => MathUtil.FloorToInt(StudioSystem.GetParameterTargetValue("VolumeSounds") * 100),
                        SetValue = (value) =>
                        {
                            StudioSystem.SetParameterValue("VolumeSounds", value / 100f);
                            _isDirty = true;
                        },
                        LangToken = GetOptionLangToken("audio", "sfx_volume"),
                        SetSelected = GetOptionSelectedAction(tab: SettingsTabs.Audio),
                    },

                    new SettingsOptionCheckbox(index: 4)
                    {
                        GetValue = () => SaveManager.Settings.AudioMuteWhenUnfocused,
                        SetValue = (value) =>
                        {
                            SaveManager.Settings.AudioMuteWhenUnfocused = value;
                            _isDirty = true;
                        },
                        LangToken = GetOptionLangToken("audio", "mute_when_unfocused"),
                        SetSelected = GetOptionSelectedAction(tab: SettingsTabs.Audio),
                    },
                ],
            },

            new SettingsTab
            {
                TitleLangToken = GetTabLangToken("interface"),
                Options = [],
            },

            new SettingsTab
            {
                TitleLangToken = GetTabLangToken("graphics"),
                Options = [],
            }
        ];

        if (OperatingSystem.IsWindows())
        {
            int tab = (int)SettingsTabs.Graphics;
            _tabs[tab].Options.Add(SettingsOptionFiller.Create(
                index: _tabs[tab].Options.Count,
                langToken: GetSectionLangToken("graphics", "windows")
            ));

            _tabs[tab].Options.Add(new SettingsOptionCheckbox(index: _tabs[tab].Options.Count)
            {
                SetValue = OnChangeFullscreen,
                GetValue = () => MainGame.IsFullscreen,
                LangToken = GetOptionLangToken("graphics", "fullscreen"),
                SetSelected = GetOptionSelectedAction(tab: SettingsTabs.Graphics),
            });
        }

        foreach (var t in _tabs)
        {
            _tabOffsets.Add(0);
            _tabOffsetTargets.Add(0);

            foreach (var o in t.Options)
            {
                o.LoadContent();
            }
        }
    }

    public void Update()
    {
        if (Keybindings.GoBack.Pressed && !_closed)
        {
            _opacityTarget = 0;
            OnClose?.Invoke();
            _closed = true;
        }

        _opacity = MathUtil.ExpDecay(_opacity, _opacityTarget, 10, 1f / 60f);

        if (_closed) return;

        for (int i = 0; i < _tabs.Count; i++)
        {
            if (new Rectangle(0, (i == 0 ? 0 : 1) + i * 11, DividerX - 6, 11 + (i == 0 ? 1 : 0))
                .Contains(MainGame.Cursor.ViewPosition) && InputManager.GetPressed(MouseButtons.LeftButton))
            {
                SetTab((SettingsTabs)i);
                break;
            }
        }

        int tabCycleDir = (Keybindings.SettingsTabNext.Pressed ? 1 : 0) - (Keybindings.SettingsTabPrev.Pressed ? 1 : 0);
        if (tabCycleDir != 0)
        {
            int tab = (int)_currentTab;

            tab = (tab + tabCycleDir) % _tabs.Count;
            if (tab < 0) tab = _tabs.Count - 1;

            SetTab((SettingsTabs)tab);
        }
        else if (Tab.Options.Count != 0)
        {
            int inputDir = (Keybindings.Down.Pressed ? 1 : 0) - (Keybindings.Up.Pressed ? 1 : 0);
            Tab.SetSelectedOption((Tab.SelectedOption + inputDir) % Tab.Options.Count);
            if (Tab.SelectedOption < 0) Tab.SetSelectedOption(Tab.Options.Count - 1);

            if (inputDir != 0 && Tab.Options.Any(o => o is not SettingsOptionFiller))
            {
                while (Tab.Options[Tab.SelectedOption] is SettingsOptionFiller)
                {
                    Tab.SetSelectedOption((Tab.SelectedOption + inputDir) % Tab.Options.Count);
                    if (Tab.SelectedOption < 0) Tab.SetSelectedOption(Tab.Options.Count - 1);
                }
            }
        }

        for (int i = 0; i < _tabs.Count; i++)
        {
            _tabOffsets[i] = MathUtil.ExpDecay(_tabOffsets[i], _tabOffsetTargets[i], 12, 1f / 60f);
        }

        foreach (var o in Tab.Options)
        {
            o.Update(Tab.SelectedOption == o.Index);
        }
    }

    public void PreDraw(SpriteBatch spriteBatch)
    {
        MainGame.Graphics.GraphicsDevice.SetRenderTarget(_renderTarget);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        {
            MainGame.Graphics.GraphicsDevice.Clear(Color.Black * 0.75f);

            for (int i = 0; i < _tabs.Count; i++)
            {
                bool isActiveTab = (int)_currentTab == i;
                Vector2 tabTitlePos = new(
                    3,
                    i * 11
                );

                var str = LocalizationManager.Get(_tabs[i].TitleLangToken);

                // spriteBatch.DrawStringSpacesFix(
                //     MainGame.FontBold,
                //     str,
                //     tabTitlePos + Vector2.One,
                //     Color.Black,
                //     6
                // );

                if (isActiveTab)
                {
                    spriteBatch.Draw(
                        MainGame.PixelTexture,
                        new Rectangle(
                            tabTitlePos.ToPoint() + new Point(-3, 1),
                            new(DividerX - 6, 11)
                        ),
                        ColorUtil.CreateFromHex(0x404040)
                    );
                }

                spriteBatch.DrawStringSpacesFix(
                    MainGame.FontBold,
                    str,
                    tabTitlePos + Vector2.UnitX * _tabOffsets[i],
                    isActiveTab ? ColorUtil.CreateFromHex(0xa7f6fd) : ColorUtil.CreateFromHex(0xa0a0a0),
                    6
                );
            }

            foreach (var o in Tab.Options)
            {
                o.Draw(spriteBatch, Tab.SelectedOption == o.Index);
            }
        }
        spriteBatch.End();
        MainGame.Graphics.GraphicsDevice.Reset();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White * _opacity);
    }

    public void SetTab(SettingsTabs tab)
    {
        Tab.SetSelectedOption(0);

        _tabOffsetTargets[(int)_currentTab] = 0;

        _currentTab = tab;

        _tabOffsetTargets[(int)_currentTab] = 3;

        int option = 0;
        if (Tab.Options.Count != 0 && Tab.Options[0] is SettingsOptionFiller && Tab.Options.Any(o => o is not SettingsOptionFiller))
        {
            while (Tab.Options[option] is SettingsOptionFiller)
            {
                option++;
                if (option >= Tab.Options.Count)
                {
                    option = 0;
                    break;
                }
            }
        }
        Tab.SetSelectedOption(option);
    }

    private static string GetTabLangToken(string name)
    {
        const string prefix = "ui.settings.tab";
        return $"{prefix}.{name}";
    }

    private static string GetOptionLangToken(string tab, string name)
    {
        const string prefix = "ui.settings.tab";
        const string infix = "option";
        return $"{prefix}.{tab}.{infix}.{name}";
    }

    private static string GetSectionLangToken(string tab, string name)
    {
        const string prefix = "ui.settings.tab";
        const string infix = "section";
        return $"{prefix}.{tab}.{infix}.{name}";
    }

    private Action<int> GetOptionSelectedAction(SettingsTabs tab)
    {
        return i => _tabs[(int)tab].SetSelectedOption(i);
    }

    class SettingsTab
    {
        private int _selectedOption;

        public int SelectedOption => _selectedOption;

        public required List<ISettingsOption> Options { get; set; }

        public required string TitleLangToken { get; set; }

        public void SetSelectedOption(int optionIndex)
        {
            _selectedOption = optionIndex;
        }
    }
}
