using System.Threading.Tasks;
using Orleans;

namespace OrleansInterface
{
    public interface IMsgServer : IGrainWithStringKey
    {
        Task Subscribe(IMsgClient msgClient);

        Task UnSubscribe(IMsgClient msgClient);

        Task BroadcastMessage(string greetings);
    }
}