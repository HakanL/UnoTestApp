using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMXCore.DMXCore100.Services;
using DMXCore.DMXCore100.Extensions;

namespace DMXCore.DMXCore100.ViewModels;

[Bindable]
public class AboutViewModel : BaseViewModel, IItemSelector
{
    public AboutViewModel(
        ILogger<AboutViewModel> logger,
        IScheduler scheduler,
        IMyNavigator navigator,
        IMenuFocusManager menuFocusManager,
        IMenuManager menuManager)
        : base(logger, scheduler, menuFocusManager, menuManager, navigator)
    {
        BackCommand = new AsyncRelayCommand(async () => await this.navigator.NavigateBackOrHomeAsync(this));

        // Just for test
        string imageFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "Images");
        LogoPath0x40 = IconHelper.GetIconName(this.log, "home-logo", imageFolder, 0, 40, internalFilename: "dmxcore_logo_large.png");

        var list = new List<object>();

        list.Add(new AboutHeaderItem("Device details"));
        list.Add(new AboutItem("Device Name", () => "DeviceName"));
        list.Add(new AboutItem("App Version", () => "1.2.3.4"));
        list.Add(new AboutItem("Supervisor Version", () => "1.2.3.4"));
        list.Add(new AboutItem("OS Version", () => "1.2.3.4"));
        list.Add(new AboutItem("VPN Status", () => $"Enabled/Connected"));

        list.Add(new AboutHeaderItem("Network settings"));
        list.Add(new AboutItem("Machine Name", Environment.MachineName));
        list.Add(new AboutItem("Network Interface", () => "eth0"));
        list.Add(new AboutItem("DHCP", () => "Yes"));
        list.Add(new AboutItem("Network Address", () => "127.0.0.1/8"));
        list.Add(new AboutItem("Gateway", () => "127.0.0.1"));

        for (int i = 0; i < 40; i++)
        {
            list.Add(new AboutItem($"Test {i + 1}", $"Test value {i + 1}"));
        }

        AboutItems = list;

        this.scheduler.Schedule(TimeSpan.FromMilliseconds(100), RefreshSettings);

        this.log.LogInformation("AboutViewModel created");
    }

    private void RefreshSettings()
    {
        try
        {
            foreach (var item in AboutItems.OfType<AboutItem>())
            {
                item.Refresh();
            }
        }
        catch (Exception ex)
        {
            this.log.LogError("Error in timer: {Message}", ex.Message);
        }
        finally
        {
            if (!this.disposed)
                this.scheduler.Schedule(TimeSpan.FromSeconds(1), RefreshSettings);
        }
    }

    public IList<object> AboutItems { get; }

    public string? LogoPath0x40 { get; private set; }

    public Task SelectorPressedShort(DependencyObject focusItem)
    {
        // We will go back even if the current focus item is in the list view
        BackCommand.Execute(null);

        return Task.CompletedTask;
    }

    public Task SelectorPressedLong(DependencyObject focusItem)
    {
        BackCommand.Execute(null);

        return Task.CompletedTask;
    }
}

[Bindable]
public class AboutItem : ObservableObject
{
    private readonly Func<string> valueFunc;

    public string Key { get; set; }

    private string value;
    public string Value { get => this.value; set => SetProperty(ref this.value, value); }

    public void Refresh()
    {
        if (this.valueFunc != null)
            Value = this.valueFunc();
    }

    public AboutItem(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public AboutItem(string key, Func<string> valueFunc)
    {
        Key = key;
        this.valueFunc = valueFunc;
    }
}

[Bindable]
public class AboutHeaderItem
{
    public string Text { get; set; }

    public AboutHeaderItem(string text)
    {
        Text = text;
    }
}
