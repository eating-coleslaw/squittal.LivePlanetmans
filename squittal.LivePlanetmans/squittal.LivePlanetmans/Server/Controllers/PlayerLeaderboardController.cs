﻿using Microsoft.AspNetCore.Mvc;
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
    [Route("api/[controller]/")]
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerHourlyStatsData>>> GetPlayerLeaderboardAsync(int rows = 20)
        {
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

                    where death.Timestamp >= startTime && death.WorldId == 10
                    group death by death.AttackerCharacterId into playerGroup
                    select new PlayerHourlyStatsData()
                    {
                        PlayerName = (from c in dbContext.Characters
                                      where c.Name == playerGroup.Key
                                      select c.Name).FirstOrDefault(),
                        PlayerId = playerGroup.Key,
                        Kills = (from k in dbContext.Deaths
                                 where k.AttackerCharacterId == playerGroup.Key
                                    && k.AttackerCharacterId != k.CharacterId
                                    && k.Timestamp >= startTime
                                    && k.WorldId == 10
                                 select k).Count(),
                        Deaths = (from d in dbContext.Deaths
                                  where d.CharacterId == playerGroup.Key
                                     && d.Timestamp >= startTime
                                     && d.WorldId == 10
                                  select d).Count()
                    };

                //IQueryable<PlayerHourlyStatsData> query =
                //    from death in dbContext.Deaths
                //    //where death.Timestamp >= startTime
                //    select new PlayerHourlyStatsData()
                //    {
                //        PlayerId = testPlayerId,
                //        Kills = (from k in dbContext.Deaths
                //                 where k.AttackerCharacterId == testPlayerId && k.AttackerCharacterId != k.CharacterId
                //                 select k).Count(),
                //        Deaths = (from d in dbContext.Deaths
                //                  where d.CharacterId == testPlayerId && d.AttackerCharacterId != d.CharacterId
                //                  select d).Count()
                //    };

                return await query
                    .AsNoTracking()
                    .OrderByDescending(p => p.Kills)
                    .Take(rows)
                    .ToArrayAsync();
            }
        }
    }
}
