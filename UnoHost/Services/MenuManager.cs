using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using DMXCore.DMXCore100.Models;
using DMXCore.DMXCore100.ViewModels;

namespace DMXCore.DMXCore100.Services;

public class MenuManager : IMenuManager
{
    private readonly ILogger log;

    public MenuManager(
        ILogger<MenuManager> logger,
        IMenuFocusManager menuFocusManager,
        IScheduler scheduler)
    {
        this.log = logger;
    }

    public Task<Menu> GetRootMenuItems(INavigator navigator)
    {
        var list = new List<MenuItem>();
        MenuItem model;

        var menuItems = new Menu
        {
            Title = "Main Menu",
            GetMenuItemsFunc = () => Task.FromResult(list)
        };

        model = new MenuItem
        {
            Name = "Presets",
            Description = "Display all presets",
            IsChildrenAvailable = () => Task.FromResult(true),
            GetChildren = () => GetPresets(navigator, menuItems),
            Icon = "presets40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Effects",
            Description = "Apply built-in effects",
            IsChildrenAvailable = () => Task.FromResult(true),
            GetChildren = () => GetEffects(navigator, menuItems),
            Icon = "patterns40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Cues",
            Description = "Display all cues",
            IsChildrenAvailable = () => Task.FromResult(true),
            GetChildren = () => GetCues(navigator, menuItems),
            Icon = "cues40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Sounds",
            Description = "Display all sounds",
            IsChildrenAvailable = () => Task.FromResult(true),
            GetChildren = () => GetSounds(navigator, menuItems),
            Icon = "sounds40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Fixture Control",
            Description = "Control light fixtures",
            IsChildrenAvailable = () => Task.FromResult(true),
            Icon = "fixture40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Settings",
            Description = "Configure system settings",
            GetChildren = () => GetSettings(navigator, menuItems),
            Icon = "settings40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Utilities",
            Description = "Utilities and Tools",
            GetChildren = () => GetUtils(navigator, menuItems),
            Icon = "utilities40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "About",
            Description = "About this system",
            TappedAction = async _ =>
            {
                await navigator.NavigateViewModelAsync<AboutViewModel>(this);
            },
            Icon = "about40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Help",
            Description = "Support and documentation",
            Icon = "help40x40.png"
        };
        list.Add(model);

        return Task.FromResult(menuItems);
    }

    public async Task<NavigationResponse?> NavigateToStart(object sender, INavigator navigator)
    {
        return await navigator.NavigateViewModelAsync<HomeViewModel>(sender, qualifier: Qualifiers.ClearBackStack);
    }

    public class BoolPlaceHolder
    {
        public bool Value { get; set; }
    }
    private Task<Menu> GetEffects(INavigator navigator, Menu parent)
    {
        var list = new List<MenuItem>();
        var menuItems = new Menu
        {
            Title = "Effects",
            GetMenuItemsFunc = () => Task.FromResult(list),
            Parent = parent
        };

        var model = new MenuItem
        {
            Name = "None",
            Description = "Turn off effects"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Sinus",
            Description = "Sinus Dimmer Effect"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Ramp Up/Down",
            Description = "Ramp Up/Down Dimmer Effect"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Rainbow",
            Description = "Rainbow Pattern"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Individual Sinus Effect",
            Description = "Sinus Effect"
        };
        list.Add(model);

        return Task.FromResult(menuItems);
    }

    private Task<Menu> GetCues(INavigator navigator, Menu parent)
    {
        var menuItems = new Menu
        {
            Title = "Cues",
            Parent = parent
        };

        menuItems.GetMenuItemsFunc = async () =>
        {
            this.log.LogTrace("GetMenuItems Culture {Culture} for thread {ThreadId}", Thread.CurrentThread.CurrentUICulture, Environment.CurrentManagedThreadId);

            // Simulate loading from DB
            await Task.Delay(1000);

            var list = new List<MenuItem>();

            for (int i = 0; i < 10; i++)
            {

                list.Add(new MenuItem
                {
                    Name = $"Cue {i + 1}",
                    Description = $"This is cue {i + 1}"
                });
            }

            return list;
        };

        return Task.FromResult(menuItems);
    }

    private Task<Menu> GetSounds(INavigator navigator, Menu parent)
    {
        var menuItems = new Menu
        {
            Title = "Sounds",
            Parent = parent
        };

        menuItems.GetMenuItemsFunc = async () =>
        {
            var list = new List<MenuItem>();

            // Simulate loading from DB
            await Task.Delay(1000);

            for (int i = 0; i < 10; i++)
            {

                list.Add(new MenuItem
                {
                    Name = $"Sound {i + 1}",
                    Description = $"This is sound {i + 1}"
                });
            }

            return list;
        };

        return Task.FromResult(menuItems);
    }

    private Task<Menu> GetPresets(INavigator navigator, Menu parent)
    {
        var menuItems = new Menu
        {
            Title = "Presets",
            Parent = parent
        };

        menuItems.GetMenuItemsFunc = async () =>
        {
            var list = new List<MenuItem>();

            // Simulate loading from DB
            await Task.Delay(1000);

            for (int i = 0; i < 10; i++)
            {
                list.Add(new MenuItem
                {
                    Name = $"Preset {i + 1}",
                    Description = $"This is preset {i + 1}"
                });
            }

            return list;
        };

        return Task.FromResult(menuItems);
    }

    private Task<Menu> GetUtils(INavigator navigator, Menu parent)
    {
        var list = new List<MenuItem>();
        var menuItems = new Menu
        {
            Title = "Utilities",
            GetMenuItemsFunc = () => Task.FromResult(list),
            Parent = parent
        };
        MenuItem model;

        model = new MenuItem
        {
            Name = "Record Cue",
            Description = "Snapshot and Capture Cues",
            Icon = "record40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Device Operations",
            Description = "Reboot and restart services",
            IsChildrenAvailable = () => Task.FromResult(true),
            Icon = "deviceop40x40.png"
        };
        list.Add(model);

        return Task.FromResult(menuItems);
    }

    private Task<Menu> GetSettings(INavigator navigator, Menu parent)
    {
        var list = new List<MenuItem>();
        var menuItems = new Menu
        {
            Title = "Settings",
            GetMenuItemsFunc = () => Task.FromResult(list),
            Parent = parent
        };
        MenuItem model;

        model = new MenuItem
        {
            Name = "System Settings",
            Description = "Configure system settings",
            Icon = "settings40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Zones",
            Description = "Configure zones",
            Icon = "zone40x40.png"
        };
        list.Add(model);

        model = new MenuItem
        {
            Name = "Fixtures",
            Description = "Configure fixtures",
            Icon = "fixture40x40.png"
        };
        list.Add(model);

        return Task.FromResult(menuItems);
    }
}
