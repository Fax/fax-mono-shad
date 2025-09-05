using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


public class AreaEffectManager
{
    private readonly EventBus _bus;
    public List<AreaEffect> Areas = new List<AreaEffect>();
    public AreaEffectManager(EventBus bus)
    {
        _bus = bus;
        _bus.Subscribe<SplashDamageEvent>(SpawnSplashDamageArea);

    }

    void SpawnSplashDamageArea(SplashDamageEvent evt)
    {

        Areas.Add(new AreaEffect { Damage = evt.SplashDamage,
            Center = evt.Position, 
            Range = evt.SplashDamageRange, 
            Duration = 4.0f,
            IsCollider = true, 
            IsRendered = true, 
            Color = Color.Orange });
    }

    public void Update(float dt)
    {
        // here I just countdown the duration of the splash damage
        foreach (var areaEffect in Areas)
        {
            areaEffect.Duration -= dt;
            if (areaEffect.Duration <= 0)
            {
                areaEffect.IsRendered = false;
                areaEffect.IsCollider = false;
            }
        }

        Areas.RemoveAll(x => x.Duration <= 0);
    }
    public void Render(SpriteBatch spriteBatch, TextureManager textureManager)
    {
        var tex = textureManager.GetTexture("bullet");
        Vector2 origin = new((float)(tex.Bounds.Width) / 2, (float)(tex.Bounds.Height) / 2);

        // grabbing a bullet, and scaling it up
        foreach (var areaEffect in Areas)
        {
            spriteBatch.Draw(
                 tex,
                 areaEffect.Center.ToBoundingBox(new(areaEffect.Range)),
                 tex.Bounds,
                 areaEffect.Color,
                   0,
                 origin,
                 SpriteEffects.None,
                 0);
        }
    }

}

