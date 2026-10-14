using System.Drawing.Drawing2D;

namespace MidpointDisplacement;

public sealed class MountainPainter : Control
{
	private IReadOnlyList<PointF> _points = Array.Empty<PointF>();

	public MountainPainter()
	{
		DoubleBuffered = true;
		BackColor = Color.FromArgb(245, 247, 250);
		ForeColor = Color.FromArgb(44, 62, 80);
		MinimumSize = new Size(400, 300);
		Dock = DockStyle.Fill;
	}

	public void SetPoints(IReadOnlyList<PointF> points)
	{
		_points = points;
		Invalidate();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);

		Graphics g = e.Graphics;
		g.SmoothingMode = SmoothingMode.AntiAlias;

		int left = 45;
		int right = Width - 25;
		int top = 30;
		int bottom = Height - 35;

		if (right <= left || bottom <= top)
			return;

		using Pen gridPen = new(Color.FromArgb(225, 230, 236), 1);
		using Pen axisPen = new(Color.FromArgb(120, 130, 140), 1);
		using Font font = new("Segoe UI", 9);
		using SolidBrush textBrush = new(Color.FromArgb(80, 90, 100));

		for (int i = 0; i <= 5; i++)
		{
			int y = top + (bottom - top) * i / 5;
			g.DrawLine(gridPen, left, y, right, y);
		}

		for (int i = 0; i <= 10; i++)
		{
			int x = left + (right - left) * i / 10;
			g.DrawLine(gridPen, x, top, x, bottom);
		}

		g.DrawLine(axisPen, left, top, left, bottom);
		g.DrawLine(axisPen, left, bottom, right, bottom);

		g.DrawString("Высота", font, textBrush, 5, 5);
		g.DrawString("Ширина", font, textBrush, right - 45, bottom + 8);

		if (_points.Count < 2)
		{
			g.DrawString("Нажмите «Сгенерировать»", font, textBrush, left + 10, top + 10);
			return;
		}

		PointF[] profile = _points.Select(p => new PointF(left + p.X * (right - left), bottom - p.Y * (bottom - top) / 100f)).ToArray();
		PointF[] mountain = [new PointF(left, bottom), .. profile, new PointF(right, bottom)];

		using SolidBrush fillBrush = new(Color.FromArgb(100, 64, 130, 85));
		using Pen mountainPen = new(Color.FromArgb(35, 105, 65), 2.2f);

		g.FillPolygon(fillBrush, mountain);
		g.DrawLines(mountainPen, profile);
	}
}
