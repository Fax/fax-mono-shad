public static class RectangleExtensions
{
    public static readonly Rectangle Empty;
    public static Rectangle ToRectangle(this Vector2 position, Vector2 size)
    {
        int x = (int)position.X, y = (int)position.Y, w = (int)size.X, h= (int)size.Y;
        return new Rectangle(x, y, w, h);
    }
    public static Rectangle Intersect(this Rectangle a, Rectangle b)
    {
        int x1 = Math.Max(a.X, b.X);
        int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
        int y1 = Math.Max(a.Y, b.Y);
        int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

        if (x2 >= x1 && y2 >= y1)
        {
            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }

        return Empty;
    }
    public static bool IntersectsWith(this Rectangle a, Rectangle b)
    {

        return (b.X < a.X + a.Width) && (a.X < b.X + b.Width) &&
           (b.Y < a.Y + a.Height) && (a.Y < b.Y + b.Height);
    }

    public static bool Contains(this Rectangle a, int x, int y) => a.X <= x && x < a.X + a.Width && a.Y <= y && y < a.Y + a.Height;
    public static bool Contains(this Rectangle a, Vector2 v) => a.X <= v.X && v.X < a.X + a.Width && a.Y <= v.Y && v.Y < a.Y + a.Height;
    public static bool Contains(this Rectangle a, Point pt) => a.X <= pt.X && pt.X < a.X + a.Width && a.Y <= pt.Y && pt.Y < a.Y + a.Height;

    public static Rectangle Union(this Rectangle a, Rectangle b)
    {

        int x1 = Math.Min(a.X, b.X);
        int x2 = Math.Max(a.X + a.Width, b.X + b.Width);
        int y1 = Math.Min(a.Y, b.Y);
        int y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

        return new Rectangle(x1, y1, x2 - x1, y2 - y1);
    }
}