using Avalonia;
using Avalonia.Animation.Easings;

namespace ConnectedAnimation.Avalonia;

public abstract class ConnectedAnimationConfiguration
{
    public abstract Easing GetEasing(Rect start, Rect end, Easing defaultEasing);

    public abstract TimeSpan GetDuration(Rect start, Rect end, TimeSpan defaultDuration);

    public abstract ICurve GetCurve(Rect start, Rect end, ICurve defaultCurve);
}
