using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using DMXCore.DMXCore100.Models;
using DMXCore.DMXCore100.Services;

namespace DMXCore.DMXCore100.ViewModels;

[Bindable]
public class MenuViewModel : BaseMenuViewModel
{
    public MenuViewModel(
        ILogger<MenuViewModel> logger,
        IScheduler scheduler,
        IMyNavigator navigator,
        Menu menu,
        IMenuManager menuManager,
        IThemeService themeService,
        IMenuFocusManager menuFocusManager)
        : base(
            logger,
            scheduler,
            navigator,
            menu,
            menuManager,
            themeService,
            menuFocusManager)
    {
    }
}
