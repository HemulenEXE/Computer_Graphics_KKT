using System.Globalization;
using System.Text;

namespace PolyhedraLab;

public sealed class Polyhedron
{
    public List<Point> Vertices { get; }
    public List<Polygon> Faces { get; }

    /// <summary>Усреднённые единичные нормали вершин в координатах модели.</summary>
    public List<Point> VertexNormals { get; private set; } = new();

    public Polyhedron(IEnumerable<Point> vertices, IEnumerable<Polygon> faces)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(faces);

        Vertices = vertices.ToList();
        Faces = faces.ToList();

        if (Vertices.Count == 0)
            throw new ArgumentException("Многогранник должен содержать вершины.", nameof(vertices));

        if (Faces.Count == 0)
            throw new ArgumentException("Многогранник должен содержать грани.", nameof(faces));

        if (Vertices.Any(v => v is null))
            throw new ArgumentException("Список вершин не должен содержать null.", nameof(vertices));

        foreach (Polygon face in Faces)
        {
            if (face.VertexIndices.Any(i => i < 0 || i >= Vertices.Count))
                throw new ArgumentException("Грань содержит недопустимый индекс вершины.", nameof(faces));
        }

        UpdateFaceNormals();
    }

    /// <summary>Пересчитывает нормали по текущим вершинам после преобразования модели.</summary>
    public void UpdateFaceNormals()
    {
        Point center = new(
            Vertices.Average(p => p.X),
            Vertices.Average(p => p.Y),
            Vertices.Average(p => p.Z));
        foreach (Polygon face in Faces)
            face.UpdateNormal(Vertices, center);

        UpdateVertexNormals();
    }

    /// <summary>
    /// Вычисляет нормаль каждой вершины как среднее нормалей всех смежных граней.
    /// Нормали граней уже ориентированы наружу методом Polygon.UpdateNormal.
    /// </summary>
    private void UpdateVertexNormals()
    {
        double[] nx = new double[Vertices.Count];
        double[] ny = new double[Vertices.Count];
        double[] nz = new double[Vertices.Count];
        int[] counts = new int[Vertices.Count];

        foreach (Polygon face in Faces)
        {
            Point normal = face.Normal;
            if (normal.X == 0 && normal.Y == 0 && normal.Z == 0)
                continue;

            foreach (int vertexIndex in face.VertexIndices)
            {
                nx[vertexIndex] += normal.X;
                ny[vertexIndex] += normal.Y;
                nz[vertexIndex] += normal.Z;
                counts[vertexIndex]++;
            }
        }

        VertexNormals = new List<Point>(Vertices.Count);
        for (int i = 0; i < Vertices.Count; i++)
        {
            double length = Math.Sqrt(nx[i] * nx[i] + ny[i] * ny[i] + nz[i] * nz[i]);
            VertexNormals.Add(length > 1e-12 && counts[i] > 0
                ? new Point(nx[i] / length, ny[i] / length, nz[i] / length)
                : new Point(0, 0, 0));
        }
    }

    public static Polyhedron CreateSphere()
    {
        Point[] profile = Enumerable.Range(0, 17)
            .Select(i => new Point(Math.Sin(Math.PI * i / 16), Math.Cos(Math.PI * i / 16), 0))
            .ToArray();
        return CreateSurfaceOfRevolution(profile, 'Y', 32);
    }

    public static Polyhedron CreateTetrahedron()
    {
        Point[] vertices =
        [
            new(1, 1, 1),
            new(1, -1, -1),
            new(-1, 1, -1),
            new(-1, -1, 1)
        ];

        int[][] faces =
        [
            [0, 1, 2],
            [0, 3, 1],
            [0, 2, 3],
            [1, 3, 2]
        ];

        return Create(vertices, faces);
    }

    public static Polyhedron CreateHexahedron()
    {
        Point[] vertices =
        [
            new(-1, -1, -1),
            new(1, -1, -1),
            new(1, 1, -1),
            new(-1, 1, -1),
            new(-1, -1, 1),
            new(1, -1, 1),
            new(1, 1, 1),
            new(-1, 1, 1)
        ];

        int[][] faces =
        [
            [0, 3, 2, 1],
            [4, 5, 6, 7],
            [0, 1, 5, 4],
            [3, 7, 6, 2],
            [0, 4, 7, 3],
            [1, 2, 6, 5]
        ];

        return Create(vertices, faces);
    }

    public static Polyhedron CreateOctahedron()
    {
        Point[] vertices =
        [
            new(1, 0, 0),
            new(-1, 0, 0),
            new(0, 1, 0),
            new(0, -1, 0),
            new(0, 0, 1),
            new(0, 0, -1)
        ];

        int[][] faces =
        [
            [0, 2, 4],
            [2, 1, 4],
            [1, 3, 4],
            [3, 0, 4],
            [2, 0, 5],
            [1, 2, 5],
            [3, 1, 5],
            [0, 3, 5]
        ];

        return Create(vertices, faces);
    }

    public static Polyhedron CreateSurfaceOfRevolution(
        IEnumerable<Point> profile,
        char axis,
        int divisions)
    {
        ArgumentNullException.ThrowIfNull(profile);

        axis = char.ToUpperInvariant(axis);
        if (axis != 'X' && axis != 'Y' && axis != 'Z')
            throw new ArgumentOutOfRangeException(nameof(axis), "Ось вращения должна быть X, Y или Z.");
        if (divisions < 3 || divisions > 512)
            throw new ArgumentOutOfRangeException(nameof(divisions), "Количество разбиений должно быть от 3 до 512.");

        List<Point> profilePoints = profile.ToList();
        if (profilePoints.Count < 2)
            throw new ArgumentException("Образующая должна содержать не менее двух точек.", nameof(profile));
        if (profilePoints.Any(point => point is null))
            throw new ArgumentException("Образующая не должна содержать null.", nameof(profile));
        if (profilePoints.Any(point =>
                !double.IsFinite(point.X) || !double.IsFinite(point.Y) || !double.IsFinite(point.Z)))
        {
            throw new ArgumentException("Координаты образующей должны быть конечными числами.", nameof(profile));
        }

        for (int i = 1; i < profilePoints.Count; i++)
        {
            Point previous = profilePoints[i - 1];
            Point current = profilePoints[i];
            if (previous.X == current.X && previous.Y == current.Y && previous.Z == current.Z)
                throw new ArgumentException("Соседние точки образующей не должны совпадать.", nameof(profile));
        }

        List<Point> vertices = new();
        List<int[]> rings = new(profilePoints.Count);

        foreach (Point profilePoint in profilePoints)
        {
            if (DistanceFromAxis(profilePoint, axis) <= 1e-10)
            {
                rings.Add([vertices.Count]);
                vertices.Add(PointOnAxis(profilePoint, axis));
                continue;
            }

            int[] ring = new int[divisions];
            for (int step = 0; step < divisions; step++)
            {
                double angle = 2 * Math.PI * step / divisions;
                ring[step] = vertices.Count;
                vertices.Add(RotateAroundAxis(profilePoint, axis, angle));
            }
            rings.Add(ring);
        }

        List<Polygon> faces = new();
        for (int profileIndex = 0; profileIndex < rings.Count - 1; profileIndex++)
        {
            int[] firstRing = rings[profileIndex];
            int[] secondRing = rings[profileIndex + 1];
            if (firstRing.Length == 1 && secondRing.Length == 1)
                continue;

            for (int step = 0; step < divisions; step++)
            {
                int next = (step + 1) % divisions;
                int[] faceIndices = firstRing.Length == 1
                    ? [firstRing[0], secondRing[step], secondRing[next]]
                    : secondRing.Length == 1
                        ? [firstRing[step], secondRing[0], firstRing[next]]
                        : [firstRing[step], secondRing[step], secondRing[next], firstRing[next]];

                OrientFaceOutward(faceIndices, vertices, axis);
                faces.Add(new Polygon(faceIndices));
            }
        }

        AddEndCap(rings[0], DirectionAlongAxis(profilePoints[0], profilePoints[1], axis) * -1);
        AddEndCap(
            rings[rings.Count - 1],
            DirectionAlongAxis(profilePoints[profilePoints.Count - 2], profilePoints[profilePoints.Count - 1], axis));

        if (faces.Count == 0)
            throw new ArgumentException("Образующая не задаёт поверхность вращения.", nameof(profile));

        return new Polyhedron(vertices, faces);

        void AddEndCap(int[] ring, double outwardDirection)
        {
            if (ring.Length == 1)
                return;

            int[] cap = outwardDirection < 0
                ? ring.Reverse().ToArray()
                : ring.ToArray();
            faces.Add(new Polygon(cap));
        }
    }

    private static double DistanceFromAxis(Point point, char axis) => axis switch
    {
        'X' => Hypot(point.Y, point.Z),
        'Y' => Hypot(point.X, point.Z),
        _ => Hypot(point.X, point.Y)
    };

    private static double Hypot(double first, double second)
    {
        double largest = Math.Max(Math.Abs(first), Math.Abs(second));
        if (largest == 0)
            return 0;

        double scaledFirst = first / largest;
        double scaledSecond = second / largest;
        return largest * Math.Sqrt(scaledFirst * scaledFirst + scaledSecond * scaledSecond);
    }

    private static Point PointOnAxis(Point point, char axis) => axis switch
    {
        'X' => new Point(point.X, 0, 0),
        'Y' => new Point(0, point.Y, 0),
        _ => new Point(0, 0, point.Z)
    };

    private static Point RotateAroundAxis(Point point, char axis, double angle)
    {
        double cosine = Math.Cos(angle);
        double sine = Math.Sin(angle);

        return axis switch
        {
            'X' => new Point(
                point.X,
                point.Y * cosine - point.Z * sine,
                point.Y * sine + point.Z * cosine),
            'Y' => new Point(
                point.X * cosine + point.Z * sine,
                point.Y,
                -point.X * sine + point.Z * cosine),
            _ => new Point(
                point.X * cosine - point.Y * sine,
                point.X * sine + point.Y * cosine,
                point.Z)
        };
    }

    private static double DirectionAlongAxis(Point first, Point second, char axis) => axis switch
    {
        'X' => second.X - first.X,
        'Y' => second.Y - first.Y,
        _ => second.Z - first.Z
    };

    private static void OrientFaceOutward(int[] indices, List<Point> vertices, char axis)
    {
        Point a = vertices[indices[0]];
        Point b = vertices[indices[1]];
        Point c = vertices[indices[2]];

        double abX = b.X - a.X;
        double abY = b.Y - a.Y;
        double abZ = b.Z - a.Z;
        double acX = c.X - a.X;
        double acY = c.Y - a.Y;
        double acZ = c.Z - a.Z;
        double normalX = abY * acZ - abZ * acY;
        double normalY = abZ * acX - abX * acZ;
        double normalZ = abX * acY - abY * acX;

        double centerX = indices.Average(index => vertices[index].X);
        double centerY = indices.Average(index => vertices[index].Y);
        double centerZ = indices.Average(index => vertices[index].Z);
        double radialDot = axis switch
        {
            'X' => normalY * centerY + normalZ * centerZ,
            'Y' => normalX * centerX + normalZ * centerZ,
            _ => normalX * centerX + normalY * centerY
        };

        if (radialDot < 0)
            Array.Reverse(indices);
    }

    private static Polyhedron Create(Point[] vertices, int[][] faces)
    {
        return new Polyhedron(
            vertices,
            faces.Select(indices => CreateFace(vertices, indices)));
    }

    private static Polygon CreateFace(Point[] vertices, int[] indices)
    {
        Point a = vertices[indices[0]];
        Point b = vertices[indices[1]];
        Point c = vertices[indices[2]];

        double abX = b.X - a.X;
        double abY = b.Y - a.Y;
        double abZ = b.Z - a.Z;

        double acX = c.X - a.X;
        double acY = c.Y - a.Y;
        double acZ = c.Z - a.Z;

        double normalX = abY * acZ - abZ * acY;
        double normalY = abZ * acX - abX * acZ;
        double normalZ = abX * acY - abY * acX;

        double centerX = indices.Average(i => vertices[i].X);
        double centerY = indices.Average(i => vertices[i].Y);
        double centerZ = indices.Average(i => vertices[i].Z);

        double dot = normalX * centerX + normalY * centerY + normalZ * centerZ;

        return new Polygon(dot < 0 ? indices.Reverse() : indices);
    }
    /// <summary>
    /// Загружает многогранник из OBJ-файла.
    /// </summary>
    public static Polyhedron LoadFromObj(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException("OBJ-файл не найден.", filePath);

        List<Point> vertices = new();
        List<Polygon> faces = new();

        string[] lines = File.ReadAllLines(filePath);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Удаляем комментарии.
            int commentIndex = line.IndexOf('#');
            if (commentIndex >= 0)
                line = line.Substring(0, commentIndex);

            string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                continue;

            switch (parts[0])
            {
                case "v":
                    // Вершина: v x y z
                    if (parts.Length < 4)
                        throw new FormatException($"Некорректная вершина в строке {i + 1}.");

                    double x = ParseDouble(parts[1]);
                    double y = ParseDouble(parts[2]);
                    double z = ParseDouble(parts[3]);

                    vertices.Add(new Point(x, y, z));
                    break;

                case "f":
                    // Грань: f v1 v2 v3 ...
                    if (parts.Length < 4)
                        throw new FormatException($"Грань должна содержать минимум три вершины. Строка {i + 1}.");

                    List<int> indices = new();

                    for (int j = 1; j < parts.Length; j++)
                    {
                       
                        //Извлекаем только вершину, текстуру и нормаль не трогаем
                        string vertexIndex = parts[j].Split('/')[0]; 

                        if (!int.TryParse(
                            vertexIndex,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out int index))
                        {
                            throw new FormatException($"Некорректный индекс вершины в строке {i + 1}.");
                        }

                        if (index == 0)
                            throw new FormatException($"Индекс вершины не может быть равен нулю. Строка {i + 1}.");

                        // OBJ использует индексацию с 1.
                        // Отрицательные индексы отсчитываются от конца списка.
                        int actualIndex = index > 0? index - 1: vertices.Count + index;

                        if (actualIndex < 0 || actualIndex >= vertices.Count)
                            throw new FormatException($"Индекс вершины выходит за допустимые границы. Строка {i + 1}.");

                        indices.Add(actualIndex);
                    }

                    faces.Add(new Polygon(indices));
                    break;
            }
        }

        return new Polyhedron(vertices, faces);
    }

    /// <summary>
    /// Преобразует строку в число с учётом формата OBJ.
    /// </summary>
    private static double ParseDouble(string value)
    {
        if (!double.TryParse(
            value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out double result) || !double.IsFinite(result))
        {
            throw new FormatException($"Некорректное значение координаты: {value}");
        }

        return result;
    }

    /// <summary>
    /// Сохраняет текущий многогранник в OBJ-файл.
    /// </summary>
    public void SaveToObj(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using StreamWriter writer = new StreamWriter(
            filePath,
            false,
            new UTF8Encoding(false));

        writer.WriteLine("# Wavefront OBJ file");
        writer.WriteLine();

        // Записываем вершины.
        foreach (Point vertex in Vertices)
        {
            writer.WriteLine(
                $"v {vertex.X.ToString("R", CultureInfo.InvariantCulture)} " +
                $"{vertex.Y.ToString("R", CultureInfo.InvariantCulture)} " +
                $"{vertex.Z.ToString("R", CultureInfo.InvariantCulture)}");
        }

        writer.WriteLine();

        // Записываем грани.
        foreach (Polygon face in Faces)
        {
            writer.Write("f");

            foreach (int index in face.VertexIndices)
            {
                // В OBJ индексация начинается с 1.
                writer.Write($" {index + 1}");
            }

            writer.WriteLine();
        }
    }
}
