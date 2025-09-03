public class PickupManager
{
    private readonly List<PickupEntity> _pickups;

    public PickupManager(List<PickupEntity> pickups)
    {
        _pickups = pickups;
    }

    public void Update(float dt)
    {
        foreach (var p in _pickups)
        {
            p.Rotation += p.RotationSpeed * dt;
        }
    }
}