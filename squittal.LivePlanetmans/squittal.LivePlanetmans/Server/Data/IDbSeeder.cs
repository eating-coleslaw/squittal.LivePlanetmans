using System.Threading;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Data
{
    public interface IDbSeeder
    {
        Task OnApplicationStartup(CancellationToken cancellationToken);
        Task OnApplicationShutdown(CancellationToken cancellationToken);
    }
}
