using lab4.Models;
using lab4.Services;
using System.Drawing.Drawing2D;

namespace lab4;

public partial class MainForm : Form
{
    // Режимы работы мыши на холсте
    private enum ToolMode
    {
        Draw,           // рисование полигонов (как было)
        Intersection,   // пересечение выбранного ребра со вторым ребром
        PointInPolygon, // точка внутри / снаружи выбранного полигона
        PointSide       // точка слева / справа от выбранного ребра
    }

    private readonly PolygonManager _polygonManager = new();

    private const int PointRadius = 5;

    // ---- элементы и состояние для проверок (часть 3-го человека) ----
    private ComboBox _modeBox = null!;
    private Label _resultLabel = null!;
    private ToolMode _mode = ToolMode.Draw;

    private readonly List<PointF> _probeEdge = [];   // второе ребро (до 2 точек)
    private PointF? _probePoint;                     // проверяемая точка
    private PointF? _intersectionPoint;              // найденная точка пересечения
    private Color _probeColor = Color.OrangeRed;     // цвет проверяемой точки

    public MainForm()
    {
        InitializeComponent();
        DoubleBuffered = true;
        CreateToolControls();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
    }

    // Создаём два элемента кодом, чтобы не трогать Designer.cs
    private void CreateToolControls()
    {
        _modeBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(640, 19),
            Size = new Size(270, 28)
        };
        _modeBox.Items.AddRange(new object[]
        {
            "Рисование полигонов",
            "Пересечение рёбер",
            "Точка в полигоне",
            "Точка слева/справа от ребра"
        });
        _modeBox.SelectedIndex = 0;
        _modeBox.SelectedIndexChanged += ModeBox_SelectedIndexChanged;

        _resultLabel = new Label
        {
            Location = new Point(920, 13),
            Size = new Size(245, 44),
            Text = string.Empty
        };

