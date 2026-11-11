using System.Drawing;
using System.Reflection;
using PolyhedraLab;
using Point3 = PolyhedraLab.Point;

internal static class Program
{
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static ProjectedPolyhedron Project(Polyhedron model, ViewSettings settings) =>
        Projector.Project(model, settings, new PointF(100, 100));

    private static void CheckNormals(Polyhedron model)
    {
        model.UpdateFaceNormals();
        Point3 center = new(model.Vertices.Average(v => v.X),
            model.Vertices.Average(v => v.Y), model.Vertices.Average(v => v.Z));
        foreach (Polygon face in model.Faces)
        {
            Point3 n = face.Normal;
            Check(Math.Abs(n.X * n.X + n.Y * n.Y + n.Z * n.Z - 1) < 1e-9, "Normal must be unit length");
            Check(n.X * (face.Center.X - center.X) + n.Y * (face.Center.Y - center.Y)
                + n.Z * (face.Center.Z - center.Z) > 0, "Normal must point outward");
            Point3 a = model.Vertices[face.VertexIndices[0]];
            foreach (int index in face.VertexIndices.Skip(1))
            {
                Point3 b = model.Vertices[index];
                Check(Math.Abs(n.X * (b.X - a.X) + n.Y * (b.Y - a.Y) + n.Z * (b.Z - a.Z)) < 1e-8,
                    "Normal must be perpendicular to the face");
            }
        }
    }

