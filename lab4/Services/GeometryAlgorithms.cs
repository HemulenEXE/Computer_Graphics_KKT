using lab4.Models;

namespace lab4.Services;

/// <summary>Результат классификации точки относительно направленного ребра A→B.</summary>
public enum PointSide
{
    /// <summary>Точка лежит слева от ребра (так, как это видит пользователь на экране).</summary>
    Left,
    /// <summary>Точка лежит справа от ребра (так, как это видит пользователь на экране).</summary>
    Right,
    /// <summary>Точка лежит на прямой, содержащей ребро.</summary>
    On
}

/// <summary>Положение точки относительно многоугольника.</summary>
public enum PointLocation
{
    Outside,
    Inside,
    OnBoundary
}

/// <summary>Вид результата пересечения двух отрезков.</summary>
public enum IntersectionKind
{
    /// <summary>Отрезки не пересекаются.</summary>
    None,
    /// <summary>Отрезки пересекаются ровно в одной точке (см. Point).</summary>
    Point,
    /// <summary>Отрезки параллельны и лежат на разных прямых.</summary>
    Parallel,
    /// <summary>Отрезки лежат на одной прямой и перекрываются (общих точек бесконечно много).</summary>
    Overlap
}

/// <summary>Результат пересечения двух отрезков.</summary>
/// <param name="Kind">Что получилось.</param>
/// <param name="Point">Точка пересечения (заполнена только при Kind == Point).</param>
public readonly record struct IntersectionResult(IntersectionKind Kind, PointF Point = default)
{
    public bool HasPoint => Kind == IntersectionKind.Point;
}

/// <summary>
/// Геометрические алгоритмы лабораторной №4 (часть третьего человека):
/// 1) пересечение двух рёбер;
/// 2) принадлежность точки выпуклому / невыпуклому многоугольнику;
/// 3) положение точки слева/справа от ребра.
///
/// Класс статический и «чистый»: ничего не рисует и не хранит состояния,
/// поэтому любую проверку можно вызывать сколько угодно раз подряд,
/// не очищая экран и не перерисовывая многоугольник.
///
/// Система координат экрана: ось Y направлена ВНИЗ. Из-за этого знак
/// векторного произведения «переворачивается» по сравнению с учебником;
/// здесь это уже учтено, и Left/Right соответствуют тому, что видит пользователь.
/// </summary>
public static class GeometryAlgorithms
{
    /// <summary>
    /// Допуск (в пикселях) на «лежит на линии». Нужен, потому что после поворотов/масштабирования
    /// координаты float содержат небольшой шум, и точное равенство нулю почти никогда не выполняется.
    /// </summary>
    public const double Epsilon = 1e-3;

    // ---------------------------------------------------------------------
    // Вспомогательные функции
    // ---------------------------------------------------------------------

    /// <summary>Псевдоскалярное (косое) произведение векторов (ax, ay) и (bx, by).</summary>
    private static double Cross(double ax, double ay, double bx, double by) => ax * by - ay * bx;

