using System;
using Microsoft.Extensions.Hosting;
using OrleansSiloHost.Helpers;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;

namespace OrleansSiloHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var logConfig = new LoggerConfiguration()
                //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.Runtime.Management.ManagementGrain", LogEventLevel.Warning)
                .MinimumLevel.Override("Orleans.Runtime.SiloControl", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                .WriteTo.Trace()
                .WriteTo.Debug();

            Log.Logger = logConfig
                .Enrich.FromLogContext().CreateLogger();

            var genericHostBuilder = GenericHostBuilderHelper.CreateHostBuilder(args);

            try
            {
                var genericHost = genericHostBuilder.Build();
                genericHost.Run();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "chat server error");
                throw;
            }
        }
    }
}
