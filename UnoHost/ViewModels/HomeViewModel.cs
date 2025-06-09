using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMXCore.DMXCore100.Extensions;
using DMXCore.DMXCore100.Services;

namespace DMXCore.DMXCore100.ViewModels;

[Bindable]
public class HomeViewModel : BaseViewModel, IItemSelector
{
    public HomeViewModel(
        ILogger<HomeViewModel> logger,
        IScheduler scheduler,
        IMyNavigator navigator,
        IMenuManager menuManager,
        IMenuFocusManager menuFocusManager)
        : base(logger, scheduler, menuFocusManager, menuManager, navigator)
    {
        BackCommand = new AsyncRelayCommand(OnHamburger);

        // Just for test
        string imageFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "Images");
        LogoPath800x300 = IconHelper.GetIconName(this.log, "home-logo", imageFolder, 800, 300, internalFilename: "dmxcore_logo_large.png");
        LogoPath800x105 = IconHelper.GetIconName(this.log, "home-logo", imageFolder, 800, 105, internalFilename: "dmxcore_logo_large.png");

        this.log.LogDebug("HomeViewModel created");
    }

    private string scheduleInfo;
    public string ScheduleInfo
    {
        get => this.scheduleInfo;
        private set
        {
            SetProperty(ref this.scheduleInfo, value);
            OnPropertyChanged(nameof(ProductNameUnderLogoVisible));
            OnPropertyChanged(nameof(ScheduleInfoVisible));
        }
    }

    public string LogoPath800x300 { get; private set; }

    public string LogoPath800x105 { get; private set; }

    public string ProductName => "Product Name";

    public string ShowName => "Show Name";

    public Visibility ShowNameVisible => !string.IsNullOrEmpty(ShowName) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ProductNameUnderLogoVisible => string.IsNullOrEmpty(this.scheduleInfo) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ScheduleInfoVisible => string.IsNullOrEmpty(this.scheduleInfo) ? Visibility.Collapsed : Visibility.Visible;

    public Visibility LogoVisible => Visibility.Visible;

    public Visibility PlayDetailsVisible => Visibility.Collapsed;

    public void SetPlayDetailsVisible(bool visible)
    {
        OnPropertyChanged(nameof(LogoVisible));
        OnPropertyChanged(nameof(PlayDetailsVisible));
    }

    public async Task OnHamburger()
    {
        var menuItems = await this.menuManager.GetRootMenuItems(this.navigator);
        await this.navigator.NavigateViewModelAsync<MenuViewModel>(this, qualifier: Qualifiers.ClearBackStack, data: menuItems);
    }

    public Task SelectorPressedShort(DependencyObject focusItem)
    {
        BackCommand.Execute(null);

        return Task.CompletedTask;
    }

    public Task SelectorPressedLong(DependencyObject focusItem)
    {
        return Task.CompletedTask;
    }
}
