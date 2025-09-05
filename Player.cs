// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public enum PlayerMovementState
{
    Normal,
    Dash
}

public class Player
{


    public int Score = 0;
    public int Experience = 0;
    public readonly int EntityId = IdManager.NextId();
    public PlayerMovementState State = PlayerMovementState.Normal;
    public Vector2 CurrentDashDirection = Vector2.Zero;
    public float DashTimer = .2f;

    public float Speed = 10f;
    public float DashSpeed = 50f;

    public float DashCooldown = 10f;
    public float DashCounter = 0.0f;
    public float DashCooldownCounter = 0.0f;
    public Player(EventBus bus)
    {
        _bus = bus;
        _bus.Subscribe<WeaponPickupEvent>(PickUpWeapon);
        _bus.Subscribe<ScoreEvent>(ScoreChange);
        _bus.Subscribe<CollectExperienceEvent>(GetExp);
    }

    public void GetExp(CollectExperienceEvent evt)
    {
        Sfx.PlayPling();
        Experience += evt.Amount;
        Console.WriteLine($"Experience: {Experience}");
        if (Experience > Levels.NextLevelExperience(Level))
        {
            Level++;
            _bus.Publish<NextLevelEvent>(new NextLevelEvent(Level));
        }
    }
    public void ScoreChange(ScoreEvent evt)
    {
        Score += evt.Score;
    }
    public void PickUpWeapon(WeaponPickupEvent evt)
    {
        if (evt.PlayerId != EntityId) return;
        // do not subscribe again
        if (weapon == null)
        {
            weapon = new Weapon(_bus) { CurrentBullet = BulletRegistry.Instance.RegisteredBullets.First() };
        }
    }
    public Vector2 Position = new Vector2(100, 100);

    public float Rotation = 0.0f;
    public Vector2 Size = new(20.0f);
    public Rectangle BoundingBox => new Rectangle
    (
        (int)Position.X,
        (int)Position.Y,
        (int)Size.X,
        (int)Size.Y
    );

    public int Level;

    public Weapon? weapon;
    private readonly EventBus _bus;

    public void DebugDraw(SpriteBatch _spritebatch, InputManager inputManager, Texture2D texture)
    {
        _spritebatch.Draw(texture, inputManager.current.MousePosition.ToRectangle(new Vector2(15f)), Color.DarkRed);
    }

    public void HandleNormal(InputManager inputManager, float dt, Vector2 aimDirection)
    {
        // movement input as a single vector
        var move = new Vector2(
            (inputManager.current.Right ? 1f : 0f) - (inputManager.current.Left ? 1f : 0f),
            (inputManager.current.Down ? 1f : 0f) - (inputManager.current.Up ? 1f : 0f)
        );
        if (move != Vector2.Zero) move = move.Normalized();

        // apply movement
        Position += move * (10f * Speed * dt);

        // dash start?
        if (inputManager.current.Dash && DashCounter == 0f && DashCooldownCounter == 0f)
        {
            // pick dash dir from movement; fallback to aim
            CurrentDashDirection = move != Vector2.Zero ? move : aimDirection;
            State = PlayerMovementState.Dash;
            DashCounter = DashTimer;
        }
    }
    public void HandleDash(float dt)
    {
        if (DashCounter <= 0f)
        {
            DashCounter = 0f;
            State = PlayerMovementState.Normal;
            DashCooldownCounter = DashCooldown;
            return;
        }

        DashCounter -= dt;
        Position += CurrentDashDirection * (10f * DashSpeed * dt);
    }

    public void Update(InputManager inputManager, float dt)
    {
        var mouse = inputManager.current.MousePosition;
        var dirToMouse = (mouse - Position);
        var aimDir = dirToMouse == Vector2.Zero ? new Vector2(0f, -1f) : dirToMouse.Normalized();
        Rotation = MathF.Atan2(aimDir.X, -aimDir.Y);

        // --- State machine ---
        switch (State)
        {
            case PlayerMovementState.Normal:
                HandleNormal(inputManager, dt, aimDir);
                break;

            case PlayerMovementState.Dash:
                HandleDash(dt);
                break;
        }

        // --- Cooldowns ---
        if (DashCooldownCounter > 0f)
            DashCooldownCounter = MathF.Max(0f, DashCooldownCounter - dt * 10f);

        // --- Weapon ---
        if (weapon != null)
        {
            if (inputManager.current.Shoot)
                weapon.TryShoot(Position, dirToMouse);
            weapon.Update(dt);
        }
    }
}


