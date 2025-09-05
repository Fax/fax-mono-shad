using Microsoft.Xna.Framework;

public class AreaEffect
{
    public Vector2 Center;
    public float Range;
    public Color Color;
    public float Damage;
    public bool IsCollider = true;
    public bool IsRendered = true;
    public float Duration = .5f;
    public int EntityId = IdManager.NextId();
    public Rectangle BoundingBox => Center.ToCenteredBoundingBox(new(Range));
}


/// <summary>
///  not types, I need to store the id (bullettype) -> bullet instance.
///  and I have to load them from disk
///  and I have to define events dynamically, maybe with a scripting language.
/// </summary>
public class BulletRegistry
{
    public List<Bullet> RegisteredBullets = new List<Bullet>();

    public void Reset()
    {
        RegisteredBullets.Clear();
        SelfRegister();
    }
    private BulletRegistry()
    {
        SelfRegister();
    }
    private static BulletRegistry _instance;
    public static BulletRegistry Instance
    {
        get
        {
            if (_instance == null) _instance = new BulletRegistry();
            return _instance;
        }
    }
       
    public void SelfRegister()
    {
        var listOfBs = (
               from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                   // alternative: from domainAssembly in domainAssembly.GetExportedTypes()
               from type in domainAssembly.GetTypes()
               where typeof(Bullet).IsAssignableFrom(type)
               // alternative: && type != typeof(B)
               // alternative: && ! type.IsAbstract
               // alternative: where type.IsSubclassOf(typeof(B))
               select type).Select(x => (Bullet)Activator.CreateInstance(x));
        Register(listOfBs);

        // this finds all the instantiable bullets, but this is hardly what I want to do. 
        // I need to be able to register multiple "Explosive bullet" instances with different settings
    }

    public void Register(IEnumerable<Bullet> b)
    {
        RegisteredBullets.AddRange(b);
    }
    public void Register(Bullet b)
    {
        RegisteredBullets.Add(b);
    }
}

public record Bullet
{
    public int BulletType = 0;

    public Vector2 Position, Velocity;
    public float Life = 2.0f;
    public float BulletSpeed = 15.0f;
    
    public Color Color = new(new Vector4(1.0f));
    public Vector2 Size = new(10, 10);
    public float Friction = 2f;
    public int EntityId = IdManager.NextId();
    public float Damage = 5f;
    public float SplashDamageRange = 0.0f;
    public float SplashDamage = 0.0f;

    public float Spread = 0.0f; // precise
    public float Cooldown = .1f;

    public string Label = "Bullet";
}


public record ExplosiveBullet : Bullet
{
    public ExplosiveBullet()
    {
        Label = "Exp";
        BulletType = 1;
        Friction = 10.5f;
        Damage = 10.0f;
        BulletSpeed = 10f;
        SplashDamageRange = 50.0f;
        SplashDamage = 0.1f;
        Cooldown = 0.1f;

    }
}

public record MagicalBullet : Bullet
{
    public MagicalBullet()
    {
        Label = "Mag";
        BulletType = 2;
        Color = new(.4f, .1f, 1.0f, 1.0f);
        Damage = 8.0f;
        BulletSpeed = 2.0f;
        SplashDamageRange = 2.0f;
        SplashDamage = 2.0f;
        Cooldown = 0.2f;
    }
}

public record MiniGunBullet : Bullet
{
    public MiniGunBullet()
    {
        Label = "Mini";
        BulletType =4;
        Color = Color.LightGreen;
        Damage = 2.0f;
        BulletSpeed = 20.0f;
        Spread = .1f;
        Life = 1f;
        Cooldown = 0.02f;
        SplashDamageRange = 0.0f;
        SplashDamage = 0.0f;
    }
}
public record BouncyBullet : Bullet
{
    public BouncyBullet()
    {
        Label = "Bounc";
        BulletType = 3;
        Color = new(.4f, .1f, 1.0f, 1.0f);
        Damage = 8.0f;
        BulletSpeed = 2.0f;
        Cooldown = 0.4f;
        SplashDamageRange = 2.0f;
        SplashDamage = 2.0f;
    }
}