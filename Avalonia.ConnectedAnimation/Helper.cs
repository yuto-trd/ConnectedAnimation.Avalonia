using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Avalonia.ConnectedAnimation;

public static class Helper
{
    public static Task WaitVisualTreeAttached(this Control control)
    {
        if (control.GetVisualRoot() != null)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();

        void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (control.GetVisualRoot() != null)
            {
                control.AttachedToVisualTree -= OnAttachedToVisualTree;
                tcs.SetResult();
            }
        }

        control.AttachedToVisualTree += OnAttachedToVisualTree;

        return tcs.Task;
    }
}
