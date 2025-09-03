




public static class ColorExtensions
{
    public static Vector4 ToVector(this Color color)
    {
        return new Vector4(color.R, color.G, color.B, color.A);
    }
}

public static class Vector2Extensions
{

    public static Vector2 Normalized(this Vector2 v)
    {
        v.Normalize();
        return v;
    }

    public static Rectangle ToBoundingBox(this Vector2 origin, Vector2 size)
    {
        return new Rectangle((int)origin.X, (int)origin.Y, (int)size.X, (int)size.Y);
    }

    public static Rectangle ToCenteredBoundingBox(this Vector2 cornerOrigin, Vector2 entitySize)
    {

        var x = entitySize.X / 2;
        var y = entitySize.Y / 2;
        return new Rectangle((int)(cornerOrigin.X - x), (int)(cornerOrigin.Y - y), (int)entitySize.X, (int)entitySize.Y);
    }
    public static Rectangle ToCenteredBoundingBox(this Vector2 cornerOrigin, Vector2 entitySize, Vector2 size)
    {

        var r = cornerOrigin.ToRectangle(size);
        r.X -= r.Width / 2;
        r.Y -= r.Height / 2;
        return r;
        var cx = entitySize.X / 2;
        var cy = entitySize.Y / 2;

        var x = size.X / 2;
        var y = size.Y / 2;
        return new Rectangle((int)(cornerOrigin.X + cx - x), (int)(cornerOrigin.Y + cy - y), (int)size.X, (int)size.Y);
    }
}