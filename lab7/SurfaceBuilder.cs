namespace PolyhedraLab;

/// <summary>Именованная функция двух переменных z = f(x, y) для списка в интерфейсе.</summary>
public sealed class SurfaceFunction
{
    public string Name { get; }
    public Func<double, double, double> Evaluate { get; }

    public SurfaceFunction(string name, Func<double, double, double> evaluate)
    {
        Name = name;
        Evaluate = evaluate;
    }

    public override string ToString() => Name;

    public static IReadOnlyList<SurfaceFunction> Presets { get; } = new[]
    {
        new SurfaceFunction("sin(x) + cos(y)",           (x, y) => Math.Sin(x) + Math.Cos(y)),
        new SurfaceFunction("x² + y² (параболоид)",      (x, y) => x * x + y * y),
        new SurfaceFunction("x² − y² (седло)",           (x, y) => x * x - y * y),
        new SurfaceFunction("sin(√(x² + y²)) (волны)",   (x, y) => Math.Sin(Math.Sqrt(x * x + y * y))),
        new SurfaceFunction("sin(x) · cos(y)",           (x, y) => Math.Sin(x) * Math.Cos(y)),
        new SurfaceFunction("sin(x² + y²)",              (x, y) => Math.Sin(x * x + y * y)),
        new SurfaceFunction("1 / (x² + y²) (разрыв в 0)", (x, y) => 1.0 / (x * x + y * y)),
    };
}

public static class SurfaceBuilder
{
    public const int MaxDivisions = 200;

    /// <summary>Значения |z| больше этого порога считаются «бесконечными» и такие точки пропускаются.</summary>
    public const double MaxAbsZ = 100.0;

    /// <summary>
    /// Строит сегмент поверхности z = f(x, y) на [x0, x1] × [y0, y1]
    /// с одинаковым числом разбиений по X и Y. Каждая ячейка сетки делится на два треугольника.
    /// Точки, где функция не определена (NaN, ±∞, слишком большие значения), пропускаются вместе с гранями, которые их используют.
    /// </summary>
    public static Polyhedron Build(
        Func<double, double, double> function,
        double x0, double x1,
        double y0, double y1,
        int divisions)
    {
        ArgumentNullException.ThrowIfNull(function);

        if (!double.IsFinite(x0) || !double.IsFinite(x1) || !double.IsFinite(y0) || !double.IsFinite(y1))
            throw new ArgumentException("Границы диапазонов должны быть конечными числами.");
        if (x0 >= x1 || y0 >= y1)
            throw new ArgumentException("Должно выполняться x0 < x1 и y0 < y1.");
        if (divisions < 1 || divisions > MaxDivisions)
            throw new ArgumentOutOfRangeException(nameof(divisions),
                $"Количество разбиений должно быть от 1 до {MaxDivisions}.");

        int count = divisions + 1;
        double dx = (x1 - x0) / divisions;
        double dy = (y1 - y0) / divisions;

        // Значения функции в узлах сетки (NaN = точка непригодна).
        double[,] z = new double[count, count];
        for (int i = 0; i < count; i++)
        {
            double x = x0 + i * dx;
            for (int j = 0; j < count; j++)
            {
                double y = y0 + j * dy;
                double value = function(x, y);
                z[i, j] = double.IsFinite(value) && Math.Abs(value) <= MaxAbsZ
                    ? value
                    : double.NaN;
            }
        }

        // Вершины создаются лениво, только если их использует хотя бы одна грань.
        int[,] map = new int[count, count];
        for (int i = 0; i < count; i++)
            for (int j = 0; j < count; j++)
                map[i, j] = -1;

        List<Point> vertices = new();
        List<Polygon> faces = new();

        int GetVertex(int i, int j)
        {
            if (map[i, j] < 0)
            {
                map[i, j] = vertices.Count;
                vertices.Add(new Point(x0 + i * dx, y0 + j * dy, z[i, j]));
            }
            return map[i, j];
        }

        void TryAddTriangle(int i1, int j1, int i2, int j2, int i3, int j3)
        {
            if (double.IsNaN(z[i1, j1]) || double.IsNaN(z[i2, j2]) || double.IsNaN(z[i3, j3]))
                return;

            faces.Add(new Polygon(new[]
            {
                GetVertex(i1, j1),
                GetVertex(i2, j2),
                GetVertex(i3, j3)
            }));
        }

        for (int i = 0; i < divisions; i++)
        {
            for (int j = 0; j < divisions; j++)
            {
                // Обход против часовой стрелки, если смотреть сверху (с +Z).
                TryAddTriangle(i, j, i + 1, j, i + 1, j + 1);
                TryAddTriangle(i, j, i + 1, j + 1, i, j + 1);
            }
        }

        if (faces.Count == 0)
            throw new InvalidOperationException("Функция не определена на заданном диапазоне.");

        return new Polyhedron(vertices, faces);
    }
}