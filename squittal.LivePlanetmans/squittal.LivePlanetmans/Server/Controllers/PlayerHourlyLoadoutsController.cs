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
        private IEnumerable<Profile> _profiles;
        private Dictionary<int, Profile> _loadoutMapping;

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

        [HttpGet("mappings/loadout/profile")]
        public async Task<ActionResult<Dictionary<int, int>>> GetLoadoutToProfileTypeIdMapping()
        {
            var mappingTask = _profileService.GetLoadoutMapping();
            _loadoutMapping = await mappingTask;

            foreach (var keyValuePair in _loadoutMapping)
            {
                Debug.WriteLine($"{keyValuePair.Key} :: {keyValuePair.Value.Name} [{keyValuePair.Value.Id}]");
            }

            return _loadoutMapping.ToDictionary(m => m.Key, m => m.Value.ProfileTypeId);

            //return _loadoutMapping;
        }
        
        [HttpGet("models/profiles")]
        public async Task<ActionResult<IEnumerable<Profile>>> GetAllProfileModelsAsync()
        {
            var modelsTask = _profileService.GetAllProfilesAsync();
            _profiles = await modelsTask;
            
            return _profiles.ToArray();
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
                            
                            Teamkills = (from kill in dbContext.Deaths
                                         where kill.AttackerCharacterId == characterId
                                            && kill.DeathEventType == DeathEventType.Teamkill
                                            && kill.AttackerLoadoutId == attackerLoadouts.Id
                                            && kill.CharacterLoadoutId == victimLoadouts.Id
                                            && kill.Timestamp >= startTime
                                         select kill).Count(),

                            Deaths = (from kill in dbContext.Deaths
                                      where kill.CharacterId == characterId
                                         //&& kill.DeathEventType == DeathEventType.Kill
                                         && kill.CharacterLoadoutId == attackerLoadouts.Id
                                         && kill.AttackerLoadoutId == victimLoadouts.Id
                                         && kill.Timestamp >= startTime
                                      select kill).Count(),

                            Suicides = (from kill in dbContext.Deaths
                                        where kill.CharacterId == characterId
                                           && kill.DeathEventType == DeathEventType.Suicide
                                           && kill.CharacterLoadoutId == attackerLoadouts.Id
                                           && kill.AttackerLoadoutId == victimLoadouts.Id
                                           && kill.Timestamp >= startTime
                                        select kill).Count(),

                            TeamkillDeaths = (from kill in dbContext.Deaths
                                              where kill.CharacterId == characterId
                                                 && kill.DeathEventType == DeathEventType.Teamkill
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
                        },

                        VictimStats = new DeathEventAggregate()
                        {
                            Kills = (from kill in dbContext.Deaths
                                     where kill.AttackerCharacterId == characterId
                                        && kill.DeathEventType == DeathEventType.Kill
                                        && kill.AttackerLoadoutId == victimLoadouts.Id
                                        && kill.CharacterLoadoutId == attackerLoadouts.Id
                                        && kill.Timestamp >= startTime
                                     select kill).Count(),

                            Headshots = (from kill in dbContext.Deaths
                                         where kill.AttackerCharacterId == characterId
                                            && kill.DeathEventType == DeathEventType.Kill
                                            && kill.AttackerLoadoutId == victimLoadouts.Id
                                            && kill.CharacterLoadoutId == attackerLoadouts.Id   
                                            && kill.Timestamp >= startTime
                                            && kill.IsHeadshot == true
                                         select kill).Count(),

                            Teamkills = (from kill in dbContext.Deaths
                                         where kill.AttackerCharacterId == characterId
                                            && kill.DeathEventType == DeathEventType.Teamkill
                                            && kill.AttackerLoadoutId == victimLoadouts.Id
                                            && kill.CharacterLoadoutId == attackerLoadouts.Id
                                            && kill.Timestamp >= startTime
                                         select kill).Count(),

                            Deaths = (from kill in dbContext.Deaths
                                      where kill.CharacterId == characterId
                                         //&& kill.DeathEventType == DeathEventType.Kill
                                         && kill.CharacterLoadoutId == victimLoadouts.Id
                                         && kill.AttackerLoadoutId == attackerLoadouts.Id
                                         && kill.Timestamp >= startTime
                                      select kill).Count(),

                            TeamkillDeaths = (from kill in dbContext.Deaths
                                              where kill.CharacterId == characterId
                                                 && kill.DeathEventType == DeathEventType.Teamkill
                                                 && kill.CharacterLoadoutId == victimLoadouts.Id
                                                 && kill.AttackerLoadoutId == attackerLoadouts.Id
                                                 && kill.Timestamp >= startTime
                                              select kill).Count(),

                            HeadshotDeaths = (from kill in dbContext.Deaths
                                              where kill.CharacterId == characterId
                                                 //&& kill.DeathEventType == DeathEventType.Kill
                                                 && kill.CharacterLoadoutId == victimLoadouts.Id
                                                 && kill.AttackerLoadoutId == attackerLoadouts.Id
                                                 && kill.Timestamp >= startTime
                                                 && kill.IsHeadshot == true
                                              select kill).Count()
                        }
                    };

                var loadoutVsLoadoutRows = await query
                                                    .AsNoTracking()
                                                    .ToArrayAsync();

                Debug.WriteLine("===============================");
                foreach (var row in loadoutVsLoadoutRows)
                {
                    Debug.WriteLine($"{row.DebugInfoString}");
                }

                var groupedLoadouts = loadoutVsLoadoutRows
                                                .GroupBy(vsRow => new { vsRow.AttackerLoadoutId, vsRow.VictimLoadoutId })
                                                .Select(grp => grp.First())
                                                .ToArray();

                Debug.WriteLine("===============================");
                foreach (var row in groupedLoadouts)
                {
                    Debug.WriteLine($"{row.DebugInfoString}");
                }

                var activePlayerLoadouts = GetActivePlayerLoadouts(loadoutVsLoadoutRows, playerFactionId);
                var activeEnemyLoadouts = GetActiveEnemyLoadouts(loadoutVsLoadoutRows, playerFactionId);
                var activeFactions = GetActiveFactions(loadoutVsLoadoutRows, playerFactionId);
                var activeFactionLoadouts = GetActiveFactionLoadouts(loadoutVsLoadoutRows, activeFactions, playerFactionId);

                foreach (var l in activePlayerLoadouts)
                {
                    Debug.WriteLine($"Active Player Loadout: {l}");
                }

                foreach (var l in activeEnemyLoadouts)
                {
                    Debug.WriteLine($"Active Enemy Loadout: {l}");
                }

                foreach (var l in activeFactions)
                {
                    Debug.WriteLine($"Active Faction: {l}");
                }

                foreach (var fl in activeFactionLoadouts)
                {
                    Debug.WriteLine($"Active Faction: {fl.FactionId}");
                    foreach (var l in fl.Loadouts)
                    {
                        Debug.WriteLine($"    Active Faction Loadout: {l}");
                    }
                }

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
                                Stats = GetStatsForEnemyLoadoutVsPlayerLoadout(groupedLoadouts, playerLoadoutId, playerFactionId, enemyLoadoutId, factionId)
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
                        //if (enemyH2HSummary.Summary.Stats.HasEvents == true)
                        //{
                            //factionLoadoutSummaries.Add(enemyH2HSummary);
                        //}
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
                            Stats = GetStatsForFactionVsPlayerLoadout(groupedLoadouts, playerLoadoutId, playerFactionId, factionId)
                        };

                        factionVsPlayerLoadoutSummaries.Add(playerLoadoutSummary);
                        //if (playerLoadoutSummary.Stats.HasEvents)
                        //{
                        //    factionVsPlayerLoadoutSummaries.Add(playerLoadoutSummary);
                        //}
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
                    //if (factionLoadoutsSummary.Summary.Stats.HasEvents)
                    //{
                    //    allFactionsLoadoutSummaries.Add(factionLoadoutsSummary);
                    //}
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
                    //if (playerLoadoutSummary.Stats.HasEvents == true)
                    //{
                        //playerLoadouts.Add(playerLoadoutSummary);
                    //}

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
                    //.Where(l => l.AttackerFactionId == playerFactionId)
                    .Where(l => l.AttackerFactionId == playerFactionId
                             && ( ( l.VictimFactionId == playerFactionId && l.AttackerStats.Teamkills > 0 )
                                  || (l.VictimFactionId != playerFactionId ) ) )
                    .Select(l => l.AttackerLoadoutId)
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<int> GetActivePlayerVictimLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            return loadoutVsLoadoutRows
                    //.Where(l => l.VictimFactionId == playerFactionId)
                    .Where(l => l.VictimFactionId == playerFactionId
                             && ( ( l.AttackerFactionId == playerFactionId && l.VictimStats.TeamkillDeaths > 0 )
                                || (l.AttackerFactionId != playerFactionId ) ) )
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
                    //.Where(l => l.AttackerFactionId == playerFactionId
                    //         && ( ( l.VictimFactionId == playerFactionId && (l.AttackerStats.TeamkillDeaths > 0 || l.AttackerStats.Teamkills > 0 ) )
                    //            || ( l.VictimFactionId != playerFactionId ) ) )
                    .Select(l => l.AttackerLoadoutId)
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<int> GetActiveEnemyVictimLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            return loadoutVsLoadoutRows
                    .Where(l => l.VictimFactionId == playerFactionId)
                    //.Where(l => l.VictimFactionId == playerFactionId
                    //         && ( ( l.AttackerFactionId == playerFactionId && ( l.VictimStats.Teamkills > 0 || l.VictimStats.TeamkillDeaths > 0 ) )
                    //            || ( l.AttackerFactionId != playerFactionId ) ) )
                    .Select(l => l.VictimLoadoutId)
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<int> GetActiveFactions(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            var activeAttackerFactions = GetActiveAttackerFactions(loadoutVsLoadoutRows, playerFactionId);

            var activeVictimFactions = GetActiveVictimFactions(loadoutVsLoadoutRows, playerFactionId);

            var activeTeamKillFactions = GetActiveTeamKillFactions(loadoutVsLoadoutRows, playerFactionId);

            return activeAttackerFactions
                    .Union(activeVictimFactions)
                    .Union(activeTeamKillFactions)
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

        private IEnumerable<int> GetActiveTeamKillFactions(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            return loadoutVsLoadoutRows
                    .Where(l => l.AttackerFactionId == playerFactionId && l.VictimFactionId == playerFactionId)
                    .Select(l => l.AttackerFactionId)
                    .Distinct()
                    .ToList();
        }

        private IEnumerable<ActiveFactionLoadouts> GetActiveFactionLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, IEnumerable<int> activeFactions, int playerFactionId)
        {
            var activeFactionLoadouts = new List<ActiveFactionLoadouts>();
            foreach (var faction in activeFactions)
            {
                var activeAttackerLoadouts = new List<int>();
                var activeVictimLoadouts = new List<int>();

                if (faction == playerFactionId)
                {
                    // Loadouts that teamkilled the player
                    activeAttackerLoadouts = (loadoutVsLoadoutRows
                                                            //.Where(l => l.AttackerFactionId == faction)

                                                            .Where(l => l.AttackerFactionId == faction
                                                                     && l.VictimFactionId == faction
                                                                     && l.VictimStats.TeamkillDeaths > 0) // || l.VictimStats.Deaths > 0))
                                                                     //&& ( l.AttackerStats.Teamkills > 0 || l.VictimStats.TeamkillDeaths > 0) )

                                                            //.Where(l => l.AttackerFactionId == faction && l.VictimFactionId != playerFactionId)
                                                                     //&& ((l.VictimFactionId == playerFactionId && (l.VictimStats.TeamkillDeaths > 0|| l.AttackerStats.Teamkills > 0)) //l.AttackerStats.Teamkills > 0) // l.VictimStats.TeamkillDeaths > 0) //(l.AttackerStats.Teamkills > 0 || l.VictimStats.TeamkillDeaths > 0))
                                                                        //|| l.VictimFactionId != playerFactionId))
                                                            //.Select(l => l.AttackerLoadoutId)
                                                            
                                                            .Select(l => l.AttackerLoadoutId)
                                                            //.Select(l => l.VictimLoadoutId)
                                                            .Distinct()
                                                            .ToList());

                    Debug.WriteLine("==================================");
                    foreach (var l in activeAttackerLoadouts)
                    {
                        Debug.WriteLine($"    activeAttackerLoadouts F{faction}: {l}");
                    }

                    //activeAttackerLoadouts.Union(loadoutVsLoadoutRows
                    //                                .Where(l => l.AttackerFactionId == faction
                    //                                         && l.VictimFactionId == faction
                    //                                         && l.VictimStats.TeamkillDeaths > 0 || l.AttackerStats.TeamkillDeaths > 0)
                    //                                //.Select(l => l.VictimLoadoutId)
                    //                                .Select(l => l.AttackerLoadoutId)
                    //                                .Distinct()
                    //                                .ToList());

                    //foreach (var l in activeAttackerLoadouts)
                    //{
                    //    Debug.WriteLine($"    activeAttackerLoadouts F{faction} - Union: {l}");
                    //}

                    // loadouts that the player teamkilled
                    activeVictimLoadouts = (loadoutVsLoadoutRows
                                                .Where(l => l.AttackerFactionId == faction
                                                         && l.VictimFactionId == faction
                                                         && l.AttackerStats.Teamkills > 0) // || l.AttackerStats.Teamkills > 0)
                                                .Select(l => l.VictimLoadoutId)
                                                //.Select(l => l.AttackerLoadoutId)
                                                .Distinct()
                                                .ToList());

                    foreach (var l in activeVictimLoadouts)
                    {
                        Debug.WriteLine($"    activeVictimLoadouts F{faction}: {l}");
                    }

                    //activeVictimLoadouts.Union(loadoutVsLoadoutRows
                    //                            .Where(l => l.AttackerFactionId == faction
                    //                                     && l.VictimFactionId == faction
                    //                                     && (l.VictimStats.Teamkills > 0 || l.AttackerStats.TeamkillDeaths > 0))
                    //                                     //&& l.VictimStats.Teamkills > 0 || l.AttackerStats.TeamkillDeaths > 0)
                    //                            .Select(l => l.VictimLoadoutId)
                    //                            //.Select(l => l.AttackerLoadoutId)
                    //                            .Distinct()
                    //                            .ToList());

                    //foreach (var l in activeVictimLoadouts)
                    //{
                    //    Debug.WriteLine($"    activeVictimLoadouts F{faction} - Union: {l}");
                    //}

                    //activeVictimLoadouts = (loadoutVsLoadoutRows
                    //                            //.Where(l => l.VictimFactionId == faction)
                    //                            .Where(l => l.VictimFactionId == faction 
                    //                                     && ((l.AttackerFactionId == playerFactionId && l.AttackerStats.Teamkills > 0) //(l.VictimStats.TeamkillDeaths > 0 || l.AttackerStats.Teamkills > 0 ) ) // && (l.VictimStats.TeamkillDeaths > 0 || l.VictimStats.TeamkillDeaths > 0))
                    //                                         || l.AttackerFactionId != playerFactionId ) )
                    //                            .Select(l => l.VictimLoadoutId)
                    //                            .Distinct()
                    //                            .ToList());
                }
                else
                {
                    activeAttackerLoadouts = (loadoutVsLoadoutRows
                                                                .Where(l => l.AttackerFactionId == faction)
                                                                //.Where(l => l.AttackerFactionId == faction
                                                                //         && ((l.VictimFactionId == faction && (l.AttackerStats.Teamkills > 0 || l.AttackerStats.TeamkillDeaths > 0))
                                                                //            || l.VictimFactionId != faction ) )
                                                                .Select(l => l.AttackerLoadoutId)
                                                                .Distinct()
                                                                .ToList());

                    activeVictimLoadouts = (loadoutVsLoadoutRows
                                                                    .Where(l => l.VictimFactionId == faction)
                                                                    //.Where(l => l.VictimFactionId == faction
                                                                         //&& ((l.AttackerFactionId == faction && (l.VictimStats.TeamkillDeaths > 0 || l.VictimStats.TeamkillDeaths > 0))
                                                                            //|| (l.AttackerFactionId != faction)))
                                                                    .Select(l => l.VictimLoadoutId)
                                                                    .Distinct()
                                                                    .ToList());
                }

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
        private DeathEventAggregate GetStatsForEnemyLoadoutVsPlayerLoadout(IEnumerable<LoadoutVsLoadoutSummaryRow> groupedVsRows, int playerLoadoutId, int playerFactionId, int enemyLoadoutId, int enemyFactionId)
        {
            //var playerFactionId = groupedVsRows
            //                        .Where(grp => grp.AttackerLoadoutId == playerLoadoutId)
            //                        .Select(grp => grp.AttackerFactionId)
            //                        .FirstOrDefault();

            //var enemyFactionId = groupedVsRows
            //                        .Where(grp => grp.AttackerLoadoutId == enemyLoadoutId)
            //                        .Select(grp => grp.AttackerFactionId)
            //                        .FirstOrDefault();

            var useTeamkills = (playerFactionId == enemyFactionId);

            if (groupedVsRows.Any(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimLoadoutId == enemyLoadoutId))
            {
                if (useTeamkills == true)
                {
                    return groupedVsRows
                            .Where(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimLoadoutId == enemyLoadoutId)
                            .Select(grp => ConvertToTeamkillAggregate(grp.AttackerStats))
                            .FirstOrDefault();
                }
                
                return groupedVsRows
                        .Where(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimLoadoutId == enemyLoadoutId)
                        .Select(grp => grp.AttackerStats)
                        .FirstOrDefault();
            }
            else if (groupedVsRows.Any(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerLoadoutId == enemyLoadoutId))
            {
                if (useTeamkills == true)
                {
                    return groupedVsRows
                            .Where(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerLoadoutId == enemyLoadoutId)
                            .Select(grp => ConvertToTeamkillAggregate(grp.VictimStats))
                            .FirstOrDefault();
                }

                return groupedVsRows
                    .Where(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerLoadoutId == enemyLoadoutId)
                    .Select(grp => grp.VictimStats)
                    .FirstOrDefault();
            }
            else
            {
                return new DeathEventAggregate();
            }
        }

        private DeathEventAggregate GetStatsForFactionVsPlayerLoadout(IEnumerable<LoadoutVsLoadoutSummaryRow> groupedVsRows, int playerLoadoutId, int playerFactionId, int enemyFactionId)
        {
            var attackerAggregate = new DeathEventAggregate();
            var victimAggregate = new DeathEventAggregate();

            var useTeamkills = (playerFactionId == enemyFactionId);

            if (groupedVsRows.Any(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimFactionId == enemyFactionId))
            {
                var targetRows = groupedVsRows.Where(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimFactionId == enemyFactionId).ToArray();
                
                if (useTeamkills == false)
                {
                    attackerAggregate.Kills = targetRows.Sum(row => row.AttackerStats.Kills);
                    attackerAggregate.Headshots = targetRows.Sum(row => row.AttackerStats.Headshots);
                    attackerAggregate.Deaths = targetRows.Sum(row => row.AttackerStats.Deaths);
                    attackerAggregate.HeadshotDeaths = targetRows.Sum(row => row.AttackerStats.HeadshotDeaths);
                }
                else
                {
                    attackerAggregate.Kills = targetRows.Sum(row => ConvertToTeamkillAggregate(row.AttackerStats).Kills);
                    attackerAggregate.Headshots = targetRows.Sum(row => ConvertToTeamkillAggregate(row.AttackerStats).Headshots);
                    attackerAggregate.Deaths = targetRows.Sum(row => ConvertToTeamkillAggregate(row.AttackerStats).Deaths);
                    attackerAggregate.HeadshotDeaths = targetRows.Sum(row => ConvertToTeamkillAggregate(row.AttackerStats).HeadshotDeaths);
                }

            }

            var victimRows = groupedVsRows.Where(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerFactionId == enemyFactionId).ToArray();
            foreach (var row in victimRows)
            {
                var rowEnemyLoadoutId = row.AttackerLoadoutId;
                var rowPlayerLoadoutId = row.VictimLoadoutId;

                if (groupedVsRows.Any(grp => grp.AttackerLoadoutId == rowPlayerLoadoutId && grp.VictimLoadoutId == rowEnemyLoadoutId))
                {
                    continue;
                }

                var addend = (useTeamkills == false) ? row.VictimStats : ConvertToTeamkillAggregate(row.VictimStats);

                victimAggregate.Add(addend);
            }

            return attackerAggregate.Add(victimAggregate);
        }

        private DeathEventAggregate GetStatsForPlayerLoadout(IEnumerable<LoadoutVsLoadoutSummaryRow> groupedVsRows, int playerLoadoutId)
        {
            var attackerAggregate = new DeathEventAggregate();
            var victimAggregate = new DeathEventAggregate();

            var attackerRows = groupedVsRows.Where(grp => grp.AttackerLoadoutId == playerLoadoutId).ToArray();
            foreach (var row in attackerRows)
            {
                var addend = (row.AttackerFactionId != row.VictimFactionId)
                                ? row.AttackerStats
                                : new DeathEventAggregate()
                                {
                                    Kills = 0,
                                    Deaths = ConvertToTeamkillAggregate(row.AttackerStats).Deaths,
                                    Headshots = 0,
                                    HeadshotDeaths = ConvertToTeamkillAggregate(row.AttackerStats).HeadshotDeaths
                                };

                attackerAggregate.Add(addend);
            }

            var victimRows = groupedVsRows.Where(grp => grp.VictimLoadoutId == playerLoadoutId).ToArray();
            foreach (var row in victimRows)
            {
                var rowEnemyLoadoutId = row.AttackerLoadoutId;

                if (groupedVsRows.Any(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimLoadoutId == row.AttackerLoadoutId))
                {
                    continue;
                }

                var addend = (row.AttackerFactionId != row.VictimFactionId)
                                ? row.VictimStats
                                : new DeathEventAggregate()
                                {
                                    Kills = 0,
                                    Deaths = ConvertToTeamkillAggregate(row.VictimStats).Deaths,
                                    Headshots = 0,
                                    HeadshotDeaths = ConvertToTeamkillAggregate(row.VictimStats).HeadshotDeaths
                                };

                victimAggregate.Add(addend);
            }

            return attackerAggregate.Add(victimAggregate);
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

        private DeathEventAggregate ConvertToTeamkillAggregate(DeathEventAggregate aggregate)
        {
            return new DeathEventAggregate()
            {
                Kills = aggregate.Teamkills,
                Deaths = aggregate.Deaths,
                Headshots = aggregate.Headshots,
                HeadshotDeaths = aggregate.HeadshotDeaths
            };
        }
        #endregion
    }

}
