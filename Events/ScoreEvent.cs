public readonly struct ScoreEvent : IGameEvent
{
    public readonly int Score = 10;
    private readonly int id = IdManager.NextId();
    int IGameEvent.EventID => id;
    public ScoreEvent(){}
}
