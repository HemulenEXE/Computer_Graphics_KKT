
namespace MidpointDisplacement;

public partial class MainForm : Form
{
	private readonly MountainPainter _painter = new();
	private readonly NumericUpDown _iterations = CreateNumber(1, 10, 7);
	private readonly NumericUpDown _height = CreateNumber(10, 90, 45);
	private readonly NumericUpDown _amplitude = CreateNumber(0, 50, 20);
	private readonly NumericUpDown _roughness = CreateNumber(0.1m, 0.9m, 0.55m, 0.05m, 2);
	private readonly Label _stepLabel = new();
	private readonly Button _previousButton = new();
	private readonly Button _nextButton = new();
	private readonly List<List<PointF>> _steps = [];
	private int _currentStep;

	public MainForm()
	{
		Text = "Midpoint Displacement — горный массив";
		StartPosition = FormStartPosition.CenterScreen;
		MinimumSize = new Size(850, 550);
		Size = new Size(1100, 720);
		Font = new Font("Segoe UI", 10);

		var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(10) };
		root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		Controls.Add(root);

		var settings = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, Padding = new Padding(0, 0, 0, 8) };
		root.Controls.Add(settings, 0, 0);
		root.Controls.Add(_painter, 0, 1);

		AddSetting(settings, "Итерации:", _iterations);
		AddSetting(settings, "Начальная высота:", _height);
		AddSetting(settings, "Амплитуда:", _amplitude);
		AddSetting(settings, "Уменьшение:", _roughness);

		var generateButton = new Button { Text = "Сгенерировать", AutoSize = true, Height = 32, Margin = new Padding(8, 3, 3, 3) };
		generateButton.Click += (_, _) => Generate();
		settings.Controls.Add(generateButton);

		var navigation = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, WrapContents = false, Padding = new Padding(0, 8, 0, 0) };
		root.Controls.Add(navigation, 0, 0);
		root.SetRow(navigation, 0);
		navigation.BringToFront();

		_previousButton.Text = "← Назад";
		_previousButton.AutoSize = true;
		_previousButton.Click += (_, _) => ShowStep(_currentStep - 1);

		_nextButton.Text = "Далее →";
		_nextButton.AutoSize = true;
		_nextButton.Click += (_, _) => ShowStep(_currentStep + 1);

		var resetButton = new Button { Text = "Сбросить", AutoSize = true };
		resetButton.Click += (_, _) => Reset();

		_stepLabel.AutoSize = true;
		_stepLabel.Margin = new Padding(12, 7, 12, 0);

		navigation.Controls.Add(_previousButton);
		navigation.Controls.Add(_nextButton);
		navigation.Controls.Add(resetButton);
		navigation.Controls.Add(_stepLabel);

		Generate();
	}

	private static NumericUpDown CreateNumber(decimal min, decimal max, decimal value, decimal increment = 1, int decimals = 0) => new() { Minimum = min, Maximum = max, Value = value, Increment = increment, DecimalPlaces = decimals, Width = 75, Margin = new Padding(4, 3, 12, 3) };

	private static void AddSetting(FlowLayoutPanel panel, string text, Control control)
	{
		panel.Controls.Add(new Label { Text = text, AutoSize = true, Margin = new Padding(4, 8, 0, 0) });
		panel.Controls.Add(control);
	}

	private void Generate()
	{
		_steps.Clear();
		_currentStep = 0;

		var random = new Random();
		var points = new List<PointF> { new(0, (float)_height.Value), new(1, (float)_height.Value) };
		_steps.Add(points.ToList());

		double displacement = (double)_amplitude.Value;

		for (int iteration = 0; iteration < (int)_iterations.Value; iteration++)
		{
			var next = new List<PointF>(points.Count * 2 - 1);

			for (int i = 0; i < points.Count - 1; i++)
			{
				PointF a = points[i];
				PointF b = points[i + 1];
				next.Add(a);

				float x = (a.X + b.X) / 2;
				float averageHeight = (a.Y + b.Y) / 2;
				float offset = (float)((random.NextDouble() * 2 - 1) * displacement);
				float y = Math.Clamp(averageHeight + offset, 0, 100);

				next.Add(new PointF(x, y));
			}

			next.Add(points[^1]);
			points = next;
			_steps.Add(points.ToList());
			displacement *= (double)_roughness.Value;
		}

		ShowStep(0);
	}

	private void ShowStep(int step)
	{
		if (_steps.Count == 0)
			return;

		_currentStep = Math.Clamp(step, 0, _steps.Count - 1);
		_painter.SetPoints(_steps[_currentStep]);

		_stepLabel.Text = $"Шаг: {_currentStep} / {_steps.Count - 1}    Точек: {_steps[_currentStep].Count}";
		_previousButton.Enabled = _currentStep > 0;
		_nextButton.Enabled = _currentStep < _steps.Count - 1;
	}

	private void Reset()
	{
		_iterations.Value = 7;
		_height.Value = 45;
		_amplitude.Value = 20;
		_roughness.Value = 0.55m;
		Generate();
	}
}
