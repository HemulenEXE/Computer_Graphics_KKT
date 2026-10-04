using lab4.Models;
using lab4.Services;
using System.Drawing.Drawing2D;

namespace lab4;

public partial class MainForm : Form
{
	private readonly PolygonManager _polygonManager = new();

	private const int PointRadius = 5;

	public MainForm()
	{
		InitializeComponent();
		DoubleBuffered = true;
	}

	private void MainForm_Load(object sender, EventArgs e)
	{
	}

	private void Canvas_MouseClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			_polygonManager.AddPoint(e.Location);
			canvas.Invalidate();
			return;
		}

		if (e.Button == MouseButtons.Right)
		{
			_polygonManager.FinishCurrentPolygon();
			canvas.Invalidate();
		}
	}

	private void Canvas_Paint(object sender, PaintEventArgs e)
	{
		e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
		foreach (Polygon polygon in _polygonManager.Polygons)
			DrawPolygon(e.Graphics, polygon);

		if (_polygonManager.CurrentPolygon is not null)
			DrawPolygon(e.Graphics, _polygonManager.CurrentPolygon, true);
	}

	private static void DrawPolygon(Graphics graphics, Polygon polygon, bool isCurrent = false)
	{
		if (polygon.Vertices.Count == 0)
			return;

		using Pen pen = new(Color.Black, 2);

		if (polygon.IsPolygon)
		{
			graphics.DrawPolygon(pen, polygon.Vertices.ToArray());

			using Brush brush = new SolidBrush(Color.FromArgb(50, Color.SteelBlue));
			graphics.FillPolygon(brush, polygon.Vertices.ToArray());
		}
		else if (polygon.IsEdge)
			graphics.DrawLine(pen, polygon.Vertices[0], polygon.Vertices[1]);

		foreach (var point in polygon.Vertices)
			DrawPoint(graphics, point, isCurrent);
	}

	private static void DrawPoint(Graphics graphics, PointF point, bool isCurrent)
	{
		float diameter = PointRadius * 2;
		using Brush brush = new SolidBrush(isCurrent ? Color.Red : Color.Black);
		graphics.FillEllipse(brush, point.X - PointRadius, point.Y - PointRadius, diameter, diameter);
	}

	private void ClearButton_Click(object sender, EventArgs e)
	{
		_polygonManager.Clear();
		canvas.Invalidate();
	}

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        _polygonManager.FinishCurrentPolygon();
        canvas.Invalidate();
        e.Handled = true;
    }
}
