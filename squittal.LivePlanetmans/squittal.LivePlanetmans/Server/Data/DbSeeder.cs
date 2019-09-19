using squittal.LivePlanetmans.Server.Services.Planetside;
using System.Threading;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Data
{
    public class DbSeeder : IDbSeeder
    {
        private readonly IWorldService _worldService;

        public DbSeeder(IWorldService worldService)
        {
            _worldService = worldService;
        }

        public async Task OnApplicationStartup(CancellationToken cancellationToken)
        {
            await _worldService.RefreshStore();
        }

        public async Task OnApplicationShutdown(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
