using Avalonia;

namespace ConnectedAnimation.Avalonia;

public interface ICurve
{
    Vector CalculatePosition(Rect start, Rect end, double progress);
}
