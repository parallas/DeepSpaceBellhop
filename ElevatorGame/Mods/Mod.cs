using System.Collections.Generic;
using ElevatorGame.Mods;

namespace ElevatorGame;

public abstract class Mod
{
    public ModDependencyCollection Dependencies { get; } = [];

    public virtual void OnBeforeRun() { }

    public virtual void OnInitialize() { }

    public virtual void OnRegistriesInit() { }

    public virtual void OnLoadContent() { }

    public virtual void OnEndRun() { }
}
