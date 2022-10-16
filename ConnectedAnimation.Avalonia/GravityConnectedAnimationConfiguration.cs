using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Utilities;

namespace ConnectedAnimation.Avalonia;

public sealed class GravityConnectedAnimationConfiguration : ConnectedAnimationConfiguration
{
    public override ICurve GetCurve(Rect start, Rect end, ICurve defaultCurve)
    {
        return new BezierCurveQuadric();
    }

    public override TimeSpan GetDuration(Rect start, Rect end, TimeSpan defaultDuration)
    {
        var length = ((Vector)(start.TopLeft - end.TopLeft)).Length + Math.Min(start.Height, end.Height) / 2;

        return TimeSpan.FromMilliseconds(MathUtilities.Clamp(length, 170, 400));
    }

    public override Easing GetEasing(Rect start, Rect end, Easing defaultEasing)
    {
        return new LinearEasing();
    }
}
