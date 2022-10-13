using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
    private readonly Rect _sourceBounds;
    private readonly Rect _destinationBounds;
    private double _progress = 0.0;
    private readonly RenderTargetBitmap _destinationImage;

    static ConnectedVisual()
    {
        AffectsRender<ConnectedVisual>(ProgressProperty);
    }

    public ConnectedVisual(Rect sourceBounds, Control destination)
    {
        _sourceBounds = sourceBounds;
        _destination = destination ?? throw new ArgumentNullException(nameof(destination));

        // Store PreviousArrange
        var prevArrange = ((ILayoutable)destination).PreviousArrange;

        var destinationBounds = _destination.Bounds;
        var root = _destination.GetVisualRoot()!;
        var topLeft = _destination.TranslatePoint(default, root);
        var bottomRight = _destination.TranslatePoint(new Point(destinationBounds.Width, destinationBounds.Height), root);

        if (topLeft.HasValue && bottomRight.HasValue)
        {
            _destinationBounds = new Rect(topLeft.Value, bottomRight.Value);
        }

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

            //_destinationBrush.Opacity = progress;
            context.DrawRectangle(_destinationBrush, null, bounds);
        }
    }
}