using Orleans;

namespace OrleansInterface
{
    public interface IMsgClient : IGrainObserver
    {
        void ReceiveMessage(string message);
    }
}