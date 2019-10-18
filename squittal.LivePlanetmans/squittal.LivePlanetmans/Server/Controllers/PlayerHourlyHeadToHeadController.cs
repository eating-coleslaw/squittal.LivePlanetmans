using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Server.Services.Planetside;
using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerHourlyHeadToHeadController : ControllerBase
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly ICharacterService _characterService;

        public PlayerHourlyHeadToHeadController(IDbContextHelper dbContextHelper, ICharacterService characterService)
        {
            _dbContextHelper = dbContextHelper;
            _characterService = characterService;
        }

        [HttpGet("players/{characterId}")]
        public async Task<ActionResult<PlayerHourlyHeadToHeadSummary>> GetHourlyHeadToHeadAsync(string characterId)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            var characterEntity = await _characterService.GetCharacterAsync(characterId);

            if (characterEntity == null)
            {
                return null;
            }

            var playerFactionId = characterEntity.FactionId;


            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<PlayerHourlyHeadToHeadSummaryRow> query =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && ( death.AttackerCharacterId == characterId
                            || death.CharacterId == characterId )
                       && death.DeathEventType != DeathEventType.Suicide

                    //group death by new { death.AttackerCharacterId, death.CharacterId } into charactersGroup

                    join character in dbContext.Characters
                      //on charactersGroup.Key.CharacterId equals character.Id into victimCharactersQ
                      on death.CharacterId equals character.Id into victimCharactersQ
                    from victimCharacters in victimCharactersQ.DefaultIfEmpty()

                    join character in dbContext.Characters
                      //on charactersGroup.Key.AttackerCharacterId equals character.Id into attackerCharactersQ
                      on death.AttackerCharacterId equals character.Id into attackerCharactersQ
                    from attackerCharacters in attackerCharactersQ.DefaultIfEmpty()


                    select new PlayerHourlyHeadToHeadSummaryRow()
                    {
                        AttackerCharacterId = attackerCharacters.Id,
                        AttackerName = attackerCharacters.Name,
                        AttackerFactionId = attackerCharacters.FactionId,
                        AttackerBattleRank = attackerCharacters.BattleRank,
                        AttackerPrestigeLevel = attackerCharacters.PrestigeLevel,

                        VictimCharacterId = victimCharacters.Id,
                        VictimName = victimCharacters.Name,
                        VictimFactionId = victimCharacters.FactionId,
                        VictimBattleRank = victimCharacters.BattleRank,
                        VictimPrestigeLevel = victimCharacters.PrestigeLevel,

                        AttackerKills = (from kill in dbContext.Deaths
                                         //where kill.AttackerCharacterId == charactersGroup.Key.AttackerCharacterId
                                         //   && kill.CharacterId == charactersGroup.Key.CharacterId
                                         where kill.AttackerCharacterId == attackerCharacters.Id
                                            && kill.CharacterId == victimCharacters.Id
                                            && kill.Timestamp >= startTime
                                         select kill).Count(),

                        AttackerHeadshots = (from kill in dbContext.Deaths
                                             //where kill.AttackerCharacterId == charactersGroup.Key.AttackerCharacterId
                                                //&& kill.CharacterId == charactersGroup.Key.CharacterId
                                             where kill.AttackerCharacterId == attackerCharacters.Id
                                                && kill.CharacterId == victimCharacters.Id
                                                && kill.Timestamp >= startTime
                                                && kill.IsHeadshot == true
                                             select kill).Count(),

                        VictimKills = (from kill in dbContext.Deaths
                                       //where kill.AttackerCharacterId == charactersGroup.Key.CharacterId
                                          //&& kill.CharacterId == charactersGroup.Key.AttackerCharacterId
                                       where kill.AttackerCharacterId == victimCharacters.Id
                                          && kill.CharacterId == attackerCharacters.Id
                                          && kill.Timestamp >= startTime
                                       select kill).Count(),
                        
                        VictimHeadshots = (from kill in dbContext.Deaths
                                           //where kill.AttackerCharacterId == charactersGroup.Key.CharacterId
                                              //&& kill.CharacterId == charactersGroup.Key.AttackerCharacterId
                                           where kill.AttackerCharacterId == victimCharacters.Id
                                              && kill.CharacterId == attackerCharacters.Id
                                              && kill.Timestamp >= startTime
                                              && kill.IsHeadshot == true
                                           select kill).Count()
                    };

                var allHeadToHeadPlayers = await query
                                                    .AsNoTracking()
                                                    .ToArrayAsync();

                var victims = allHeadToHeadPlayers
                                .GroupBy(p => new { p.AttackerCharacterId, p.VictimCharacterId })
                                .Select(grp => grp.First()) // new PlayerHourlyHeadToHeadSummaryRow()
                                //{
                                //    AttackerCharacterId = grp.Key.AttackerCharacterId,
                                //    AttackerName = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId).Select(p => p.AttackerName).FirstOrDefault(),
                                //    AttackerFactionId = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId).Select(p => p.AttackerFactionId).FirstOrDefault(),
                                    
                                //    VictimCharacterId = grp.Key.VictimCharacterId,
                                //    VictimName = grp.Where(p => p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.VictimName).FirstOrDefault(),
                                //    VictimFactionId = grp.Where(p => p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.VictimFactionId).FirstOrDefault(),

                                //    AttackerKills = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId && p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.AttackerKills).FirstOrDefault(),
                                //    AttackerHeadshots = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId && p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.AttackerHeadshots).FirstOrDefault(),
                                //    VictimKills = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId && p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.VictimKills).FirstOrDefault(),
                                //    VictimHeadshots = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId && p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.VictimHeadshots).FirstOrDefault()
                                //})
                                .OrderByDescending(grp => grp.AttackerKills)
                                .Where(grp => grp.AttackerCharacterId == characterId)
                                .ToArray();

                var nemeses = allHeadToHeadPlayers
                                .GroupBy(p => new { p.AttackerCharacterId, p.VictimCharacterId })
                                .Select(grp => grp.First()) //new PlayerHourlyHeadToHeadSummaryRow()
                                //{
                                //    AttackerCharacterId = grp.Key.AttackerCharacterId,
                                //    AttackerName = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId).Select(p => p.AttackerName).FirstOrDefault(),
                                //    AttackerFactionId = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId).Select(p => p.AttackerFactionId).FirstOrDefault(),

                                //    VictimCharacterId = grp.Key.VictimCharacterId,
                                //    VictimName = grp.Where(p => p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.VictimName).FirstOrDefault(),
                                //    VictimFactionId = grp.Where(p => p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.VictimFactionId).FirstOrDefault(),

                                //    AttackerKills = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId && p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.AttackerKills).FirstOrDefault(),
                                //    AttackerHeadshots = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId && p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.AttackerHeadshots).FirstOrDefault(),
                                //    VictimKills = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId && p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.VictimKills).FirstOrDefault(),
                                //    VictimHeadshots = grp.Where(p => p.AttackerCharacterId == grp.Key.AttackerCharacterId && p.VictimCharacterId == grp.Key.VictimCharacterId).Select(p => p.VictimHeadshots).FirstOrDefault()
                                //})
                                .OrderByDescending(grp => grp.AttackerKills)
                                .Where(grp => grp.VictimCharacterId == characterId)
                                .ToArray();

                return new PlayerHourlyHeadToHeadSummary()
                {
                    PlayerId = characterId,
                    PlayerFactionId = playerFactionId,
                    QueryStartTime = startTime,
                    QueryNowUtc = nowUtc,
                    TopPlayersByKills = victims,
                    TopPlayersByDeaths = nemeses
                };
            }
        }
    }
}
