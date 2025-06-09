using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace DMXCore.DMXCore100.Models;

public class Menu : IDisposable
{
    private List<MenuItem> cacheMenuItems;

    public string? Title { get; set; }

    public string? SubTitle { get; set; }

    public string? Logo { get; set; }

    public int LogoHeight { get; set; }

    public string? BackgroundImage { get; set; }

    public bool UseSmallView { get; set; }

    public Func<UserControl>? GetExpanderUserControl { get; set; }

    public bool NavigateBack { get; set; }

    public bool DoNotClearBackStack { get; set; }

    public bool IsCustomMenu { get; set; }

    public Func<Task<List<MenuItem>>>? GetMenuItemsFunc { private get; set; }

    public async Task<List<MenuItem>> GetMenuItems(IThemeService themeService)
    {
        var list = await GetMenuItemsFunc();

        foreach (var item in list)
        {
            item.ThemeService = themeService;
        }

        this.cacheMenuItems = list;

        return list;
    }

    public void Refresh()
    {
        if (this.cacheMenuItems == null)
            return;

        foreach (var item in this.cacheMenuItems)
        {
            item.Refresh();
        }
    }

    public List<IDisposable> Disposables { get; } = new();

    public void Dispose()
    {
        var expanderUserControl = GetExpanderUserControl?.Invoke();
        (expanderUserControl?.DataContext as IDisposable)?.Dispose();
        (expanderUserControl as IDisposable)?.Dispose();

        foreach (var item in Disposables)
        {
            item.Dispose();
        }
        Disposables.Clear();
    }

    public Menu? Parent { get; set; }
}
