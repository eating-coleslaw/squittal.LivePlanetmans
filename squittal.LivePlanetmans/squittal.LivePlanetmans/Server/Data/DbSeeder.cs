using squittal.LivePlanetmans.Server.Services.Planetside;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Data
{
    public class DbSeeder : IDbSeeder
    {
        private readonly IWorldService _worldService;
        private readonly IFactionService _factionService;

        public DbSeeder(IWorldService worldService, IFactionService factionService)
        {
            _worldService = worldService;
            _factionService = factionService;
        }

        public async Task OnApplicationStartup(CancellationToken cancellationToken)
        {
            List<Task> TaskList = new List<Task>();
            
            Task worldsTask = _worldService.RefreshStore();
            TaskList.Add(worldsTask);

            Task factionsTask = _factionService.RefreshStore();
            TaskList.Add(factionsTask);

            await Task.WhenAll(TaskList);
        }

        public async Task OnApplicationShutdown(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
