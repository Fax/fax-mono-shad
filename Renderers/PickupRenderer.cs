


using Microsoft.Xna.Framework.Graphics;

public class PickupRenderer
{

    private readonly SpriteBatch _spriteBatch;
    private readonly TextureManager _textureManager;

    public PickupRenderer(SpriteBatch spriteBatch, TextureManager textureManager)
    {

        _spriteBatch = spriteBatch;
        _textureManager = textureManager;
    }

    public void Render(PickupEntity p)
    {
        var tex = _textureManager.GetTexture("pickup");
        _spriteBatch.Draw(
           tex,
           p.Position.ToRectangle(p.Size),
           tex.Bounds,
           p.Color,
           p.Rotation,
           new((float)(tex.Bounds.Width) / 2, (float)(tex.Bounds.Height) / 2),
           SpriteEffects.None,
           0);

        

    }


}
