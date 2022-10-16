using Avalonia;

namespace ConnectedAnimation.Avalonia;

public sealed class LinearCurve : ICurve
{
    public Vector CalculatePosition(Rect start, Rect end, double progress)
    {
        return (end.Position - start.Position) * progress + start.Position;
    }
}
