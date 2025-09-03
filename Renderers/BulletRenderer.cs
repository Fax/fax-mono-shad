using System;
using System.Runtime.Intrinsics.X86;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class BulletRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly TextureManager _textureManager;

    public BulletRenderer(SpriteBatch spriteBatch, TextureManager textureManager)
    {
        _spriteBatch = spriteBatch;
        _textureManager = textureManager;
    }

    public void Render(Bullet p)
    {
        var tex = _textureManager.GetTexture("bullet");
        Vector2 origin = new((float)(tex.Bounds.Width) / 2, (float)(tex.Bounds.Height) / 2);


        _spriteBatch.Draw(
            tex,
            p.Position.ToBoundingBox(p.Size),
            tex.Bounds,
            p.Color,
              0,
            origin,
            SpriteEffects.None,
            0);
    }


}
