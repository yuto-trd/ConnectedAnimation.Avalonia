using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.ConnectedAnimation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;

using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Navigation;

namespace SampleApp;

public partial class FirstPage : UserControl, IPage
{
    public record ItemViewModel(string Avatar, string Name);
    private static readonly ItemViewModel[] s_items =
    {
        new ItemViewModel("https://avatars.githubusercontent.com/u/66758394?v=4", "indigo-san"),
        new ItemViewModel("https://avatars.githubusercontent.com/u/79445014?v=4", "b-editor"),
        new ItemViewModel("https://avatars.githubusercontent.com/u/9141961?v=4", "dotnet"),
        new ItemViewModel("https://avatars.githubusercontent.com/u/14075148?v=4", "AvaloniaUI"),
    };

    public FirstPage()
    {
        InitializeComponent();
        list.Items = s_items;

        list.GetObservable(SelectingItemsControl.SelectedItemProperty).Subscribe(OnSelectedItemChanged);
    }

    private void OnSelectedItemChanged(object? obj)
    {
        if (this.FindLogicalAncestorOfType<Frame>() is { } frame
            && obj is ItemViewModel viewModel)
        {
            frame.Navigate(typeof(SecondPage), viewModel, MainWindow.DefaultTransition);
        }
    }

    public async void OnNavigateTo(NavigationEventArgs args)
    {
        if (args.Parameter is ItemViewModel itemViewModel)
        {
            list.SelectedItem = itemViewModel;
        }

        // Without this dispatch, old Image, TextBlock will be returned.
        await Dispatcher.UIThread.InvokeAsync(() => { });
        var index = list.SelectedIndex;
        var container = list.ItemContainerGenerator.ContainerFromIndex(index);

        var anm = ConnectedAnimationService.GetForCurrentView(this);
        ConnectedAnimation? avatarAnm = anm.GetAnimation("avatarImage");
        ConnectedAnimation? userNameAnm = anm.GetAnimation("userName");
        if (avatarAnm != null /*&& userNameAnm != null*/
            && container is ListBoxItem item
            && item.Presenter is { Child: StackPanel child })
        {
            avatarAnm.Configuration = new DirectConnectedAnimationConfiguration();

            var avatar = child.GetLogicalChildren().OfType<Image>().First(x => x.Name == "avatarImage");
            var name = child.GetLogicalChildren().OfType<TextBlock>().First(x => x.Name == "userName");

            await avatarAnm.TryStart(avatar, new Control[] { name });
            //await Task.WhenAll(avatarAnm.TryStart(avatar), userNameAnm.TryStart(name));
        }
        else
        {
            avatarAnm?.Cancel();
            userNameAnm?.Cancel();
        }
    }

    public void OnNavigateFrom(NavigatingCancelEventArgs args)
    {
        if (args.Parameter is ItemViewModel viewModel)
        {
            var anm = ConnectedAnimationService.GetForCurrentView(this);
            var index = Array.IndexOf(s_items, viewModel);
            var container = list.ItemContainerGenerator.ContainerFromIndex(index);
            if (container is ListBoxItem item
                && item.Presenter is { Child: StackPanel child })
            {
                var avatar = child.GetLogicalChildren().OfType<Image>().First(x => x.Name == "avatarImage");
                //var name = child.GetLogicalChildren().OfType<TextBlock>().First(x => x.Name == "userName");
                anm.PrepareToAnimate("avatarImage", avatar);
                //anm.PrepareToAnimate("userName", name);
            }
        }
    }
}
