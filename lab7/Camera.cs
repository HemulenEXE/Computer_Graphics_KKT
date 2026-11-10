using System;

namespace PolyhedraLab
{
    /// <summary>Камера, вращающаяся вокруг выбранной точки сцены.</summary>
    public sealed class Camera
    {
        public Point Position { get; private set; } = new Point(0, 0, 6);
        public Point Direction { get; private set; } = new Point(0, 0, -1);
        public Point Target { get; private set; } = new Point(0, 0, 0);
        public double[,] ViewMatrix { get; private set; } = Projection.Identity();
        public double[,] ProjectionMatrix { get; private set; } = Projection.Orthographic();

        public void Orbit(
            Point target,
            double yawDegrees,
            double pitchDegrees,
            double distance,
            ProjectionKind projectionKind)
        {
            ArgumentNullException.ThrowIfNull(target);
            if (!double.IsFinite(target.X) || !double.IsFinite(target.Y) || !double.IsFinite(target.Z))
                throw new ArgumentException("Координаты цели камеры должны быть конечными.", nameof(target));
            if (!double.IsFinite(yawDegrees))
                throw new ArgumentOutOfRangeException(nameof(yawDegrees), "Угол камеры должен быть конечным.");
            if (!double.IsFinite(pitchDegrees))
                throw new ArgumentOutOfRangeException(nameof(pitchDegrees), "Угол камеры должен быть конечным.");
            if (!double.IsFinite(distance) || distance <= 0)
                throw new ArgumentOutOfRangeException(nameof(distance), "Расстояние до цели должно быть положительным.");

            Target = target.Clone();

            double yaw = yawDegrees * Math.PI / 180.0;
            double pitch = pitchDegrees * Math.PI / 180.0;
            double sinYaw = Math.Sin(yaw);
            double cosYaw = Math.Cos(yaw);
            double sinPitch = Math.Sin(pitch);
            double cosPitch = Math.Cos(pitch);

            double rightX = cosYaw;
            double rightY = 0;
            double rightZ = sinYaw;
            double upX = sinPitch * sinYaw;
            double upY = cosPitch;
            double upZ = -sinPitch * cosYaw;
            double backwardX = -sinYaw * cosPitch;
            double backwardY = sinPitch;
            double backwardZ = cosYaw * cosPitch;

            Position = new Point(
                target.X + backwardX * distance,
                target.Y + backwardY * distance,
                target.Z + backwardZ * distance);
            Direction = new Point(-backwardX, -backwardY, -backwardZ);

            double[,] view = Projection.Identity();
            view[0, 0] = rightX;
            view[1, 0] = rightY;
            view[2, 0] = rightZ;
            view[0, 1] = upX;
            view[1, 1] = upY;
            view[2, 1] = upZ;
            view[0, 2] = backwardX;
            view[1, 2] = backwardY;
            view[2, 2] = backwardZ;
            view[3, 0] = -(Position.X * rightX + Position.Y * rightY + Position.Z * rightZ);
            view[3, 1] = -(Position.X * upX + Position.Y * upY + Position.Z * upZ);
            view[3, 2] = -(Position.X * backwardX + Position.Y * backwardY + Position.Z * backwardZ);
            ViewMatrix = view;

            ProjectionMatrix = projectionKind == ProjectionKind.Perspective
                ? Projection.Perspective(distance)
                : Projection.Orthographic();
        }
    }
}
