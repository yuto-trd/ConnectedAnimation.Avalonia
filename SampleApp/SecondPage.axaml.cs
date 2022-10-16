using Avalonia.Controls;
using Avalonia.Threading;

using ConnectedAnimation.Avalonia;

using FluentAvalonia.UI.Navigation;

namespace SampleApp;

public partial class SecondPage : UserControl, IPage
{
    public SecondPage()
    {
        InitializeComponent();
    }

    public async void OnNavigateTo(NavigationEventArgs args)
    {
        DataContext = args.Parameter;
        // Without this dispatch, the size of the TextBlock will be Stretched.
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.MinValue);

        var anm = ConnectedAnimationService.GetForCurrentView(this);
        var avatarAnm = anm.GetAnimation("avatarImage");
        if (avatarAnm != null)
        {
            await avatarAnm.TryStart(avatarImage, new Control[] { userName, coordinated });
        }
        else
        {
            avatarAnm?.Cancel();
        }
    }

    public void OnNavigateFrom(NavigatingCancelEventArgs args)
    {
        var anm = ConnectedAnimationService.GetForCurrentView(this);
        anm.PrepareToAnimate("avatarImage", avatarImage);
    }
}
