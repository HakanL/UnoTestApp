#define TRACE_MASTER_CLOCK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DMXCore.DMXCore100.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace DMXCore.DMXCore100;

public static class ServiceCollectionExtensions
{
    public const string FileTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3} [{SourceContext}] {Message:lj}{NewLine}{Exception}";
#if DEBUG && TRACE_MASTER_CLOCK
    public const string TraceTemplate = "{Timestamp:HH:mm:ss.fff} {Level:u3} {MasterClock}#[{SourceContext}] {Message:lj}{NewLine}{Exception}";
#else
    public const string TraceTemplate = "{Timestamp:HH:mm:ss.fff} {Level:u3} [{SourceContext}] {Message:lj}{NewLine}{Exception}";
#endif
    public const string ConsoleTemplate = "{Timestamp:HH:mm:ss.fff} {Level:u3} [{SourceContext}] {Message:lj}{NewLine}{Exception}";

    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        services.AddHostedService<StartupService>();

        services.AddSingleton<IScheduler>(new SynchronizationContextScheduler(SynchronizationContext.Current));

        return services;
    }

    public static IApplicationBuilder AddSharedApplicationBuilder(this IApplicationBuilder builder)
    {
        return builder;
    }

    public static IHostBuilder AddSharedHostBuilder(this IHostBuilder builder, Action<LoggerConfiguration>? configureLogging)
    {
        builder
#if DEBUG
        // Switch to Development environment when running in DEBUG
        .UseEnvironment(Environments.Development)
#endif
        .UseSerilog((ctx, logConfig) =>
        {
            string logFolder = Path.Combine(Path.GetTempPath(), "DMXCore100Logs");
            var file = Path.Combine(logFolder, "DMXCore100-.log");

            logConfig
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.UI", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.UI.Xaml.Media.ImageBrush", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.UI.Xaml.Media.ImageSource", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.UI.Xaml.XamlRoot", LogEventLevel.Information)

                // Filter out ASP.NET Core infrastructre logs that are Information and below
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Cors.Infrastructure.CorsService", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.Connections", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Http.Connections.Internal.HttpConnectionDispatcher", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc.Infrastructure", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing.EndpointMiddleware", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.SignalR", LogEventLevel.Information);

            // Apply host-specific logging configuration
            configureLogging?.Invoke(logConfig);

            logConfig
                .Enrich.FromLogContext()
                .Enrich.With<SimpleClassNameEnricher>()
                .Enrich.WithMachineName()
                .WriteTo.Async(a => a.Debug(outputTemplate: TraceTemplate))
                .WriteTo.Async(a => a.Console(outputTemplate: ConsoleTemplate, restrictedToMinimumLevel: LogEventLevel.Verbose));
        });

        return builder;
    }
}
