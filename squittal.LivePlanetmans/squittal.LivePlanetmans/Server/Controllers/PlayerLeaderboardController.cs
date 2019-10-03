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
using System.Diagnostics;

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

            Debug.WriteLine($"Getting Player Leaderboard: worldId={worldId}, {startTime.ToShortTimeString()} - {nowUtc.ToShortTimeString()}");

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<PlayerHourlyStatsData> topPlayersQuery =

                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && death.WorldId == worldId
                    //&& death.AttackerCharacterId != "0"

                    group death by death.AttackerCharacterId into playerGroup

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
                                        where (d.AttackerCharacterId == playerGroup.Key
                                               || d.CharacterId == playerGroup.Key)
                                           && d.Timestamp >= startTime
                                           && d.WorldId == worldId
                                        orderby d.Timestamp descending
                                        select d.ZoneId).FirstOrDefault(),

                        LatestLoginTime = (from login in dbContext.PlayerLogins
                                           where login.CharacterId == playerGroup.Key
                                           orderby login.Timestamp descending
                                           select login.Timestamp).FirstOrDefault(),

                        LatestLogoutTime = (from logout in dbContext.PlayerLogouts
                                           where logout.CharacterId == playerGroup.Key
                                           orderby logout.Timestamp descending
                                           select logout.Timestamp).FirstOrDefault(),

                        LatestDeathEventTime = (from death in dbContext.Deaths
                                                where (death.AttackerCharacterId == playerGroup.Key
                                                      || death.CharacterId == playerGroup.Key)
                                                   && death.Timestamp >= startTime
                                                   && death.WorldId == worldId
                                                orderby death.Timestamp
                                                select death.Timestamp).FirstOrDefault(),

                        QueryStartTime = startTime,

                        QueryNowUtc = nowUtc,

                        Kills = (from k in dbContext.Deaths
                                 where k.AttackerCharacterId == playerGroup.Key
                                    && k.DeathEventType == DeathEventType.Kill
                                    //&& k.AttackerCharacterId != k.CharacterId
                                    //&& k.AttackerFactionId != k.CharacterFactionId
                                    //&& ((k.AttackerFactionId != k.CharacterFactionId)
                                    //    || k.AttackerFactionId == 4 || k.CharacterFactionId == 4     //Nanite Systems
                                    //    || k.CharacterFactionId == null || k.CharacterFactionId == 0)
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
                                        && h.DeathEventType == DeathEventType.Kill
                                        //&& h.AttackerCharacterId != h.CharacterId
                                        //&& h.AttackerFactionId != h.CharacterFactionId
                                        //&& ((k.AttackerFactionId != k.CharacterFactionId)
                                        //    || k.AttackerFactionId == 4 || k.CharacterFactionId == 4     //Nanite Systems
                                        //    || k.CharacterFactionId == null || k.CharacterFactionId == 0)
                                        && h.Timestamp >= startTime
                                        && h.WorldId == worldId
                                     select h).Count(),

                        //TeamKills = (from tk in dbContext.Deaths
                        //             where tk.AttackerCharacterId == playerGroup.Key
                        //                && tk.AttackerCharacterId != tk.CharacterId
                        //                && tk.AttackerFactionId == tk.CharacterFactionId
                        //                && tk.Timestamp >= startTime
                        //                && tk.WorldId == worldId
                        //             select tk).Count(),

                        //Suicides = (from s in dbContext.Deaths
                        //            where s.CharacterId == playerGroup.Key
                        //               && (s.AttackerCharacterId == s.CharacterId
                        //                   || s.AttackerCharacterId == "0")
                        //               && s.Timestamp >= startTime
                        //               && s.WorldId == worldId
                        //            select s).Count()
                    };

                var topPlayers = await topPlayersQuery
                                          .AsNoTracking()
                                          .Where(row => row.PlayerId != "0")
                                          .OrderByDescending(p => p.Kills)
                                          .Take(rows)
                                          .ToArrayAsync();

                // Get Latest Zone Name
                //var zoneList = await _zoneService.GetAllZonesAsync();
                foreach (var player in topPlayers)
                {
                    //player.LatestZoneName = zoneList.FirstOrDefault(z => z.Id == player.LatestZoneId)?.Name ?? string.Empty;

                    if (player.LatestLoginTime != null)
                    {

                        DateTime sessionStartTime = (player.LatestLoginTime ?? startTime); //(playerStats.LatestLoginTime != null) ? (playerStats.LatestLoginTime ?? startTime) : startTime;
                        DateTime sessionEndTime = (player.LatestLogoutTime ?? nowUtc); // (playerStats.LatestLogoutTime != null) ? (playerStats.LatestLogoutTime ?? nowUtc) : nowUtc;

                        if (sessionEndTime <= sessionStartTime)
                        {
                            sessionEndTime = nowUtc;
                        }

                        player.SessionKills = await dbContext.Deaths.CountAsync(death => death.AttackerCharacterId == player.PlayerId
                                                                                      && death.DeathEventType == DeathEventType.Kill
                                                                                      //&& death.CharacterId != player.PlayerId
                                                                                      //&& ((death.AttackerFactionId != death.CharacterFactionId)
                                                                                      //    || death.AttackerFactionId == 4 || death.CharacterFactionId == 4
                                                                                      //    || death.CharacterFactionId == null || death.CharacterFactionId == 0) //Nanite Systems
                                                                                      && death.Timestamp >= sessionStartTime
                                                                                      && death.Timestamp <= sessionEndTime);
                    }
                }

            return topPlayers;
            }
        }
    }
}
