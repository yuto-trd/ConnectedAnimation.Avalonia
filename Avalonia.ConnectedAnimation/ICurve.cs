using Avalonia.Animation.Animators;
using Avalonia.Animation.Easings;

namespace Avalonia.ConnectedAnimation;

public interface ICurve
{
    Vector CalculatePosition(Rect start, Rect end, double progress);
}
