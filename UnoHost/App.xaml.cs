//#define TRACE_INPUT

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using DMXCore.DMXCore100.Models;
using DMXCore.DMXCore100.Services;
using DMXCore.DMXCore100.ViewModels;
using DMXCore.DMXCore100.Views;
using Microsoft.AspNetCore.Hosting;
using Serilog.Events;
using DMXCore.DMXCore100.Extensions;

namespace DMXCore.DMXCore100;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application, ILifetimeControl
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    protected static Window? MainWindow { get; private set; }
    public static IHost? Host { get; private set; }

    private readonly CancellationTokenSource shutdownEvent = new();
    private readonly ManualResetEvent systemShutdown = new(false);
    private readonly ISubject<bool> runningSubject = new Subject<bool>();

    public void Shutdown() => this.shutdownEvent.Cancel();

    public IObservable<bool> Running => this.runningSubject.AsObservable();

    public void RegisterShutdownEvent(Action shutdownEvent)
    {
        this.shutdownEvent.Token.Register(shutdownEvent);
    }

    public void SetSystemShutdownEvent()
    {
        this.systemShutdown.Set();
    }

    public void SetSystemShuttingDown()
    {
        this.runningSubject.OnNext(false);
    }

    public void WaitUntilSystemShutdown()
    {
        // Max 5 seconds
        this.systemShutdown.WaitOne(5_000);
    }

    public static void InitializeEarlyLogging()
    {
        // Logging is disabled by default for release builds, as it incurs a significant
        // initialization cost from Microsoft.Extensions.Logging setup. If startup performance
        // is a concern for your application, keep this disabled. If you're running on web or 
        // desktop targets, you can use url or command line parameters to enable it.
        //
        // For more performance documentation: https://platform.uno/docs/articles/Uno-UI-Performance.html

        var factory = LoggerFactory.Create(builder =>
        {
#if __WASM__
            builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__
            builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
            builder.AddDebug();
#else
            builder.AddSimpleConsole(c =>
            {
                c.TimestampFormat = "HH:mm:ss.fff ";
                c.IncludeScopes = true;
                c.SingleLine = true;
            });

            builder.AddDebug();
#endif

            // Exclude logs below this level
            builder.SetMinimumLevel(LogLevel.Debug);

            // Default filters for Uno Platform namespaces
            builder.AddFilter("Uno", LogLevel.Warning);
            builder.AddFilter("Windows", LogLevel.Warning);
            builder.AddFilter("Microsoft", LogLevel.Warning);
            //            builder.AddFilter("Uno.UI.Runtime.Skia.GLValidationSurface", LogLevel.Trace);
            //builder.AddFilter("Uno.WinUI.Runtime.Skia", LogLevel.Debug);
            builder.AddFilter("Uno.UI.Runtime.Skia", LogLevel.Debug);

            // Disable the logging for this issue: https://github.com/unoplatform/uno.toolkit.ui/discussions/1258
            builder.AddFilter("Microsoft.UI.Xaml.Data", LogLevel.None);

            builder.AddFilter("Microsoft.UI.Xaml.Controls.Layouter", LogLevel.Debug);
            builder.AddFilter("Microsoft.UI.Xaml.Controls.Panel", LogLevel.Debug);

            builder.AddFilter("Uno.UI.Xaml.Core.VisualTree", LogLevel.None);
            builder.AddFilter("Uno.UI.Xaml.Core.InputManager", LogLevel.None);
            builder.AddFilter("Uno.WinUI.Runtime.Skia.X11", LogLevel.None);
            builder.AddFilter("Uno.UI.DataBinding", LogLevel.None);     // Binding errors
            builder.AddFilter("Uno.Toolkit.UI", LogLevel.None);

            // RemoteControl and HotReload related
            builder.AddFilter("Uno.UI.RemoteControl", LogLevel.None);
        });

        global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

        global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
    }

#pragma warning disable VSTHRD100 // Avoid async void methods
    protected async override void OnLaunched(LaunchActivatedEventArgs args)
