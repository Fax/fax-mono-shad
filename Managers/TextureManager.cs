using Microsoft.Xna.Framework.Graphics;

public class TextureManager
{
    private readonly GraphicsDevice _device;
    Dictionary<string, Texture2D> _textures;
    private SpriteFont? _uiFont;
    public TextureManager(GraphicsDevice device, Microsoft.Xna.Framework.Content.ContentManager content)
    {
        _device = device;
        _textures = new Dictionary<string, Texture2D>();
        _uiFont = content.Load<SpriteFont>("UIFont");
    }
    public SpriteFont GetFont()
    {
        if (_uiFont == null) throw new Exception("font missing");
        return _uiFont;
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