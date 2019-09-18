using System.Threading;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.CensusStream
{
    public interface IWebsocketMonitor
    {
        Task OnApplicationStartup(CancellationToken cancellationToken);
        Task OnApplicationShutdown(CancellationToken cancellationToken);
    }
}
