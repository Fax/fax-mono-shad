using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public readonly struct SplashDamageEvent : IGameEvent
{
    private readonly int id = IdManager.NextId();
    int IGameEvent.EventID => id;

    public readonly float SplashDamage = 0.0f;
    public readonly float SplashDamageRange = 0.0f;
    public readonly Vector2 Position = new Vector2();

    public SplashDamageEvent(Vector2 position, float d, float r)
    {
        Position = position;
        SplashDamage = d;
        SplashDamageRange = r;
    }
}

