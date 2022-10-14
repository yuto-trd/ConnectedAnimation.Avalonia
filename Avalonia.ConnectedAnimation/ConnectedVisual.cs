using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Avalonia.ConnectedAnimation;

internal sealed class ConnectedVisual : Control
{
    public static readonly DirectProperty<ConnectedVisual, double> ProgressProperty = AvaloniaProperty.RegisterDirect<ConnectedVisual, double>(
        "Progress",
        o => o.Progress,
        (o, v) => o.Progress = v);

    private readonly Control _destination;
    private readonly Brush _destinationBrush;
    private double _progress = 0.0;
    private readonly RenderTargetBitmap _destinationImage;

    static ConnectedVisual()
    {
        AffectsRender<ConnectedVisual>(ProgressProperty);
    }

    public ConnectedVisual(Rect sourceBounds, Control destination)
    {
        SourceBounds = sourceBounds;
        _destination = destination ?? throw new ArgumentNullException(nameof(destination));

        // Store PreviousArrange
        var prevArrange = ((ILayoutable)destination).PreviousArrange;

        DestinationBounds = destination.AbsoluteBounds();

        // Pre render
        destination.Measure(Size.Infinity);
        destination.Arrange(new Rect(destination.DesiredSize));
        _destinationImage = new RenderTargetBitmap(PixelSize.FromSize(destination.DesiredSize, 1));
        _destinationImage.Render(destination);
        if (prevArrange.HasValue)
        {
            destination.Arrange(prevArrange.Value);
        }
        else
        {
            _destination.InvalidateArrange();
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

    public Rect DestinationBounds { get; }

    public Rect SourceBounds { get; }

    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(
            (DestinationBounds.Left - SourceBounds.Left) * _progress + SourceBounds.Left,
            (DestinationBounds.Top - SourceBounds.Top) * _progress + SourceBounds.Top,
            (DestinationBounds.Width - SourceBounds.Width) * _progress + SourceBounds.Width,
            (DestinationBounds.Height - SourceBounds.Height) * _progress + SourceBounds.Height);

        using (context.PushPreTransform(Matrix.CreateTranslation(bounds.X, bounds.Y)))
        {
            bounds = bounds.WithX(0).WithY(0);

            //_destinationBrush.Opacity = progress;
            context.DrawRectangle(_destinationBrush, null, bounds);
        }
    }

    public async Task RunAnimation(TimeSpan duration, Easing easing)
    {
        var animation = new Animation.Animation()
        {
            Duration = duration,
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

        var storedOpacity = _destination.Opacity;
        _destination.Opacity = 0;
        await animation.RunAsync(this, null);

        _destination.Opacity = storedOpacity;
    }
}
