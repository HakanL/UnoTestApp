using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMXCore.DMXCore100.Models;
using DMXCore.DMXCore100.Services;

namespace DMXCore.DMXCore100.ViewModels;

[Bindable]
public class BaseViewModel : ObservableObject, IDisposable
{
    private static int invokationCounter = 0;
    private static Dictionary<int, string> aliveInvokationIds = new();

    protected readonly ILogger log;
    private readonly Timer clockTimer;
    private string lastCurrentTime;
    protected readonly IScheduler scheduler;
    protected readonly IMyNavigator navigator;
    protected readonly IMenuFocusManager menuFocusManager;
    protected readonly IMenuManager menuManager;
    protected List<IDisposable> disposables = [];
    protected bool disposed;
    public readonly int InvokationId = ++invokationCounter;

    public BaseViewModel(
        ILogger logger,
        IScheduler scheduler,
        IMenuFocusManager menuFocusManager,
        IMenuManager menuManager,
        IMyNavigator navigator)
    {
        this.log = logger;
        this.scheduler = scheduler;
        this.menuManager = menuManager;
        this.navigator = navigator;
        this.menuFocusManager = menuFocusManager;
        this.clockTimer = new(ClockTimer_Tick, null, 1000, 100);

        this.log.LogTrace("Creating instance of {InstanceType} with id {InvokationId}", this.GetType().Name, InvokationId);

        Status = "Test";
        IsAdmin = false;
        IsAdminLockDown = false;


        // Force update
        OutputActiveBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));

        AdminCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        UpdateCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        LockCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        OutputCommand = new AsyncRelayCommand(() => Task.CompletedTask);
        ClockHeldCommand = new AsyncRelayCommand(async () =>
            {
                string info = $"IP Address: 127.0.0.1\r\n" +
                    $"App Version: 1.2.3.4";

                await navigator.ShowMessageDialogAsync(this, title: "Host Information", content: info);
            });

        menuFocusManager.TrackViewModel(this);

        aliveInvokationIds.Add(InvokationId, this.GetType().Name);
        this.log.LogTrace("Created instance of {InstanceType} with id {InvokationId}", this.GetType().Name, InvokationId);
    }

    public ICommand BackCommand { get; protected set; }

    public string CurrentTime => DateTime.Now.ToString("hh:mm:ss", CultureInfo.DefaultThreadCurrentUICulture);

    public Visibility UpdateAvailableVisibility => Visibility.Collapsed;

    public Visibility OutputActiveVisibility => Visibility.Visible;

    public Visibility RemoteConnectedVisibility => Visibility.Collapsed;

    public Visibility PluginConnectedVisibility => Visibility.Collapsed;

    public string OutputGlyph => "\uE8D6";

    public string Status { get; set; }

    private Brush outputActiveBrush;
    public Brush OutputActiveBrush { get => this.outputActiveBrush; private set => SetProperty(ref this.outputActiveBrush, value); }

    private bool remoteConnected;
    public bool RemoteConnected
    {
        get => this.remoteConnected;
        private set
        {
            SetProperty(ref this.remoteConnected, value);
            RemoteConnectedBrush = RemoteConnected ?
                new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)) :
                new SolidColorBrush(Windows.UI.Color.FromArgb(255, 80, 80, 80));
        }
    }

    private Brush remoteConnectedBrush;
    public Brush RemoteConnectedBrush { get => this.remoteConnectedBrush; private set => SetProperty(ref this.remoteConnectedBrush, value); }

    private bool pluginConnected;
    public bool PluginConnected
    {
        get => this.pluginConnected;
        private set
        {
            SetProperty(ref this.pluginConnected, value);
            PluginConnectedBrush = PluginConnected ?
                new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255)) :
                new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0));
        }
    }

    private Brush pluginConnectedBrush;
    public Brush PluginConnectedBrush { get => this.pluginConnectedBrush; private set => SetProperty(ref this.pluginConnectedBrush, value); }

    private bool isAdmin;
    public bool IsAdmin
    {
        get => this.isAdmin;
        private set
        {
            SetProperty(ref this.isAdmin, value);
            OnPropertyChanged(nameof(BackgroundBrush));

            OnPropertyChanged(nameof(InfoLabel1));
            OnPropertyChanged(nameof(InfoLabel1Visibility));
            OnPropertyChanged(nameof(InfoLabel1Brush));
            OnPropertyChanged(nameof(BackButtonVisibility));
        }
    }

    private bool isAdminLockDown;
    public bool IsAdminLockDown
    {
        get => this.isAdminLockDown;
        private set
        {
            SetProperty(ref this.isAdminLockDown, value);
            OnPropertyChanged(nameof(BackButtonVisibility));
        }
    }

    public Visibility LoginVisibility => Visibility.Visible;

    public Visibility AdminButtonVisibility => Visibility.Visible;

    public Visibility UserButtonVisibility => Visibility.Collapsed;

    public Visibility LoggedOutButtonVisibility => Visibility.Collapsed;

    public Visibility LockVisibility => Visibility.Visible;

    public Visibility BackButtonVisibility
    {
        get
        {
            return Visibility.Visible;
        }
    }

    public Brush BackgroundBrush
    {
        get => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 32, 32, 32));
    }

    private string GetDeviceName()
    {
        return Environment.MachineName;
    }

    public Visibility InfoLabel1Visibility => Visibility.Visible;

    public Visibility InfoLabel2Visibility => Visibility.Visible;

    public string InfoLabel1 => "User Name";

    public string InfoLabel2 => GetDeviceName();

    public Brush InfoLabel1Brush
    {
        get => this.isAdmin ?
            new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)) :
            new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255));
    }

    public Brush StatusBrush
    {
        get => new SolidColorBrush(Windows.UI.Color.FromArgb(255, 128, 128, 128));
    }

    private void ClockTimer_Tick(object state)
    {
        if (this.lastCurrentTime == CurrentTime)
            return;

        try
        {
            this.lastCurrentTime = CurrentTime;
            this.scheduler.Schedule(() =>
            {
                OnPropertyChanged(nameof(CurrentTime));
            });
        }
        catch (Exception ex)
        {
            this.log.LogError("Error in ClockTimer: {Message}", ex.Message);
        }
    }

    public ICommand AdminCommand { get; }

    public ICommand UpdateCommand { get; }

    public ICommand LockCommand { get; }

    public ICommand OutputCommand { get; }

    public ICommand ClockHeldCommand { get; }

    private bool adminOptionVisibleInControl = true;
    public bool AdminOptionVisibleInControl
    {
        get => this.adminOptionVisibleInControl;
        set
        {
            if (this.adminOptionVisibleInControl != value)
            {
                this.adminOptionVisibleInControl = value;
                OnPropertyChanged(nameof(LoginVisibility));
            }
        }
    }

    public virtual void Dispose()
    {
        aliveInvokationIds.Remove(InvokationId);
        this.log.LogTrace("Disposing {InstanceType} with id {InvokationId}", this.GetType().Name, InvokationId);

        foreach (var kvp in aliveInvokationIds)
        {
            this.log.LogTrace("Alive instance: id {InvokationId} of type {InstanceType}", kvp.Key, kvp.Value);
        }

        this.disposed = true;
        this.clockTimer.Dispose();
        this.disposables.ForEach(x => x.Dispose());
        this.disposables.Clear();
    }
}
