public class ExperienceManager
{
    private readonly EventBus _bus;
    private List<ExperienceEntity> _nuggets;
    private readonly Player _player;

    public ExperienceManager(EventBus bus, List<ExperienceEntity> nuggets, Player player)
    {
        _bus = bus;
        _bus.Subscribe<SpawnExperienceEvent>(SpawnExperience);
        _nuggets = nuggets;
        _player = player;
    }

    void SpawnExperience(SpawnExperienceEvent evt)
    {
        _nuggets.Add(new ExperienceEntity
        {
            Position = evt.Position,
            Amount = evt.Amount
        });
    }

    public void Update(float dt)
    {
        foreach (var nugget in _nuggets.Where(x => x.Active && x.Speed > 0))
        {
            nugget.Position += (_player.Position - nugget.Position).Normalized() * dt * nugget.Speed;
            nugget.Speed *= 1.3f;
        }

        _nuggets.RemoveAll(x => !x.Active);
    }
}