using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Rendering;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Avalonia.ConnectedAnimation;

internal sealed class ConnectedAnimationAdorner : Canvas
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

    public static ConnectedAnimationAdorner FindFrom(Visual visual, IRenderRoot? renderRoot = null)
    {
        if (renderRoot is not Window)
        {
            renderRoot = visual.GetVisualRoot();
        }

        if (renderRoot is Window { Content: Visual root })
        {
            var layer = AdornerLayer.GetAdornerLayer(root);
            if (layer != null)
            {
                var adorner = layer.Children.OfType<ConnectedAnimationAdorner>().FirstOrDefault();
                if (adorner == null)
                {
                    adorner = new ConnectedAnimationAdorner(root);
                    layer.Children.Add(adorner);
                }
                return adorner;
            }
        }
        throw new InvalidOperationException("The specified Visual is not yet connected to the visible visual tree and no container to host the animation can be found.");
    }

    public static void ClearFor(Visual visual)
    {
        if (visual.GetVisualRoot() is Window window
            && window.Content is Visual root)
        {
            var layer = AdornerLayer.GetAdornerLayer(root);
            if (layer != null)
            {
                var adorner = layer.Children.OfType<ConnectedAnimationAdorner>().FirstOrDefault();
                if (adorner != null)
                {
                    layer.Children.Remove(adorner);
                }
            }
        }
    }
}