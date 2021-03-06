﻿using Microsoft.AspNetCore.Mvc;
using squittal.LivePlanetmans.Server.Services.Planetside;
using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorldsController : ControllerBase
    {
        private readonly IWorldService _worldService;

        public WorldsController(IWorldService worldService)
        {
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