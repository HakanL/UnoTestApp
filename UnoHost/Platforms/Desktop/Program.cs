//#define EARLY_TRACING
///#define TRACE_PERFORMANCE

using System;
using System.Runtime.InteropServices;
using Uno.UI.Hosting;

namespace DMXCore.DMXCore100;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Console.WriteLine($"Starting up at {System.DateTime.Now:O}");

#if EARLY_TRACING
        App.InitializeEarlyLogging();
#endif

#if DEBUG && TRACE_PERFORMANCE
        App.EnableTracing();
#endif



        //ExceptionManager.UnhandledException += delegate (UnhandledExceptionArgs expArgs)
        //{
        //    Console.WriteLine("GLIB UNHANDLED EXCEPTION" + expArgs.ExceptionObject.ToString());
        //    //expArgs.ExitApplication = true;
        //};

        try
        {
            var hostBuilder = UnoPlatformHostBuilder.Create()
                .App(() => new App())
                .UseX11();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                hostBuilder = hostBuilder.UseWin32();
            }

            var host = hostBuilder
                .Build();

            System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += (e) =>
            {
                Console.WriteLine("Shutting down via Loader Unloading");

                ((App)Microsoft.UI.Xaml.Application.Current)?.Shutdown();
            };

            host.Run();

            if (App.Current is App mainApp)
            {
                mainApp.Shutdown();
                mainApp.WaitUntilSystemShutdown();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Startup exception: {ex}");
            Environment.ExitCode = 100;
        }
        finally
        {
            Console.WriteLine("Final shutdown");
        }
    }
}
