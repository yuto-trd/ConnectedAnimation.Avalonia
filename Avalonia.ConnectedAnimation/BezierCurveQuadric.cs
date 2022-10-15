// https://github.com/opentk/opentk/blob/131db3c812584aad52325c67675ab32ab8c9ddc2/src/OpenTK.Mathematics/Vector/Vector2.cs

using System.Diagnostics.Contracts;
using System.Numerics;

namespace Avalonia.ConnectedAnimation;

internal sealed class BezierCurveQuadric : ICurve
{
    public Vector StartAnchor;

    public Vector EndAnchor;

    public Vector ControlPoint;

    public double Parallel;

    public BezierCurveQuadric()
    {
        StartAnchor = Vector.Zero;
        EndAnchor = Vector.One;
        ControlPoint = new Vector(0, 0.5);
        Parallel = 0.0;
    }

    public BezierCurveQuadric(Vector startAnchor, Vector endAnchor, Vector controlPoint)
    {
        StartAnchor = startAnchor;
        EndAnchor = endAnchor;
        ControlPoint = controlPoint;
        Parallel = 0.0;
    }

    public BezierCurveQuadric(double parallel, Vector startAnchor, Vector endAnchor, Vector controlPoint)
    {
        Parallel = parallel;
        StartAnchor = startAnchor;
        EndAnchor = endAnchor;
        ControlPoint = controlPoint;
    }

    public Vector CalculatePoint(double progress)
    {
        static Vector PerpendicularRight(Vector point)
        {
            // https://github.com/opentk/opentk/blob/131db3c812584aad52325c67675ab32ab8c9ddc2/src/OpenTK.Mathematics/Vector/Vector2.cs#L144
            return new Vector(point.Y, -point.X);
        }

        var c = 1.0 - progress;
        var r = new Vector
        (
            (c * c * StartAnchor.X) + (2 * progress * c * ControlPoint.X) + (progress * progress * EndAnchor.X),
            (c * c * StartAnchor.Y) + (2 * progress * c * ControlPoint.Y) + (progress * progress * EndAnchor.Y)
        );

        if (Parallel == 0.0)
        {
            return r;
        }

        Vector perpendicular;

        if (progress == 0.0)
        {
            perpendicular = ControlPoint - StartAnchor;
        }
        else
        {
            perpendicular = r - CalculatePointOfDerivative(progress);
        }

        return r + (PerpendicularRight(perpendicular.Normalize()) * Parallel);
    }

    private Vector CalculatePointOfDerivative(double t)
    {
        var r = new Vector
        (
            ((1.0f - t) * StartAnchor.X) + (t * ControlPoint.X),
            ((1.0f - t) * StartAnchor.Y) + (t * ControlPoint.Y)
        );

        return r;
    }

    public double CalculateLength(double precision)
    {
        var length = 0.0;
        var old = CalculatePoint(0.0);

        for (var i = precision; i < 1.0 + precision; i += precision)
        {
            var n = CalculatePoint(i);
            length += (n - old).Length;
            old = n;
        }

        return length;
    }

    public Vector CalculatePosition(Rect start, Rect end, double progress)
    {
        StartAnchor = start.Position;
        EndAnchor = end.Position;

        var centerX = ((end.X - start.X) * 0.5) + start.X;
        var bottom = Math.Max(start.Bottom, end.Bottom) + Math.Min(start.Height, end.Height) / 2;

        ControlPoint = new Vector(centerX, bottom);

        return CalculatePoint(progress);
    }
}