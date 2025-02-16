using System;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using FMOD;
using FmodForFoxes;
using FmodForFoxes.Studio;
using Microsoft.Xna.Framework;

namespace Engine;

public static class FmodController
{
    // FMOD
    private static readonly INativeFmodLibrary _nativeLibrary = new DesktopNativeFmodLibrary();
    private static Bank _masterBank;
    private static Bus _masterBus;
    private static FMOD.ChannelGroup _channelGroup;

    private static string _rootDir;

    #region System
    public static void Init(string rootDir = "Content")
    {
        _rootDir = rootDir;
        // FMOD Setup
        FmodManager.Init(_nativeLibrary, FmodInitMode.CoreAndStudio, _rootDir, preInitAction: PreInit, enableLogging: false);
    }

    private static void PreInit()
    {
        long seed = DateTime.Now.ToBinary();
        var advancedSettings = new ADVANCEDSETTINGS
        {
            randomSeed = ((uint)seed) ^ ((uint)(seed >> 32))
        };
        CoreSystem.Native.setAdvancedSettings(ref advancedSettings);
    }

    // private static async Task LoadContentAsync()
    // {
    //     // var native = _masterBus.Native;
    //     // while (native.getChannelGroup(out _channelGroup) != RESULT.OK) { await Task.Yield(); }

    //     // _masterBus.UnlockChannelGroup();
    // }

    public static void LoadContent(string banksPath, bool loadStrings, string[] banksToLoad, string[] bussesToLoad)
    {
        Console.WriteLine("FmodController: Loading content");
        // Load banks
        foreach (string bank in banksToLoad)
        {
            Console.WriteLine($"FmodController: Loading bank \"{Path.Combine(banksPath, bank)}.bank\"");
            _masterBank = StudioSystem.LoadBank($"{banksPath}/{bank}.bank");

            var stringPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _rootDir, banksPath, $"{bank}.strings.bank");
            if (loadStrings && File.Exists(stringPath))
            {
                Console.WriteLine($"FmodController: Loading bank strings \"{Path.Combine(banksPath, bank)}.strings.bank\"");
                _masterBank = StudioSystem.LoadBank($"{banksPath}/{bank}.strings.bank");
            }
        }

        _masterBank.LoadSampleData();

        // Loading busses must be after loading banks, for some reason
        _masterBus = StudioSystem.GetBus("bus:/Sounds");

        // Loads FMOD DSPs which won't load until the update after FMOD is initialized
        // LoadContentAsync();
    }

    public static void UnloadContent()
    {
        FmodManager.Unload();
        _masterBank.Unload();
    }

    public static void Update()
    {
        FmodManager.Update();
    }
    #endregion

    public static void PlayOneShot(string path)
    {
        var eventInstance = StudioSystem.GetEvent(path).CreateInstance();
        eventInstance.Start();
        eventInstance.Dispose();
    }
}
