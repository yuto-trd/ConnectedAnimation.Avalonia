using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace ConnectedAnimation.Avalonia;

public class ConnectedAnimationService : AvaloniaObject
{
    private static readonly AttachedProperty<ConnectedAnimationService> AnimationServiceProperty =
        AvaloniaProperty.RegisterAttached<ConnectedAnimationService, AvaloniaObject, ConnectedAnimationService>("AnimationService");

    private readonly Dictionary<string, ConnectedAnimation> _connectingAnimations = new();

    private ConnectedAnimationService()
    {
    }

    // 450
    // 750
    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMilliseconds(300);

    // https://easings.net/
    //var easing = new SplineEasing(0, .79, 1, -0.18);
    //var easing = new SplineEasing(0, 0, 1, 0);
    //var easing = new SplineEasing(0, 0.83, 1, 0.17);
    //var easing = new QuinticEaseInOut();
    //var easing = new ExponentialEaseInOut();
    //var easing = new CircularEaseInOut();
    public Easing DefaultEasingFunction { get; set; } = new SplineEasing(0.1, 0.9, 0.2, 1);

    public ICurve DefaultCurve { get; set; } = new LinearCurve();

    public void PrepareToAnimate(string key, Control source)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (_connectingAnimations.TryGetValue(key, out var info))
        {
            throw new ArgumentException("The specified key is already prepared for animation and should not be prepared repeatedly.", nameof(key));
        }

        info = new ConnectedAnimation(key, source, OnAnimationCompleted, this);
        _connectingAnimations.Add(key, info);
    }

    private void OnAnimationCompleted(object? sender, EventArgs e)
    {
        if (sender is ConnectedAnimation connectedAnimation)
        {
            var key = connectedAnimation.Key;
            if (_connectingAnimations.ContainsKey(key))
            {
                _connectingAnimations.Remove(key);
            }
        }
    }

    public ConnectedAnimation? GetAnimation(string key)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        if (_connectingAnimations.TryGetValue(key, out var info))
        {
            return info;
        }
        return null;
    }

    public static ConnectedAnimationService GetForCurrentView(Visual visual)
    {
        if (visual.GetVisualRoot() is not Window window)
        {
            throw new ArgumentException("This Visual is not connected to the visible visual tree.", nameof(visual));
        }

        var service = window.GetValue(AnimationServiceProperty);
        if (service == null)
        {
            service = new ConnectedAnimationService();
            window.SetValue(AnimationServiceProperty, service);
        }
        return service;
    }
}
