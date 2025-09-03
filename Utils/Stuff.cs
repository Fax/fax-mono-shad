

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

public class CollisionManager
{

    public bool CheckCollision(Rectangle a, Rectangle b)
    {

        return a.IntersectsWith(b);
    }
}
public static class Levels
{
    public static int NextLevelExperience(int currentLevel)
    {
        return (int)(1000 * Math.Pow(1.8, currentLevel));
    }
}
