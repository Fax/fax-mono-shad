using Microsoft.Xna.Framework.Graphics;

public class TextureManager
{
    private readonly GraphicsDevice _device;
    Dictionary<string, Texture2D> _textures;
    public TextureManager(GraphicsDevice device)
    {
        _device = device;
        _textures = new Dictionary<string, Texture2D>();
    }

    public Texture2D GetTexture(string key)
    {
        return _textures[key];
    }

    public void Initialize(Dictionary<string, Color> entities)
    {
        foreach (var r in entities)
            GenerateSquare(r.Key, r.Value);
    }

    public void GenerateSquare(string key, Color color)
    {
        var texture = new Texture2D(_device, 1, 1);
        texture.SetData(new[] { color });
        _textures.Add(key, texture);
    }
    

}