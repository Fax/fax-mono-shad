public readonly struct NextLevelEvent : IGameEvent
{
    private readonly int Id = IdManager.NextId();
    public readonly int Level;
    public NextLevelEvent(int level)
    {
        Level = level;
    }

    public int EventID => Id;
}