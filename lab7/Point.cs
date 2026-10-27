namespace PolyhedraLab;

public sealed class Point
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public Point(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Point Clone() => new(X, Y, Z);
}
