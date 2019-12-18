using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Server.Services.Planetside;
using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace squittal.LivePlanetmans.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayerLeaderboardController : ControllerBase
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly ICharacterService _characterService;

        private readonly MemoryCache _loginCache;

        public IList<PlayerHourlyStatsData> Players { get; private set; }

        public PlayerLeaderboardController(IDbContextHelper dbContextHelper, ICharacterService characterService, PlayerLoginMemoryCache loginCache)
        {
            _dbContextHelper = dbContextHelper;
            _characterService = characterService;

            _loginCache = loginCache.Cache;

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
                                        && h.Timestamp >= startTime
                                        && h.WorldId == worldId
                                     select h).Count(),
                    };

                var topPlayers = await topPlayersQuery
                                          .AsNoTracking()
                                          .Where(row => row.PlayerId != "0")
                                          .OrderByDescending(p => p.Kills)
                                          .Take(rows)
                                          .ToArrayAsync();

                // Get Latest Zone Name
                foreach (var player in topPlayers)
                {
                    var resolveLoginTimeTask = TryGetPlayerLastLoginTime(player.PlayerId, player.LatestLoginTime);
                    player.LatestLoginTime = await resolveLoginTimeTask;

                    player.SetSessionTimes();

                    if (player.LatestLoginTime != null)
                    {
                        var sessionStartTime = player.SessionTimes.StartTime;
                        var sessionEndTime = player.SessionTimes.EndTime;

                        //DateTime sessionStartTime = (player.LatestLoginTime ?? startTime);
                        //DateTime sessionEndTime = (player.LatestLogoutTime ?? nowUtc);

                        //if (sessionEndTime <= sessionStartTime)
                        //{
                        //    sessionEndTime = nowUtc;
                        //}

                        player.SessionKills = await dbContext.Deaths.CountAsync(death => death.AttackerCharacterId == player.PlayerId
                                                                                      && death.DeathEventType == DeathEventType.Kill
                                                                                      && death.Timestamp >= sessionStartTime
                                                                                      && death.Timestamp <= sessionEndTime);
                    }
                }

                return topPlayers;
            }
        }

        private async Task<DateTime?> TryGetPlayerLastLoginTime(string characterId, DateTime? storeLoginTime)
        {
            var loginKey = PlayerLoginMemoryCache.GetPlayerLoginKey(characterId);
            
            if (!_loginCache.TryGetValue(loginKey, out DateTime cacheEntry))
            {
                var resolvedLoginTime = await ResolvePlayerLastLoginTime(characterId, storeLoginTime);

                if (resolvedLoginTime == null)
                {
                    return null;
                }

                cacheEntry = (DateTime)resolvedLoginTime;

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSize(1)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(15));

                _loginCache.Set(loginKey, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        private async Task<DateTime?> ResolvePlayerLastLoginTime(string characterId, DateTime? storeLoginTime)
        {
            var censusTimes = await _characterService.GetCharacterTimesAsync(characterId);

            if (censusTimes == null)
            {
                return storeLoginTime;
            }

            var censusLoginTime = censusTimes.LastLoginDate;

            if (censusLoginTime == default)
            {
                return storeLoginTime;
            }
            if (storeLoginTime == null)
            {
                return censusLoginTime;
            }

            return (censusLoginTime > storeLoginTime) ? censusLoginTime : storeLoginTime;
        }
    }
}
