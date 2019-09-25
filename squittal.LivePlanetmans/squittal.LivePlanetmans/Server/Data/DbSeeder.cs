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
        private readonly IItemService _itemService;
        private readonly IZoneService _zoneService;

        public DbSeeder(
            IWorldService worldService,
            IFactionService factionService,
            IItemService itemService,
            IZoneService zoneService)
        {
            _worldService = worldService;
            _factionService = factionService;
            _itemService = itemService;
            _zoneService = zoneService;
        }

        public async Task OnApplicationStartup(CancellationToken cancellationToken)
        {
            List<Task> TaskList = new List<Task>();
            
            Task worldsTask = _worldService.RefreshStore();
            TaskList.Add(worldsTask);

            Task factionsTask = _factionService.RefreshStore();
            TaskList.Add(factionsTask);

            Task itemsTask = _itemService.RefreshStore();
            TaskList.Add(itemsTask);

            Task zoneTask = _zoneService.RefreshStore();
            TaskList.Add(zoneTask);

            await Task.WhenAll(TaskList);
        }

        public async Task OnApplicationShutdown(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
