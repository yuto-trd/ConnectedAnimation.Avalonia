using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Avalonia.ConnectedAnimation;

public class ConnectedAnimationService : AvaloniaObject
{
    private static readonly AttachedProperty<ConnectedAnimationService> AnimationServiceProperty =
        AvaloniaProperty.RegisterAttached<ConnectedAnimationService, AvaloniaObject, ConnectedAnimationService>("AnimationService");

    private readonly Dictionary<string, ConnectedAnimation> _connectingAnimations = new();

    private ConnectedAnimationService()
    {
    }

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

        info = new ConnectedAnimation(key, source, OnAnimationCompleted);
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
