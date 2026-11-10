using System.Windows.Forms;

namespace PolyhedraLab;

/// <summary>Диалог построения графика функции двух переменных. Результат — в свойстве Result.</summary>
public sealed class SurfaceDialog : Form
{
    private readonly ComboBox _function = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Dock = DockStyle.Fill
    };
    private readonly NumericUpDown _x0 = CreateRange(-3);
    private readonly NumericUpDown _x1 = CreateRange(3);
    private readonly NumericUpDown _y0 = CreateRange(-3);
    private readonly NumericUpDown _y1 = CreateRange(3);
    private readonly NumericUpDown _divisions = new()
    {
        Minimum = 1,
        Maximum = SurfaceBuilder.MaxDivisions,
        Value = 30,
        Dock = DockStyle.Fill
    };

    public Polyhedron? Result { get; private set; }

    public SurfaceDialog()
    {
        Text = "График функции двух переменных";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(340, 245);

        foreach (SurfaceFunction f in SurfaceFunction.Presets)
            _function.Items.Add(f);
        _function.SelectedIndex = 0;

        TableLayoutPanel table = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(10)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));

        AddRow(table, "z = f(x, y):", _function);
        AddRow(table, "x от:", _x0);
        AddRow(table, "x до:", _x1);
        AddRow(table, "y от:", _y0);
        AddRow(table, "y до:", _y1);
        AddRow(table, "Разбиений:", _divisions);

        Button ok = new() { Text = "Построить", AutoSize = true };
        ok.Click += OnOk;

        Button cancel = new()
        {
            Text = "Отмена",
            AutoSize = true,
            DialogResult = DialogResult.Cancel
        };

        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(ok);

        table.Controls.Add(buttons, 0, table.RowCount);
        table.SetColumnSpan(buttons, 2);
        table.RowCount++;

        Controls.Add(table);
        AcceptButton = ok;
        CancelButton = cancel;
    }

    private static NumericUpDown CreateRange(decimal value) => new()
    {
        DecimalPlaces = 2,
        Increment = 0.5m,
        Minimum = -1000,
        Maximum = 1000,
        Value = value,
        Dock = DockStyle.Fill
    };

    private static void AddRow(TableLayoutPanel table, string caption, Control control)
    {
        Label label = new()
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        int row = table.RowCount;
        table.Controls.Add(label, 0, row);
        table.Controls.Add(control, 1, row);
        table.RowCount++;
    }

    private void OnOk(object? sender, EventArgs e)
    {
        try
        {
            SurfaceFunction function = (SurfaceFunction)_function.SelectedItem!;

            Result = SurfaceBuilder.Build(
                function.Evaluate,
                (double)_x0.Value, (double)_x1.Value,
                (double)_y0.Value, (double)_y1.Value,
                (int)_divisions.Value);

            DialogResult = DialogResult.OK;   // закрывает диалог
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(this, ex.Message, "Не удалось построить график",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}