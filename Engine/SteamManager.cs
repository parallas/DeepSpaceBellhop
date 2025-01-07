using System.Diagnostics;
using Steamworks;

namespace Engine;

public static class SteamManager
{
    public static uint steam_appid { get; private set; } = 480;

    public static bool IsSteamRunning { get; private set; }

    public static void Initialize(uint appid)
    {
        if (IsSteamRunning) return;

        steam_appid = appid;

        try
        {
            SteamClient.Init(appid);
            SteamUtils.OverlayNotificationPosition = NotificationPosition.BottomRight;
            IsSteamRunning = true;
            InitializeCallbacks();
        }
        catch (Exception e)
        {
            LogError($"Error initializing Steamworks: {e}");
            IsSteamRunning = false;
        }
    }

    public static void Update()
    {
        if (!IsSteamRunning) return;

        SteamClient.RunCallbacks();
    }

    public static void Cleanup()
    {
        if (!IsSteamRunning) return;

        StoreStats();

        Log("Shutting down Steam");
        SteamClient.Shutdown();
    }

    private static void InitializeCallbacks()
    {
        if (!IsSteamRunning) return;

        SteamUserStats.RequestCurrentStats();
    }

    public static void Log(object? message)
    {
        Console.WriteLine($"[Steamworks/INFO]: {message ?? ""}");
    }

    public static void LogError(object? message)
    {
        Console.WriteLine($"[Steamworks/ERROR]: {message ?? ""}");
    }

    public static void SetStat(string name, int value)
    {
        if (!IsSteamRunning) return;

        SteamUserStats.SetStat(name, value);
    }

    public static void SetStat(string name, float value)
    {
        if (!IsSteamRunning) return;

        SteamUserStats.SetStat(name, value);
    }

    public static void AddStat(string name, int value = 1)
    {
        if (!IsSteamRunning) return;

        SteamUserStats.AddStat(name, value);
    }

    public static void AddStat(string name, float value = 1)
    {
        if (!IsSteamRunning) return;

        SteamUserStats.AddStat(name, value);
    }

    public static int GetStatInt(string name, int fallback)
    {
        if (!IsSteamRunning) return fallback;

        return SteamUserStats.GetStatInt(name);
    }

    public static float GetStatFloat(string name, float fallback)
    {
        if (!IsSteamRunning) return fallback;

        return SteamUserStats.GetStatFloat(name);
    }

    public static void StoreStats()
    {
        if (!IsSteamRunning) return;

        SteamUserStats.StoreStats();
    }

    public static bool AchievementIsUnlocked(string id)
    {
        return SteamUserStats.Achievements.First(a => a.Identifier == id).State;
    }

    public static void UnlockAchievement(string id, bool apply = true)
    {
        SteamUserStats.Achievements.First(a => a.Identifier == id).Trigger(apply);
    }

    [Conditional("DEBUG")]
    public static void ResetAllStatsAndAchievements()
    {
        if (!IsSteamRunning) return;

        SteamUserStats.ResetAll(true);
    }
}
