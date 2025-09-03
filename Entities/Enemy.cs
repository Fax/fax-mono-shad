using Microsoft.Xna.Framework;



public class EnemyEntity
{
    public readonly int EntityId = IdManager.NextId();
    public float Life = 2.0f;
    public Vector2 Size = new(30f);
    public Color Color = Color.Red;
    public float Speed = 25.0f;
    public float Rotation = 0.0f;
    public Vector2 Position;
    public bool Active = true;
}
