using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace _1a_3;

public partial class Form1 : Form
{
	private readonly PictureBox _pictureBox;
	private readonly Bitmap _bitmap;

	private readonly NumericUpDown _seedX;
	private readonly NumericUpDown _seedY;

	private readonly NumericUpDown _ax;
	private readonly NumericUpDown _ay;
	private readonly NumericUpDown _bx;
	private readonly NumericUpDown _by;
	private readonly NumericUpDown _cx;
	private readonly NumericUpDown _cy;

	private readonly Button _fillColorButton;
	private readonly Button _colorAButton;
	private readonly Button _colorBButton;
	private readonly Button _colorCButton;

	private Color _fillColor = Color.Yellow;
	private Color _colorA = Color.Red;
	private Color _colorB = Color.Green;
	private Color _colorC = Color.Blue;

	public Form1()
	{
		Text = "Компьютерная графика — заливка и градиентный треугольник";
		Width = 1250;
		Height = 800;
		StartPosition = FormStartPosition.CenterScreen;

		_bitmap = new Bitmap(1000, 650, PixelFormat.Format32bppArgb);

		using (Graphics graphics = Graphics.FromImage(_bitmap))
			graphics.Clear(Color.White);

		_pictureBox = new PictureBox
		{
			Image = _bitmap,
			Dock = DockStyle.Fill,
			SizeMode = PictureBoxSizeMode.Normal
		};

		var controls = new Panel
		{
			Dock = DockStyle.Right,
			Width = 230,
			Padding = new Padding(10)
		};

		var fillGroup = CreateFillGroup(out _seedX, out _seedY, out _fillColorButton);
		var triangleGroup = CreateTriangleGroup(
			out _ax,
			out _ay,
			out _bx,
			out _by,
			out _cx,
			out _cy,
			out _colorAButton,
			out _colorBButton,
			out _colorCButton);

		controls.Controls.Add(triangleGroup);
		controls.Controls.Add(fillGroup);

		Controls.Add(_pictureBox);
		Controls.Add(controls);

		_fillColorButton.Click += (_, _) => ChooseColor(_fillColorButton, color => _fillColor = color);
		_colorAButton.Click += (_, _) => ChooseColor(_colorAButton, color => _colorA = color);
		_colorBButton.Click += (_, _) => ChooseColor(_colorBButton, color => _colorB = color);
		_colorCButton.Click += (_, _) => ChooseColor(_colorCButton, color => _colorC = color);
	}

	private GroupBox CreateFillGroup(
		out NumericUpDown seedX,
		out NumericUpDown seedY,
		out Button fillColorButton)
	{
		var group = new GroupBox
		{
			Text = "Рекурсивная заливка",
			Dock = DockStyle.Top,
			Height = 200
		};

		seedX = CreateNumber(100, 0, 999);
		seedY = CreateNumber(100, 0, 649);

		fillColorButton = new Button
		{
			Text = "Цвет заливки",
			Width = 190,
			Height = 30,
			BackColor = _fillColor
		};

		var fillButton = new Button
		{
			Text = "Выполнить заливку",
			Width = 190,
			Height = 35
		};

		fillButton.Click += (_, _) => ExecuteFill();

		AddControl(group, new Label { Text = "X начальной точки:", AutoSize = true }, 10, 25);
		AddControl(group, seedX, 10, 45);

		AddControl(group, new Label { Text = "Y начальной точки:", AutoSize = true }, 10, 75);
		AddControl(group, seedY, 10, 95);

		AddControl(group, fillColorButton, 10, 125);
		AddControl(group, fillButton, 10, 160);

		return group;
	}

