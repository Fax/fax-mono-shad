


using Microsoft.Xna.Framework.Graphics;

public class ExperienceRenderer 
{
    private List<ExperienceEntity> _nuggets;
    private readonly SpriteBatch _spriteBatch;
    private readonly TextureManager _textureManager;

    public ExperienceRenderer(SpriteBatch spriteBatch, TextureManager textureManager, List<ExperienceEntity> nuggets)
    {
        _nuggets = nuggets;
        _spriteBatch = spriteBatch;
        _textureManager = textureManager;
    }


    public void Render()
    {
        foreach (var nugget in _nuggets.Where(x => x.Active))
        {
            _spriteBatch.Draw(
               _textureManager.GetTexture("exp"),
               nugget.Position.ToRectangle(new Vector2(10.0f)),
               nugget.Color);

        }
    }

}
