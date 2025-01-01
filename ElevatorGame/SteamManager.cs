using Engine.Steamworks;
using Microsoft.Xna.Framework;
using Steamworks;

namespace Engine;

public static class SteamManager
{
    public static uint steam_appid { get; private set; } = 480;

    public static AppId_t AppID => (AppId_t)steam_appid;

    public static bool IsSteamRunning { get; private set; }

    private static bool _steamFailed;

    internal static HashSet<SteamCallback> callbacks;

    public static void SetAppID(uint value)
    {
        steam_appid = value;
    }

    public static void PreInitialize(Game game)
    {
        try
        {
            if (SteamAPI.RestartAppIfNecessary(AppID))
            {
                LogError("Game wasn't started by Steam-client! Restarting..");
                game.Exit();
            }
        }
        catch (DllNotFoundException e)
        {
            // We check this here as it will be the first instance of it.
            LogError("Could not load [lib]steam_api.dll/so/dylib.\nCaused by " + e);
            _steamFailed = true;
        }
    }

    public static void Initialize()
    {
        try
        {
            if (!_steamFailed)
            {
                InternalInit();
            }
        }
        catch (Exception e)
        {
            LogError($"Error initializing Steamworks: {e}");
        }
    }

    private static bool InternalInit()
    {
        if (IsSteamRunning) return true;

        try
        {
            if (!SteamAPI.Init())
            {
                LogError("SteamAPI.Init() failed!");
                return false;
            }
        }
        catch (DllNotFoundException e) // We check this here as it will be the first instance of it.
        {
            LogError(e);
            return false;
        }

        if (!Packsize.Test())
        {
            LogError("You're using the wrong Steamworks.NET Assembly for this platform!");
            return false;
        }

        if (!DllCheck.Test())
        {
            LogError("You're using the wrong libraries for this platform!");
            return false;
        }

        InitializeCallbacks();

        return IsSteamRunning = true;
    }

    public static void Update()
    {
        if (!IsSteamRunning) return;

        SteamAPI.RunCallbacks();
    }

    public static void Cleanup()
    {
        if (!IsSteamRunning) return;

        Log("Shutting down Steam...");
        SteamAPI.Shutdown();
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
