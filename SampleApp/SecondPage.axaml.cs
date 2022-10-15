using Avalonia.ConnectedAnimation;
using Avalonia.Controls;
using Avalonia.Threading;

using FluentAvalonia.UI.Navigation;

using System.Threading.Tasks;

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
        await Dispatcher.UIThread.InvokeAsync(() => { });

        var anm = ConnectedAnimationService.GetForCurrentView(this);
        ConnectedAnimation? avatarAnm = anm.GetAnimation("avatarImage");
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
