using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace ConnectedAnimation.Avalonia;

public class ConnectedAnimation
{
    private readonly Control _source;
    private readonly EventHandler _reportCompleted;
    private readonly ConnectedAnimationService _service;
    private readonly Rect _sourceBounds;
    private readonly RenderTargetBitmap _sourceImage;
    private bool _isDisposed;

    internal ConnectedAnimation(string key, Control source, EventHandler completed, ConnectedAnimationService service)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _reportCompleted = completed ?? throw new ArgumentNullException(nameof(completed));
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _sourceBounds = source.AbsoluteBounds().Inflate(source.Margin);
        // Pre render
        source.Measure(Size.Infinity);
        source.Arrange(new Rect(source.DesiredSize));
        _sourceImage = new RenderTargetBitmap(PixelSize.FromSize(source.DesiredSize, 1));
        _sourceImage.Render(source);
    }

    public string Key { get; }

    public ConnectedAnimationConfiguration Configuration { get; set; } = new GravityConnectedAnimationConfiguration();

    public void Cancel()
    {
        _sourceImage.Dispose();
        _isDisposed = true;

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
        if (Equals(_source, destination) || _isDisposed)
        {
            return false;
        }

        var renderRoot = destination.GetVisualRoot();
        //await destination.WaitVisualTreeAttached();
        if (renderRoot == null)
        {
            // Wait to be attached to VisualTree
            await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.MinValue);
            renderRoot = destination.GetVisualRoot();
        }

        var connectionHost = new ConnectedVisual(_sourceImage, _sourceBounds, destination, _service.DefaultCurve, Configuration);
        var coordinatedHosts = coordinatedElements
            .Select(x => new CoordinatedVisual(connectionHost, x, _service.DefaultCurve, Configuration))
            .ToArray();

        var adorner = ConnectedAnimationAdorner.FindFrom(destination, renderRoot);
        adorner.Children.Add(connectionHost);
        adorner.Children.AddRange(coordinatedHosts);

        var duration = Configuration.GetDuration(_sourceBounds, connectionHost.DestinationBounds, _service.DefaultDuration);
        var easing = Configuration.GetEasing(_sourceBounds, connectionHost.DestinationBounds, _service.DefaultEasingFunction);

        var tasks = Task.WhenAll(coordinatedHosts.Select(x => x.RunAnimation(duration, easing, TimeSpan.Zero)));
        await connectionHost.RunAnimation(duration, easing);
        await tasks;

        _reportCompleted?.Invoke(this, EventArgs.Empty);

        adorner.Children.Remove(connectionHost);
        adorner.Children.RemoveAll(coordinatedHosts);

        return true;
    }
}
