using System;
using System.Threading.Tasks;
using ConsoleClient.MessageClient;
using ConsoleClient.OrleansClientBuilder;
using OrleansInterface;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace ConsoleClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName()
                .Enrich.WithThreadId()
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .WriteTo.Trace()
                .WriteTo.Debug();

            Log.Logger = logConfig.CreateLogger();

            try
            {
                Log.Information("Press Enter to begin connecting to server");
                Console.ReadLine();

                IMsgClient msgClient = new MsgClient();

                using (var client = OrleansClientBuilderHelper.CreateClientBuilder(args).Build())
                {
                    await client.Connect();
                    Log.Information("Client successfully connect to silo host");

                    var grain = client.GetGrain<IMsgServer>("test");

                    var msgClientInvoker = await client.CreateObjectReference<IMsgClient>(msgClient);

                    await grain.Subscribe(msgClientInvoker);
                    Log.Information("client subscribe server complete");

                    while (true)
                    {
                        Log.Information("Input something to send to server:");
                        var input = Console.ReadLine();
                        await grain.BroadcastMessage(input);

                        Log.Information("Press 'x' key to exit, or press any key to continue");
                        var pressedKey = Console.ReadKey().KeyChar;
                        if (pressedKey == 'x')
                        {
                            Log.Information("Exit demo...");
                            break;
                        }
                    }
                    
                    await grain.UnSubscribe(msgClientInvoker);
                    Log.Information("Client unsubscribe from server complete");
                    await client.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Client side error");
                throw;
            }
        }
    }
}
