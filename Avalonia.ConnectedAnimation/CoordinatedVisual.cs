using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;

namespace Avalonia.ConnectedAnimation;

internal sealed class CoordinatedVisual : Control, IDisposable
{
    public static readonly DirectProperty<CoordinatedVisual, double> ProgressProperty = ConnectedVisual.ProgressProperty.AddOwner<CoordinatedVisual>(
        o => o.Progress,
        (o, v) => o.Progress = v);
    private readonly Control _control;
    private readonly Brush _destinationBrush;
    private readonly ICurve _curve;
    private readonly Rect _sourceBounds;
    private readonly Rect _destinationBounds;
    private readonly RenderTargetBitmap _destinationImage;
    private readonly RelativeLocation _location;
    private double _progress = 0.0;

    static CoordinatedVisual()
    {
        AffectsRender<CoordinatedVisual>(ProgressProperty);
    }

    public CoordinatedVisual(ConnectedVisual connectedVisual, Control control, ICurve defaultCurve, ConnectedAnimationConfiguration configuration)
    {
        _control = control ?? throw new ArgumentNullException(nameof(control));

        // Store PreviousArrange
        var prevArrange = ((ILayoutable)control).PreviousArrange;

        _destinationBounds = control.AbsoluteBounds();
        _location = Helper.GetRelativeLocation(connectedVisual.DestinationBounds, _destinationBounds);

        // Find the optimal sourceBounds.
        {
            var width = _destinationBounds.Width;
            var height = _destinationBounds.Height;

            var pos = _destinationBounds.Position - connectedVisual.DestinationBounds.Position;
            double x = connectedVisual.SourceBounds.X + pos.X;
            double y = connectedVisual.SourceBounds.Y + pos.Y;
            // 170

            switch (_location)
            {
                case RelativeLocation.Above:
                    y = connectedVisual.SourceBounds.Y + (_destinationBounds.Top - connectedVisual.DestinationBounds.Top);
                    break;
                case RelativeLocation.Below:
                    y = connectedVisual.SourceBounds.Y + (_destinationBounds.Bottom - connectedVisual.DestinationBounds.Bottom);
                    break;
                case RelativeLocation.Right:
                    x = connectedVisual.SourceBounds.X + (_destinationBounds.Right - connectedVisual.DestinationBounds.Right);
                    break;
                case RelativeLocation.Left:
                    x = connectedVisual.SourceBounds.X + (_destinationBounds.Left - connectedVisual.DestinationBounds.Left);
                    break;
                default:
                    break;
            }

            _sourceBounds = new Rect(x, y, width, height);
        }

        _destinationBounds = _destinationBounds.Inflate(control.Margin);
        _sourceBounds = _sourceBounds.Inflate(control.Margin);

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

        _curve = configuration.GetCurve(_sourceBounds, _destinationBounds, defaultCurve);
    }

    public double Progress
    {
        get => _progress;
        set => SetAndRaise(ProgressProperty, ref _progress, Math.Clamp(value, 0, 1));
    }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(new Size(
            (_destinationBounds.Width - _sourceBounds.Width) * _progress + _sourceBounds.Width,
            (_destinationBounds.Height - _sourceBounds.Height) * _progress + _sourceBounds.Height));

        var point = _curve.CalculatePosition(_sourceBounds, _destinationBounds, _progress);

        using (context.PushPreTransform(Matrix.CreateTranslation(point.X, point.Y)))
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

    public void Dispose()
    {
        _destinationImage.Dispose();
    }
}