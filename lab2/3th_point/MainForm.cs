using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace _3th_point;

public class MainForm : Form
{
    private Bitmap? _originalImage;
    private Bitmap? _resultImage;

    private PictureBox pictureBoxOriginal = null!;
    private PictureBox pictureBoxResult = null!;

    private TextBox textBoxHue = null!;
    private TextBox textBoxSaturation = null!;
    private TextBox textBoxBrightness = null!;

    private Button buttonOpen = null!;
    private Button buttonConvert = null!;
    private Button buttonSave = null!;
    private Button buttonReset = null!;

    private Label labelStatus = null!;

    public MainForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Преобразование RGB → HSV";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 1200;
        Height = 750;
        MinimumSize = new Size(900, 600);

        pictureBoxOriginal = new PictureBox
        {
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.LightGray,
            Dock = DockStyle.Fill
        };

        pictureBoxResult = new PictureBox
        {
            BorderStyle = BorderStyle.FixedSingle,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.LightGray,
            Dock = DockStyle.Fill
        };

        var originalLabel = new Label
        {
            Text = "Исходное изображение",
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        var resultLabel = new Label
        {
            Text = "Результат",
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        var originalPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };

        originalPanel.Controls.Add(pictureBoxOriginal);
        originalPanel.Controls.Add(originalLabel);

        var resultPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };

        resultPanel.Controls.Add(pictureBoxResult);
        resultPanel.Controls.Add(resultLabel);

