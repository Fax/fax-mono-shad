// See https://aka.ms/new-console-template for more information


using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Microsoft.Xna.Framework.Graphics;

class PlayerRenderer
{
    private readonly SpriteBatch _spriteBatch;
    private readonly TextureManager _textureManager;

    public PlayerRenderer(SpriteBatch spriteBatch, TextureManager textureManager)
    {
        _spriteBatch = spriteBatch;

        _textureManager = textureManager;

    }

    public void Render(Player p, Color color)
    {

        var ptext = _textureManager.GetTexture("player");


        _spriteBatch.Draw(
            ptext,
            p.Position.ToRectangle(p.Size),
            ptext.Bounds,
            Color.Red,
            p.Rotation,
            new(((float)ptext.Width) / 2.0f, ((float)ptext.Height) / 2.0f),
            SpriteEffects.None,
            0
            );

    }

}


