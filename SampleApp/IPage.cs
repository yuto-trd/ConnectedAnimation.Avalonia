using FluentAvalonia.UI.Navigation;

namespace SampleApp;

public interface IPage
{
    void OnNavigateFrom(NavigatingCancelEventArgs args);

    void OnNavigateTo(NavigationEventArgs args);
}
