using System.Collections;
using Steamworks;

namespace Engine.Steamworks;

public class SteamCallResult<T> : SteamCallback
{
    public event CallResult<T>.APIDispatchDelegate Event;

    internal CallResult<T> _callback;

    public SteamCallResult() : base()
    {
        _callback = CallResult<T>.Create((param, bIOFailure) =>
        {
            Event?.Invoke(param, bIOFailure);
        });
    }

    public SteamCallResult(CallResult<T>.APIDispatchDelegate func) : this()
    {
        if (func is not null)
        {
            Event += func;
        }
    }

    public async Task<T> GetResult(SteamAPICall_t APICall)
    {
        return await Task.Run(() =>
        {
            T result = default;
            bool ioError = false;
            bool resolved = false;
            Event += (call, io) =>
            {
                resolved = true;
                result = io ? default : call;
                ioError = io;
            };
            Cancelled += () =>
            {
                resolved = true;
                ioError = true;
            };
            while (true)
            {
                if (resolved)
                {
                    break;
                }
            }
            return ioError ? default : result;
        });
    }

    public override bool Unsubscribe()
    {
        _callback.Cancel();
        InvokeCancelled();
        return SteamManager.callbacks.Remove(this);
    }
}
