namespace PolyhedraLab;

public sealed class Polygon
{
    public IReadOnlyList<int> VertexIndices { get; }

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
