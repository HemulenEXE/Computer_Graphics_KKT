using System.Text;

namespace PolyhedraLab;

public partial class Form1 : Form
{
    private readonly ComboBox cmbPolyhedron = new();
    private readonly Button btnCreate = new();
    private readonly TextBox txtResult = new();

    public Form1()
    {
        InitializeControls();
    }

    private void InitializeControls()
    {
        Text = "Лабораторная работа — многогранники";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(620, 440);
        Size = new Size(760, 560);

        cmbPolyhedron.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPolyhedron.Items.AddRange(["Тетраэдр", "Гексаэдр (куб)", "Октаэдр"]);
        cmbPolyhedron.SelectedIndex = 0;
        cmbPolyhedron.Dock = DockStyle.Fill;

        btnCreate.Text = "Создать";
        btnCreate.AutoSize = true;
        btnCreate.Click += btnCreate_Click;

        txtResult.Multiline = true;
        txtResult.ReadOnly = true;
        txtResult.ScrollBars = ScrollBars.Vertical;
        txtResult.WordWrap = false;
        txtResult.Dock = DockStyle.Fill;
        txtResult.Font = new Font("Consolas", 10);

        var topPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10),
            ColumnCount = 2,
            RowCount = 1
        };
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        topPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topPanel.Controls.Add(cmbPolyhedron, 0, 0);
        topPanel.Controls.Add(btnCreate, 1, 0);

        Controls.Add(txtResult);
        Controls.Add(topPanel);

        btnCreate_Click(this, EventArgs.Empty);
    }

    private void btnCreate_Click(object? sender, EventArgs e)
    {
        Polyhedron polyhedron = cmbPolyhedron.SelectedIndex switch
        {
            0 => Polyhedron.CreateTetrahedron(),
            1 => Polyhedron.CreateHexahedron(),
            2 => Polyhedron.CreateOctahedron(),
            _ => throw new InvalidOperationException("Выберите многогранник.")
        };

        StringBuilder result = new();
        result.AppendLine($"Вершин: {polyhedron.Vertices.Count}");
        result.AppendLine($"Граней: {polyhedron.Faces.Count}");
        result.AppendLine();
        result.AppendLine("Вершины:");

        for (int i = 0; i < polyhedron.Vertices.Count; i++)
        {
            Point p = polyhedron.Vertices[i];
            result.AppendLine($"V{i}: ({p.X}, {p.Y}, {p.Z})");
        }

        result.AppendLine();
        result.AppendLine("Грани:");

        for (int i = 0; i < polyhedron.Faces.Count; i++)
            result.AppendLine($"F{i}: {string.Join(", ", polyhedron.Faces[i].VertexIndices)}");

        txtResult.Text = result.ToString();
    }
}
