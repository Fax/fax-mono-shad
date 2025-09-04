using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace fax_mono_shad;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private InputManager inputManager;
    private TextureManager textureManager;

    Player player;
    PlayerRenderer playerRenderer;
    BulletRenderer bulletRenderer;

    EventBus eventBus = new();
    List<Bullet> bullets = new List<Bullet>();

    List<EnemyEntity> enemies = new List<EnemyEntity>();
    EnemyManager enemyManager;
    EnemyRenderer enemyRenderer;
    BulletSpawner bulletSpawner;
    PickupSpawner pickupSpawner;
    PickupManager pickupManager;
    PickupRenderer pickupRenderer;
    ExperienceManager expManager;
    ExperienceRenderer expRenderer;
    CollisionManager collisionManager = new CollisionManager();
    List<PickupEntity> pickups = new();
    List<ExperienceEntity> nuggets = new();

    UIRenderer gui;
    private Vector2 _screenSize;
    private void UpdateScreenSize()
    {
        int w = GraphicsDevice.Viewport.Width;
        int h = GraphicsDevice.Viewport.Height;

        // update your corner positions here
        _screenSize = new Vector2(w, h);
    }
    private void OnResize(object sender, EventArgs e)
    {
        UpdateScreenSize();
    }
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        Content.RootDirectory = "Content";
        IsMouseVisible = true;



    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Sfx.Init();
        inputManager = new();
        textureManager = new(GraphicsDevice);
        UpdateScreenSize();
        Window.ClientSizeChanged += OnResize;
        textureManager.Initialize(new Dictionary<string, Color>
        {
            { "enemy", Color.White},
            { "player", Color.White},
            { "exp", Color.Gray},
            { "bar", Color.DimGray},
            { "barfill", Color.SlateGray},
            { "bullet", Color.LightGray},
            { "pickup", Color.LightSlateGray},

        });
        // TODO: use this.Content to load your game content here

        player = new Player(eventBus) { Position = new(_screenSize.X / 2, _screenSize.Y / 2), weapon = new Weapon(eventBus), Level = 0, Experience = 0 };
        playerRenderer = new PlayerRenderer(_spriteBatch, textureManager);
        bulletRenderer = new BulletRenderer(_spriteBatch, textureManager);
        bulletSpawner = new BulletSpawner(eventBus, bullets);
        pickupSpawner = new PickupSpawner(eventBus, pickups);
        pickupManager = new PickupManager(pickups);
        pickupRenderer = new PickupRenderer(_spriteBatch, textureManager);
        enemyManager = new EnemyManager(eventBus, enemies);
        enemyRenderer = new EnemyRenderer(_spriteBatch, textureManager, enemies);
        expManager = new ExperienceManager(eventBus, nuggets, player);
        expRenderer = new ExperienceRenderer(_spriteBatch, textureManager, nuggets);
        gui = new(_spriteBatch, textureManager, _screenSize, player);
    }
    public bool Pause = false;
    public bool prev_p = false;
    protected override void Update(GameTime gameTime)
    {
        if (!IsActive) return;
        var k = Keyboard.GetState();
        var m = Mouse.GetState();
        if (m.LeftButton == ButtonState.Pressed)
        {
            Console.WriteLine("Click");
        }

        inputManager.Update(k, m);

        var c = k.IsKeyDown(Keys.P);
        if (c != prev_p && c)
        {
            Pause = !Pause;
        }
        prev_p = c;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        if (Pause) return;
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;


        player.Update(inputManager, dt);

        bulletSpawner.Update(dt);
        pickupSpawner.Update(dt);
        pickupManager.Update(dt);
        enemyManager.Update(dt, player.Position);

        // TODO: Add your update logic here
        foreach (var b in bullets)
        {
            var box = b.Position.ToCenteredBoundingBox(b.Size);
            foreach (var enemy in enemies)
            {
                var enemyBox = enemy.Position.ToCenteredBoundingBox(enemy.Size);
                if (collisionManager.CheckCollision(box, enemyBox))
                {
                    eventBus.Publish<BulletCollisionEvent>(new BulletCollisionEvent(b.EntityId, enemy.EntityId));
                }

            }
        }

        foreach (var x in pickups)
        {
            var box = new Rectangle((int)x.Position.X, (int)x.Position.Y, (int)x.Size.X, (int)x.Size.Y);
            if (collisionManager.CheckCollision(player.BoundingBox, box))
            {
                switch (x)
                {
                    case WeaponPickup we:
                        eventBus.Publish<WeaponPickupEvent>(new WeaponPickupEvent(we.BulletType, we.Id, player.EntityId));
                        break;
                }
            }
        }

        var playerRangeBox = player.Position.ToCenteredBoundingBox(player.Size, new(150.0f));
        var playerBox = player.BoundingBox;
        foreach (var nugget in nuggets.Where(x => x.Active))
        {
            if (nugget.Speed > 0)
            {
                // this is already moving
                if (collisionManager.CheckCollision(playerBox, nugget.Position.ToBoundingBox(new(15.0f))))
                {
                    nugget.Active = false;
                    eventBus.Publish<CollectExperienceEvent>(new CollectExperienceEvent(nugget.Amount, nugget.Position));
                }
            }
            else
            {
                if (collisionManager.CheckCollision(playerRangeBox, nugget.Position.ToBoundingBox(new(15f))))
                {
                    nugget.Speed += .9f;
                }
            }
        }

        expManager.Update(dt);
        eventBus.Drain();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();
        player.DebugDraw(_spriteBatch, inputManager, textureManager.GetTexture("player"));
        playerRenderer.Render(player, Color.LawnGreen);
        foreach (var b in bullets)
        {
            bulletRenderer.Render(b);
        }
        foreach (var p in pickups)
        {
            pickupRenderer.Render(p);
        }
        expRenderer.Render();
        enemyRenderer.Render();

        gui.Render();

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
