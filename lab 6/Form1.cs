using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using lab_6.Core;
using Point = lab_6.Core.Point;
namespace lab_6;

public partial class Form1 : Form
{
    private List<Point> points = new List<Point>();
    private List<Triangle> triangulation = new List<Triangle>();

    public Form1()
    {
        InitializeComponent();
        this.DoubleBuffered = true;
        this.MouseDown += Form1_MouseDown;
    }

    private void Form1_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            points.Add(new Point(e.X, e.Y));
            triangulation = Delaunay.Triangulate(points);
            this.Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var pen = new Pen(Color.Black, 2);
        foreach (var t in triangulation)
        {
            g.DrawLine(pen, (float)t.P1.X, (float)t.P1.Y, (float)t.P2.X, (float)t.P2.Y);
            g.DrawLine(pen, (float)t.P2.X, (float)t.P2.Y, (float)t.P3.X, (float)t.P3.Y);
            g.DrawLine(pen, (float)t.P3.X, (float)t.P3.Y, (float)t.P1.X, (float)t.P1.Y);
        }

        using var brush = new SolidBrush(Color.Red);
        foreach (var p in points)
        {
            g.FillEllipse(brush, (float)(p.X - 3), (float)(p.Y - 3), 6, 6);
        }
    }
}
