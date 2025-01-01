using Microsoft.Xna.Framework;
using Steamworks;

namespace Engine;

public static class SteamManager
{
    public static uint steam_appid { get; private set; } = 480;

    public static AppId AppID => (AppId)steam_appid;

    public static bool IsSteamRunning { get; private set; }

    public static void Initialize(uint appid)
    {
        if (IsSteamRunning) return;

        steam_appid = appid;

        try
        {
            SteamClient.Init(AppID);
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

        Log("Shutting down Steam...");
        SteamClient.Shutdown();
    }

    private static void InitializeCallbacks()
    {
        if (!IsSteamRunning) return;
    }

    public static void Log(object? message)
    {
        Console.WriteLine($"[Steamworks/INFO]: {message ?? ""}");
    }

    public static void LogError(object? message)
    {
        Console.WriteLine($"[Steamworks/ERROR]: {message ?? ""}");
    }
}
