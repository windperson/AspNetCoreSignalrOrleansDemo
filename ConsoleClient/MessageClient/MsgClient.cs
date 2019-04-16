using OrleansInterface;
using Serilog;

namespace ConsoleClient.MessageClient
{
    public class MsgClient : IMsgClient
    {
        public void ReceiveMessage(string message)
        {
            Log.Information($"Client Receive: {message}");
        }
    }
}