    /// <summary>Расстояние от точки p до ОТРЕЗКА ab.</summary>
    private static double DistanceToSegment(PointF a, PointF b, PointF p)
    {
        double abx = b.X - a.X, aby = b.Y - a.Y;
        double apx = p.X - a.X, apy = p.Y - a.Y;
        double len2 = abx * abx + aby * aby;

        if (len2 < 1e-12)
            return Math.Sqrt(apx * apx + apy * apy);        // отрезок выродился в точку

        double t = Math.Clamp((apx * abx + apy * aby) / len2, 0.0, 1.0);
        double dx = apx - t * abx, dy = apy - t * aby;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    // ---------------------------------------------------------------------
    // 3) Точка слева / справа от ребра
    // ---------------------------------------------------------------------

    /// <summary>
    /// Определяет, по какую сторону от направленного ребра A→B лежит точка P.
    /// Сторона определяется знаком векторного произведения (B−A)×(P−A).
    /// Так как ось Y экрана направлена вниз, положительное значение означает «справа» на экране.
    /// </summary>
    public static PointSide ClassifyPoint(PointF a, PointF b, PointF p)
    {
        double abx = b.X - a.X, aby = b.Y - a.Y;
        double apx = p.X - a.X, apy = p.Y - a.Y;

        double cross = Cross(abx, aby, apx, apy);
        double length = Math.Sqrt(abx * abx + aby * aby);

        // Ребро выродилось в точку — сторона не определена.
        if (length < 1e-9)
            return PointSide.On;

        // cross / length — расстояние от P до ПРЯМОЙ AB (со знаком).
        double signedDistance = cross / length;

        if (Math.Abs(signedDistance) <= Epsilon) return PointSide.On;
        return signedDistance > 0 ? PointSide.Right : PointSide.Left;
    }

    /// <summary>То же для ребра, заданного полигоном из двух вершин (Polygon.IsEdge).</summary>
    public static PointSide ClassifyPoint(Polygon edge, PointF p)
    {
        if (!edge.IsEdge)
            throw new ArgumentException("Полигон должен быть ребром (ровно 2 вершины).", nameof(edge));

        return ClassifyPoint(edge.Vertices[0], edge.Vertices[1], p);
    }

    // ---------------------------------------------------------------------
    // 1) Пересечение двух рёбер (отрезков)
    // ---------------------------------------------------------------------

    /// <summary>
    /// Пересечение отрезков AB и CD параметрическим методом (как на слайдах):
    ///   P(t) = A + t·(B−A),  нормаль к CD: n = (−s.y, s.x),
    ///   t = n·(C−A) / n·(B−A),
    /// отрезки пересекаются, если 0 ≤ t ≤ 1 и 0 ≤ u ≤ 1 (u — параметр точки на CD).
    /// </summary>
    public static IntersectionResult IntersectSegments(PointF a, PointF b, PointF c, PointF d)
    {
        double rx = b.X - a.X, ry = b.Y - a.Y;   // направление AB
        double sx = d.X - c.X, sy = d.Y - c.Y;   // направление CD
        double acx = c.X - a.X, acy = c.Y - a.Y; // вектор A→C

        double rLen = Math.Sqrt(rx * rx + ry * ry);
        double sLen = Math.Sqrt(sx * sx + sy * sy);

        // Вырожденные случаи: один или оба «отрезка» — точки.
        if (rLen < 1e-9 || sLen < 1e-9)
        {
            if (rLen < 1e-9 && sLen < 1e-9)
                return Distance(a, c) <= Epsilon
                    ? new IntersectionResult(IntersectionKind.Point, a)
                    : new IntersectionResult(IntersectionKind.None);

            if (rLen < 1e-9)
                return DistanceToSegment(c, d, a) <= Epsilon
                    ? new IntersectionResult(IntersectionKind.Point, a)
                    : new IntersectionResult(IntersectionKind.None);

            return DistanceToSegment(a, b, c) <= Epsilon
                ? new IntersectionResult(IntersectionKind.Point, c)
                : new IntersectionResult(IntersectionKind.None);
        }

        // denom = n·(B−A), n = (−sy, sx). Равен 0, если AB ∥ CD.
        double denom = -sy * rx + sx * ry;

        // sin угла между отрезками: |denom| / (|r|·|s|). Малое значение => параллельны.
        if (Math.Abs(denom) / (rLen * sLen) < 1e-9)
        {
            // Параллельны. Лежат ли на одной прямой? Расстояние от C до прямой AB.
            double distToLine = Math.Abs(Cross(rx, ry, acx, acy)) / rLen;
            if (distToLine > Epsilon)
                return new IntersectionResult(IntersectionKind.Parallel);

            // Одна прямая: проецируем C и D на AB и смотрим, перекрываются ли интервалы.
            double tC = (acx * rx + acy * ry) / (rLen * rLen);
            double tD = ((d.X - a.X) * rx + (d.Y - a.Y) * ry) / (rLen * rLen);
            double lo = Math.Min(tC, tD), hi = Math.Max(tC, tD);

            double tol = Epsilon / rLen;
            if (hi < -tol || lo > 1 + tol)
                return new IntersectionResult(IntersectionKind.None);

            // Перекрытие: если оно вырождается в одну точку (отрезки лишь касаются концами) — вернём точку.
            if (Math.Abs(hi - 0) <= tol && lo < 0 || Math.Abs(lo - 1) <= tol && hi > 1)
            {
                double tp = Math.Abs(hi) <= tol ? 0.0 : 1.0;
                return new IntersectionResult(IntersectionKind.Point,
                    new PointF((float)(a.X + tp * rx), (float)(a.Y + tp * ry)));
            }

            return new IntersectionResult(IntersectionKind.Overlap);
        }

        // Общий случай: единственная точка пересечения ПРЯМЫХ.
        double t = (-sy * acx + sx * acy) / denom;             // n·(C−A) / n·(B−A)
        double u = (-ry * acx + rx * acy) / denom;             // параметр на CD

        double tolT = Epsilon / rLen;
        double tolU = Epsilon / sLen;
        if (t < -tolT || t > 1 + tolT || u < -tolU || u > 1 + tolU)
            return new IntersectionResult(IntersectionKind.None);   // прямые пересекаются, а отрезки — нет

        return new IntersectionResult(IntersectionKind.Point,
            new PointF((float)(a.X + t * rx), (float)(a.Y + t * ry)));
    }

    /// <summary>Пересечение двух рёбер, заданных полигонами из 2 вершин.</summary>
    public static IntersectionResult IntersectEdges(Polygon edge1, Polygon edge2)
    {
        if (!edge1.IsEdge || !edge2.IsEdge)
            throw new ArgumentException("Оба полигона должны быть рёбрами (ровно 2 вершины).");

        return IntersectSegments(edge1.Vertices[0], edge1.Vertices[1],
                                 edge2.Vertices[0], edge2.Vertices[1]);
    }

    private static double Distance(PointF p, PointF q)
    {
        double dx = p.X - q.X, dy = p.Y - q.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    // ---------------------------------------------------------------------
    // 2) Точка в многоугольнике
    // ---------------------------------------------------------------------

    /// <summary>Лежит ли точка на границе (на каком-либо ребре) многоугольника.</summary>
    public static bool IsOnBoundary(IReadOnlyList<PointF> v, PointF p)
    {
        int n = v.Count;
        if (n == 0) return false;
        if (n == 1) return Distance(v[0], p) <= Epsilon;

        for (int i = 0; i < n; i++)
        {
            if (DistanceToSegment(v[i], v[(i + 1) % n], p) <= Epsilon)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Выпуклый ли многоугольник. Условие: все повороты при обходе в одну сторону
    /// и суммарный поворот равен ровно одному обороту (это отсекает самопересекающиеся «звёзды»,
    /// у которых все повороты тоже в одну сторону, но суммарный поворот — 2 оборота и больше).
    /// </summary>
    public static bool IsConvex(IReadOnlyList<PointF> v)
    {
        int n = v.Count;
        if (n < 3) return false;

        int sign = 0;
        double totalAngle = 0;

        for (int i = 0; i < n; i++)
        {
            PointF p0 = v[i], p1 = v[(i + 1) % n], p2 = v[(i + 2) % n];
            double e1x = p1.X - p0.X, e1y = p1.Y - p0.Y;
            double e2x = p2.X - p1.X, e2y = p2.Y - p1.Y;

            double cross = Cross(e1x, e1y, e2x, e2y);
            double dot = e1x * e2x + e1y * e2y;

            // Нулевой поворот (три вершины на одной прямой) на выпуклость не влияет.
            if (Math.Abs(cross) > 1e-9)
            {
                int s = cross > 0 ? 1 : -1;
                if (sign == 0) sign = s;
                else if (s != sign) return false;      // поворот в другую сторону => есть «впадина»
            }

            totalAngle += Math.Atan2(cross, dot);
        }

        if (sign == 0) return false;                    // все вершины на одной прямой
        return Math.Abs(Math.Abs(totalAngle) - 2 * Math.PI) < 1e-6;
    }

    /// <summary>
    /// Точка в ВЫПУКЛОМ многоугольнике: точка внутри, если она лежит по одну и ту же сторону
    /// от всех рёбер (при обходе многоугольника в одном направлении). Работает при любом направлении обхода.
    /// </summary>
    public static PointLocation PointInConvexPolygon(IReadOnlyList<PointF> v, PointF p)
    {
        int n = v.Count;
        if (n < 3) return IsOnBoundary(v, p) ? PointLocation.OnBoundary : PointLocation.Outside;

        if (IsOnBoundary(v, p)) return PointLocation.OnBoundary;

        int sign = 0;
        for (int i = 0; i < n; i++)
        {
            PointF a = v[i], b = v[(i + 1) % n];
            double cross = Cross(b.X - a.X, b.Y - a.Y, p.X - a.X, p.Y - a.Y);
            if (Math.Abs(cross) < 1e-12) continue;

            int s = cross > 0 ? 1 : -1;
            if (sign == 0) sign = s;
            else if (s != sign) return PointLocation.Outside;   // нашлось ребро с другой стороны
        }

        return PointLocation.Inside;
    }

    /// <summary>
    /// Точка в НЕВЫПУКЛОМ многоугольнике: метод луча (чётность числа пересечений).
    /// Из точки выпускается горизонтальный луч вправо и считаются пересечения с рёбрами.
    /// Нечётное число — внутри, чётное — снаружи.
    /// Особые случаи (луч проходит через вершину или вдоль ребра) решаются правилом «полуоткрытого интервала»:
    /// ребро считается пересечённым, если одна его вершина строго ниже линии луча, а другая — не ниже.
    /// Так вершина, лежащая на луче, учитывается ровно один раз (или ни разу — для касательных),
    /// а горизонтальные рёбра не учитываются вовсе.
    /// Для самопересекающихся многоугольников действует правило чётности (even-odd).
    /// </summary>
    public static PointLocation PointInNonConvexPolygon(IReadOnlyList<PointF> v, PointF p)
    {
        int n = v.Count;
        if (n < 3) return IsOnBoundary(v, p) ? PointLocation.OnBoundary : PointLocation.Outside;

        if (IsOnBoundary(v, p)) return PointLocation.OnBoundary;

        bool inside = false;
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            PointF a = v[j], b = v[i];

            // Ребро пересекает линию луча (горизонтальные рёбра сюда не попадают).
            if ((a.Y > p.Y) != (b.Y > p.Y))
            {
                // X-координата пересечения ребра с горизонталью y = p.Y.
                double xCross = a.X + (p.Y - a.Y) * (b.X - a.X) / (double)(b.Y - a.Y);
                if (xCross > p.X)       // пересечение справа от точки — луч его «видит»
                    inside = !inside;
            }
        }

        return inside ? PointLocation.Inside : PointLocation.Outside;
    }

    /// <summary>
    /// Универсальная проверка: сама выбирает алгоритм — быстрый для выпуклого
    /// или метод луча для невыпуклого многоугольника.
    /// </summary>
    public static PointLocation PointInPolygon(IReadOnlyList<PointF> v, PointF p)
        => IsConvex(v) ? PointInConvexPolygon(v, p) : PointInNonConvexPolygon(v, p);

    /// <summary>То же для объекта Polygon.</summary>
    public static PointLocation PointInPolygon(Polygon polygon, PointF p)
        => PointInPolygon(polygon.Vertices, p);
}