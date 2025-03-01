namespace ElevatorGame;

public class SettingsData()
{
    // game
    public string LanguagePreference { get; set; } = "en-us";

    // audio
    public float AudioMasterVolume { get; set; } = 0.85f;
    public float AudioMusicVolume { get; set; } = 1;
    public float AudioSFXVolume { get; set; } = 1;
    public bool AudioMuteWhenUnfocused { get; set; }

    // interface
    public bool UseNativeCursor { get; set; } = false;

    // graphics
    public float LcdEffect { get; set; } = 1;
    public float FrameBlending { get; set; } = 1;
}
