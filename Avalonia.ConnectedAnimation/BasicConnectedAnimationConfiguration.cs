using Avalonia.Animation.Easings;

namespace Avalonia.ConnectedAnimation;

public sealed class BasicConnectedAnimationConfiguration : ConnectedAnimationConfiguration
{
    public override ICurve GetCurve(Rect start, Rect end, ICurve defaultCurve)
    {
        return defaultCurve;
    }

    public override TimeSpan GetDuration(Rect start, Rect end, TimeSpan defaultDuration)
    {
        return defaultDuration;
    }

    public override Easing GetEasing(Rect start, Rect end, Easing defaultEasing)
    {
        return defaultEasing;
    }
}
