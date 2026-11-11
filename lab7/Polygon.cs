namespace PolyhedraLab;

public sealed class Polygon
{
    public IReadOnlyList<int> VertexIndices { get; }

    /// <summary>Единичная внешняя нормаль в координатах модели; нулевая у вырожденной грани.</summary>
    public Point Normal { get; private set; } = new(0, 0, 0);

    public Point Center { get; private set; } = new(0, 0, 0);

    internal void UpdateNormal(IReadOnlyList<Point> vertices, Point objectCenter)
    {
        double nx = 0, ny = 0, nz = 0;
        Center = new Point(
            VertexIndices.Average(i => vertices[i].X),
            VertexIndices.Average(i => vertices[i].Y),
            VertexIndices.Average(i => vertices[i].Z));

        // Сумма (B - A) × (C - A) по вееру треугольников.
        // Работает и тогда, когда первые три вершины лежат на одной прямой.
        Point a = vertices[VertexIndices[0]];
        for (int i = 1; i < VertexIndices.Count - 1; i++)
        {
            Point b = vertices[VertexIndices[i]];
            Point c = vertices[VertexIndices[i + 1]];
            double ux = b.X - a.X, uy = b.Y - a.Y, uz = b.Z - a.Z;
            double vx = c.X - a.X, vy = c.Y - a.Y, vz = c.Z - a.Z;
            nx += uy * vz - uz * vy;
            ny += uz * vx - ux * vz;
            nz += ux * vy - uy * vx;
        }

        // Для выпуклого тела внешняя нормаль направлена от его центра к грани.
        // Проверка также исправляет направление после отражения объекта.
        if (nx * (Center.X - objectCenter.X)
            + ny * (Center.Y - objectCenter.Y)
            + nz * (Center.Z - objectCenter.Z) < 0)
        {
            nx = -nx; ny = -ny; nz = -nz;
        }

        double length = Math.Sqrt(nx * nx + ny * ny + nz * nz);
        Normal = length > 1e-12
            ? new Point(nx / length, ny / length, nz / length)
            : new Point(0, 0, 0);
    }

    public Polygon(IEnumerable<int> vertexIndices)
    {
        ArgumentNullException.ThrowIfNull(vertexIndices);

        int[] indices = vertexIndices.ToArray();

        if (indices.Length < 3)
            throw new ArgumentException("Грань должна содержать не менее трех вершин.", nameof(vertexIndices));

        if (indices.Distinct().Count() != indices.Length)
            throw new ArgumentException("Грань не должна содержать повторяющиеся вершины.", nameof(vertexIndices));

        VertexIndices = Array.AsReadOnly(indices);
    }
}
