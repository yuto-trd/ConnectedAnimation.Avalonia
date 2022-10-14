using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Avalonia.ConnectedAnimation;

internal sealed class CoordinatedVisual : Control
{
    public static readonly DirectProperty<CoordinatedVisual, double> ProgressProperty = ConnectedVisual.ProgressProperty.AddOwner<CoordinatedVisual>(
        o => o.Progress,
        (o, v) => o.Progress = v);
    private readonly ConnectedVisual _connectedVisual;
    private readonly Control _control;
    private readonly AnimationDirection _direction;
    private readonly Brush _destinationBrush;
    private readonly Rect _sourceBounds;
    private readonly Rect _destinationBounds;
    private double _progress = 0.0;
    private readonly RenderTargetBitmap _destinationImage;
    private readonly RelativeLocation _location;

    static CoordinatedVisual()
    {
        AffectsRender<CoordinatedVisual>(ProgressProperty);
    }

    public CoordinatedVisual(ConnectedVisual connectedVisual, Control control, AnimationDirection direction)
    {
        _connectedVisual = connectedVisual;
        _control = control ?? throw new ArgumentNullException(nameof(control));
        _direction = direction;

        // Store PreviousArrange
        var prevArrange = ((ILayoutable)control).PreviousArrange;

        _destinationBounds = control.AbsoluteBounds();
        _location = Helper.GetRelativeLocation(connectedVisual.DestinationBounds, _destinationBounds);

        // Find the optimal sourceBounds.
        {
            var width = _destinationBounds.Width;
            var height = _destinationBounds.Height;

            double x = 0, y = 0;
            var pos = _destinationBounds.Position - connectedVisual.DestinationBounds.Position;
            x = connectedVisual.SourceBounds.X + pos.X;
            y = connectedVisual.SourceBounds.Y + pos.Y;
            // 170

            //switch (_location)
            //{
            //    case RelativeLocation.Above:
            //        if (direction.HasFlag(AnimationDirection.Lower))
            //        {
            //            y = connectedVisual.DestinationBounds.Top;
            //        }

            //        if (direction.HasFlag(AnimationDirection.Upper))
            //        {
            //            y = connectedVisual.SourceBounds.Top;
            //        }

            //        x = 0;
            //        break;
            //    case RelativeLocation.Below:
            //        if (direction.HasFlag(AnimationDirection.Lower))
            //        {
            //            y = connectedVisual.DestinationBounds.Bottom;
            //        }

            //        if (direction.HasFlag(AnimationDirection.Upper))
            //        {
            //            y = connectedVisual.SourceBounds.Bottom;
            //        }

            //        x = 0;
            //        break;
            //    case RelativeLocation.Right:
            //        if (direction.HasFlag(AnimationDirection.Left))
            //        {
            //            x = connectedVisual.SourceBounds.Right;
            //        }

            //        if (direction.HasFlag(AnimationDirection.Right))
            //        {
            //            x = connectedVisual.DestinationBounds.Right;
            //        }

            //        y = 0;
            //        break;
            //    case RelativeLocation.Left:
            //        if (direction.HasFlag(AnimationDirection.Left))
            //        {
            //            x = connectedVisual.SourceBounds.Left;
            //        }

            //        if (direction.HasFlag(AnimationDirection.Right))
            //        {
            //            x = connectedVisual.DestinationBounds.Left;
            //        }

            //        y = 0;
            //        break;
            //    default:
            //        break;
            //}

            _sourceBounds = new Rect(x, y, width, height);
        }

        // Pre render
        control.Measure(Size.Infinity);
        control.Arrange(new Rect(control.DesiredSize));
        _destinationImage = new RenderTargetBitmap(PixelSize.FromSize(control.DesiredSize, 1));
        _destinationImage.Render(control);
        if (prevArrange.HasValue)
        {
            control.Arrange(prevArrange.Value);
        }
        else
        {
            _control.InvalidateArrange();
        }

        _destinationBrush = new ImageBrush(_destinationImage)
        {
            Stretch = Stretch.Fill,
            BitmapInterpolationMode = BitmapInterpolationMode.HighQuality
        };


    }

    public double Progress
    {
        get => _progress;
        set => SetAndRaise(ProgressProperty, ref _progress, Math.Clamp(value, 0, 1));
    }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(
            (_destinationBounds.Left - _sourceBounds.Left) * _progress + _sourceBounds.Left,
            (_destinationBounds.Top - _sourceBounds.Top) * _progress + _sourceBounds.Top,
            (_destinationBounds.Width - _sourceBounds.Width) * _progress + _sourceBounds.Width,
            (_destinationBounds.Height - _sourceBounds.Height) * _progress + _sourceBounds.Height);

        using (context.PushPreTransform(Matrix.CreateTranslation(bounds.X, bounds.Y)))
        {
            bounds = bounds.WithX(0).WithY(0);

            _destinationBrush.Opacity = _progress;
            context.DrawRectangle(_destinationBrush, null, bounds);
        }
    }

    public async Task RunAnimation(TimeSpan duration, Easing easing, TimeSpan delay)
    {
        var animation = new Animation.Animation()
        {
            Duration = duration,
            Delay = delay,
            Easing = easing,
            Children =
            {
                new KeyFrame()
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter(ProgressProperty, 0.0)
                    }
                },
                new KeyFrame()
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(ProgressProperty, 1.0)
                    }
                }
            }
        };

        var storedOpacity = _control.Opacity;
        _control.Opacity = 0;
        await animation.RunAsync(this, null);

        _control.Opacity = storedOpacity;
    }
}