	private GroupBox CreateTriangleGroup(
		out NumericUpDown ax,
		out NumericUpDown ay,
		out NumericUpDown bx,
		out NumericUpDown by,
		out NumericUpDown cx,
		out NumericUpDown cy,
		out Button colorAButton,
		out Button colorBButton,
		out Button colorCButton)
	{
		var group = new GroupBox
		{
			Text = "Градиентный треугольник",
			Dock = DockStyle.Top,
			Height = 390
		};

		ax = CreateNumber(150, 0, 999);
		ay = CreateNumber(100, 0, 649);

		bx = CreateNumber(700, 0, 999);
		by = CreateNumber(150, 0, 649);

		cx = CreateNumber(400, 0, 999);
		cy = CreateNumber(500, 0, 649);

		colorAButton = CreateColorButton("Цвет A", _colorA);
		colorBButton = CreateColorButton("Цвет B", _colorB);
		colorCButton = CreateColorButton("Цвет C", _colorC);

		var drawButton = new Button
		{
			Text = "Растеризовать треугольник",
			Width = 190,
			Height = 40
		};

		drawButton.Click += (_, _) => DrawGradientTriangle();

		AddControl(group, new Label { Text = "Вершина A", AutoSize = true }, 10, 25);
		AddControl(group, new Label { Text = "X:", AutoSize = true }, 10, 50);
		AddControl(group, ax, 30, 47);
		AddControl(group, new Label { Text = "Y:", AutoSize = true }, 115, 50);
		AddControl(group, ay, 135, 47);
		AddControl(group, colorAButton, 10, 75);

		AddControl(group, new Label { Text = "Вершина B", AutoSize = true }, 10, 115);
		AddControl(group, new Label { Text = "X:", AutoSize = true }, 10, 140);
		AddControl(group, bx, 30, 137);
		AddControl(group, new Label { Text = "Y:", AutoSize = true }, 115, 140);
		AddControl(group, by, 135, 137);
		AddControl(group, colorBButton, 10, 165);

		AddControl(group, new Label { Text = "Вершина C", AutoSize = true }, 10, 205);
		AddControl(group, new Label { Text = "X:", AutoSize = true }, 10, 230);
		AddControl(group, cx, 30, 227);
		AddControl(group, new Label { Text = "Y:", AutoSize = true }, 115, 230);
		AddControl(group, cy, 135, 227);
		AddControl(group, colorCButton, 10, 255);

		AddControl(group, drawButton, 10, 305);

		return group;
	}

	private static NumericUpDown CreateNumber(decimal value, decimal min, decimal max)
	{
		return new NumericUpDown
		{
			Minimum = min,
			Maximum = max,
			Value = value,
			Width = 70
		};
	}

	private static Button CreateColorButton(string text, Color color)
	{
		return new Button
		{
			Text = text,
			BackColor = color,
			Width = 190,
			Height = 30
		};
	}

	private static void AddControl(Control parent, Control control, int x, int y)
	{
		control.Left = x;
		control.Top = y;
		parent.Controls.Add(control);
	}

	private static void ChooseColor(Button button, Action<Color> setColor)
	{
		using var dialog = new ColorDialog();

		if (dialog.ShowDialog() != DialogResult.OK)
			return;

		setColor(dialog.Color);
		button.BackColor = dialog.Color;
	}

	private void ExecuteFill()
	{
		int x = (int)_seedX.Value;
		int y = (int)_seedY.Value;

		FillBySeries(x, y, _fillColor);

		_pictureBox.Invalidate();
	}

	private void FillBySeries(int startX, int startY, Color fillColor)
	{
		BitmapData data = _bitmap.LockBits(
			new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
			ImageLockMode.ReadWrite,
			PixelFormat.Format32bppArgb);

		int bytes = Math.Abs(data.Stride) * _bitmap.Height;
		byte[] buffer = new byte[bytes];

		try
		{
			Marshal.Copy(data.Scan0, buffer, 0, bytes);

			Color sourceColor = GetPixel(buffer, data.Stride, startX, startY);

			if (sourceColor.ToArgb() == fillColor.ToArgb())
				return;

			FillSeriesRecursive(
				buffer,
				data.Stride,
				startX,
				startY,
				sourceColor.ToArgb(),
				fillColor);

			Marshal.Copy(buffer, 0, data.Scan0, bytes);
		}
		finally
		{
			_bitmap.UnlockBits(data);
		}
	}

	private void FillSeriesRecursive(
		byte[] buffer,
		int stride,
		int x,
		int y,
		int sourceArgb,
		Color fillColor)
	{
		if (x < 0 || x >= _bitmap.Width || y < 0 || y >= _bitmap.Height)
			return;

		if (GetPixel(buffer, stride, x, y).ToArgb() != sourceArgb)
			return;

		int left = x;

		while (left >= 0 && GetPixel(buffer, stride, left, y).ToArgb() == sourceArgb)
			left--;

		left++;

		int right = x;

		while (right < _bitmap.Width && GetPixel(buffer, stride, right, y).ToArgb() == sourceArgb)
			right++;

		right--;

		for (int currentX = left; currentX <= right; currentX++)
			SetPixel(buffer, stride, currentX, y, fillColor);

		ScanNeighborLine(
			buffer,
			stride,
			left,
			right,
			y - 1,
			sourceArgb,
			fillColor);

		ScanNeighborLine(
			buffer,
			stride,
			left,
			right,
			y + 1,
			sourceArgb,
			fillColor);
	}

	private void ScanNeighborLine(
		byte[] buffer,
		int stride,
		int left,
		int right,
		int y,
		int sourceArgb,
		Color fillColor)
	{
		if (y < 0 || y >= _bitmap.Height)
			return;

		int x = left;

		while (x <= right)
		{
			while (x <= right && GetPixel(buffer, stride, x, y).ToArgb() != sourceArgb)
				x++;

			if (x > right)
				return;

			int seedX = x;

			while (x <= right && GetPixel(buffer, stride, x, y).ToArgb() == sourceArgb)
				x++;

			FillSeriesRecursive(
				buffer,
				stride,
				seedX,
				y,
				sourceArgb,
				fillColor);
		}
	}