#pragma warning restore VSTHRD100 // Avoid async void methods
    {
        var startupWatch = Stopwatch.StartNew();

        try
        {
            var builder = this.CreateBuilder(args)
            // Add navigation support for toolkit controls such as TabBar and NavigationView
            .UseToolkitNavigation()
            .Configure(host => host
                .UseThemeSwitching()
                .UseLogging(configure: (context, logBuilder) =>
                {
                    logBuilder.Configure(o => o.ActivityTrackingOptions = ActivityTrackingOptions.TraceId);

                    // Configure log levels for different categories of logging
                    logBuilder.SetMinimumLevel(
                        context.HostingEnvironment.IsDevelopment() ?
                            LogLevel.Information :
                            LogLevel.Warning);

                    logBuilder.CoreLogLevel(LogLevel.Trace);
#if DEBUG
                }, enableUnoLogging: true)
#else
                    }, enableUnoLogging: false)
#endif
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        // Would be better to use ContentSource, but it's not working yet: https://github.com/unoplatform/uno.extensions/issues/1497
                        .EmbeddedSource<App>()
                        .EmbeddedSource<App>("development")
                )
                .ConfigureServices((context, services) =>
                {
                    // Add shared services
                    services.AddSharedServices();

                    services.AddHostedService<UIStartupService>();

                    // Register services
                    services.AddSingleton<App>(this);
                    services.AddSingleton<IMenuManager, MenuManager>();
                    services.AddSingleton<ILifetimeControl>(this);
                    services.AddSingleton<IMenuFocusManager, MenuFocusManager>();
                    services.AddScoped<IMyNavigator, MyNavigator>();
                })
                .UseNavigation(RegisterRoutes)
                .AddSharedHostBuilder(logConfig =>
                {
                    logConfig
                        .MinimumLevel.Override("Uno", LogEventLevel.Information)
                        .MinimumLevel.Override("Uno.Extensions.Navigation", LogEventLevel.Warning)
                        .MinimumLevel.Override("Uno.UI.DataBinding.BindingPath", LogEventLevel.Information)
                        .MinimumLevel.Override("Uno.UI.Runtime.Skia.Gtk.UI.GtkDispatch", LogEventLevel.Information)
                        .MinimumLevel.Override("Uno.UI.Xaml.Core.VisualTree", LogEventLevel.Warning)
                        .MinimumLevel.Override("Uno.UI.Xaml.Input.FocusRectManager", LogEventLevel.Information)
                        .MinimumLevel.Override("Windows", LogEventLevel.Information)

                        // Binding related messages
                        .MinimumLevel.Override("Windows.UI.Xaml.Data", LogEventLevel.Debug)

#if DEBUG && TRACE_INPUT
                        .MinimumLevel.Override("Uno.UI.Runtime.Skia.FrameBufferPointerInputSource", LogEventLevel.Verbose)
                        .MinimumLevel.Override("Uno.UI.Xaml.Core.InputManager", LogEventLevel.Verbose)
                        .MinimumLevel.Override("Uno.UI.Xaml.Core.InputManager+PointerManager", LogEventLevel.Verbose)
#else
                        .MinimumLevel.Override("Uno.UI.Runtime.Skia", LogEventLevel.Debug)
                        .MinimumLevel.Override("Uno.UI.Runtime.Skia.FrameBufferPointerInputSource", LogEventLevel.Warning)
                        .MinimumLevel.Override("Uno.UI.Xaml.Core.InputManager", LogEventLevel.Warning)
                        .MinimumLevel.Override("Uno.UI.Xaml.Core.InputManager+PointerManager", LogEventLevel.Warning)
                        .MinimumLevel.Override("Uno.UI.Xaml.Core.PointerCapture", LogEventLevel.Warning)
#endif

                        .MinimumLevel.Override("Microsoft.UI.Xaml.Controls.Primitives.ComboBoxTemplateSettings", LogEventLevel.Fatal)
                        .MinimumLevel.Override("Microsoft.UI.Xaml.Controls.ScrollViewer", LogEventLevel.Fatal)
                        .MinimumLevel.Override("Microsoft.UI.Xaml.Media.Animation.SplitCloseThemeAnimation", LogEventLevel.Fatal)
                        .MinimumLevel.Override("Microsoft.UI.Xaml.VisualTransition", LogEventLevel.Fatal);
                })
            );

            MainWindow = builder.Window;

#if DEBUG
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && Debugger.IsAttached)
            {
                MainWindow.UseStudio();
            }
#endif

#if __SKIA__
			//Uno.UI.FeatureConfiguration.ToolTip.UseToolTips = false;
#endif

#if HAS_UNO
            Uno.UI.FeatureConfiguration.ScrollViewer.DefaultAutoHideDelay = TimeSpan.MaxValue;
#endif

            MainWindow.Closed += (s, e) =>
            {
                Shutdown();
            };

            MainWindow.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 800 + 16, Height = 480 + 39 });

            var appPresenter = MainWindow.AppWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            appPresenter.IsResizable = false;

