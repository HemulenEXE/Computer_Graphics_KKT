using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PolyhedraLab
{
    public partial class Form1 : Form
    {
        // ----- данные -----
        private readonly MatrixOperations _ops = new MatrixOperations();
        private Polyhedron _polyhedron = Polyhedron.CreateTetrahedron();
        private string _lastOperation = "Создан: Тетраэдр";
        private bool _updatingZoom = false;

        // ----- область рисования и результат -----
        private readonly PolyhedronView _view = new PolyhedronView();
        private readonly TextBox txtResult = new TextBox();

        // ----- выбор многогранника -----
        private readonly ComboBox cmbPolyhedron = new ComboBox();
        private readonly Button btnCreate = new Button();

        // ----- проекция -----
        private readonly RadioButton rbAxonometric = new RadioButton();
        private readonly RadioButton rbPerspective = new RadioButton();
        private readonly NumericUpDown numYaw = Num(45, -360, 360, 2, 5);
        private readonly NumericUpDown numPitch = Num(35.264m, -360, 360, 3, 5);
        private readonly NumericUpDown numDistance = Num(6, 1, 100, 1, 0.5m);
        private readonly NumericUpDown numZoom = Num(90, 10, 500, 0, 10);
        private readonly CheckBox chkAxes = new CheckBox();
        private readonly CheckBox chkHideBack = new CheckBox();
        private readonly CheckBox chkLabels = new CheckBox();

        // ----- смещение -----
        private readonly NumericUpDown numDx = Num(0, -100, 100, 2, 0.5m);
        private readonly NumericUpDown numDy = Num(0, -100, 100, 2, 0.5m);
        private readonly NumericUpDown numDz = Num(0, -100, 100, 2, 0.5m);
        private readonly Button btnTranslate = new Button();

        // ----- повороты вокруг координатных осей -----
        private readonly NumericUpDown numRotX = Num(0, -360, 360, 2, 5);
        private readonly NumericUpDown numRotY = Num(0, -360, 360, 2, 5);
        private readonly NumericUpDown numRotZ = Num(0, -360, 360, 2, 5);
        private readonly Button btnRotateAxes = new Button();

        // ----- масштаб -----
        private readonly NumericUpDown numSx = Num(1, -100, 100, 2, 0.1m);
        private readonly NumericUpDown numSy = Num(1, -100, 100, 2, 0.1m);
        private readonly NumericUpDown numSz = Num(1, -100, 100, 2, 0.1m);
        private readonly Button btnScaleOrigin = new Button();
        private readonly Button btnScaleCenter = new Button();

        // ----- отражение -----
        private readonly ComboBox cmbPlane = new ComboBox();
        private readonly Button btnReflect = new Button();

        // ----- вращение вокруг прямой через центр, параллельной оси -----
        private readonly ComboBox cmbAxis = new ComboBox();
        private readonly NumericUpDown numAngleCenter = Num(30, -360, 360, 2, 5);
        private readonly Button btnRotateCenter = new Button();

        // ----- вращение вокруг произвольной прямой AB -----
        private readonly NumericUpDown numAx = Num(0, -100, 100, 2, 0.5m);
        private readonly NumericUpDown numAy = Num(0, -100, 100, 2, 0.5m);
        private readonly NumericUpDown numAz = Num(0, -100, 100, 2, 0.5m);
        private readonly NumericUpDown numBx = Num(1, -100, 100, 2, 0.5m);
        private readonly NumericUpDown numBy = Num(1, -100, 100, 2, 0.5m);
        private readonly NumericUpDown numBz = Num(1, -100, 100, 2, 0.5m);
        private readonly NumericUpDown numAngleLine = Num(30, -360, 360, 2, 5);
        private readonly CheckBox chkShowLine = new CheckBox();
        private readonly Button btnRotateLine = new Button();

        public Form1()
        {
            InitializeControls();
        }

        // ==================================================================
        //  Построение интерфейса
        // ==================================================================

        private static NumericUpDown Num(decimal value, decimal min, decimal max, int decimals, decimal step)
        {
            NumericUpDown n = new NumericUpDown();
            n.DecimalPlaces = decimals;
            n.Minimum = min;
            n.Maximum = max;
            n.Increment = step;
            n.Value = value;
            n.Width = 62;
            n.Margin = new Padding(2, 2, 6, 2);
            return n;
        }

        private static Label Lbl(string text)
        {
            Label l = new Label();
            l.Text = text;
            l.AutoSize = true;
            l.Margin = new Padding(0, 6, 2, 0);
            return l;
        }

        private static FlowLayoutPanel Row(params Control[] controls)
        {
            FlowLayoutPanel row = new FlowLayoutPanel();
            row.FlowDirection = FlowDirection.LeftToRight;
            row.WrapContents = false;
            row.AutoSize = true;
            row.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            row.Margin = new Padding(0, 1, 0, 1);
            row.Controls.AddRange(controls);
            return row;
        }

        private static GroupBox Group(string title, params Control[] rows)
        {
            FlowLayoutPanel stack = new FlowLayoutPanel();
            stack.FlowDirection = FlowDirection.TopDown;
            stack.WrapContents = false;
            stack.AutoSize = true;
            stack.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            stack.Location = new System.Drawing.Point(8, 20);
            stack.Controls.AddRange(rows);

            GroupBox box = new GroupBox();
            box.Text = title;
            box.AutoSize = true;
            box.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            box.Padding = new Padding(6, 4, 6, 8);
            box.Margin = new Padding(0, 0, 0, 8);
            box.MinimumSize = new Size(380, 0);
            box.Controls.Add(stack);
            return box;
        }

        private static Button Btn(Button b, string text)
        {
            b.Text = text;
            b.AutoSize = true;
            b.Margin = new Padding(2, 2, 6, 2);
            return b;
        }

        private void InitializeControls()
        {
            Text = "Лабораторная работа — многогранники";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 650);
            Size = new Size(1280, 820);

            // --- многогранник ---
            cmbPolyhedron.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPolyhedron.Items.AddRange(new object[] { "Тетраэдр", "Гексаэдр (куб)", "Октаэдр" });
            cmbPolyhedron.SelectedIndex = 0;
            cmbPolyhedron.Width = 170;
            Btn(btnCreate, "Создать / сбросить");

            // --- проекция ---
            rbAxonometric.Text = "Аксонометрическая";
            rbAxonometric.AutoSize = true;
            rbAxonometric.Checked = true;
            rbPerspective.Text = "Перспективная";
            rbPerspective.AutoSize = true;

            chkAxes.Text = "Оси координат";
            chkAxes.AutoSize = true;
            chkAxes.Checked = true;
            chkHideBack.Text = "Скрывать нелицевые грани";
            chkHideBack.AutoSize = true;
            chkHideBack.Checked = true;
            chkLabels.Text = "Подписи вершин";
            chkLabels.AutoSize = true;

            Label zoomHint = Lbl("Масштаб: колёсико мыши (после щелчка по рисунку)");
            zoomHint.ForeColor = Color.DimGray;

            // --- отражение / оси ---
            cmbPlane.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPlane.Items.AddRange(new object[] { "XY", "XZ", "YZ" });
            cmbPlane.SelectedIndex = 0;
            cmbPlane.Width = 60;

            cmbAxis.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAxis.Items.AddRange(new object[] { "X", "Y", "Z" });
            cmbAxis.SelectedIndex = 1;
            cmbAxis.Width = 60;

            Btn(btnTranslate, "Применить");
            Btn(btnRotateAxes, "Применить");
            Btn(btnScaleOrigin, "От начала координат");
            Btn(btnScaleCenter, "От центра");
            Btn(btnReflect, "Применить");
            Btn(btnRotateCenter, "Применить");
            Btn(btnRotateLine, "Повернуть");

            chkShowLine.Text = "Показать прямую";
            chkShowLine.AutoSize = true;

            // --- панель справа ---
            FlowLayoutPanel stack = new FlowLayoutPanel();
            stack.FlowDirection = FlowDirection.TopDown;
            stack.WrapContents = false;
            stack.AutoSize = true;
            stack.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            stack.Dock = DockStyle.Top;

            stack.Controls.Add(Group("Многогранник",
                Row(cmbPolyhedron, btnCreate)));

            stack.Controls.Add(Group("Проекция",
                Row(rbAxonometric, rbPerspective),
                Row(Lbl("Вид: поворот Y°"), numYaw, Lbl("X°"), numPitch),
                Row(Lbl("Расстояние до камеры d"), numDistance, Lbl("Масштаб"), numZoom),
                Row(chkAxes, chkLabels),
                Row(chkHideBack),
                Row(zoomHint)));

            stack.Controls.Add(Group("Смещение",
                Row(Lbl("dx"), numDx, Lbl("dy"), numDy, Lbl("dz"), numDz),
                Row(btnTranslate)));

            stack.Controls.Add(Group("Поворот вокруг осей (X, затем Y, затем Z)",
                Row(Lbl("X°"), numRotX, Lbl("Y°"), numRotY, Lbl("Z°"), numRotZ),
                Row(btnRotateAxes)));

            stack.Controls.Add(Group("Масштаб",
                Row(Lbl("α (X)"), numSx, Lbl("β (Y)"), numSy, Lbl("γ (Z)"), numSz),
                Row(btnScaleOrigin, btnScaleCenter)));

            stack.Controls.Add(Group("Отражение относительно плоскости",
                Row(Lbl("Плоскость"), cmbPlane, btnReflect)));

            stack.Controls.Add(Group("Вращение вокруг оси через центр",
                Row(Lbl("Ось, параллельная:"), cmbAxis, Lbl("Угол°"), numAngleCenter, btnRotateCenter)));

            stack.Controls.Add(Group("Поворот вокруг прямой AB",
                Row(Lbl("A:"), numAx, numAy, numAz),
                Row(Lbl("B:"), numBx, numBy, numBz),
                Row(Lbl("Угол°"), numAngleLine, chkShowLine),
                Row(btnRotateLine)));

            Panel rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Right;
            rightPanel.Width = 420;
            rightPanel.AutoScroll = true;
            rightPanel.Padding = new Padding(8);
            rightPanel.Controls.Add(stack);

            // --- результат ---
            txtResult.Multiline = true;
            txtResult.ReadOnly = true;
            txtResult.ScrollBars = ScrollBars.Vertical;
            txtResult.WordWrap = false;
            txtResult.Dock = DockStyle.Bottom;
            txtResult.Height = 200;
            txtResult.Font = new Font("Consolas", 10);

            // --- область рисования ---
            _view.Dock = DockStyle.Fill;

            // Порядок добавления важен: последним добавленный Dock-элемент занимает край первым.
            Controls.Add(_view);
            Controls.Add(txtResult);
            Controls.Add(rightPanel);

            WireEvents();
            ApplyViewSettings();
            UpdateRotationLine();
            RefreshAll();
        }

        private void WireEvents()
        {
            btnCreate.Click += delegate { CreateSelected(); };

            // проекция и вид
            rbAxonometric.CheckedChanged += delegate { ApplyViewSettings(); };
            rbPerspective.CheckedChanged += delegate { ApplyViewSettings(); };
            numYaw.ValueChanged += delegate { ApplyViewSettings(); };
            numPitch.ValueChanged += delegate { ApplyViewSettings(); };
            numDistance.ValueChanged += delegate { ApplyViewSettings(); };
            numZoom.ValueChanged += delegate { ApplyViewSettings(); };
            chkAxes.CheckedChanged += delegate { ApplyViewSettings(); };
            chkHideBack.CheckedChanged += delegate { ApplyViewSettings(); };
            chkLabels.CheckedChanged += delegate { ApplyViewSettings(); };
            _view.ZoomChanged += delegate { SyncZoomBox(); };

            // преобразования
            btnTranslate.Click += delegate { OnTranslate(); };
            btnRotateAxes.Click += delegate { OnRotateAxes(); };
            btnScaleOrigin.Click += delegate { OnScale(false); };
            btnScaleCenter.Click += delegate { OnScale(true); };
            btnReflect.Click += delegate { OnReflect(); };
            btnRotateCenter.Click += delegate { OnRotateCenter(); };
            btnRotateLine.Click += delegate { OnRotateLine(); };

            // прямая AB показывается сразу при изменении полей
            chkShowLine.CheckedChanged += delegate { UpdateRotationLine(); };
            numAx.ValueChanged += delegate { UpdateRotationLine(); };
            numAy.ValueChanged += delegate { UpdateRotationLine(); };
            numAz.ValueChanged += delegate { UpdateRotationLine(); };
            numBx.ValueChanged += delegate { UpdateRotationLine(); };
            numBy.ValueChanged += delegate { UpdateRotationLine(); };
            numBz.ValueChanged += delegate { UpdateRotationLine(); };
        }

        // ==================================================================
        //  Проекция и вид
        // ==================================================================

        private void ApplyViewSettings()
        {
            ViewSettings s = _view.Settings;

            s.Kind = rbPerspective.Checked ? ProjectionKind.Perspective : ProjectionKind.Axonometric;
            s.Yaw = (double)numYaw.Value;
            s.Pitch = (double)numPitch.Value;
            s.Distance = (double)numDistance.Value;
            s.HideBackFaces = chkHideBack.Checked;

            if (!_updatingZoom)
                s.Zoom = (double)numZoom.Value;

            // Расстояние до камеры нужно только перспективе.
            numDistance.Enabled = rbPerspective.Checked;

            _view.ShowAxes = chkAxes.Checked;
            _view.ShowVertexLabels = chkLabels.Checked;
            _view.Invalidate();
        }

        // Колёсико мыши изменило масштаб — показываем его в поле.
        private void SyncZoomBox()
        {
            _updatingZoom = true;
            try
            {
                decimal z = (decimal)Math.Round(_view.Settings.Zoom);
                if (z < numZoom.Minimum) z = numZoom.Minimum;
                if (z > numZoom.Maximum) z = numZoom.Maximum;
                numZoom.Value = z;
            }
            finally
            {
                _updatingZoom = false;
            }
        }

        private void UpdateRotationLine()
        {
            _view.LineA = new Point((double)numAx.Value, (double)numAy.Value, (double)numAz.Value);
            _view.LineB = new Point((double)numBx.Value, (double)numBy.Value, (double)numBz.Value);
            _view.ShowLine = chkShowLine.Checked;
            _view.Invalidate();
        }

        // ==================================================================
        //  Создание многогранника
        // ==================================================================

        private void CreateSelected()
        {
            switch (cmbPolyhedron.SelectedIndex)
            {
                case 0:
                    _polyhedron = Polyhedron.CreateTetrahedron();
                    break;
                case 1:
                    _polyhedron = Polyhedron.CreateHexahedron();
                    break;
                case 2:
                    _polyhedron = Polyhedron.CreateOctahedron();
                    break;
                default:
                    MessageBox.Show(this, "Выберите многогранник.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
            }

            _lastOperation = "Создан: " + cmbPolyhedron.Text;
            RefreshAll();
        }

        // ==================================================================
        //  Преобразования (сами матрицы — в MatrixOperations)
        // ==================================================================

        /// <summary>Применяет операцию к вершинам текущего многогранника и обновляет картинку и текст.</summary>
        private void Apply(string name, Func<List<Point>, List<Point>> operation)
        {
            try
            {
                // Копируем результат до очистки списка: операция может вернуть тот же самый список.
                List<Point> result = new List<Point>(operation(_polyhedron.Vertices));

                _polyhedron.Vertices.Clear();
                _polyhedron.Vertices.AddRange(result);

                _lastOperation = name;
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void OnTranslate()
        {
            float dx = (float)numDx.Value, dy = (float)numDy.Value, dz = (float)numDz.Value;

            Apply("Смещение на (" + Fmt(dx) + "; " + Fmt(dy) + "; " + Fmt(dz) + ")",
                v => _ops.TranslationPoints(v, dx, dy, dz));
        }

        private void OnRotateAxes()
        {
            float ax = (float)numRotX.Value, ay = (float)numRotY.Value, az = (float)numRotZ.Value;

            Apply("Поворот вокруг осей координат: X " + Fmt(ax) + "°, Y " + Fmt(ay) + "°, Z " + Fmt(az) + "°",
                v =>
                {
                    List<Point> r = v;
                    if (ax != 0f) r = _ops.RotationPoints(r, 'X', ax);
                    if (ay != 0f) r = _ops.RotationPoints(r, 'Y', ay);
                    if (az != 0f) r = _ops.RotationPoints(r, 'Z', az);
                    return r;
                });
        }

        private void OnScale(bool aroundCenter)
        {
            float sx = (float)numSx.Value, sy = (float)numSy.Value, sz = (float)numSz.Value;
            string factors = "(" + Fmt(sx) + "; " + Fmt(sy) + "; " + Fmt(sz) + ")";

            if (aroundCenter)
                Apply("Масштаб относительно центра " + factors, v => _ops.DilatationCenterPoints(v, sx, sy, sz));
            else
                Apply("Масштаб относительно начала координат " + factors, v => _ops.ScalePoints(v, sx, sy, sz));
        }

        private void OnReflect()
        {
            string plane = cmbPlane.Text;

            Apply("Отражение относительно плоскости " + plane, v => _ops.ReflectionPoints(v, plane));
        }

        private void OnRotateCenter()
        {
            char axis = cmbAxis.Text[0];
            float angle = (float)numAngleCenter.Value;

            Apply("Вращение вокруг прямой через центр, параллельной оси " + axis + ", на " + Fmt(angle) + "°",
                v => _ops.RotationCenterPoints(v, axis, angle));
        }

        private void OnRotateLine()
        {
            Point a = new Point((double)numAx.Value, (double)numAy.Value, (double)numAz.Value);
            Point b = new Point((double)numBx.Value, (double)numBy.Value, (double)numBz.Value);
            float angle = (float)numAngleLine.Value;

            if (a.X == b.X && a.Y == b.Y && a.Z == b.Z)
            {
                MessageBox.Show(this, "Точки A и B совпадают: прямая не задана.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Apply("Поворот на " + Fmt(angle) + "° вокруг прямой A(" + Fmt(a.X) + "; " + Fmt(a.Y) + "; " + Fmt(a.Z)
                + ") — B(" + Fmt(b.X) + "; " + Fmt(b.Y) + "; " + Fmt(b.Z) + ")",
                v => _ops.RotationAroundLinePoints(v, a, b, angle));
        }

        // ==================================================================
        //  Вывод результата
        // ==================================================================

        private void RefreshAll()
        {
            _view.Model = _polyhedron;
            _view.Invalidate();
            txtResult.Text = BuildReport();
        }

        private static string Fmt(double value)
        {
            if (Math.Abs(value) < 0.0005) value = 0;     // чтобы не печатать «-0»
            return value.ToString("0.###");
        }

        private string BuildReport()
        {
            StringBuilder sb = new StringBuilder();
            List<Point> vs = _polyhedron.Vertices;

            double minX = vs[0].X, maxX = vs[0].X;
            double minY = vs[0].Y, maxY = vs[0].Y;
            double minZ = vs[0].Z, maxZ = vs[0].Z;
            foreach (Point p in vs)
            {
                minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y);
                minZ = Math.Min(minZ, p.Z); maxZ = Math.Max(maxZ, p.Z);
            }

            sb.AppendLine("Последняя операция: " + _lastOperation);
            sb.AppendLine("Вершин: " + vs.Count + ",  граней: " + _polyhedron.Faces.Count);
            sb.AppendLine("Центр: (" + Fmt((minX + maxX) / 2) + "; " + Fmt((minY + maxY) / 2) + "; " + Fmt((minZ + maxZ) / 2) + ")");
            sb.AppendLine();
            sb.AppendLine("Вершины:");

            for (int i = 0; i < vs.Count; i++)
                sb.AppendLine("V" + i + ": (" + Fmt(vs[i].X) + "; " + Fmt(vs[i].Y) + "; " + Fmt(vs[i].Z) + ")");

            sb.AppendLine();
            sb.AppendLine("Грани:");

            for (int i = 0; i < _polyhedron.Faces.Count; i++)
                sb.AppendLine("F" + i + ": " + string.Join(", ", _polyhedron.Faces[i].VertexIndices));

            return sb.ToString();
        }
    }
}
