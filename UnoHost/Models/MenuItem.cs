using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using DMXCore.DMXCore100.Extensions;
using DMXCore.DMXCore100.Services;
//using MathNet.Numerics.Financial;
using Microsoft.Extensions.Logging;

namespace DMXCore.DMXCore100.Models;

[Microsoft.UI.Xaml.Data.Bindable]
public class MenuItem : ObservableModelBase
{
    public string? name;
    public string? Name
    {
        get
        {
            string value = GetName != null ? GetName() : this.name;

            // Null/empty string messed up the card layout
            return string.IsNullOrEmpty(value) ? " " : value;
        }
        set => this.name = value;
    }

    private string? description;
    public string? Description
    {
        get
        {
            string value = GetDescription != null ? GetDescription() : this.description;

            // Null/empty string messed up the card layout
            return string.IsNullOrEmpty(value) ? " " : value;
        }
        set => this.description = value;
    }

    public Func<string>? GetDescription { get; set; }

    public Func<string>? GetName { get; set; }

    public Func<MenuItem, Task>? TappedAction { private get; set; }

    public Func<MenuItem, Task<bool>>? GetIsAvailable { get; set; }

    public Func<Task<bool>>? IsChildrenAvailable { get; set; }

    public async Task<bool> GetIsVisible()
    {
        if (GetIsAvailable != null)
        {
            bool isAvailable = await GetIsAvailable(this);
            if (!isAvailable)
                return false;
        }

        if (IsChildrenAvailable != null)
        {
            // Check this instead of loading the children
            return await IsChildrenAvailable();
        }

        if (GetChildren != null)
        {
            // See if we have any child items
            var childMenu = await GetChildren();
            if (childMenu != null)
            {
                var childMenuItems = await childMenu.GetMenuItems(ThemeService);

                bool anyVisible = false;
                foreach (var childItem in childMenuItems)
                {
                    if (await childItem.GetIsVisible())
                    {
                        anyVisible = true;
                        break;
                    }
                }

                if (!anyVisible)
                    return false;
            }
        }

        return true;
    }

    public bool ConfirmAction { get; set; }

    public Func<Task>? HeldAction { private get; set; }

    public Func<Task<Menu>>? GetChildren { get; set; }

    public string? Icon { get; set; }

    public string FullIconPath
    {
        get
        {
            return IconHelper.GetThemeBasedIcon(Icon ?? "default40x40.png", ThemeService?.IsDark == true);
        }
    }

    public Brush? Background { get; set; }

    // Primarily used for the external control
    public double Width { get; set; }

    public IThemeService? ThemeService { get; internal set; }

    public void Tapped(IMenuFocusManager menuFocusManager, INavigator navigator, IScheduler scheduler, ILogger log, IThemeService themeService, Action? doneAction = null)
    {
        if (TappedAction == null)
            return;

        ThemeService = themeService;

        scheduler.ScheduleAsync(async (ctrl, ct) =>
        {
            bool result = true;

            if (ConfirmAction)
            {
                result = await menuFocusManager.DisplayConfirmYesNo(this, navigator, title: "Confirm", content: "Are you sure?");
            }

            if (result)
            {
                try
                {
                    await TappedAction(this);

                    doneAction?.Invoke();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failure during TappedAction: {Message}", ex.Message);
                }
            }
        });
    }

    public void Held(IMenuFocusManager menuFocusManager, INavigator navigator, IScheduler scheduler, ILogger log)
    {
        if (HeldAction == null)
            return;

        scheduler.ScheduleAsync(async (ctrl, ct) =>
        {
            bool result = true;

            if (ConfirmAction)
            {
                result = await menuFocusManager.DisplayConfirmYesNo(this, navigator, title: "Confirm", content: "Are you sure?");
            }

            if (result)
            {
                try
                {
                    await HeldAction();

                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Failure during TappedAction: {Message}", ex.Message);
                }
            }
        });
    }

    public void Copy(MenuItem input)
    {
        Name = input.Name;
        Description = input.Description;
        GetDescription = input.GetDescription;
        GetName = input.GetName;
        TappedAction = input.TappedAction;
        HeldAction = input.HeldAction;
        GetChildren = input.GetChildren;
        Icon = input.Icon;
        Background = input.Background;
        Width = input.Width;
        GetIsAvailable = input.GetIsAvailable;
    }

    public void Refresh()
    {
        RaisePropertyChanged(nameof(Name));
        RaisePropertyChanged(nameof(Description));
        RaisePropertyChanged(nameof(Background));
    }
}
