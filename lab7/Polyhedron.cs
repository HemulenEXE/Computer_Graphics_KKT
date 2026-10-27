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
}
