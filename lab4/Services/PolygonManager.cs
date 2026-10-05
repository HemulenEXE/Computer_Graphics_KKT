using lab4.Models;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace lab4.Services;

public class PolygonManager
{
    private readonly List<Polygon> _polygons = [];
    private readonly MatrixOperations _matrixManager = new MatrixOperations();

    public IReadOnlyList<Polygon> Polygons => _polygons;

    public Polygon? CurrentPolygon { get; private set; }
    public Polygon? SelectedPolygon { get; private set; }

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
        SelectedPolygon = CurrentPolygon;
        CurrentPolygon = null;
    }

    public void TranslationSelectedPolygon(float dx, float dy)
    {
        if (SelectedPolygon is null || SelectedPolygon.IsEmpty)
        {
            SelectedPolygon = null;
            return;
        }

        List<PointF> new_points = _matrixManager.TranslationPoints(SelectedPolygon.Vertices, dx, dy);

        SelectedPolygon.ChangePoints(new_points);
    }
    public void RotationSelectedPolygon(PointF point, float degrees)
    {
        if (SelectedPolygon is null || SelectedPolygon.IsEmpty)
        {
            SelectedPolygon = null;
            return;
        }

        List<PointF> new_points = _matrixManager.RotationPoints(SelectedPolygon.Vertices, point, degrees);

        SelectedPolygon.ChangePoints(new_points);
    }
    public void RotationCenterSelectedPolygon(float degrees)
    {
        if (SelectedPolygon is null || SelectedPolygon.IsEmpty)
        {
            SelectedPolygon = null;
            return;
        }

        List<PointF> new_points = _matrixManager.RotationCenterPoints(SelectedPolygon.Vertices, degrees);

        SelectedPolygon.ChangePoints(new_points);
    }
    public void DilatationSelectedPolygon(PointF point, float a, float b)
    {
        if (SelectedPolygon is null || SelectedPolygon.IsEmpty)
        {
            SelectedPolygon = null;
            return;
        }

        List<PointF> new_points = _matrixManager.DilatationPoints(SelectedPolygon.Vertices, point, a, b);

        SelectedPolygon.ChangePoints(new_points);
    }
    public void DilatationCenterSelectedPolygon(float a, float b)
    {
        if (SelectedPolygon is null || SelectedPolygon.IsEmpty)
        {
            SelectedPolygon = null;
            return;
        }

        List<PointF> new_points = _matrixManager.DilatationCenterPoints(SelectedPolygon.Vertices, a, b);

        SelectedPolygon.ChangePoints(new_points);
    }

    public void Clear()
    {
        _polygons.Clear();
        CurrentPolygon = null;
        SelectedPolygon = null;
    }
}