#if WINDOWS10_0_19041_0_OR_GREATER
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(MainWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            var presenter = appWindow.Presenter as OverlappedPresenter;
            appWindow.Resize(new global::Windows.Graphics.SizeInt32 { Width = 800, Height = 480 });
            appWindow.Title = "DMX Core 100";
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
#endif

            Host = await builder.NavigateAsync<Shell>();

            var log = Host.Services.GetRequiredService<ILogger<App>>();

            var menuFocusManager = Host.Services.GetRequiredService<IMenuFocusManager>();
            menuFocusManager.Activate();

            // Signal that we're running
            this.runningSubject.OnNext(true);

            startupWatch.Stop();
            log.LogInformation("End of OnLaunched, duration = {StartupDuration:N0} ms", startupWatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Startup exception: {ex}");

            Application.Current.Exit();
        }
    }

    private static void RegisterRoutes(Uno.Extensions.Navigation.IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellViewModel)),
            new ViewMap<HomePage, HomeViewModel>(),
            new DataViewMap<MenuPage, MenuViewModel, Menu>(),
            new DataViewMap<MenuPageSmall, MenuSmallViewModel, Menu>(),
            new ViewMap<AboutPage, AboutViewModel>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellViewModel>(),
                Nested:
                [
                    new RouteMap("Home", View: views.FindByViewModel<HomeViewModel>()),
                    new RouteMap("Menu", View: views.FindByViewModel<MenuViewModel>()),
                    new RouteMap("Menu2", View: views.FindByViewModel<MenuViewModel>()),
                    new RouteMap("MenuSmall", View: views.FindByViewModel<MenuSmallViewModel>()),
                    new RouteMap("About", View: views.FindByViewModel<AboutViewModel>())
                ]
            )
        );
    }
}

public class UIStartupService : IHostedService
{
    private readonly Microsoft.Extensions.Logging.ILogger log;
    private readonly IServiceProvider serviceProvider;

    public UIStartupService(ILogger<UIStartupService> logger, IServiceProvider serviceProvider)
    {
        this.log = logger;
        this.serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var joinableTaskFactory = new Microsoft.VisualStudio.Threading.JoinableTaskFactory(new Microsoft.VisualStudio.Threading.JoinableTaskContext());

        try
        {
            var compositionCapabilities = new Microsoft.UI.Composition.CompositionCapabilities();
            bool acceleratedRendering = compositionCapabilities.AreEffectsFast();
            this.log.LogInformation("Accelerated Rendering: {AcceleratedRendering}", acceleratedRendering);

            ThemeBasedUriConverter.IsDarkMode = false;

            var menuFocusManager = this.serviceProvider.GetRequiredService<IMenuFocusManager>();

            var routeNotifier = this.serviceProvider.GetRequiredService<IRouteNotifier>();
            routeNotifier.RouteChanged += (s, e) =>
            {
                log.LogTrace("Route changed");

                // We need to filter out the Uno DiagnosticsOverlay
                var popups = VisualTreeHelper.GetOpenPopups(Window.Current).Where(x => x.Child is not Uno.Diagnostics.UI.DiagnosticsOverlay).ToList();

                // Used to determine if we're displaying a popup or not and not turn off the backlight if one is displayed
                // Not used in this example

                var page = ((ContentControl)e.Region.Children.FirstOrDefault()?.View)?.Content;

                if (page != null)
                {
                    menuFocusManager.SetCurrentPage(page);
                }
                else
                {
                    if (e.Navigator is ContentDialogNavigator contentDialogNavigator && !string.IsNullOrEmpty(contentDialogNavigator.Route?.Base))
                    {
                        menuFocusManager.UpdateCurrentPage();
                    }
                    else
                    {
                        menuFocusManager.SetCloseDialog();
                    }
                }
            };

            // Just for test
            string imageFolder = Path.Combine(AppContext.BaseDirectory, "Assets", "Images");

            // Prime the image cache
            this.log.LogDebug("Priming image cache");
            IconHelper.GetIconName(this.log, "menu-background", imageFolder, 800, 389);
            IconHelper.GetIconName(this.log, "home-logo", imageFolder, 0, 40, internalFilename: "dmxcore_logo_large.png");
            IconHelper.GetIconName(this.log, "home-logo", imageFolder, 800, 300, internalFilename: "dmxcore_logo_large.png");
            IconHelper.GetIconName(this.log, "home-logo", imageFolder, 800, 105, internalFilename: "dmxcore_logo_large.png");
            this.log.LogDebug("Priming image cache...done");

            App.Current.UnhandledException += (s, e) =>
            {
                this.log.LogError(e.Exception, "Unhandled exception: {Message}", e.Message);
            };

            if (OperatingSystem.IsLinux())
            {
                Uno.UI.FeatureConfiguration.CompositionTarget.FrameRate = 15;
            }

#if DEBUG
            Application.Current.DebugSettings.EnableFrameRateCounter = true;

            // Uno settings
            //Microsoft.UI.Xaml.FrameworkTemplatePool.IsPoolingEnabled = false;
#endif

        }
        catch (Exception ex)
        {
            this.log.LogCritical(ex, "Failed to start: {Message}", ex.Message);

            Console.WriteLine($"Failed to start: {ex}");

#if DEBUG
            throw;
#endif
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
