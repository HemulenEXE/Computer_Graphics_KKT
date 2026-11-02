using System.Globalization;
using System.Text;

namespace PolyhedraLab;

public sealed class Polyhedron
{
    public List<Point> Vertices { get; }
    public List<Polygon> Faces { get; }

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

            string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

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
