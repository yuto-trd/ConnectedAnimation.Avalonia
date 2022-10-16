using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace ConnectedAnimation.Avalonia;

public static class Helper
{
    public static Task WaitVisualTreeAttached(this Control control)
    {
        if (control.GetVisualRoot() != null)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();

        void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (control.GetVisualRoot() != null)
            {
                control.AttachedToVisualTree -= OnAttachedToVisualTree;
                tcs.SetResult();
            }
        }

        control.AttachedToVisualTree += OnAttachedToVisualTree;

        return tcs.Task;
    }

    internal static AnimationDirection GetDirection(Rect source, Rect destination)
    {
        var sourceCenter = source.Center;
        var destinationCenter = destination.Center;

        if (sourceCenter.NearlyEquals(destinationCenter))
        {
            return AnimationDirection.None;
        }
        else if (sourceCenter.X < destinationCenter.X)
        {
            // 右に移動
            if (sourceCenter.Y < destinationCenter.Y)
            {
                // 下に移動
                return AnimationDirection.RightLower;
            }
            else
            {
                // 上に移動
                return AnimationDirection.RightUpper;
            }
        }
        else
        {
            // 左に移動
            if (sourceCenter.Y < destinationCenter.Y)
            {
                // 下に移動
                return AnimationDirection.LeftLower;
            }
            else
            {
                // 上に移動
                return AnimationDirection.LeftUpper;
            }
        }
    }

    internal static double NormalizeRadian(double radian)
    {
        const double TWO_PI = 2 * Math.PI;

        double normalized = radian % TWO_PI;
        normalized = (normalized + TWO_PI) % TWO_PI;
        return normalized <= Math.PI ? normalized : normalized - TWO_PI;
    }

    internal static double GetRadian(Point point1, Point point2)
    {
        return Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
    }

    // rect は relativeTo の (上、右、下、左) に配置されます。
    internal static RelativeLocation GetRelativeLocation(Rect relativeTo, Rect rect)
    {
        var topLeftToBtmRight = GetRadian(relativeTo.TopLeft, relativeTo.BottomRight);
        var btmLeftToTopRight = -topLeftToBtmRight;
        var topRightToBtmLeft = Math.PI - topLeftToBtmRight;
        var btmRightToTopLeft = -topRightToBtmLeft;

        var radian = GetRadian(relativeTo.Center, rect.Center);

        if (btmLeftToTopRight < radian && radian < topLeftToBtmRight)
        {
            return RelativeLocation.Right;
        }
        else if (topLeftToBtmRight < radian && radian < topRightToBtmLeft)
        {
            return RelativeLocation.Below;
        }
        else if (topRightToBtmLeft < radian && btmRightToTopLeft < radian)
        {
            return RelativeLocation.Left;
        }
        else if (btmRightToTopLeft < radian && radian < btmLeftToTopRight)
        {
            return RelativeLocation.Above;
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    internal static Rect AbsoluteBounds(this Layoutable layoutable)
    {
        var destinationBounds = layoutable.Bounds;
        var root = layoutable.GetVisualRoot()!;
        var topLeft = layoutable.TranslatePoint(default, root);
        var bottomRight = layoutable.TranslatePoint(new Point(destinationBounds.Width, destinationBounds.Height), root);

        if (topLeft.HasValue && bottomRight.HasValue)
        {
            return new Rect(topLeft.Value, bottomRight.Value);
        }
        else
        {
            return Rect.Empty;
        }
    }

    internal static System.Numerics.Vector2 ToVector2(this Point point)
    {
        return new System.Numerics.Vector2((float)point.X, (float)point.Y);
    }
}
