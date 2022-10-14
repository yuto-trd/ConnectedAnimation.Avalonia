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

        _sourceBounds = source.AbsoluteBounds();
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
        var direction = Helper.GetDirection(_sourceBounds, connectionHost.DestinationBounds);
        var coordinatedHosts = coordinatedElements
            .Select(x => new CoordinatedVisual(connectionHost, x, direction))
            .ToArray();

        var adorner = ConnectedAnimationAdorner.FindFrom(destination, renderRoot);
        adorner.Children.Add(connectionHost);
        adorner.Children.AddRange(coordinatedHosts);

        var duration = TimeSpan.FromMilliseconds(500);
        var easing = new CircularEaseInOut();

        var tasks = Task.WhenAll(coordinatedHosts.Select(x => x.RunAnimation(duration, easing, TimeSpan.Zero)));
        await connectionHost.RunAnimation(duration, easing);
        await tasks;

        _reportCompleted?.Invoke(this, EventArgs.Empty);

        adorner.Children.Remove(connectionHost);
        adorner.Children.RemoveAll(coordinatedHosts);

        return true;
    }
}