        Controls.Add(_modeBox);
        Controls.Add(_resultLabel);
    }

    private void ModeBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _mode = (ToolMode)_modeBox.SelectedIndex;
        ResetProbe();

        _resultLabel.Text = _mode switch
        {
            ToolMode.Intersection => "Выбранное ребро + 2 клика: второе ребро",
            ToolMode.PointInPolygon => "Кликайте точки: проверка по выбранному полигону",
            ToolMode.PointSide => "Кликайте точки: проверка по выбранному ребру",
            _ => string.Empty
        };

        canvas.Invalidate();
        canvas.Focus();
    }

    private void ResetProbe()
    {
        _probeEdge.Clear();
        _probePoint = null;
        _intersectionPoint = null;
    }

    private void Canvas_MouseClick(object sender, MouseEventArgs e)
    {
        // Режимы проверок: каждый ЛКМ — новая проверка, экран не очищается
        if (_mode != ToolMode.Draw)
        {
            if (e.Button == MouseButtons.Left)
            {
                HandleProbeClick(e.Location);
                canvas.Invalidate();
            }
            return;
        }

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

    // ------------------------------------------------------------------
    // Обработка кликов в режимах проверок
    // ------------------------------------------------------------------
    private void HandleProbeClick(PointF p)
    {
        Polygon? selected = _polygonManager.SelectedPolygon;

        switch (_mode)
        {
            case ToolMode.Intersection:
                if (selected is null || !selected.IsEdge)
                {
                    _resultLabel.Text = "Нужно ребро: нарисуйте 2 точки и завершите (ПКМ)";
                    return;
                }

                // Третий клик начинает новое второе ребро — очищать ничего не нужно
                if (_probeEdge.Count >= 2)
                {
                    _probeEdge.Clear();
                    _intersectionPoint = null;
                }

                _probeEdge.Add(p);

                if (_probeEdge.Count < 2)
                {
                    _resultLabel.Text = "Кликните конец второго ребра";
                    return;
                }

                IntersectionResult res = GeometryAlgorithms.IntersectSegments(
                    selected.Vertices[0], selected.Vertices[1],
                    _probeEdge[0], _probeEdge[1]);

                switch (res.Kind)
                {
                    case IntersectionKind.Point:
                        _intersectionPoint = res.Point;
                        _resultLabel.Text = $"Пересекаются: ({res.Point.X:F1}; {res.Point.Y:F1})";
                        break;
                    case IntersectionKind.Parallel:
                        _resultLabel.Text = "Рёбра параллельны";
                        break;
                    case IntersectionKind.Overlap:
                        _resultLabel.Text = "Рёбра лежат на одной прямой и перекрываются";
                        break;
                    default:
                        _resultLabel.Text = "Рёбра не пересекаются";
                        break;
                }
                break;

            case ToolMode.PointInPolygon:
                if (selected is null || selected.IsEmpty)
                {
                    _resultLabel.Text = "Сначала нарисуйте и завершите полигон";
                    return;
                }

                _probePoint = p;
                PointLocation loc = GeometryAlgorithms.PointInPolygon(selected, p);
                string kind = selected.IsPolygon
                    ? (GeometryAlgorithms.IsConvex(selected.Vertices) ? "выпуклый" : "невыпуклый")
                    : "не полигон";

                (_probeColor, _resultLabel.Text) = loc switch
                {
                    PointLocation.Inside => (Color.Green, $"Внутри ({kind})"),
                    PointLocation.OnBoundary => (Color.DarkOrange, $"На границе ({kind})"),
                    _ => (Color.Red, $"Снаружи ({kind})")
                };
                break;

            case ToolMode.PointSide:
                if (selected is null || !selected.IsEdge)
                {
                    _resultLabel.Text = "Нужно ребро: нарисуйте 2 точки и завершите (ПКМ)";
                    return;
                }

                _probePoint = p;
                PointSide side = GeometryAlgorithms.ClassifyPoint(selected, p);

                (_probeColor, _resultLabel.Text) = side switch
                {
                    PointSide.Left => (Color.Blue, "Точка слева от ребра"),
                    PointSide.Right => (Color.Red, "Точка справа от ребра"),
                    _ => (Color.DarkOrange, "Точка на прямой ребра")
                };
                break;
        }
    }

    private void Canvas_Paint(object sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        foreach (Polygon polygon in _polygonManager.Polygons)
            DrawPolygon(e.Graphics, polygon);

        if (_polygonManager.CurrentPolygon is not null)
            DrawPolygon(e.Graphics, _polygonManager.CurrentPolygon, true);

        DrawProbe(e.Graphics);
    }

    // Минимальная отрисовка проверок, чтобы результат было видно
    // (первый человек может заменить на свою)
    private void DrawProbe(Graphics graphics)
    {
        if (_probeEdge.Count == 2)
        {
            using Pen pen = new(Color.OrangeRed, 2) { DashStyle = DashStyle.Dash };
            graphics.DrawLine(pen, _probeEdge[0], _probeEdge[1]);
        }

        using (Brush edgeBrush = new SolidBrush(Color.OrangeRed))
        {
            foreach (PointF v in _probeEdge)
                graphics.FillEllipse(edgeBrush, v.X - 4, v.Y - 4, 8, 8);
        }

        if (_intersectionPoint is PointF ip)
        {
            using Brush brush = new SolidBrush(Color.Red);
            graphics.FillEllipse(brush, ip.X - 6, ip.Y - 6, 12, 12);
        }

        if (_probePoint is PointF pp)
        {
            using Brush brush = new SolidBrush(_probeColor);
            graphics.FillEllipse(brush, pp.X - 5, pp.Y - 5, 10, 10);
        }
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
        ResetProbe();
        _resultLabel.Text = string.Empty;
        canvas.Invalidate();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        if (_mode == ToolMode.Draw)
        {
            _polygonManager.FinishCurrentPolygon();
            canvas.Invalidate();
        }
        e.Handled = true;
    }

    //Обработчики событий по аффинным преобразованиям
    private void ApplySettingsButton_Click(object sender, EventArgs e)
    {
        switch (comboBox1.SelectedIndex)
        {
            case 0: // Перенос
                _polygonManager.TranslationSelectedPolygon(
                    (float)dxInput.Value,
                    (float)dyInput.Value);
                break;

            case 1: // Поворот вокруг точки
                _polygonManager.RotationSelectedPolygon(
                    new PointF((float)pointXInput.Value, (float)pointYInput.Value),
                    (float)angleInput.Value);
                break;

            case 2: // Поворот вокруг центра
                _polygonManager.RotationCenterSelectedPolygon(
                    (float)angleInput.Value);
                break;

            case 3: // Масштабирование относительно точки
                _polygonManager.DilatationSelectedPolygon(
                    new PointF((float)pointXInput.Value, (float)pointYInput.Value),
                    (float)scaleXInput.Value,
                    (float)scaleYInput.Value);
                break;

            case 4: // Масштабирование относительно центра
                _polygonManager.DilatationCenterSelectedPolygon(
                    (float)scaleXInput.Value,
                    (float)scaleYInput.Value);
                break;
        }

        canvas.Invalidate();
    }
}