        var imagesPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(10)
        };

        imagesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        imagesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        imagesPanel.Controls.Add(originalPanel, 0, 0);
        imagesPanel.Controls.Add(resultPanel, 1, 0);

        buttonOpen = new Button
        {
            Text = "Открыть изображение",
            Width = 160,
            Height = 35
        };

        buttonConvert = new Button
        {
            Text = "Преобразовать",
            Width = 160,
            Height = 35
        };

        buttonSave = new Button
        {
            Text = "Сохранить результат",
            Width = 160,
            Height = 35
        };

        buttonReset = new Button
        {
            Text = "Сбросить",
            Width = 120,
            Height = 35
        };

        buttonOpen.Click += buttonOpen_Click;
        buttonConvert.Click += buttonConvert_Click;
        buttonSave.Click += buttonSave_Click;
        buttonReset.Click += buttonReset_Click;

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 50,
            Padding = new Padding(10, 5, 10, 5)
        };

        buttonsPanel.Controls.Add(buttonOpen);
        buttonsPanel.Controls.Add(buttonConvert);
        buttonsPanel.Controls.Add(buttonSave);
        buttonsPanel.Controls.Add(buttonReset);

        textBoxHue = CreateValueTextBox();
        textBoxSaturation = CreateValueTextBox();
        textBoxBrightness = CreateValueTextBox();

        var huePanel = CreateValuePanel("Оттенок:", textBoxHue);
        var saturationPanel = CreateValuePanel("Насыщенность:", textBoxSaturation);
        var brightnessPanel = CreateValuePanel("Яркость:", textBoxBrightness);

        labelStatus = new Label
        {
            Text = "Введите значения от -100 до 100",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var settingsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 160,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(10)
        };

        settingsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        settingsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        settingsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        settingsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

        settingsPanel.Controls.Add(huePanel, 0, 0);
        settingsPanel.Controls.Add(saturationPanel, 0, 1);
        settingsPanel.Controls.Add(brightnessPanel, 0, 2);
        settingsPanel.Controls.Add(labelStatus, 0, 3);

        Controls.Add(imagesPanel);
        Controls.Add(settingsPanel);
        Controls.Add(buttonsPanel);
    }

    private static TextBox CreateValueTextBox()
    {
        return new TextBox
        {
            Text = "0",
            Width = 100,
            TextAlign = HorizontalAlignment.Center
        };
    }

    private static Panel CreateValuePanel(string labelText, TextBox textBox)
    {
        var label = new Label
        {
            Text = labelText,
            Width = 150,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var hint = new Label
        {
            Text = "(-100 ... 100)",
            Width = 120,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var panel = new Panel
        {
            Dock = DockStyle.Fill
        };

        label.Location = new Point(0, 5);
        textBox.Location = new Point(150, 2);
        hint.Location = new Point(260, 5);

        panel.Controls.Add(label);
        panel.Controls.Add(textBox);
        panel.Controls.Add(hint);

        return panel;
    }

    private void buttonOpen_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            var image = new Bitmap(dialog.FileName);
            var newImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(newImage)) 
                graphics.DrawImage(image, 0, 0, image.Width, image.Height);

            image.Dispose();

            _originalImage?.Dispose();
            _resultImage?.Dispose();

            _originalImage = newImage;
            _resultImage = new Bitmap(newImage);

            var oldOriginal = pictureBoxOriginal.Image;
            var oldResult = pictureBoxResult.Image;

            pictureBoxOriginal.Image = new Bitmap(_originalImage);
            pictureBoxResult.Image = new Bitmap(_resultImage);

            oldOriginal?.Dispose();
            oldResult?.Dispose();

            textBoxHue.Text = "0";
            textBoxSaturation.Text = "0";
            textBoxBrightness.Text = "0";

            labelStatus.Text = "Изображение загружено";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void buttonConvert_Click(object? sender, EventArgs e)
    {
        if (_originalImage == null)
        {
            MessageBox.Show("Сначала откройте изображение.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!TryGetValue(textBoxHue, true, "Оттенок", out int hue))
            return;

        if (!TryGetValue(textBoxSaturation, false, "Насыщенность", out int saturation))
            return;

        if (!TryGetValue(textBoxBrightness, false, "Яркость", out int brightness))
            return;

        buttonConvert.Enabled = false;
        buttonOpen.Enabled = false;
        buttonSave.Enabled = false;

        labelStatus.Text = "Выполняется преобразование...";

        try
        {
            Bitmap source = new Bitmap(_originalImage);
            Bitmap result = await Task.Run(() => ConvertImage(source, hue, saturation, brightness));

            source.Dispose();

            _resultImage?.Dispose();
            _resultImage = result;

            var oldImage = pictureBoxResult.Image;
            pictureBoxResult.Image = new Bitmap(result);
            oldImage?.Dispose();

            labelStatus.Text = "Преобразование завершено";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            labelStatus.Text = "Ошибка преобразования";
        }
        finally
        {
            buttonConvert.Enabled = true;
            buttonOpen.Enabled = true;
            buttonSave.Enabled = true;
        }
    }

    private static bool TryGetValue(TextBox textBox, bool h, string name, out int value)
    {
        value = 0;

        if (!int.TryParse(textBox.Text, out value))
        {
            MessageBox.Show($"{name}: введите целое число.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            textBox.Focus();
            textBox.SelectAll();
            return false;
        }

        if (value < (h ? -360 : -100) || value > ( h ? 360 : 100))
        {
            MessageBox.Show($"{name}: значение должно быть от -100 до 100.", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            textBox.Focus();
            textBox.SelectAll();
            return false;
        }

        return true;
    }

    private void buttonReset_Click(object? sender, EventArgs e)
    {
        if (_originalImage == null)
            return;

        textBoxHue.Text = "0";
        textBoxSaturation.Text = "0";
        textBoxBrightness.Text = "0";

        _resultImage?.Dispose();
        _resultImage = new Bitmap(_originalImage);

        var oldImage = pictureBoxResult.Image;
        pictureBoxResult.Image = new Bitmap(_resultImage);
        oldImage?.Dispose();

        labelStatus.Text = "Изменения сброшены";
    }

    private async void buttonSave_Click(object? sender, EventArgs e)
    {
        if (_resultImage == null)
        {
            MessageBox.Show("Нет изображения для сохранения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "PNG|*.png|JPEG|*.jpg|BMP|*.bmp",
            DefaultExt = "png"
        };

        if (dialog.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            using var imageToSave = new Bitmap(_resultImage);

            ImageFormat format = Path.GetExtension(dialog.FileName).ToLowerInvariant() switch
                {
                    ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                    ".bmp" => ImageFormat.Bmp,
                    _ => ImageFormat.Png
                };

            await Task.Run(() => imageToSave.Save(dialog.FileName, format));
            labelStatus.Text = "Изображение сохранено";
            MessageBox.Show("Изображение сохранено.", "Готово", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Bitmap ConvertImage(Bitmap source, int hueShift, int saturationShift, int brightnessShift)
    {
        int width = source.Width;
        int height = source.Height;

        var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var rectangle = new Rectangle(0, 0, width, height);

        BitmapData sourceData = source.LockBits(rectangle, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        BitmapData resultData = result.LockBits(rectangle, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        int bytes = Math.Abs(sourceData.Stride) * height;

        byte[] sourceBuffer = new byte[bytes];
        byte[] resultBuffer = new byte[bytes];

        try
        {
            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, bytes);

            for (int y = 0; y < height; y++)
            {
                int sourceRow = y * sourceData.Stride;
                int resultRow = y * resultData.Stride;

                for (int x = 0; x < width; x++)
                {
                    int sourceIndex = sourceRow + x * 4;
                    int resultIndex = resultRow + x * 4;

                    byte blue = sourceBuffer[sourceIndex];
                    byte green = sourceBuffer[sourceIndex + 1];
                    byte red = sourceBuffer[sourceIndex + 2];
                    byte alpha = sourceBuffer[sourceIndex + 3];

                    RgbToHsv(red, green, blue, out double hue, out double saturation, out double value);

                    hue += hueShift;

                    while (hue >= 360)
                        hue -= 360;

                    while (hue < 0)
                        hue += 360;

                    saturation += saturationShift / 100.0;
                    value += brightnessShift / 100.0;

                    saturation = Math.Clamp(saturation, 0, 1);
                    value = Math.Clamp(value, 0, 1);

                    HsvToRgb(hue, saturation, value, out byte newRed, out byte newGreen, out byte newBlue);

                    resultBuffer[resultIndex] = newBlue;
                    resultBuffer[resultIndex + 1] = newGreen;
                    resultBuffer[resultIndex + 2] = newRed;
                    resultBuffer[resultIndex + 3] = alpha;
                }
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);
        }
        finally
        {
            source.UnlockBits(sourceData);
            result.UnlockBits(resultData);
        }

        return result;
    }

    private static void RgbToHsv(byte red, byte green, byte blue, out double hue, out double saturation, out double value)
    {
        double r = red / 255.0;
        double g = green / 255.0;
        double b = blue / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));

        double delta = max - min;

        value = max;

        if (max == 0)
            saturation = 0;
        else
            saturation = delta / max;

        if (delta == 0)
        {
            hue = 0;
            return;
        }

        if (max == r)
            hue = 60 * (((g - b) / delta) % 6);
        else if (max == g)
            hue = 60 * (((b - r) / delta) + 2);
        else
            hue = 60 * (((r - g) / delta) + 4);

        if (hue < 0)
            hue += 360;
    }

    private static void HsvToRgb(double hue, double saturation, double value, out byte red, out byte green, out byte blue)
    {
        double c = value * saturation;
        double x = c * (1 - Math.Abs((hue / 60 % 2) - 1));

        double m = value - c;

        double r;
        double g;
        double b;

        if (hue < 60)
        {
            r = c;
            g = x;
            b = 0;
        }
        else if (hue < 120)
        {
            r = x;
            g = c;
            b = 0;
        }
        else if (hue < 180)
        {
            r = 0;
            g = c;
            b = x;
        }
        else if (hue < 240)
        {
            r = 0;
            g = x;
            b = c;
        }
        else if (hue < 300)
        {
            r = x;
            g = 0;
            b = c;
        }
        else
        {
            r = c;
            g = 0;
            b = x;
        }

        red = (byte)Math.Clamp((r + m) * 255, 0, 255);
        green = (byte)Math.Clamp((g + m) * 255, 0, 255);
        blue = (byte)Math.Clamp((b + m) * 255, 0, 255);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        pictureBoxOriginal.Image?.Dispose();
        pictureBoxResult.Image?.Dispose();

        _originalImage?.Dispose();
        _resultImage?.Dispose();

        base.OnFormClosed(e);
    }
}