using Avalonia;
using Avalonia.Animation.Easings;

namespace ConnectedAnimation.Avalonia;

public sealed class DirectConnectedAnimationConfiguration : ConnectedAnimationConfiguration
{
    private static readonly Easing s_easing = new SplineEasing(0.1, 0.9, 0.2, 1);

    public override ICurve GetCurve(Rect start, Rect end, ICurve defaultCurve)
    {
        return defaultCurve;
    }

    public override TimeSpan GetDuration(Rect start, Rect end, TimeSpan defaultDuration)
    {
        var length = ((Vector)(start.TopLeft - end.TopLeft)).Length;

        return TimeSpan.FromMilliseconds(Math.Max(length, 150));
    }

    public override Easing GetEasing(Rect start, Rect end, Easing defaultEasing)
    {
        return s_easing;
    }
}
