using Avalonia;
using Avalonia.ConnectedAnimation;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;

using FluentAvalonia.UI.Navigation;

using System.Diagnostics;
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
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.MinValue);

        var anm = ConnectedAnimationService.GetForCurrentView(this);
        ConnectedAnimation? avatarAnm = anm.GetAnimation("avatarImage");
        ConnectedAnimation? userNameAnm = anm.GetAnimation("userName");
        if (avatarAnm != null && userNameAnm != null)
        {
            await Task.WhenAll(avatarAnm.TryStart(avatarImage), userNameAnm.TryStart(userName));
        }
        else
        {
            avatarAnm?.Cancel();
            userNameAnm?.Cancel();
        }
    }

    public void OnNavigateFrom(NavigatingCancelEventArgs args)
    {
        var anm = ConnectedAnimationService.GetForCurrentView(this);
        anm.PrepareToAnimate("avatarImage", avatarImage);
        anm.PrepareToAnimate("userName", userName);
    }
}
