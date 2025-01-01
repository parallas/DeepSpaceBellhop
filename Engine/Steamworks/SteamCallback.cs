using Steamworks;

namespace Engine.Steamworks;

public class SteamCallback<T> : SteamCallback
{
    public event Callback<T>.DispatchDelegate Event;

    internal Callback<T> _callback;

    public SteamCallback() : base()
    {
        _callback = Callback<T>.Create(param =>
        {
            Event?.Invoke(param);
        });
    }

    public SteamCallback(Callback<T>.DispatchDelegate func) : this()
    {
        if (func is not null)
        {
            Event += func;
        }
    }

    public override bool Unsubscribe()
    {
        _callback.Unregister();
        InvokeCancelled();
        return SteamManager.callbacks.Remove(this);
    }
}

public abstract class SteamCallback
{
    public event Action Cancelled;

    protected void InvokeCancelled()
    {
        Cancelled?.Invoke();
    }

    public SteamCallback()
    {
        SteamManager.callbacks.Add(this);
    }

    public abstract bool Unsubscribe();
}
