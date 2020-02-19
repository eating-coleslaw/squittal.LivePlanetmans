using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Server.Services.Planetside;
using squittal.LivePlanetmans.Shared.Models;
using System;
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

        [HttpGet("h2h/{characterId}")]
        public async Task<ActionResult<HourlyPlayerHeadToHeadReport>> GetHourlyPlayerHeadToHeadReportAsync(string characterId)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            var queryTimes = new DbQueryTimes()
            {
                QueryNowUtc = nowUtc,
                QueryStartTimeUtc = startTime
            };

            var characterEntity = await _characterService.GetCharacterAsync(characterId);

            if (characterEntity == null)
            {
                return null;
            }

            var playerFactionId = characterEntity.FactionId;


            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<HeadToHeadSummaryRow> query =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && (death.AttackerCharacterId == characterId
                            || death.CharacterId == characterId)
                       && death.DeathEventType != DeathEventType.Suicide

                    join character in dbContext.Characters
                      on death.CharacterId equals character.Id into victimCharactersQ
                    from victimCharacters in victimCharactersQ.DefaultIfEmpty()

                    join character in dbContext.Characters
                      on death.AttackerCharacterId equals character.Id into attackerCharactersQ
                    from attackerCharacters in attackerCharactersQ.DefaultIfEmpty()

                    select new HeadToHeadSummaryRow()
                    {
                        AttackerDetails = new PlayerDetails()
                        {
                            PlayerId = attackerCharacters.Id,
                            PlayerName = attackerCharacters.Name,
                            FactionId = attackerCharacters.FactionId,
                            BattleRank = attackerCharacters.BattleRank,
                            PrestigeLevel = attackerCharacters.PrestigeLevel
                        },

                        VictimDetails = new PlayerDetails()
                        {
                            PlayerId = victimCharacters.Id,
                            PlayerName = victimCharacters.Name,
                            FactionId = victimCharacters.FactionId,
                            BattleRank = victimCharacters.BattleRank,
                            PrestigeLevel = victimCharacters.PrestigeLevel,

                            OutfitAlias = (from m in dbContext.OutfitMembers
                                           join o in dbContext.Outfits on m.OutfitId equals o.Id
                                           where m.CharacterId == victimCharacters.Id
                                           select o.Alias).FirstOrDefault(),
                        },

                        AttackerStats = new DeathEventAggregate()
                        {
                            Kills = (from kill in dbContext.Deaths
                                             where kill.AttackerCharacterId == attackerCharacters.Id
                                                && kill.CharacterId == victimCharacters.Id
                                                && kill.Timestamp >= startTime
                                             select kill).Count(),

                            Headshots = (from kill in dbContext.Deaths
                                                 where kill.AttackerCharacterId == attackerCharacters.Id
                                                    && kill.CharacterId == victimCharacters.Id
                                                    && kill.Timestamp >= startTime
                                                    && kill.IsHeadshot == true
                                                 select kill).Count(),

                            Deaths = (from kill in dbContext.Deaths
                                      where kill.CharacterId == attackerCharacters.Id
                                         && kill.AttackerCharacterId == victimCharacters.Id
                                         && kill.Timestamp >= startTime
                                      select kill).Count(),

                            HeadshotDeaths = (from kill in dbContext.Deaths
                                              where kill.CharacterId == attackerCharacters.Id
                                                 && kill.AttackerCharacterId == victimCharacters.Id
                                                 && kill.Timestamp >= startTime
                                                 && kill.IsHeadshot == true
                                              select kill).Count(),
                        }
                    };

                var allHeadToHeadPlayers = await query
                                                    .AsNoTracking()
                                                    .Where(h2h => h2h.AttackerDetails.PlayerId == characterId
                                                                  || h2h.VictimDetails.PlayerId == characterId)
                                                    .Distinct()
                                                    .ToArrayAsync();

                var allPlayerIsAttackerH2H = allHeadToHeadPlayers
                                                .Where(h2h => h2h.AttackerDetails.PlayerId == characterId)
                                                .ToArray();
                                                
                var allPlayerIsVictimH2H = allHeadToHeadPlayers
                                                .Where(h2h => h2h.VictimDetails.PlayerId == characterId)
                                                .ToArray();

                var attackerPlayerH2HSummaries = allPlayerIsAttackerH2H
                                                    .GroupBy(row => new { AttackerCharacterId = row.AttackerDetails.PlayerId, VictimCharacterId = row.VictimDetails.PlayerId })
                                                    .Select(grp => grp.First())
                                                    .Select(grp => new PlayerHeadToHeadSummaryRow()
                                                    {
                                                        EnemyDetails = grp.VictimDetails,
                                                        PlayerStats= grp.AttackerStats,
                                                        EnemyStats = new DeathEventAggregate()
                                                        {
                                                            Kills = grp.AttackerStats.Deaths,
                                                            Headshots = grp.AttackerStats.HeadshotDeaths,
                                                            Deaths = grp.AttackerStats.Kills,
                                                            HeadshotDeaths = grp.AttackerStats.Headshots
                                                        }
                                                    })
                                                    .ToList();

                var victimPlayerH2HSummaries = allPlayerIsVictimH2H
                                                .GroupBy(row => new { AttackerCharacterId = row.AttackerDetails.PlayerId, VictimCharacterId = row.VictimDetails.PlayerId })
                                                .Select(grp => grp.First())
                                                .Select(grp => new PlayerHeadToHeadSummaryRow()
                                                {
                                                    EnemyDetails = grp.AttackerDetails,
                                                    EnemyStats = grp.AttackerStats,
                                                    PlayerStats = new DeathEventAggregate()
                                                    {
                                                        Kills = grp.AttackerStats.Deaths,
                                                        Headshots = grp.AttackerStats.HeadshotDeaths,
                                                        Deaths = grp.AttackerStats.Kills,
                                                        HeadshotDeaths = grp.AttackerStats.Headshots
                                                    }
                                                })
                                                .ToList();

                var allPlayerH2HSummaries = attackerPlayerH2HSummaries
                                            .Union(victimPlayerH2HSummaries)
                                            .ToArray();

                var playerDetails = allHeadToHeadPlayers.Any(h2h => h2h.AttackerDetails.PlayerId == characterId)
                                        ? allHeadToHeadPlayers
                                            .Where(h2h => h2h.AttackerDetails.PlayerId == characterId)
                                            .Select(h2h => h2h.AttackerDetails)
                                            .FirstOrDefault()
                                        : allHeadToHeadPlayers
                                            .Where(h2h => h2h.VictimDetails.PlayerId == characterId)
                                            .Select(h2h => h2h.VictimDetails)
                                            .FirstOrDefault();

                return new HourlyPlayerHeadToHeadReport(playerDetails, allPlayerH2HSummaries, queryTimes);
            }
        }
    }
}
