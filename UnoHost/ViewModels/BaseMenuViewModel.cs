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
using DMXCore.DMXCore100.Models;
using DMXCore.DMXCore100.Services;
using DMXCore.DMXCore100.Views;
using Microsoft.Extensions.Logging;

namespace DMXCore.DMXCore100.ViewModels;

[Bindable]
public abstract class BaseMenuViewModel : BaseViewModel, IItemSelector, INotifyOfNavigatingTo
{
    private readonly Menu menu;
    private readonly IThemeService themeService;
    private bool populated;
    private UserControl? expanderUserControl;
    private readonly Func<UserControl>? getExpanderUserControl;

    public BaseMenuViewModel(
        ILogger logger,
        IScheduler scheduler,
        IMyNavigator navigator,
        Menu menu,
        IMenuManager menuManager,
        IThemeService themeService,
        IMenuFocusManager menuFocusManager)
        : base(logger, scheduler, menuFocusManager, menuManager, navigator)
    {
        this.menu = menu;
        this.themeService = themeService;

        BackCommand = new AsyncRelayCommand(OnBack);

        var joinableTaskFactory = new Microsoft.VisualStudio.Threading.JoinableTaskFactory(new Microsoft.VisualStudio.Threading.JoinableTaskContext());

        // Populate menu items
        UpdateTitle();
        SubTitle = menu.SubTitle;
        Logo = menu.Logo;
        LogoHeight = menu.LogoHeight;
        this.getExpanderUserControl = menu.GetExpanderUserControl;
        IsExpanded = false;

        string? backgroundName = menu.BackgroundImage;

        if (string.IsNullOrEmpty(backgroundName))
            backgroundName = "menu-background";

        // Just for test
        string imageFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "Images");
        BackgroundImagePath = IconHelper.GetIconName(this.log, backgroundName, imageFolder, 800, 389);

        this.log.LogDebug("Populating menu with id {InvocationId}", InvokationId);

        joinableTaskFactory.Run(PopulateMenu);

        this.disposables.Add(menu);
        this.log.LogDebug("MenuViewModel created with id {InvocationId}", InvokationId);
    }

    public ObservableCollection<MenuItem> MenuItems { get; } = new ObservableCollection<MenuItem>();

    public string Title { get; set; }

    public string SubTitle { get; set; }

    public string Logo { get; set; }

    public int LogoHeight { get; set; }

    public string? BackgroundImagePath { get; set; }

    public bool HasTopExpander => this.getExpanderUserControl != null;

    public Visibility MainVisibility => Visibility.Visible;

    public Visibility HasTopExpanderVisibility => HasTopExpander ? Visibility.Visible : Visibility.Collapsed;

    public Visibility HasTopExpanderInverseVisibility => HasTopExpander ? Visibility.Collapsed : Visibility.Visible;

    public Visibility SubTitleVisibility => !string.IsNullOrEmpty(SubTitle) ? Visibility.Visible : Visibility.Collapsed;

    public Visibility LogoVisibility => !string.IsNullOrEmpty(Logo) ? Visibility.Visible : Visibility.Collapsed;

    public bool IsExpanded { get; set; } = false;

    public UserControl? ExpanderUserControl
    {
        get
        {
            if (this.expanderUserControl == null && this.getExpanderUserControl != null)
            {
                this.expanderUserControl = this.getExpanderUserControl();
            }

            return this.expanderUserControl;
        }
    }

    public async Task OnBack()
    {
        if (this.menu.NavigateBack)
        {
            var response = await this.navigator.NavigateBackAsync(this);

            if (response != null)
            {
                return;
            }
            else
            {
                this.log.LogWarning("Navigating back didn't take us anywhere {MenuTitle}", this.menu.Title);
            }
        }

        if (this.menu.Parent != null)
        {
            // A parent menu
            await this.navigator.NavigateViewModelAsync<MenuViewModel>(this, qualifier: this.menu.DoNotClearBackStack ? Qualifiers.None : Qualifiers.ClearBackStack, data: this.menu.Parent);
        }
        else
        {
            await this.menuManager.NavigateToStart(this, this.navigator);
        }
    }

    public async Task SelectorPressedShort(DependencyObject focusItem)
    {
        if (focusItem is FrameworkElement frameworkElement && frameworkElement.DataContext is MenuItem menuItem)
        {
            if (menuItem.GetChildren != null)
            {
                var subMenuItems = await menuItem.GetChildren();

                if (subMenuItems.UseSmallView)
                {
                    await this.navigator.NavigateViewAsync<MenuPageSmall>(this, qualifier: Qualifiers.ClearBackStack, data: subMenuItems);
                }
                else
                {
                    await this.navigator.NavigateViewAsync<MenuPage>(this, qualifier: subMenuItems.DoNotClearBackStack ? Qualifiers.None : Qualifiers.ClearBackStack, data: subMenuItems);
                }
            }
            else
            {
                menuItem.Tapped(this.menuFocusManager, this.navigator, this.scheduler, this.log, this.themeService, doneAction: () =>
                {
                    // Update titles
                    UpdateTitle();
                    SubTitle = this.menu.SubTitle;

                    OnPropertyChanged(nameof(Title));
                    OnPropertyChanged(nameof(SubTitle));
                    OnPropertyChanged(nameof(SubTitleVisibility));

                    menuItem.Refresh();
                    // Force it to be re-populated
                    this.populated = false;
                });
            }
        }
    }

    private void UpdateTitle()
    {
        Title = this.menu.Title;
    }

    public Task SelectorPressedLong(DependencyObject focusItem)
    {
        if (focusItem is FrameworkElement frameworkElement && frameworkElement.DataContext is MenuItem menuItem)
        {
            menuItem.Held(this.menuFocusManager, this.navigator, this.scheduler, this.log);

            // Force it to be reloaded
            this.populated = false;
        }
        else
        {
            if (focusItem is Button button && button.Command == BackCommand)
            {
                this.menuFocusManager.ClickButton(button);
            }
        }

        return Task.CompletedTask;
    }

    public void NavigatingTo(bool wasInDialog)
    {
        if (!wasInDialog)
        {
            IsExpanded = false;
            OnPropertyChanged(nameof(IsExpanded));
        }

        if (!this.populated)
        {
            // Hack
            this.scheduler.ScheduleAsync(async (s, ct) =>
            {
                await PopulateMenu();
            });
        }
    }

    private async Task PopulateMenu()
    {
        this.populated = true;

        try
        {
            var allItems = await this.menu.GetMenuItems(this.themeService);

            var filteredItems = new List<MenuItem>();
            foreach (var menuItem in allItems)
            {
                bool showMenuItem = await menuItem.GetIsVisible();

                if (showMenuItem)
                    filteredItems.Add(menuItem);
            }

            for (int i = 0; i < MenuItems.Count; i++)
            {
                if (i > filteredItems.Count - 1)
                {
                    MenuItems.RemoveAt(i);
                }
                else
                {
                    MenuItems[i].Copy(filteredItems[i]);
                }
            }

            for (int i = MenuItems.Count; i < filteredItems.Count; i++)
            {
                MenuItems.Add(filteredItems[i]);
            }

            // Refresh all
            foreach (var item in MenuItems)
            {
                item.Refresh();
            }
        }
        catch (Exception ex)
        {
            this.log.LogWarning(ex, "Failed to get menu items: {Message}", ex.Message);

            await this.navigator.ShowMessageDialogAsync(this, title: "Error", content: $"Failed to get menu items: {ex.Message}");
        }
    }
}
