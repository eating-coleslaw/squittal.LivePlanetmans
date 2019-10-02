using Microsoft.AspNetCore.Mvc;
using squittal.LivePlanetmans.Server.Services.Planetside;
using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZonesController : ControllerBase
    {
        private readonly IZoneService _zoneService;

        public ZonesController(IZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Zone>>> GetAllWorlds()
        {
            await _zoneService.RefreshStore();
            IEnumerable<Zone> result = await _zoneService.GetAllZonesAsync();

            return result.ToArray();
        }
    }
}
    {
    }
}
