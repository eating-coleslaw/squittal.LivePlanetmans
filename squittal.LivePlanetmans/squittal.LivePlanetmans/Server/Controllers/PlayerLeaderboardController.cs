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


        [HttpGet("test/{worldId}")]
        public async Task<IEnumerable<PlayerHourlyStatsData>> GetWorldKillLeaderboard(int worldId)
        {
            int rows = 20;

            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            var topPlayerKillCounts = await GetTopPlayerKillCounts(worldId, startTime, rows);

            //var topPlayerKillStats = await GetTopPlayerKillStats(topPlayerKillCounts, startTime);
            //var topPlayerDeathStats = await GetTopPlayerDeathStats(topPlayerKillCounts, startTime);
            //var topPlayerDetails = await GetTopPlayerDetails(topPlayerKillCounts);

            var playerLeaderboard = new List<PlayerHourlyStatsData>();

            foreach (var player in topPlayerKillCounts)
            {
                //var details = topPlayerDetails.FirstOrDefault(character => character.PlayerId == player.PlayerId);
                //var killStats = topPlayerKillStats.FirstOrDefault(stats => stats.PlayerId == player.PlayerId);
                //var deathStats = topPlayerDeathStats.FirstOrDefault(stats => stats.PlayerId == player.PlayerId);

                playerLeaderboard.Add(new PlayerHourlyStatsData
                    {
                        PlayerId = player.PlayerId, //details.PlayerId,
                        //PlayerName = details.PlayerName,
                        //FactionId = details.FactionId,
                        //BattleRank = details.BattleRank,
                        //PrestigeLevel = details.PrestigeLevel,
                        //OutfitAlias = details.OutfitAlias,
                        Kills = player.Kills //killStats.Kills,
                        //Headshots = killStats.Headshots,
                        //TeamKills = killStats.Teamkills,
                        //LatestZoneId = killStats.LatestZoneId,
                        //Deaths = deathStats.Deaths,
                        //Suicides = deathStats.Suicides,
                    });
            }

            //playerLeaderboard = await ResolveLatestZoneNames(playerLeaderboard);

            return playerLeaderboard
                        .OrderByDescending(player => player.Kills)
                        .ToArray();

        }

        private async Task<IEnumerable<PlayerKillCount>> GetTopPlayerKillCounts(int worldId, DateTime startTime, int rows = 20)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                //var topPlayersQuery  =

                    return await (from death in dbContext.Deaths
                      where death.WorldId == worldId
                         && death.Timestamp >= startTime
                         && death.AttackerCharacterId != death.CharacterId
                         && death.AttackerCharacterId != "0"

                      group death by death.AttackerCharacterId into attackerGroup

                      join kills in dbContext.Deaths on attackerGroup.Key equals kills.AttackerCharacterId

                      //let count = attackerGroup.Key.Count()

                      //where count > 0

                    select new PlayerKillCount()
                    {
                        PlayerId = kills.AttackerCharacterId,
                        Kills = kills.AttackerCharacterId.Count() //count
                    }).OrderByDescending(p => p.Kills)
                                .Take(rows)
                                .ToListAsync(); ;

                //return await topPlayersQuery
                //                //.AsNoTracking()
                //                .OrderByDescending(p => p.Kills)
                //                .Take(rows)
                //                .ToListAsync();
            }
        }

        private async Task<IEnumerable<PlayerKillStats>> GetTopPlayerKillStats(IEnumerable<PlayerKillCount> playerKillCounts, DateTime startTime)
        {
            var playerIds = playerKillCounts.Select(p => p.PlayerId).ToList();

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<PlayerKillStats> killStatsQuery = 

                    //from playerId in playerIds

                    //join death in dbContext.Deaths on playerId equals death.AttackerCharacterId into playerGroup
                    //    from death in playerGroup
                    from death in dbContext.Deaths

                      where playerIds.Contains(death.AttackerCharacterId)
                         && death.AttackerCharacterId != death.CharacterId
                         && death.Timestamp >= startTime

                      group death by death.AttackerCharacterId into playerGroup

                    select new PlayerKillStats()
                    {
                        PlayerId = playerGroup.Key,
                        Kills = playerGroup.Count(k => (k.AttackerFactionId != k.CharacterFactionId)
                                                       || k.AttackerFactionId == 4 || k.CharacterFactionId == 4 //Nanite Systems
                                                       || k.CharacterFactionId == null || k.CharacterFactionId == 0),
                        Headshots = playerGroup.Count(h => h.IsHeadshot
                                                        && ((h.AttackerFactionId != h.CharacterFactionId)
                                                            || h.AttackerFactionId == 4 || h.CharacterFactionId == 4 //Nanite Systems
                                                            || h.CharacterFactionId == null || h.CharacterFactionId == 0)),
                        Teamkills = playerGroup.Count(tk => tk.AttackerFactionId == tk.CharacterFactionId),
                        LatestZoneId = playerGroup.OrderByDescending(d => d.Timestamp).Select(d => d.ZoneId).FirstOrDefault()
                        //(from d in playerGroup
                                          //group d by d.ZoneId into zoneGroup
                                        //select new { ZoneId = zoneGroup.Key, Timestamp = zoneGroup.Max(t => t.Timestamp) }
                                       //).OrderByDescending(grp => grp.Timestamp).Select(grp => grp.ZoneId).FirstOrDefault()
                    };

                return await killStatsQuery
                                .AsNoTracking()
                                .OrderByDescending(p => p.Kills)
                                .ToListAsync();
            }
        }

        private async Task<IEnumerable<PlayerDeathStats>> GetTopPlayerDeathStats(IEnumerable<PlayerKillCount> playerKillCounts, DateTime startTime)
        {
            var playerIds = playerKillCounts.Select(p => p.PlayerId).ToList();

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<PlayerDeathStats> deathStatsQuery =

                    from death in dbContext.Deaths

                    where playerIds.Contains(death.CharacterId)
                       && death.Timestamp >= startTime

                    group death by death.CharacterId into playerGroup

                    select new PlayerDeathStats()
                    {
                        PlayerId = playerGroup.Key,
                        Deaths = playerGroup.Count(),
                        Suicides = playerGroup.Count(d => d.AttackerCharacterId == playerGroup.Key
                                                       || d.AttackerCharacterId == "0")
                    };

                return await deathStatsQuery
                                .AsNoTracking()
                                .ToListAsync();
            }
        }

        private async Task<IEnumerable<PlayerLeaderboardDetails>> GetTopPlayerDetails(IEnumerable<PlayerKillCount> playerKillCounts)
        {
            var playerIds = playerKillCounts.Select(p => p.PlayerId).ToList();

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<PlayerLeaderboardDetails> playerDetailsQuery =

                    from character in dbContext.Characters

                      where playerIds.Contains(character.Id)
                      
                      join outfitMember in dbContext.OutfitMembers on character.Id equals outfitMember.CharacterId into outfitMemberQ
                        from outfitMember in outfitMemberQ.DefaultIfEmpty()
                      
                      join outfit in dbContext.Outfits on outfitMember.OutfitId equals outfit.Id into outfitQ
                        from outfit in outfitQ.DefaultIfEmpty()

                    select new PlayerLeaderboardDetails()
                    {
                        PlayerId = character.Id,
                        PlayerName = character.Name,
                        FactionId = character.FactionId,
                        BattleRank = character.BattleRank,
                        PrestigeLevel = character.PrestigeLevel,
                        OutfitAlias = outfit.Alias ?? string.Empty
                    };

                return await playerDetailsQuery
                                .AsNoTracking()
                                .ToListAsync();
            }
        }

        private async Task<List<PlayerHourlyStatsData>> ResolveLatestZoneNames(List<PlayerHourlyStatsData> players)
        {
            var zoneList = await _zoneService.GetAllZonesAsync();
            foreach (var player in players)
            {
                player.LatestZoneName = zoneList.FirstOrDefault(z => z.Id == player.LatestZoneId)?.Name ?? string.Empty;
            }

            return players;
        }


        private class PlayerKillCount
        {
            public string PlayerId { get; set; }
            public int Kills { get; set; }
        }

        private class PlayerKillStats
        {
            public string PlayerId { get; set; }
            public int Kills { get; set; }
            public int Headshots { get; set; }
            public int Teamkills { get; set; }
            public int LatestZoneId { get; set; }
        }

        private class PlayerDeathStats
        {
            public string PlayerId { get; set; }
            public int Deaths { get; set; }
            public int Suicides { get; set; }
        }

        private class PlayerLeaderboardDetails
        {
            public string PlayerId { get; set; }
            public string PlayerName { get; set; }
            public int FactionId { get; set; }
            public int WorldId { get; set; }
            public int BattleRank { get; set; }
            public int PrestigeLevel { get; set; }
            public string OutfitAlias { get; set; } = string.Empty;
        }

        private class PlayerLatestKillZone
        {
            public string PlayerId { get; set; }
            public int LatestZoneId { get; set; }
            public string LatestZoneName { get; set; }
        }
    }
}
