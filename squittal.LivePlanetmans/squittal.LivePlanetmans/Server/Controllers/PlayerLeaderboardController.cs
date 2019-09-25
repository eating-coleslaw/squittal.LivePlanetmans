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
    public class PlayerLeaderboardController : ControllerBase
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly IZoneService _zoneService;
        private readonly ILogger<PlayerLeaderboardController> _logger;

        public IList<PlayerHourlyStatsData> Players { get; private set; }

        public PlayerLeaderboardController(IDbContextHelper dbContextHelper, IZoneService zoneService, ILogger<PlayerLeaderboardController> logger)
        {
            _dbContextHelper = dbContextHelper;
            _zoneService = zoneService;
            _logger = logger;
        }

        [HttpGet("{worldId}")]
        public async Task<ActionResult<IEnumerable<PlayerHourlyStatsData>>> GetPlayerLeaderboardAsync(int worldId)
        {
            int rows = 20;
            
            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                // Get all zone names


                var topPlayersQuery =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && death.WorldId == worldId
                       && death.AttackerCharacterId != "0"

                    group death by death.AttackerCharacterId into playerGroup

                    //join character in dbContext.Characters on playerGroup.Key equals character.Id into charactersQ
                    //from character in charactersQ.DefaultIfEmpty()

                    select new PlayerHourlyStatsData()
                    {
                        PlayerId = playerGroup.Key,
                        PlayerName = (from c in dbContext.Characters
                                      where c.Id == playerGroup.Key
                                      select c.Name).FirstOrDefault(),
                        OutfitAlias = (from m in dbContext.OutfitMembers
                                       join o in dbContext.Outfits on m.OutfitId equals o.Id
                                       where m.CharacterId == playerGroup.Key
                                       select o.Alias).FirstOrDefault(),
                        FactionId = (from c in dbContext.Characters
                                     where c.Id == playerGroup.Key
                                     select c.FactionId).FirstOrDefault(),
                        BattleRank = (from c in dbContext.Characters
                                      where c.Id == playerGroup.Key
                                      select c.BattleRank).FirstOrDefault(),
                        PrestigeLevel = (from c in dbContext.Characters
                                         where c.Id == playerGroup.Key
                                         select c.PrestigeLevel).FirstOrDefault(),

                        LatestZoneId = (from d in dbContext.Deaths
                                        where d.AttackerCharacterId == playerGroup.Key
                                           && d.Timestamp >= startTime
                                           && d.WorldId == worldId
                                        group d by d.ZoneId into g
                                        select new { ZoneId = g.Key, Timestamp = g.Max(t => t.Timestamp) }
                                        ).OrderByDescending(t => t.Timestamp).Select(t => t.ZoneId).FirstOrDefault(),

                        //LatestZoneName = (from zoneIdTimes in
                        //                    (from d in dbContext.Deaths
                        //                     where d.AttackerCharacterId == playerGroup.Key
                        //                        && d.Timestamp >= startTime
                        //                        && d.WorldId == worldId
                        //                     group d by d.ZoneId into g
                        //                     select new { ZoneId = g.Key, Timestamp = g.Max(t => t.Timestamp) })

                        //                  join zone in dbContext.Zones on zoneIdTimes.ZoneId equals zone.Id into zonesQ
                        //                  from zone in zonesQ.DefaultIfEmpty()

                        //                  select new { zoneIdTimes.ZoneId, zone.Name, zoneIdTimes.Timestamp }
                        //                ).OrderByDescending(t => t.Timestamp).Select(t => t.Name).FirstOrDefault(),

                        Kills = (from k in dbContext.Deaths
                                 where k.AttackerCharacterId == playerGroup.Key
                                    && k.AttackerCharacterId != k.CharacterId
                                    && ((k.AttackerFactionId != k.CharacterFactionId) || k.AttackerFactionId == 4 || k.CharacterFactionId == 4 || k.CharacterFactionId == null || k.CharacterFactionId == 0) //Nanite Systems
                                    && k.Timestamp >= startTime
                                    && k.WorldId == worldId
                                 select k).Count(),

                        Deaths = (from d in dbContext.Deaths
                                  where d.CharacterId == playerGroup.Key
                                     && d.Timestamp >= startTime
                                     && d.WorldId == worldId
                                  select d).Count(),

                        Headshots = (from h in dbContext.Deaths
                                     where h.IsHeadshot == true
                                        && h.AttackerCharacterId == playerGroup.Key
                                        && h.AttackerCharacterId != h.CharacterId
                                        && ((h.AttackerFactionId != h.CharacterFactionId)
                                            || h.AttackerFactionId == 4 || h.CharacterFactionId == 4  //Nanite Systems
                                            || h.CharacterFactionId == null || h.CharacterFactionId == 0)
                                        && h.Timestamp >= startTime
                                        && h.WorldId == worldId
                                     select h).Count(),

                        TeamKills = (from tk in dbContext.Deaths
                                     where tk.AttackerCharacterId == playerGroup.Key
                                        && tk.AttackerCharacterId != tk.CharacterId
                                        && tk.AttackerFactionId == tk.CharacterFactionId
                                        && tk.Timestamp >= startTime
                                        && tk.WorldId == worldId
                                     select tk).Count(),
                        Suicides = (from s in dbContext.Deaths
                                    where s.CharacterId == playerGroup.Key
                                       && s.AttackerCharacterId == s.CharacterId
                                       && s.Timestamp >= startTime
                                       && s.WorldId == worldId
                                    select s).Count()
                    };

                var topPlayers = await topPlayersQuery
                                          .AsNoTracking()
                                          .OrderByDescending(p => p.Kills)
                                          .Take(rows)
                                          .ToArrayAsync();

                //var characterIds = topPlayers
                //                    .GroupBy(player => player.PlayerId)
                //                    .Select(grp => grp.First().PlayerId)
                //                    .ToList();


                //IQueryable<PlayerHourlyStatsData> leaderboardQuery =

                //    from death in dbContext.Deaths

                //      where ( characterIds.Contains(death.AttackerCharacterId)
                //              || characterIds.Contains(death.CharacterId) )
                //         && death.Timestamp >= startTime
                //         && death.WorldId == worldId
                    
                //      group death by new { AttackerKey = death.AttackerCharacterId, VictimKey = death.CharacterId } into playerGroup

                //      from character in dbContext.Characters

                //      where characterIds.Contains(character.Id)
                //         && character.Id == playerGroup.Key.AttackerKey

                //      //join outfitMember in dbContext.OutfitMembers on character.Id equals outfitMember.CharacterId into outfitMemberQ
                //      //  from outfitMember in outfitMemberQ.DefaultIfEmpty()

                //      //join outfit in dbContext.Outfits on outfitMember.OutfitId equals outfit.Id into outfitQ
                //      //  from outfit in outfitQ.DefaultIfEmpty()

                //      //join kill in dbContext.Deaths on character.Id equals kill.AttackerCharacterId OR into killQ
                //        //from kill in killQ.DefaultIfEmpty()

                //      //join death in dbContext.Deaths on character.Id equals death.CharacterId into deathQ
                //      //  from death in deathQ.DefaultIfEmpty()

                //      //where kill.Timestamp >= startTime
                //         //&& death.Timestamp >= startTime

                //    select new PlayerHourlyStatsData() { 
                        
                //        PlayerId = character.Id,
                //        PlayerName = character.Name,
                //        //OutfitAlias = outfit.Alias,
                //        FactionId = character.FactionId,
                //        BattleRank = character.BattleRank,
                //        PrestigeLevel = character.PrestigeLevel,

                //        LatestZoneId = (from d in dbContext.Deaths
                //                        where d.AttackerCharacterId == character.Id
                //                           && d.Timestamp >= startTime
                //                           && d.WorldId == worldId
                //                        group d by d.ZoneId into g
                //                        select new { ZoneId = g.Key, Timestamp = g.Max(t => t.Timestamp) }
                //                        ).OrderByDescending(t => t.Timestamp).Select(t => t.ZoneId).FirstOrDefault(),

                //        Kills = playerGroup.Count(k => playerGroup.Key.AttackerKey == character.Id
                //                                    && k.AttackerCharacterId == character.Id
                //                                    && k.AttackerCharacterId != k.CharacterId
                //                                    && ((k.AttackerFactionId != k.CharacterFactionId)
                //                                         || k.AttackerFactionId == 4 || k.CharacterFactionId == 4  //Nanite Systems
                //                                         || k.CharacterFactionId == null || k.CharacterFactionId == 0)),
                //                //leaderboard.FirstOrDefault(l => l.PlayerId == character.Id).Kills,

                //        Deaths = playerGroup.Count(d => playerGroup.Key.VictimKey == character.Id),

                //        Headshots = playerGroup.Count(h => h.IsHeadshot == true
                //                                        && playerGroup.Key.AttackerKey == character.Id
                //                                        && h.AttackerCharacterId == character.Id
                //                                        && h.AttackerCharacterId != h.CharacterId
                //                                        && ((h.AttackerFactionId != h.CharacterFactionId)
                //                                            || h.AttackerFactionId == 4 || h.CharacterFactionId == 4  //Nanite Systems
                //                                            || h.CharacterFactionId == null || h.CharacterFactionId == 0)),

                //        TeamKills = playerGroup.Count(tk => playerGroup.Key.AttackerKey == character.Id
                //                                         && tk.AttackerCharacterId == character.Id
                //                                         && tk.AttackerCharacterId != tk.CharacterId
                //                                         && tk.AttackerFactionId == tk.CharacterFactionId),

                //        Suicides = playerGroup.Count(s => playerGroup.Key.VictimKey == character.Id
                //                                       && playerGroup.Key.AttackerKey == character.Id
                //                                       && s.AttackerCharacterId == s.CharacterId)
                //    };


                //var leaderboard = await leaderboardQuery
                //                            .AsNoTracking()
                //                            .GroupBy(p => p.PlayerId)
                //                            .Select(grp => grp.First())
                //                            .OrderByDescending(p => p.Kills)
                //                            .ToArrayAsync();


              // Get Latest Zone Name
                var zoneList = await _zoneService.GetAllZonesAsync();
                foreach (var player in topPlayers)
                {
                    player.LatestZoneName = zoneList.FirstOrDefault(z => z.Id == player.LatestZoneId)?.Name ?? string.Empty;
                }

                return topPlayers;
            }
        }
    }
}
