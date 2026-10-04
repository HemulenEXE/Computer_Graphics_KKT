using lab4.Models;

namespace lab4.Services;

public class PolygonManager
{
    private readonly List<Polygon> _polygons = [];

    public IReadOnlyList<Polygon> Polygons => _polygons;

    public Polygon? CurrentPolygon { get; private set; }

    public void AddPoint(PointF point)
    {
        CurrentPolygon ??= new Polygon();
        CurrentPolygon.AddPoint(point);
    }

    public void FinishCurrentPolygon()
    {
        if (CurrentPolygon is null || CurrentPolygon.IsEmpty)
        {
            CurrentPolygon = null;
            return;
        }

        _polygons.Add(CurrentPolygon);
        CurrentPolygon = null;
    }

    public void Clear()
    {
        _polygons.Clear();
        CurrentPolygon = null;
    }
}
