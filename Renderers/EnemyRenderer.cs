using System;
using System.Runtime.Intrinsics.X86;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public class EnemyRenderer
{
    private List<EnemyEntity> _enemies;
    private readonly SpriteBatch _spriteBatch;
    private readonly TextureManager _textureManager;

    public EnemyRenderer(SpriteBatch spriteBatch, TextureManager textureManager, List<EnemyEntity> enemies)
    {
        _enemies = enemies;
        _spriteBatch = spriteBatch;
        _textureManager = textureManager;
    }


    public void Render()
    {
        var tex = _textureManager.GetTexture("enemy");
        Vector2 origin = new((float)(tex.Bounds.Width) / 2, (float)(tex.Bounds.Height) / 2);

        foreach (var enemy in _enemies.Where(x => x.Active))
        {
            // draw bounding box too


            _spriteBatch.Draw(
                tex,
                enemy.Position.ToBoundingBox(enemy.Size),
                tex.Bounds,

                enemy.Color,
                0,
                origin,
                SpriteEffects.None,
                0);
        }
    }

}
