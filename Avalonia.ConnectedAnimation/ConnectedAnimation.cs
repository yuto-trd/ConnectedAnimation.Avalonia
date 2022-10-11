using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;

using System.Diagnostics.CodeAnalysis;

namespace Avalonia.ConnectedAnimation;

public class ConnectedAnimation
{
    private readonly Control _source;
    private readonly EventHandler _reportCompleted;
    private readonly Rect _sourceBounds;

    internal ConnectedAnimation(string key, Control source, EventHandler completed)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _reportCompleted = completed ?? throw new ArgumentNullException(nameof(completed));

        var sourceBounds = _source.Bounds;

        var root = _source.GetVisualRoot()!;
        var topLeft = _source.TranslatePoint(default, root);
        var bottomRight = _source.TranslatePoint(new Point(sourceBounds.Width, sourceBounds.Height), root);

        if (topLeft.HasValue && bottomRight.HasValue)
        {
            _sourceBounds = new Rect(topLeft.Value, bottomRight.Value);
        }
    }

    public string Key { get; }

    public void Cancel()
    {
        _reportCompleted?.Invoke(this, EventArgs.Empty);
    }

    public Task<bool> TryStart(Control destination)
    {
        return TryStart(destination, Enumerable.Empty<Control>());
    }

    public async Task<bool> TryStart(Control destination, IEnumerable<Control> coordinatedElements)
    {
        if (destination == null)
        {
            throw new ArgumentNullException(nameof(destination));
        }
        if (coordinatedElements == null)
        {
            throw new ArgumentNullException(nameof(coordinatedElements));
        }
        if (Equals(_source, destination))
        {
            return false;
        }

        var renderRoot = destination.GetVisualRoot();
        //await destination.WaitVisualTreeAttached();
        if (renderRoot == null)
        {
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.MinValue);
            renderRoot = destination.GetVisualRoot();
        }

        var connectionHost = new ConnectedVisual(_sourceBounds, destination);
        var adorner = ConnectedAnimationAdorner.FindFrom(destination, renderRoot);
        adorner.Children.Add(connectionHost);

        var animation = new Animation.Animation()
        {
            Duration = TimeSpan.FromMilliseconds(500),
            Easing = new CircularEaseInOut(),
            Children =
            {
                new KeyFrame()
                {
                    Cue = new Cue(0),
                    Setters =
                    {
                        new Setter(ConnectedVisual.ProgressProperty, 0.0)
                    }
                },
                new KeyFrame()
                {
                    Cue = new Cue(1),
                    Setters =
                    {
                        new Setter(ConnectedVisual.ProgressProperty, 1.0)
                    }
                }
            }
        };

        var storedOpacity = destination.Opacity;
        destination.Opacity = 0;
        await animation.RunAsync(connectionHost, null);
        _reportCompleted?.Invoke(this, EventArgs.Empty);

        adorner.Children.Remove(connectionHost);
        destination.Opacity = storedOpacity;

        return true;
    }

    private class ConnectedVisual : Control
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

    private class ConnectedAnimationAdorner : Canvas
    {
        private ConnectedAnimationAdorner(Visual adornedElement)
        {
            AdornerLayer.SetAdornedElement(this, adornedElement);
            IsHitTestVisible = false;
        }

        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    foreach (var child in Children)
        //    {
        //        child.Arrange(new Rect(child.DesiredSize));
        //    }
        //    return finalSize;
        //}

        internal static ConnectedAnimationAdorner FindFrom(Visual visual, IRenderRoot? renderRoot = null)
        {
            if (renderRoot is not Window window)
            {
                renderRoot = visual.GetVisualRoot();
            }

            if (renderRoot is Window { Content: Visual root })
            {
                var layer = AdornerLayer.GetAdornerLayer(root);
                if (layer != null)
                {
                    var adorner = layer.Children?.OfType<ConnectedAnimationAdorner>().FirstOrDefault();
                    if (adorner == null)
                    {
                        adorner = new ConnectedAnimationAdorner(root);
                        layer.Children!.Add(adorner);
                    }
                    return adorner;
                }
            }
            throw new InvalidOperationException("The specified Visual is not yet connected to the visible visual tree and no container to host the animation can be found.");
        }

        internal static void ClearFor([NotNull] Visual visual)
        {
            if (visual.GetVisualRoot() is Window window
                && window.Content is Visual root)
            {
                var layer = AdornerLayer.GetAdornerLayer(root);
                var adorner = layer?.Children?.OfType<ConnectedAnimationAdorner>().FirstOrDefault();
                if (layer != null && adorner != null)
                {
                    layer.Children.Remove(adorner);
                }
            }
        }
    }
}
