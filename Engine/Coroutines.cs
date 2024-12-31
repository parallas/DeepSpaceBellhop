#region LICENSE
/*

MIT License

Copyright (c) 2017 Chevy Ray Johnston

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

// original source: https://github.com/ChevyRay/Coroutines
// original fork used: https://github.com/Linerichka/Coroutines
// basis: https://github.com/tmaster-terrarian/magequest/blob/main/MageQuest/Engine/Coroutines.cs

// edited significantly to use frames instead of seconds
#endregion

using System.Collections;
using System.Collections.Generic;

namespace Engine;

/// <summary>
/// A container for running multiple routines in parallel. Coroutines can be nested.
/// </summary>
public class CoroutineRunner
{
    /// <summary>
    /// How many coroutines are currently running.
    /// </summary>
    public int Count => _coroutines.Count;

    private readonly Dictionary<string, CoroutineHandle> _coroutines = [];

    private readonly Dictionary<string, CoroutineHandle> _toAdd = [];

    private bool running;

    public CoroutineHandle Run(string methodName, IEnumerator enumerator, int delay = 0)
    {
        CoroutineHandle coroutineHandle = new(this, methodName, delay, enumerator);
        if(running)
        {
            _toAdd.Add(methodName, coroutineHandle);
        }
        else
            _coroutines.Add(methodName, coroutineHandle);
        return coroutineHandle;
    }

    public bool TryRun(string methodName, IEnumerator enumerator, out CoroutineHandle handle)
        => TryRun(methodName, enumerator, 0, out handle);

    public bool TryRun(string methodName, IEnumerator enumerator, int delay, out CoroutineHandle handle)
    {
        if (!IsRunning(methodName) && !_toAdd.ContainsKey(methodName))
        {
            handle = Run(methodName, enumerator, delay);
            return true;
        }
        handle = null;
        return false;
    }

    public bool Stop(string methodName)
    {
        return _coroutines.Remove(methodName);
    }

    public CoroutineHandle GetHandle(string methodName) => _coroutines[methodName];

    /// <summary>
    /// Stop all running routines.
    /// </summary>
    public void StopAll()
    {
        if(running)
            return;
        _coroutines.Clear();
    }

    /// <summary>
    /// Check if the routine is currently running.
    /// </summary>
    /// <returns>True if the routine is running.</returns>
    public bool IsRunning(string methodName) => _coroutines.ContainsKey(methodName);

    /// <summary>
    /// Update all running coroutines.
    /// </summary>
    public void Update()
    {
        Queue<string> corountineForRemoval = [];

        if(_toAdd.Count > 0)
        {
            foreach(var item in _toAdd)
            {
                _coroutines.Add(item.Key, item.Value);
            }
            _toAdd.Clear();
        }

        running = true;

        foreach(var coroutine in _coroutines.Values)
        {
            if(coroutine.Delay > 0)
            {
                coroutine.Delay--;

                if(coroutine.Delay > 0) continue;
            }

            bool moveNext = MoveNext(coroutine);

            if(!moveNext)
            {
                corountineForRemoval.Enqueue(coroutine.MethodName);
            }
        }

        running = false;

        while(corountineForRemoval.TryDequeue(out string methodName))
        {
            _coroutines.Remove(methodName);
        }
    }

    private static bool MoveNext(CoroutineHandle coroutine, IEnumerator? enumerator = null)
    {
        enumerator ??= coroutine.Enumerator;

        if(enumerator.Current is IEnumerator nested)
        {
            if(MoveNext(coroutine, nested))
                return true;
            coroutine.Delay = 0;
        }

        bool result = enumerator.MoveNext();

        if(!result)
            return false;
        else if(enumerator.Current is null)
            return true;
        else if(enumerator.Current is int current)
            coroutine.Delay = current;

        return true;
    }
}

public class CoroutineHandle(CoroutineRunner runner, string methodName, int delay, IEnumerator enumerator)
{
    public CoroutineRunner Runner => runner;
    public string MethodName => methodName;
    public int Delay { get; set; } = delay;
    public IEnumerator Enumerator => enumerator;

    /// <summary>
    /// Stop this coroutine if it is running.
    /// </summary>
    /// <returns>True if the coroutine was stopped.</returns>
    public bool Stop() => Runner.Stop(MethodName);

    /// <summary>
    /// A routine to wait until this coroutine has finished running.
    /// </summary>
    /// <returns>The wait enumerator.</returns>
    public IEnumerator Wait()
    {
        if (Enumerator != null)
        {
            while(Runner.IsRunning(MethodName))
            {
                yield return null;
            }
        }
    }

    /// <summary>
    /// True if the enumerator is currently running.
    /// </summary>
    public bool IsRunning => Runner.IsRunning(MethodName);
}