	private void DrawGradientTriangle()
	{
		PointF a = new((float)_ax.Value, (float)_ay.Value);
		PointF b = new((float)_bx.Value, (float)_by.Value);
		PointF c = new((float)_cx.Value, (float)_cy.Value);

		if (Math.Abs(Cross(b, c, a)) < double.Epsilon)
		{
			MessageBox.Show(
				"Вершины треугольника лежат на одной прямой.",
				"Ошибка",
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);

			return;
		}

		BitmapData data = _bitmap.LockBits(
			new Rectangle(0, 0, _bitmap.Width, _bitmap.Height),
			ImageLockMode.ReadWrite,
			PixelFormat.Format32bppArgb);

		int bytes = Math.Abs(data.Stride) * _bitmap.Height;
		byte[] buffer = new byte[bytes];

		try
		{
			Marshal.Copy(data.Scan0, buffer, 0, bytes);

			RasterizeTriangle(
				buffer,
				data.Stride,
				a,
				b,
				c,
				_colorA,
				_colorB,
				_colorC);

			Marshal.Copy(buffer, 0, data.Scan0, bytes);
		}
		finally
		{
			_bitmap.UnlockBits(data);
		}

		_pictureBox.Invalidate();
	}

	private void RasterizeTriangle(
		byte[] buffer,
		int stride,
		PointF a,
		PointF b,
		PointF c,
		Color colorA,
		Color colorB,
		Color colorC)
	{
		int minX = Math.Max(
			0,
			(int)Math.Floor(Math.Min(a.X, Math.Min(b.X, c.X))));

		int maxX = Math.Min(
			_bitmap.Width - 1,
			(int)Math.Ceiling(Math.Max(a.X, Math.Max(b.X, c.X))));

		int minY = Math.Max(
			0,
			(int)Math.Floor(Math.Min(a.Y, Math.Min(b.Y, c.Y))));

		int maxY = Math.Min(
			_bitmap.Height - 1,
			(int)Math.Ceiling(Math.Max(a.Y, Math.Max(b.Y, c.Y))));

		double denominator =
			(b.Y - c.Y) * (a.X - c.X) +
			(c.X - b.X) * (a.Y - c.Y);

		for (int y = minY; y <= maxY; y++)
		{
			for (int x = minX; x <= maxX; x++)
			{
				double alpha =
					((b.Y - c.Y) * (x - c.X) +
					 (c.X - b.X) * (y - c.Y)) /
					denominator;

				double beta =
					((c.Y - a.Y) * (x - c.X) +
					 (a.X - c.X) * (y - c.Y)) /
					denominator;

				double gamma = 1 - alpha - beta;

				if (alpha < 0 || beta < 0 || gamma < 0)
					continue;

				byte red = Interpolate(
					alpha,
					beta,
					gamma,
					colorA.R,
					colorB.R,
					colorC.R);

				byte green = Interpolate(
					alpha,
					beta,
					gamma,
					colorA.G,
					colorB.G,
					colorC.G);

				byte blue = Interpolate(
					alpha,
					beta,
					gamma,
					colorA.B,
					colorB.B,
					colorC.B);

				SetPixel(
					buffer,
					stride,
					x,
					y,
					Color.FromArgb(255, red, green, blue));
			}
		}
	}

	private static byte Interpolate(
		double alpha,
		double beta,
		double gamma,
		byte a,
		byte b,
		byte c)
	{
		return (byte)Math.Clamp(
			alpha * a +
			beta * b +
			gamma * c,
			0,
			255);
	}

	private static double Cross(PointF a, PointF b, PointF c)
	{
		return (b.X - a.X) * (c.Y - a.Y) -
			   (b.Y - a.Y) * (c.X - a.X);
	}

	private static Color GetPixel(byte[] buffer, int stride, int x, int y)
	{
		int index = y * stride + x * 4;

		return Color.FromArgb(
			buffer[index + 3],
			buffer[index + 2],
			buffer[index + 1],
			buffer[index]);
	}

	private static void SetPixel(
		byte[] buffer,
		int stride,
		int x,
		int y,
		Color color)
	{
		int index = y * stride + x * 4;

		buffer[index] = color.B;
		buffer[index + 1] = color.G;
		buffer[index + 2] = color.R;
		buffer[index + 3] = color.A;
	}

	protected override void OnFormClosed(FormClosedEventArgs e)
	{
		_bitmap.Dispose();
		base.OnFormClosed(e);
	}
}