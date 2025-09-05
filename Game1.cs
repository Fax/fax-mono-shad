using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using System.Linq.Expressions;

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
    AreaEffectManager areaEffectManager;
    EnemyManager enemyManager;
    EnemyRenderer enemyRenderer;
    BulletSpawner bulletSpawner;


    // not currently hooked up as the "on shoot" event is not wired
    BulletEcsManager bulletEcsManager;

    PickupSpawner pickupSpawner;
    PickupManager pickupManager;
    PickupRenderer pickupRenderer;
    ExperienceManager expManager;
    ExperienceRenderer expRenderer;
    CollisionManager collisionManager = new CollisionManager();
    List<PickupEntity> pickups = new();
    List<ExperienceEntity> nuggets = new();
    BulletRegistry bulletRegistry = BulletRegistry.Instance;
    UIRenderer gui;


    private OrthographicCamera _camera;

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

        var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 800, 480);
        _camera = new OrthographicCamera(viewportAdapter);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Sfx.Init();
        inputManager = new();
        textureManager = new(GraphicsDevice, Content);
        UIHelpers.Init(_spriteBatch, textureManager);
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

        bulletEcsManager = new BulletEcsManager(_spriteBatch, textureManager);

        pickupSpawner = new PickupSpawner(eventBus, pickups);
        pickupManager = new PickupManager(pickups);
        pickupRenderer = new PickupRenderer(_spriteBatch, textureManager);
        enemyManager = new EnemyManager(eventBus, enemies);
        enemyRenderer = new EnemyRenderer(_spriteBatch, textureManager, enemies);
        expManager = new ExperienceManager(eventBus, nuggets, player);
        expRenderer = new ExperienceRenderer(_spriteBatch, textureManager, nuggets);
        areaEffectManager = new AreaEffectManager(eventBus);
        gui = new(_spriteBatch, textureManager, _screenSize, player);
    }
    public bool Pause = false;
    public bool prev_p = false;


    int scroll = 0;

    private void AdjustZoom()
    {
        var state = Mouse.GetState();

        float zoomPerTick = 0.01f;
        if (state.ScrollWheelValue > scroll)
        {
            _camera.ZoomIn(zoomPerTick);
        }
        if (state.ScrollWheelValue < scroll)
        {
            _camera.ZoomOut(zoomPerTick);
        }
        scroll = state.ScrollWheelValue;
    }

    private void CameraFollow(float dt, Vector2 target)
    {
        var d = Vector2.Distance(target, _camera.Position);
        if (d > 10)
        {
            _camera.Position = Vector2.Lerp(target, _camera.Position, .991f);
        }
        else
        if (d > 5)
        {
            _camera.Position = Vector2.Lerp(target, _camera.Position, .92f);
        }
        else
        if (d > 1)
        {
            _camera.Position = Vector2.Lerp(target, _camera.Position, .9f);
        }

    }
    protected override void Update(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (!IsActive) return;

        eventBus.Drain(); // Drain all the events emitted on the previous frame
        CameraFollow(dt, player.Position - _screenSize / 2);
        AdjustZoom();
        var k = Keyboard.GetState();
        var m = Mouse.GetState();


        if (k.IsKeyDown(Keys.R))
        {
            BulletRegistry.Instance.Reset();
        }

        if (inputManager.current.Next)
        {

            eventBus.Publish<WeaponPickupEvent>(new WeaponPickupEvent((player.weapon.CurrentBullet.BulletType + 1) % (BulletRegistry.Instance.RegisteredBullets.Count ), 0, player.EntityId));
        }


        inputManager.Update(k, m, _camera);

        var c = k.IsKeyDown(Keys.P);
        if (c != prev_p && c)
        {
            Pause = !Pause;
        }
        prev_p = c;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        if (Pause) return;


        player.Update(inputManager, dt);

        bulletSpawner.Update(dt);
        // this is a test. Monogame Extended has a small ECS embedded, and I have no idea of how it behaves, so I am testing it with the bullets.
        bulletEcsManager.Update(gameTime);
        pickupSpawner.Update(dt);
        pickupManager.Update(dt);
        areaEffectManager.Update(dt);
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
                    eventBus.Publish<BulletCollisionEvent>(new BulletCollisionEvent(b.EntityId, enemy.EntityId, b.Damage));
                }

            }
        }

        // inefficient, but I need it working now
        foreach (var a in areaEffectManager.Areas.Where(x => x.IsCollider))
        {
            foreach (var enemy in enemies)
            {
                var enemyBox = enemy.Position.ToCenteredBoundingBox(enemy.Size);
                if (collisionManager.CheckCollision(a.BoundingBox, enemyBox))
                {
                    eventBus.Publish<AreaCollisionEvent>(new AreaCollisionEvent(a.EntityId, enemy.EntityId, a.Damage));
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

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        var transformMatrix = _camera.GetViewMatrix();
        _spriteBatch.Begin(transformMatrix: transformMatrix);
        player.DebugDraw(_spriteBatch, inputManager, textureManager.GetTexture("player"));
        areaEffectManager.Render(_spriteBatch, textureManager);
        playerRenderer.Render(player, Color.LawnGreen);
        bulletEcsManager.Draw(gameTime);// the new renderer
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
        _spriteBatch.End();
        // the ui is on another thread
        _spriteBatch.Begin();
        gui.Render();

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
