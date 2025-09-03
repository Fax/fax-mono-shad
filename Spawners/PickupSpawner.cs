
using System.Security.Cryptography.X509Certificates;


public class PickupSpawner : ISpawner<PickupEntity>
{
    private readonly EventBus _bus;
    public float TimeToSpawn = 10.0f;
    private float Counter = 11.0f;
    public Random Rnd = new Random();
    private List<PickupEntity> _entities;

    public PickupSpawner(EventBus bus, List<PickupEntity> entities)
    {
        _bus = bus;

        _bus.Subscribe<WeaponPickupEvent>(PickedUp);

        _entities = entities;

    }

    public void PickedUp(WeaponPickupEvent evt)
    {
        _entities.RemoveAll(x => x.Id == evt.EntityId);
    }
    public void Update(float dt)
    {
        Counter += dt;
        if (Counter >= TimeToSpawn)
        {
            Counter = 0.0f;
            _entities.Clear();
            var w = new WeaponPickup((int)Rnd.NextInt64(0, 2))
            {
                Position = new Vector2(
                    Rnd.NextSingle() * 800.0f,
                    Rnd.NextSingle() * 700.0f
                ),
            };

            _entities.Add(w);
        }
    }
}