using System.Reactive.Linq;
using System.Reactive.Subjects;
using DMXCore.DMXCore100.Services;
using DMXCore.DMXCore100.Views;
using Uno.Extensions.Navigation;

namespace DMXCore.DMXCore100.ViewModels;

[Bindable]
public class ShellViewModel
{
    private ILogger log;
    private readonly IMyNavigator navigator;
    private readonly IMenuManager menuManager;

    public ShellViewModel(
        ILogger<ShellViewModel> logger,
        IMyNavigator navigator,
        ILifetimeControl lifetimeControl,
        IMenuManager menuManager)
    {
        this.log = logger;
        this.navigator = navigator;
        this.menuManager = menuManager;

        lifetimeControl.Running.Where(x => x).Select(async _ => await Start()).Subscribe();
    }

    public Action WhenStarted { get; set; }

    public async Task Start()
    {
        await this.menuManager.NavigateToStart(this, this.navigator);

        WhenStarted?.Invoke();
    }
}
