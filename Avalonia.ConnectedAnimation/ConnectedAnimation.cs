using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
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
            // Wait to be attached to VisualTree
            await Dispatcher.UIThread.InvokeAsync(() => { });
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
}
