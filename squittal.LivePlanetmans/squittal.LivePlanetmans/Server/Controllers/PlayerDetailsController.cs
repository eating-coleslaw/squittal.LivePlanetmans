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
    public class PlayerDetailsController : ControllerBase
    {
        private readonly ICharacterService _characterService;
        private readonly ILogger<PlayerDetailsController> _logger;


        public PlayerDetailsController(ICharacterService characterService, ILogger<PlayerDetailsController> logger)
        {
            _characterService = characterService;
            _logger = logger;
        }

        [HttpGet("{characterId}")]
        public async Task<ActionResult<Character>> GetPlayerDetailsAsync(string characterId)
        {

            return await _characterService.GetCharacterAsync(characterId);
        }
    }
}
