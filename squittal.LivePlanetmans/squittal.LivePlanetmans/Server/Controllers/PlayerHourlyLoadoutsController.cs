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
        private readonly IFactionService _factionService;

        private IEnumerable<Loadout> _loadouts;

        public PlayerHourlyLoadoutsController(IDbContextHelper dbContextHelper, IProfileService profileService, ICharacterService characterService, IFactionService factionService)
        {
            _dbContextHelper = dbContextHelper;
            _profileService = profileService;
            _characterService = characterService;
            _factionService = factionService;
        }

        [HttpGet("models/loadouts")]
        public async Task<ActionResult<IEnumerable<Loadout>>> GetAllLoadoutModelsAsync()
        {
            var modelsTask = _profileService.GetAllLoadoutsAsync();
            _loadouts = await modelsTask;

            return _loadouts.ToArray();
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
                       //&& death.DeathEventType == DeathEventType.Kill

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
                                         //&& kill.DeathEventType == DeathEventType.Kill
                                         && kill.CharacterLoadoutId == attackerLoadouts.Id
                                         && kill.AttackerLoadoutId == victimLoadouts.Id
                                         && kill.Timestamp >= startTime
                                      select kill).Count(),

                            HeadshotDeaths = (from kill in dbContext.Deaths
                                              where kill.CharacterId == characterId
                                                 //&& kill.DeathEventType == DeathEventType.Kill
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

                    var factionEntity = await _factionService.GetFactionAsync(factionId);
                    var factionName = factionEntity.Name ?? string.Empty;

                    var factionLoadoutsSummary = new FactionLoadoutsSummary
                    {
                        Summary = new FactionSummary()
                        {
                            Details = new FactionDetails()
                            {
                                Id = factionId,
                                Name = factionName
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

                    PlayerLoadouts = playerLoadouts,
                    FactionLoadouts = allFactionsLoadoutSummaries,
                    PlayerStats = playerStats
                };
            }
        }

        #region Active Loadout & Factions Helpers
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
        #endregion

        #region Loadout Stat Helpers
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
        #endregion
    }

}
