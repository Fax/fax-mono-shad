
using Microsoft.Xna.Framework;

public readonly struct SpawnExperienceEvent : IGameEvent
{
    private readonly int id = IdManager.NextId();
    int IGameEvent.EventID => id;
    public readonly int Amount = 100;
    public readonly Vector2 Position;

    public SpawnExperienceEvent(int am, Vector2 position)
    {
        Amount = am;
        Position = position;
    }

}

public readonly struct CollectExperienceEvent : IGameEvent
{
    private readonly int id = IdManager.NextId();
    int IGameEvent.EventID => id;
    public readonly int Amount = 100;
    public readonly Vector2 Position;
    
    public CollectExperienceEvent(int am, Vector2 position)
    {
        Amount = am;
        Position = position;
    }

}