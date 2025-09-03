using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


public class ExperienceEntity
{
    public int Amount = 100;
    public bool Active = true;
    public Vector2 Position;
    public float Speed = 0;


    public Color Color
    {
        get
        {
            return Amount switch
            {
                < 1000 => Color.Blue,
                < 10000 => Color.Purple,
                < 100000 => Color.Green,
                _ => Color.Blue
            };
        }
    }
}