﻿using Microsoft.AspNetCore.Mvc;
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
        private readonly IDbContextHelper _dbContextHelper;
        private readonly ILogger<PlayerDetailsController> _logger;


        public PlayerDetailsController(ICharacterService characterService, IDbContextHelper dbContextHelper, ILogger<PlayerDetailsController> logger)
        {
            _characterService = characterService;
            _dbContextHelper = dbContextHelper;
            _logger = logger;
        }

        [HttpGet("{characterId}")]
        public async Task<ActionResult<Character>> GetPlayerDetailsAsync(string characterId)
        {

            return await _characterService.GetCharacterAsync(characterId);
        }

        [HttpGet("kills/{characterId}")]
        public async Task<ActionResult<IEnumerable<PlayerKillboardItem>>> GetPlayDeaths(string characterId) //, int rows = 20)
        {
            int rows = 50;

            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<PlayerKillboardItem> query =
                    from death in dbContext.Deaths

                    join character in dbContext.Characters on death.CharacterId equals character.Id into characterQ
                    from character in characterQ.DefaultIfEmpty()

                    where death.Timestamp >= startTime && (death.AttackerCharacterId == characterId || death.CharacterId == characterId)
                    select new PlayerKillboardItem()
                    {
                        VictimId = death.CharacterId,
                        VictimName = (from c in dbContext.Characters
                                      where c.Id == death.CharacterId
                                      select c.Name).FirstOrDefault(),
                        VictimFactionId = (from c in dbContext.Characters
                                           where c.Id == death.CharacterId
                                           select c.FactionId).FirstOrDefault(),
                        IsHeadshot = death.IsHeadshot,
                        VictimLoadoutId = death.CharacterLoadoutId,
                        AttackerName = (from c in dbContext.Characters
                                        where c.Id == death.AttackerCharacterId
                                        select c.Name).FirstOrDefault(),
                        AttackerLoadoutId = death.AttackerLoadoutId,
                        AttackerWeaponId = death.AttackerWeaponId,
                        KillTimestamp = death.Timestamp
                    };

                return await query
                    .AsNoTracking()
                    .OrderByDescending(k => k.KillTimestamp)
                    //.Take(rows)
                    .ToArrayAsync();
            }
        }
    }
}