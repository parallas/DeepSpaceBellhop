using System.Diagnostics;

#if STEAM
using Steamworks;
#endif

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
#if STEAM
            SteamClient.Init(appid);
            SteamUtils.OverlayNotificationPosition = NotificationPosition.BottomRight;
            IsSteamRunning = true;
            InitializeCallbacks();
#else
            throw new Exception("Game was not compiled with Steam enabled.");
#endif
        }
        catch (Exception e)
        {
            LogError($"Error initializing Steamworks: {e}");
            IsSteamRunning = false;
        }
    }

    [Conditional("STEAM")]
    public static void Update()
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamClient.RunCallbacks();
        #endif
    }

    [Conditional("STEAM")]
    public static void Cleanup()
    {
        #if STEAM
        if (!IsSteamRunning) return;

        StoreStats();

        Log("Shutting down Steam");
        SteamClient.Shutdown();

        IsSteamRunning = false;
        #endif
    }

    [Conditional("STEAM")]
    private static void InitializeCallbacks()
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamUserStats.RequestCurrentStats();
        #endif
    }

    public static void Log(object? message)
    {
        Console.WriteLine($"[Steamworks/INFO]: {message ?? ""}");
    }

    public static void LogError(object? message)
    {
        Console.WriteLine($"[Steamworks/ERROR]: {message ?? ""}");
    }

    public static IEnumerable<(string, bool)> GetAchievements()
    {
        #if STEAM
        if(!IsSteamRunning) return [];

        return from a in SteamUserStats.Achievements
               select (a.Identifier, a.State);
        #else
        return [];
        #endif
    }

    [Conditional("STEAM")]
    public static void SetStat(string name, int value)
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamUserStats.SetStat(name, value);
        #endif
    }

    [Conditional("STEAM")]
    public static void SetStat(string name, float value)
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamUserStats.SetStat(name, value);
        #endif
    }

    [Conditional("STEAM")]
    public static void AddStat(string name, int value = 1)
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamUserStats.AddStat(name, value);
        #endif
    }

    [Conditional("STEAM")]
    public static void AddStat(string name, float value = 1)
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamUserStats.AddStat(name, value);
        #endif
    }

    public static int GetStatInt(string name, int fallback)
    {
        #if STEAM
        if (!IsSteamRunning) return fallback;

        return SteamUserStats.GetStatInt(name);
        #else
        return fallback;
        #endif
    }

    public static float GetStatFloat(string name, float fallback)
    {
        #if STEAM
        if (!IsSteamRunning) return fallback;

        return SteamUserStats.GetStatFloat(name);
        #else
        return fallback;
        #endif
    }

    [Conditional("STEAM")]
    public static void StoreStats()
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamUserStats.StoreStats();
        #endif
    }

    public static bool AchievementIsUnlocked(string id)
    {
        #if STEAM
        if (!IsSteamRunning) return false;

        return SteamUserStats.Achievements.First(a => a.Identifier == id).State;
        #else
        return false;
        #endif
    }

    [Conditional("STEAM")]
    public static void UnlockAchievement(string id, bool apply = true)
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamUserStats.Achievements.First(a => a.Identifier == id).Trigger(apply);
        #endif
    }

    [Conditional("STEAM")]
    public static void ClearAchievement(string id)
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamUserStats.Achievements.First(a => a.Identifier == id).Clear();
        #endif
    }

    [Conditional("DEBUG")]
    [Conditional("STEAM")]
    public static void ResetAllStatsAndAchievements()
    {
        #if STEAM
        if (!IsSteamRunning) return;

        SteamUserStats.ResetAll(true);
        #endif
    }
}
