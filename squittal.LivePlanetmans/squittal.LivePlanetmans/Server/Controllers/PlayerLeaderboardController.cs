using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squittal.LivePlanetmans.Server.Data;
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
        private readonly ILogger<PlayerLeaderboardController> _logger;

        public IList<PlayerHourlyStatsData> Players { get; private set; }

        public PlayerLeaderboardController(IDbContextHelper dbContextHelper, ILogger<PlayerLeaderboardController> logger)
        {
            _dbContextHelper = dbContextHelper;
            _logger = logger;
        }

        [HttpGet("{worldId}")]
        public async Task<ActionResult<IEnumerable<PlayerHourlyStatsData>>> GetPlayerLeaderboardAsync(int worldId) //, int rows = 20)
        {
            int rows = 20;
            
            DateTime nowUTC = DateTime.UtcNow;
            TimeSpan hourSpan = new TimeSpan(0, 0, 0, 3600);
            //TimeSpan minuteSpan = new TimeSpan(0, 0, 5, 0);
            //DateTime startTime = nowUTC.Subtract(hourSpan);

            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            //string testPlayerId = "5428059527803872017";

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<PlayerHourlyStatsData> query =
                    from death in dbContext.Deaths

                    join character in dbContext.Characters on death.AttackerCharacterId equals character.Id into characterQ
                    from character in characterQ.DefaultIfEmpty()

                    where death.Timestamp >= startTime && death.WorldId == worldId
                    group death by death.AttackerCharacterId into playerGroup
                    select new PlayerHourlyStatsData()
                    {
                        PlayerId = playerGroup.Key,
                        PlayerName = (from c in dbContext.Characters
                                      where c.Id == playerGroup.Key
                                      select c.Name).FirstOrDefault(),
                        FactionId = (from c in dbContext.Characters
                                     where c.Id == playerGroup.Key
                                     select c.FactionId).FirstOrDefault(),
                        Kills = (from k in dbContext.Deaths
                                 where k.AttackerCharacterId == playerGroup.Key
                                    && k.AttackerCharacterId != k.CharacterId
                                    && k.Timestamp >= startTime
                                    && k.WorldId == worldId
                                 select k).Count(),
                        Deaths = (from d in dbContext.Deaths
                                  where d.CharacterId == playerGroup.Key
                                     && d.Timestamp >= startTime
                                     && d.WorldId == worldId
                                  select d).Count()
                    };

                return await query
                    .AsNoTracking()
                    .OrderByDescending(p => p.Kills)
                    .Take(rows)
                    .ToArrayAsync();
            }
        }
    }
}
