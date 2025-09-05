
using System;
using Microsoft.Xna.Framework;



public static class IdManager
{
    private static int CurrentId = 0;
    private static Object locker = new();
    public static int NextId()
    {
        lock (locker)
        {
            CurrentId++;
        }
        return CurrentId;
    }
}

public class PickupEntity
{
    public int Id = IdManager.NextId();
    public Vector2 Position;
    public Vector2 Size;
    public float Rotation;
    public float RotationSpeed;
    public Color Color;

    public bool Active = true;

}

public class WeaponPickup : PickupEntity
{
    public int BulletType = 0;
    public WeaponPickup(Bullet bullet)
    {
        Size = new Vector2(10, 10);
        RotationSpeed = 8.0f;
        BulletType = bullet.BulletType;
        Color = bullet.Color;
    }
    public WeaponPickup(int weaponType)
    {
        BulletType = weaponType;
        switch (weaponType)
        {
            case 0:
                Color = Color.AliceBlue;

                break;
            case 1:
                Color = Color.Orange;
                break;
            case 2:
            default:
                Color = Color.Green;
                break;
        }
        Size = new Vector2(10, 10);
        RotationSpeed = 15.0f;
    }
}