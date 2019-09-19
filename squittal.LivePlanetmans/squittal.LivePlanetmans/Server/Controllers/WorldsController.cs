using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Server.Services.Planetside;
using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorldsController : ControllerBase
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly ILogger<PlayerLeaderboardController> _logger;
        private readonly IWorldService _worldService;

        public IList<PlayerHourlyStatsData> Players { get; private set; }

        public WorldsController(IDbContextHelper dbContextHelper, IWorldService worldService, ILogger<PlayerLeaderboardController> logger)
        {
            _dbContextHelper = dbContextHelper;
            _logger = logger;
            _worldService = worldService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<World>>> GetAllWorlds()
        {
            await _worldService.RefreshStore();
            IEnumerable<World> result = await _worldService.GetAllWorldsAsync();

            return result.ToArray();
        }
    }
}