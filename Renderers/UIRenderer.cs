




using Microsoft.Xna.Framework.Graphics;

static class UIHelpers
{
    public static SpriteBatch _spriteBatch;
    public static TextureManager _textureManager;

    public static void Init(SpriteBatch s, TextureManager m)
    {
        _spriteBatch = s;
        _textureManager = m;
    }
    public static void RenderHorizontalBar(Vector2 position, float lenght, float height, float filledPc, bool inverted = false)
    {
        Vector2 pos = position;
        Vector2 siz = new(lenght, height);
        var barRect = pos.ToRectangle(siz);
        _spriteBatch.Draw(
            _textureManager.GetTexture("bar"),
            barRect,
            Color.AliceBlue);
        var pc = inverted ? 1 - filledPc : filledPc;
        Vector2 sizFilled = new(lenght * pc, height);
        var filledRec = pos.ToRectangle(sizFilled);
        _spriteBatch.Draw(
                    _textureManager.GetTexture("bar"),
                    filledRec,
                    Color.BlueViolet);
    }
}

class UIRenderer
{
    private Vector2 _screenSize;
    private readonly Player _player;
    private readonly SpriteBatch _spriteBatch;
    private readonly TextureManager _textureManager;

    public UIRenderer(SpriteBatch spriteBatch, TextureManager textureManager, Vector2 screenSize, Player player)
    {
        _screenSize = screenSize;
        _player = player;
        _spriteBatch = spriteBatch;
        _textureManager = textureManager;
    }

    int lastLevel = 0;
    int lastStart = 0;
    int lastCap = 0;



    public void Render()
    {

        Vector2 pos = new(10, _screenSize.Y - 20);
        var l = _player.Level;
        if (l != lastLevel || lastCap == 0)
        {
            lastLevel = _player.Level;
            lastCap = Levels.NextLevelExperience(l);
            lastStart = l == 0 ? 0 : Levels.NextLevelExperience(l - 1);
        }
        float exp = _player.Experience;

        // width
        float end = lastCap - lastStart;
        float fillPc = ((exp - lastStart) / end);

        UIHelpers.RenderHorizontalBar(pos, _screenSize.X - 20, 10, fillPc);

        Vector2 cooldownPosition = new(10, _screenSize.Y - 40);
        float cooldownLen = 100;
        float coolDownPerc = _player.DashCooldownCounter / _player.DashCooldown;
        UIHelpers.RenderHorizontalBar(cooldownPosition, cooldownLen, 10, coolDownPerc, inverted: true);


        Vector2 shootCooldownTimerPosition = new(cooldownLen +50 , _screenSize.Y - 40);
        float shootCoolDownPerc = _player.weapon.CooldownTimer / _player.weapon.CurrentBullet.Cooldown;
        UIHelpers.RenderHorizontalBar(shootCooldownTimerPosition, cooldownLen, 10, shootCoolDownPerc, inverted: true);

        Vector2 gunPosition = new(10, _screenSize.Y - 60);

        var b = _player.weapon.CurrentBullet;
        _spriteBatch.DrawString(_textureManager.GetFont(), $"{b.Label} S{b.BulletSpeed} C{b.Cooldown} D{b.Damage} F{b.Friction}", gunPosition, Color.Black);
    }
}