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
    public class PlayerHourlyLoadoutsController : ControllerBase
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly IProfileService _profileService;
        private readonly ICharacterService _characterService;

        private IEnumerable<Loadout> _loadouts;

        public PlayerHourlyLoadoutsController(IDbContextHelper dbContextHelper, IProfileService profileService, ICharacterService characterService)
        {
            _dbContextHelper = dbContextHelper;
            _profileService = profileService;
            _characterService = characterService;
        }

        [HttpGet("models/loadouts")]
        public async Task<ActionResult<IEnumerable<Loadout>>> GetAllLoadoutModelsAsync()
        {
            var modelsTask = _profileService.GetAllLoadoutsAsync();
            _loadouts = await modelsTask;

            return _loadouts.ToArray();
        }

        [HttpGet("loadouts/{characterId}")]
        public async Task<ActionResult<PlayerHourlyLoadoutsSummary>> GetHourlyLoadoutsAsync(string characterId)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            var character = await _characterService.GetCharacterAsync(characterId);

            if (character == null)
            {
                return null;
            }

            var playerFactionId = character.FactionId;


            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<LoadoutVsLoadoutSummaryRow> query =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && (death.AttackerCharacterId == characterId
                            || death.CharacterId == characterId)
                       && death.DeathEventType == DeathEventType.Kill

                    //group death by new { death.AttackerCharacterId, death.CharacterId } into charactersGroup

                    join loadout in dbContext.Loadouts
                      //on charactersGroup.Key.CharacterId equals character.Id into victimCharactersQ
                      on death.CharacterLoadoutId equals loadout.Id into victimLoadoutsQ
                    from victimLoadouts in victimLoadoutsQ.DefaultIfEmpty()

                    join loadout in dbContext.Loadouts
                      //on charactersGroup.Key.AttackerCharacterId equals character.Id into attackerCharactersQ
                      on death.AttackerLoadoutId equals loadout.Id into attackerLoadoutsQ
                    from attackerLoadouts in attackerLoadoutsQ.DefaultIfEmpty()

                    select new LoadoutVsLoadoutSummaryRow()
                    {
                        AttackerLoadoutId = attackerLoadouts.Id,
                        AttackerLoadoutName = attackerLoadouts.CodeName,
                        AttackerProfileId = attackerLoadouts.ProfileId,
                        AttackerFactionId = attackerLoadouts.FactionId,

                        VictimLoadoutId = victimLoadouts.Id,
                        VictimLoadoutName = victimLoadouts.CodeName,
                        VictimProfileId = victimLoadouts.ProfileId,
                        VictimFactionId = victimLoadouts.FactionId,

                        AttackerStats = new DeathEventAggregate()
                        {
                            Kills = (from kill in dbContext.Deaths
                                     where kill.AttackerCharacterId == characterId
                                        && kill.DeathEventType == DeathEventType.Kill
                                        && kill.AttackerLoadoutId == attackerLoadouts.Id
                                        && kill.CharacterLoadoutId == victimLoadouts.Id
                                        && kill.Timestamp >= startTime
                                     select kill).Count(),

                            Headshots = (from kill in dbContext.Deaths
                                         where kill.AttackerCharacterId == characterId
                                            && kill.DeathEventType == DeathEventType.Kill
                                            && kill.AttackerLoadoutId == attackerLoadouts.Id
                                            && kill.CharacterLoadoutId == victimLoadouts.Id
                                            && kill.Timestamp >= startTime
                                            && kill.IsHeadshot == true
                                         select kill).Count(),

                            Deaths = (from kill in dbContext.Deaths
                                      where kill.CharacterId == characterId
                                         && kill.DeathEventType == DeathEventType.Kill
                                         && kill.CharacterLoadoutId == attackerLoadouts.Id
                                         && kill.AttackerLoadoutId == victimLoadouts.Id
                                         && kill.Timestamp >= startTime
                                      select kill).Count(),

                            HeadshotDeaths = (from kill in dbContext.Deaths
                                              where kill.CharacterId == characterId
                                                 && kill.DeathEventType == DeathEventType.Kill
                                                 && kill.CharacterLoadoutId == attackerLoadouts.Id
                                                 && kill.AttackerLoadoutId == victimLoadouts.Id
                                                 && kill.Timestamp >= startTime
                                                 && kill.IsHeadshot == true
                                              select kill).Count()
                        }
                    };


                var loadoutVsLoadoutRows = await query
                                                    .AsNoTracking()
                                                    .ToArrayAsync();

                var distinctAttackerLoadouts = loadoutVsLoadoutRows
                                                .GroupBy(vsRow => vsRow.AttackerLoadoutId)
                                                .Select(grp => grp.Key)
                                                .Distinct()
                                                .ToArray();


                var attackerLoadoutAggregates = loadoutVsLoadoutRows
                                                    .GroupBy(vsRow => vsRow.AttackerLoadoutId) //new { vsRow.AttackerLoadoutId, vsRow.VictimLoadoutId })
                                                    .Select(grp => new HourlyLoadoutSummaryRow()
                                                    {
                                                        LoadoutId = grp.Key,
                                                        LoadoutName = grp.Select(g => g.AttackerLoadoutName).First(),
                                                        ProfileId = grp.Select(g => g.AttackerProfileId).First(),
                                                        FactionId = grp.Select(g => g.AttackerFactionId).First(),

                                                        LoadoutDeathEventAggregate = new DeathEventAggregate()
                                                        {
                                                            Kills = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.Kills), //  .Where(vGrp => vGrp.  Where(g => g.AttackerLoadoutId == grp.Key).Select(g => g.AttackerDeathEventAggregate.Kills).Sum(),
                                                            Headshots = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.Headshots),
                                                            Deaths = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.Deaths),
                                                            HeadshotDeaths = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.HeadshotDeaths)
                                                        },

                                                        LoadoutVsLoadoutAggregates = grp.Select(g => new VictimLoadoutSummary1()
                                                        {
                                                            VictimLoadoutId = g.VictimLoadoutId,
                                                            VictimLoadoutName = grp.Where(v => v.VictimLoadoutId == g.VictimLoadoutId).Select(v => v.VictimLoadoutName).First(),
                                                            VictimProfileId = grp.Where(v => v.VictimLoadoutId == g.VictimLoadoutId).Select(v => v.VictimProfileId).First(),
                                                            VictimFactionId = grp.Where(v => v.VictimLoadoutId == g.VictimLoadoutId).Select(v => v.VictimFactionId).First(),

                                                            AttackerDeathEventAggregate = new DeathEventAggregate()
                                                            {
                                                                Kills = grp.Where(a => a.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerStats.Kills), // Select(a => a.AttackerDeathEventAggregate.Kills).Sum(),
                                                                Headshots = grp.Where(a => g.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerStats.Headshots), //Select(a => a.AttackerDeathEventAggregate.Headshots).Sum(),
                                                                Deaths = grp.Where(a => g.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerStats.Deaths), //Select(a => a.AttackerDeathEventAggregate.Deaths).Sum(),
                                                                HeadshotDeaths = grp.Where(a => g.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerStats.HeadshotDeaths) //Select(a => a.AttackerDeathEventAggregate.HeadshotDeaths).Sum()
                                                            }
                                                        }).GroupBy(vls => vls.VictimLoadoutId).Select(vlsGrp => vlsGrp.First())
                                                    })
                                                    .Where(grp => grp.FactionId == playerFactionId) // FactionId == playerFactionId)
                                                    .OrderBy(grp => grp.LoadoutId)
                                                    .ThenBy(grp => grp.LoadoutVsLoadoutAggregates.Select(g => g.VictimLoadoutId).First())
                                                    .Distinct()
                                                    .ToArray();

                
                //var finalSummary = attackerLoadoutAggregates
                //                    .GroupBy(l => new { l.LoadoutId, )

                /*
                foreach (var al in attackerLoadoutAggregates)
                {
                    Debug.WriteLine($"*{al.LoadoutName} [{al.LoadoutId}]* [Kills: {al.LoadoutDeathEventAggregate.Kills}  Deaths: {al.LoadoutDeathEventAggregate.Deaths}]");

                    foreach (var vl in al.LoadoutVsLoadoutAggregates)
                    {
                        Debug.WriteLine($"   {vl.VictimLoadoutName} [{vl.VictimLoadoutId}]   Kills: {vl.AttackerDeathEventAggregate.Kills}  Deaths: {vl.AttackerDeathEventAggregate.Deaths}");
                    }
                    Debug.WriteLine($"________________________________");

                }
                */


                //var topByKills = allHeadToHeadPlayers
                //                .GroupBy(p => new { p.AttackerLoadoutId, p.VictimLoadoutId })
                //                .Select(grp => grp.First())
                //                //.OrderByDescending(grp => grp.AttackerKills)
                //                .OrderByDescending(grp => grp.AttackerLoadoutId)
                //                .ThenByDescending(grp => grp.VictimLoadoutId)
                //                .Where(grp => grp.AttackerFactionId == playerFactionId)
                //                .ToArray();

                //var topByDeaths = allHeadToHeadPlayers
                //                .GroupBy(p => new { p.AttackerLoadoutId, p.VictimLoadoutId })
                //                .Select(grp => grp.First())
                //                //.OrderByDescending(grp => grp.AttackerKills)
                //                .OrderByDescending(grp => grp.AttackerLoadoutId)
                //                .ThenBy(grp => grp.VictimLoadoutId)
                //                .Where(grp => grp.VictimFactionId == playerFactionId)
                //                .ToArray();

                //Debug.WriteLine($"{topByKills.Count()}");
                //Debug.WriteLine($"{topByDeaths.Count()}");


                return new PlayerHourlyLoadoutsSummary()
                {
                    PlayerId = characterId,
                    PlayerFactionId = playerFactionId,
                    QueryStartTime = startTime,
                    QueryNowUtc = nowUtc,
                    LoadoutAggregates = attackerLoadoutAggregates
                    //TopLoadoutsByKills = topByKills,
                    //TopLoadoutsByDeaths = topByDeaths
                };
            }
        }

        [HttpGet("h2h/{characterId}")]
        public async Task<ActionResult<PlayerLoadoutsReport>> GetHourlyLoadoutsHeadToHeadAsync(string characterId)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            var character = await _characterService.GetCharacterAsync(characterId);

            if (character == null)
            {
                return null;
            }

            var playerFactionId = character.FactionId;


            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<LoadoutVsLoadoutSummaryRow> query =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && (death.AttackerCharacterId == characterId
                            || death.CharacterId == characterId)
                       && death.DeathEventType == DeathEventType.Kill

                    join loadout in dbContext.Loadouts
                      on death.CharacterLoadoutId equals loadout.Id into victimLoadoutsQ
                    from victimLoadouts in victimLoadoutsQ.DefaultIfEmpty()

                    join loadout in dbContext.Loadouts
                      on death.AttackerLoadoutId equals loadout.Id into attackerLoadoutsQ
                    from attackerLoadouts in attackerLoadoutsQ.DefaultIfEmpty()

                    select new LoadoutVsLoadoutSummaryRow()
                    {
                        AttackerLoadoutId = attackerLoadouts.Id,
                        AttackerLoadoutName = attackerLoadouts.CodeName,
                        AttackerProfileId = attackerLoadouts.ProfileId,
                        AttackerFactionId = attackerLoadouts.FactionId,

                        VictimLoadoutId = victimLoadouts.Id,
                        VictimLoadoutName = victimLoadouts.CodeName,
                        VictimProfileId = victimLoadouts.ProfileId,
                        VictimFactionId = victimLoadouts.FactionId,

                        AttackerStats = new DeathEventAggregate()
                        {
                            Kills = (from kill in dbContext.Deaths
                                     where kill.AttackerCharacterId == characterId
                                        && kill.DeathEventType == DeathEventType.Kill
                                        && kill.AttackerLoadoutId == attackerLoadouts.Id
                                        && kill.CharacterLoadoutId == victimLoadouts.Id
                                        && kill.Timestamp >= startTime
                                     select kill).Count(),

                            Headshots = (from kill in dbContext.Deaths
                                         where kill.AttackerCharacterId == characterId
                                            && kill.DeathEventType == DeathEventType.Kill
                                            && kill.AttackerLoadoutId == attackerLoadouts.Id
                                            && kill.CharacterLoadoutId == victimLoadouts.Id
                                            && kill.Timestamp >= startTime
                                            && kill.IsHeadshot == true
                                         select kill).Count(),

                            Deaths = (from kill in dbContext.Deaths
                                      where kill.CharacterId == characterId
                                         && kill.DeathEventType == DeathEventType.Kill
                                         && kill.CharacterLoadoutId == attackerLoadouts.Id
                                         && kill.AttackerLoadoutId == victimLoadouts.Id
                                         && kill.Timestamp >= startTime
                                      select kill).Count(),

                            HeadshotDeaths = (from kill in dbContext.Deaths
                                              where kill.CharacterId == characterId
                                                 && kill.DeathEventType == DeathEventType.Kill
                                                 && kill.CharacterLoadoutId == attackerLoadouts.Id
                                                 && kill.AttackerLoadoutId == victimLoadouts.Id
                                                 && kill.Timestamp >= startTime
                                                 && kill.IsHeadshot == true
                                              select kill).Count()
                        }
                    };

                var loadoutVsLoadoutRows = await query
                                                    .AsNoTracking()
                                                    .ToArrayAsync();

                var groupedLoadouts = loadoutVsLoadoutRows
                                                .GroupBy(vsRow => new { vsRow.AttackerLoadoutId, vsRow.VictimLoadoutId })
                                                .Select(grp => grp.First())
                                                .ToArray();

                var activePlayerLoadouts = GetActivePlayerLoadouts(loadoutVsLoadoutRows, playerFactionId);
                var activeEnemyLoadouts = GetActiveEnemyLoadouts(loadoutVsLoadoutRows, playerFactionId);
                var activeFactions = GetActiveFactions(loadoutVsLoadoutRows, playerFactionId);
                var activeFactionLoadouts = GetActiveFactionLoadouts(loadoutVsLoadoutRows, activeFactions);


                var allFactionsLoadoutSummaries = new List<FactionLoadoutsSummary>();
                
                foreach (var factionId in activeFactions)
                {
                    var factionLoadouts = activeFactionLoadouts.Where(f => f.FactionId == factionId).SelectMany(f => f.Loadouts);

                    var factionLoadoutSummaries = new List<EnemyLoadoutHeadToHeadSummary>();

                    foreach (var enemyLoadoutId in factionLoadouts)
                    {
                        var h2hPlayerLoadouts = new List<LoadoutSummary>();
                        
                        foreach (var playerLoadoutId in activePlayerLoadouts)
                        {
                            var playerLoadoutProfile = await _profileService.GetProfileFromLoadoutIdAsync(playerLoadoutId);
                            var playerLoadoutName = playerLoadoutProfile.Name;

                            var playerLoadoutSummary = new LoadoutSummary
                            {
                                Details = new LoadoutDetails()
                                {
                                    Id = playerLoadoutId,
                                    FactionId = playerFactionId,
                                    Name = playerLoadoutName
                                },
                                Stats = GetStatsForEnemyLoadoutVsPlayerLoadout(groupedLoadouts, playerLoadoutId, enemyLoadoutId)
                            };

                            h2hPlayerLoadouts.Add(playerLoadoutSummary);
                        }

                        var enemyLoadoutProfile = await _profileService.GetProfileFromLoadoutIdAsync(enemyLoadoutId);
                        var enemyLoadoutName = enemyLoadoutProfile.Name ?? string.Empty;

                        var enemyH2HSummary = new EnemyLoadoutHeadToHeadSummary
                        {
                            Summary = new LoadoutSummary
                            {
                                Details = new LoadoutDetails
                                {
                                    Id = enemyLoadoutId,
                                    FactionId = factionId,
                                    Name = enemyLoadoutName
                                },
                                Stats = GetSummedLoadoutSummaryStats(h2hPlayerLoadouts)
                            },
                            PlayerLoadouts = h2hPlayerLoadouts
                        };

                        factionLoadoutSummaries.Add(enemyH2HSummary);
                    }

                    var factionVsPlayerLoadoutSummaries = new List<LoadoutSummary>();

                    foreach (var playerLoadoutId in activePlayerLoadouts)
                    {
                        var playerLoadoutProfile = await _profileService.GetProfileFromLoadoutIdAsync(playerLoadoutId);
                        var playerLoadoutName = playerLoadoutProfile.Name;

                        var playerLoadoutSummary = new LoadoutSummary
                        {
                            Details = new LoadoutDetails()
                            {
                                Id = playerLoadoutId,
                                FactionId = playerFactionId,
                                Name = playerLoadoutName
                            },
                            Stats = GetStatsForFactionVsPlayerLoadout(groupedLoadouts, playerLoadoutId, factionId)
                        };

                        factionVsPlayerLoadoutSummaries.Add(playerLoadoutSummary);
                    }

                    var factionLoadoutsSummary = new FactionLoadoutsSummary
                    {
                        Summary = new FactionSummary()
                        {
                            Details = new FactionDetails()
                            {
                                Id = factionId
                            },
                            Stats = GetSummedLoadoutSummaryStats(factionVsPlayerLoadoutSummaries)
                        },
                        FactionLoadouts = factionLoadoutSummaries,
                        PlayerLoadouts = factionVsPlayerLoadoutSummaries
                    };

                    allFactionsLoadoutSummaries.Add(factionLoadoutsSummary);
                }

                var playerLoadouts = new List<LoadoutSummary>();

                foreach (var loadoutId in activePlayerLoadouts)
                {
                    var playerLoadoutProfile = await _profileService.GetProfileFromLoadoutIdAsync(loadoutId);
                    var playerLoadoutName = playerLoadoutProfile.Name ?? string.Empty;

                    var playerLoadoutSummary = new LoadoutSummary()
                    {
                        Details = new LoadoutDetails()
                        {
                            Id = loadoutId,
                            FactionId = playerFactionId,
                            Name = playerLoadoutName
                        },
                        Stats = GetStatsForPlayerLoadout(groupedLoadouts, loadoutId)
                    };

                    playerLoadouts.Add(playerLoadoutSummary);
                }

                var playerStats = GetSummedLoadoutSummaryStats(playerLoadouts);

                return new PlayerLoadoutsReport()
                {
                    PlayerId = characterId,
                    PlayerFactionId = playerFactionId,
                    QueryStartTime = startTime,
                    QueryNowUtc = nowUtc,
                    ActivePlayerLoadoutIds = activePlayerLoadouts,
                    ActiveEnemyLoadoutIds = activeEnemyLoadouts,
                    ActiveFactionLoadouts = activeFactionLoadouts,
                    HeadToHeadLoadouts = groupedLoadouts,

                    PlayerLoadouts = playerLoadouts,
                    FactionLoadouts = allFactionsLoadoutSummaries,
                    PlayerStats = playerStats
                };
            }
        }


        private IEnumerable<int> GetActivePlayerLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            var activePlayerAttackerLoadouts = GetActivePlayerAttackerLoadouts(loadoutVsLoadoutRows, playerFactionId);

            var activePlayerVictimLoadouts = GetActivePlayerVictimLoadouts(loadoutVsLoadoutRows, playerFactionId);

            return activePlayerAttackerLoadouts
                    .Union(activePlayerVictimLoadouts)
                    .OrderBy(l => l)
                    .ToList();
        }

        private IEnumerable<int> GetActivePlayerAttackerLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            return loadoutVsLoadoutRows
                    .Where(l => l.AttackerFactionId == playerFactionId)
                    .Select(l => l.AttackerLoadoutId)
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<int> GetActivePlayerVictimLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            return loadoutVsLoadoutRows
                    .Where(l => l.VictimFactionId == playerFactionId)
                    .Select(l => l.VictimLoadoutId)
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<int> GetActiveEnemyLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            var activeEnemyAttackerLoadouts = GetActiveEnemyAttackerLoadouts(loadoutVsLoadoutRows, playerFactionId);

            var activeEnemyVictimLoadouts = GetActiveEnemyVictimLoadouts(loadoutVsLoadoutRows, playerFactionId);

            return activeEnemyAttackerLoadouts
                    .Union(activeEnemyVictimLoadouts)
                    .OrderBy(l => l)
                    .ToList();
        }

        private IEnumerable<int> GetActiveEnemyAttackerLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            return loadoutVsLoadoutRows
                    .Where(l => l.AttackerFactionId == playerFactionId)
                    .Select(l => l.AttackerLoadoutId)
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<int> GetActiveEnemyVictimLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            return loadoutVsLoadoutRows
                    .Where(l => l.VictimFactionId == playerFactionId)
                    .Select(l => l.VictimLoadoutId)
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<int> GetActiveFactions(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            var activeAttackerFactions = GetActiveAttackerFactions(loadoutVsLoadoutRows, playerFactionId);

            var activeVictimFactions = GetActiveVictimFactions(loadoutVsLoadoutRows, playerFactionId);

            return activeAttackerFactions
                    .Union(activeVictimFactions)
                    .OrderBy(f => f)
                    .ToList();
        }

        private IEnumerable<int> GetActiveAttackerFactions(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            return loadoutVsLoadoutRows
                    .Where(l => l.AttackerFactionId != playerFactionId)
                    .Select(l => l.AttackerFactionId)
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<int> GetActiveVictimFactions(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            return loadoutVsLoadoutRows
                    .Where(l => l.VictimFactionId != playerFactionId)
                    .Select(l => l.VictimFactionId)
                    .Distinct()
                    .ToList();
        }


        private IEnumerable<ActiveFactionLoadouts> GetActiveFactionLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, IEnumerable<int> activeFactions)
        {
            var activeFactionLoadouts = new List<ActiveFactionLoadouts>();
            foreach (var faction in activeFactions)
            {
                var activeAttackerLoadouts = new List<int>(loadoutVsLoadoutRows
                                                            .Where(l => l.AttackerFactionId == faction)
                                                            .Select(l => l.AttackerLoadoutId)
                                                            .Distinct()
                                                            .ToList());

                var activeVictimLoadouts = new List<int>(loadoutVsLoadoutRows
                                                                .Where(l => l.VictimFactionId == faction)
                                                                .Select(l => l.VictimLoadoutId)
                                                                .Distinct()
                                                                .ToList());

                activeFactionLoadouts.Add(new ActiveFactionLoadouts()
                {
                    FactionId = faction,
                    Loadouts = activeAttackerLoadouts
                                .Union(activeVictimLoadouts)
                                .OrderBy(l => l)
                                .ToList()
                });
            }

            return activeFactionLoadouts;
        }

        private DeathEventAggregate GetStatsForEnemyLoadoutVsPlayerLoadout(IEnumerable<LoadoutVsLoadoutSummaryRow> groupedVsRows, int playerLoadoutId, int enemyLoadoutId)
        {
            if (groupedVsRows.Any(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimLoadoutId == enemyLoadoutId))
            {
                return groupedVsRows
                        .Where(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimLoadoutId == enemyLoadoutId)
                        .Select(grp => grp.AttackerStats)
                        .FirstOrDefault();
            }
            else if (groupedVsRows.Any(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerLoadoutId == enemyLoadoutId))
            {
                return groupedVsRows
                    .Where(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerLoadoutId == enemyLoadoutId)
                    .Select(grp => grp.AttackerStats)
                    .FirstOrDefault();
            }
            else
            {
                return new DeathEventAggregate();
            }
        }

        private DeathEventAggregate GetStatsForFactionVsPlayerLoadout(IEnumerable<LoadoutVsLoadoutSummaryRow> groupedVsRows, int playerLoadoutId, int enemyFactionId)
        {
            if (groupedVsRows.Any(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimFactionId == enemyFactionId))
            {
                var targetRows = groupedVsRows.Where(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimFactionId == enemyFactionId).ToArray();
                return new DeathEventAggregate()
                {
                    Kills = targetRows.Sum(rows => rows.AttackerStats.Kills),
                    Headshots = targetRows.Sum(rows => rows.AttackerStats.Headshots),
                    Deaths = targetRows.Sum(rows => rows.AttackerStats.Deaths),
                    HeadshotDeaths = targetRows.Sum(rows => rows.AttackerStats.HeadshotDeaths),
                };
            }
            else if (groupedVsRows.Any(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerFactionId == enemyFactionId))
            {
                var targetRows = groupedVsRows.Where(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerFactionId == enemyFactionId).ToArray();
                return new DeathEventAggregate()
                {
                    Kills = targetRows.Sum(rows => rows.AttackerStats.Kills),
                    Headshots = targetRows.Sum(rows => rows.AttackerStats.Headshots),
                    Deaths = targetRows.Sum(rows => rows.AttackerStats.Deaths),
                    HeadshotDeaths = targetRows.Sum(rows => rows.AttackerStats.HeadshotDeaths),
                };
            }
            else
            {
                return new DeathEventAggregate();
            }
        }

        private DeathEventAggregate GetStatsForPlayerLoadout(IEnumerable<LoadoutVsLoadoutSummaryRow> groupedVsRows, int playerLoadoutId)
        {
            if (groupedVsRows.Any(grp => grp.AttackerLoadoutId == playerLoadoutId))
            {
                var targetRows = groupedVsRows.Where(grp => grp.AttackerLoadoutId == playerLoadoutId).ToArray();
                return new DeathEventAggregate()
                {
                    Kills = targetRows.Sum(rows => rows.AttackerStats.Kills),
                    Headshots = targetRows.Sum(rows => rows.AttackerStats.Headshots),
                    Deaths = targetRows.Sum(rows => rows.AttackerStats.Deaths),
                    HeadshotDeaths = targetRows.Sum(rows => rows.AttackerStats.HeadshotDeaths)
                };
            }
            else if (groupedVsRows.Any(grp => grp.VictimLoadoutId == playerLoadoutId))
            {
                var targetRows = groupedVsRows.Where(grp => grp.VictimLoadoutId == playerLoadoutId).ToArray();
                return new DeathEventAggregate()
                {
                    Kills = targetRows.Sum(rows => rows.AttackerStats.Kills),
                    Headshots = targetRows.Sum(rows => rows.AttackerStats.Headshots),
                    Deaths = targetRows.Sum(rows => rows.AttackerStats.Deaths),
                    HeadshotDeaths = targetRows.Sum(rows => rows.AttackerStats.HeadshotDeaths)
                };
            }
            else
            {
                return new DeathEventAggregate();
            }
        }

        private DeathEventAggregate GetSummedLoadoutSummaryStats(IEnumerable<LoadoutSummary> loadoutSummaries)
        {
            return new DeathEventAggregate()
            {
                Kills = loadoutSummaries.Sum(ls => ls.Stats.Kills),
                Headshots = loadoutSummaries.Sum(ls => ls.Stats.Headshots),
                Deaths = loadoutSummaries.Sum(ls => ls.Stats.Deaths),
                HeadshotDeaths = loadoutSummaries.Sum(ls => ls.Stats.HeadshotDeaths)
            };
        }
    }

}
