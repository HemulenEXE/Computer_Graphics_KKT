namespace lab4.Models;

public class Polygon
{
    public List<PointF> Vertices { get; } = [];

    public bool IsEmpty => Vertices.Count == 0;

    public bool IsPoint => Vertices.Count == 1;

    public bool IsEdge => Vertices.Count == 2;

    public bool IsPolygon => Vertices.Count >= 3;

    public void AddPoint(PointF point)
    {
        Vertices.Add(point);
    }
    public void ChangePoints(List<PointF> points)
    {
        Vertices.Clear();
        Vertices.AddRange(points);
    }
}
