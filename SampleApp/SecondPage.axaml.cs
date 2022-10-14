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
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Layout);

        var anm = ConnectedAnimationService.GetForCurrentView(this);
        ConnectedAnimation? avatarAnm = anm.GetAnimation("avatarImage");
        ConnectedAnimation? userNameAnm = anm.GetAnimation("userName");
        if (avatarAnm != null /*&& userNameAnm != null*/)
        {
            await avatarAnm.TryStart(avatarImage, new Control[] { userName });
            //await Task.WhenAll(avatarAnm.TryStart(avatarImage), userNameAnm.TryStart(userName));
        }
        else
        {
            avatarAnm?.Cancel();
            //userNameAnm?.Cancel();
        }
    }

    public void OnNavigateFrom(NavigatingCancelEventArgs args)
    {
        var anm = ConnectedAnimationService.GetForCurrentView(this);
        anm.PrepareToAnimate("avatarImage", avatarImage);
        //anm.PrepareToAnimate("userName", userName);
    }
}