    [STAThread]
    private static void Main()
    {
        (string Name, Action Run)[] tests =
        [
            ("Outward normals of all presets", () =>
            {
                foreach (Polyhedron model in new[] { Polyhedron.CreateHexahedron(), Polyhedron.CreateTetrahedron(),
                    Polyhedron.CreateOctahedron(), Polyhedron.CreateSphere() }) CheckNormals(model);
            }),
            ("Cube: one front face on axis, three at default view, six with culling off", () =>
            {
                Polyhedron cube = Polyhedron.CreateHexahedron();
                ViewSettings s = new() { Yaw = 0, Pitch = 0 };
                Check(Project(cube, s).Faces.Single().Index == 1, "+Z face expected");
                s.Yaw = 180;
                Check(Project(cube, s).Faces.Single().Index == 0, "-Z face expected");
                s.Yaw = 0; s.Pitch = 90;
                Check(Project(cube, s).Faces.Single().Index == 3, "+Y face expected");
                s = new ViewSettings();
                Check(Project(cube, s).Faces.Count == 3, "Three faces expected");
                s.HideBackFaces = false;
                Check(Project(cube, s).Faces.Count == 6, "Six faces expected");
            }),
            ("Perspective uses a separate viewing ray for an offset object", () =>
            {
                Polyhedron cube = Polyhedron.CreateHexahedron();
                ViewSettings s = new() { Yaw = 0, Pitch = 0, Kind = ProjectionKind.Perspective };
                Check(Project(cube, s).Faces.Count == 1, "One face at center expected");
                ProjectedPolyhedron shifted = Projector.Project(cube, s, PointF.Empty, 10, 0, 0);
                Check(shifted.Faces.Select(f => f.Index).Order().SequenceEqual(new[] { 1, 4 }),
                    "Offset cube must show +Z and -X faces");
                s.Kind = ProjectionKind.Axonometric;
                Check(Projector.Project(cube, s, PointF.Empty, 10, 0, 0).Faces.Count == 1,
                    "Parallel viewing direction must not depend on offset");
            }),
            ("Normals after rotation, nonuniform scaling, reflection and translation", () =>
            {
                Polyhedron cube = Polyhedron.CreateHexahedron();
                MatrixOperations ops = new();
                List<Point3> v = ops.RotationPoints(cube.Vertices, 'Y', 37);
                v = ops.ScalePoints(v, 2, 0.5f, 3);
                v = ops.ReflectionPoints(v, "YZ");
                v = ops.TranslationPoints(v, 5, -3, 2);
                cube.Vertices.Clear(); cube.Vertices.AddRange(v);
                // Project must update normals even if the vertices were changed directly.
                Check(Project(cube, new ViewSettings()).Faces.Count == 3, "Transformed cube must show three faces");
                CheckNormals(cube);
            }),
            ("Reversed face winding is corrected outward", () =>
            {
                Polyhedron cube = Polyhedron.CreateHexahedron();
                Polyhedron reversed = new(cube.Vertices,
                    cube.Faces.Select(f => new Polygon(f.VertexIndices.Reverse())));
                CheckNormals(reversed);
                Check(Project(reversed, new ViewSettings()).Faces.Select(f => f.Index)
                    .SequenceEqual(Project(cube, new ViewSettings()).Faces.Select(f => f.Index)), "Visibility must match");
            }),
            ("Collinear first three vertices and a fully degenerate face", () =>
            {
                Polyhedron model = new(new Point3[] { new(0, 0, 1), new(1, 0, 1), new(2, 0, 1), new(0, 1, 1) },
                    new[] { new Polygon(new[] { 0, 1, 2, 3 }), new Polygon(new[] { 0, 1, 2 }) });
                Check(Math.Abs(model.Faces[0].Normal.Z - 1) < 1e-9, "A noncollinear triangle must be used");
                Check(Project(model, new ViewSettings { HideBackFaces = false }).Faces.Count == 1,
                    "Degenerate face must be skipped");
            }),
            ("Z-buffer keeps opaque cube images with culling on and off", () =>
            {
                foreach (ProjectionKind kind in Enum.GetValues<ProjectionKind>())
                {
                    Polyhedron cube = Polyhedron.CreateHexahedron();
                    ViewSettings s = new() { Kind = kind, Zoom = 40 };
                    using Bitmap culled = ZBufferRenderer.Render(Project(cube, s).Faces, 200, 200, Color.SteelBlue, Color.Black);
                    s.HideBackFaces = false;
                    using Bitmap all = ZBufferRenderer.Render(Project(cube, s).Faces, 200, 200, Color.SteelBlue, Color.Black);
                    Check(culled.GetPixel(100, 100).A == 255, "Cube center must remain opaque");
                    int differences = 0;
                    for (int y = 0; y < 200; y++)
                        for (int x = 0; x < 200; x++)
                            if (culled.GetPixel(x, y) != all.GetPixel(x, y)) differences++;
                    Check(differences < 400, "Culling must preserve the Z-buffer image except for rasterized edge tolerances");
                }
            }),
            ("Form controls, animation and both rendering modes", () =>
            {
                using Form1 form = new();
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                T Field<T>(string name) => (T)typeof(Form1).GetField(name, flags)!.GetValue(form)!;
                CheckBox rotate = Field<CheckBox>("chkAutoRotate");
                Polyhedron model = Field<Polyhedron>("_polyhedron");
                double originalX = model.Vertices[0].X;
                rotate.Checked = true;
                System.Threading.Thread.Sleep(40);
                typeof(Form1).GetMethod("AnimateObject", flags)!.Invoke(form, null);
                rotate.Checked = false;
                Check(Math.Abs(model.Vertices[0].X - originalX) > 1e-5, "Animation must rotate object vertices");
                CheckNormals(model);
                PolyhedronView view = Field<PolyhedronView>("_view");
                using Bitmap image = new(500, 400);
                using Graphics g = Graphics.FromImage(image);
                foreach (bool wireframe in new[] { false, true })
                {
                    Field<CheckBox>("chkWireframe").Checked = wireframe;
                    Field<CheckBox>("chkNormals").Checked = true;
                    Check(view.Wireframe == wireframe && view.ShowNormals, "Controls must update the view");
                    view.Render(g, image.Size);
                }
            })
        ];

        foreach (var test in tests)
        {
            test.Run();
            Console.WriteLine("PASS: " + test.Name);
        }
        Console.WriteLine($"Passed {tests.Length} checks.");
    }
}
