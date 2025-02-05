namespace ElevatorGame;

public class SettingsData()
{
    public float AudioMasterVolume { get; set; } = 0.85f;
    public float AudioMusicVolume { get; set; } = 1;
    public float AudioSFXVolume { get; set; } = 1;
    public bool AudioMuteWhenUnfocused { get; set; }
    public string LanguagePreference { get; set; } = "en-us";
}
