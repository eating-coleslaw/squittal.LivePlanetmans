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

        /*
        [HttpGet("loadouts/{characterId}")]
        public async Task<ActionResult<PlayerHourlyLoadoutsSummary>> GetHourlyLoadoutsSummaryAsync(string characterId)
        {
            var character = await _characterService.GetCharacterAsync(characterId);

            if (character == null)
            {
                return null;
            }

            var playerFactionId = character.FactionId;

            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<FactionLoadoutSummaryRow> query =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && (death.AttackerCharacterId == characterId
                            || death.CharacterId == characterId)
                       && death.DeathEventType == DeathEventType.Kill
                       && death.AttackerFactionId != 4

                    join loadout in dbContext.Loadouts
                      on death.AttackerLoadoutId equals loadout.Id into attackerLoadoutsQ
                    from attackerLoadout in attackerLoadoutsQ.DefaultIfEmpty()

                    join profile in dbContext.Profiles
                      on attackerLoadout.ProfileId equals profile.Id into attackerProfilesQ
                    from attackerProfile in attackerProfilesQ.DefaultIfEmpty()

                        //join loadout in dbContext.Loadouts
                        //  on death.CharacterLoadoutId equals loadout.Id into victimLoadoutsQ
                        //from victimLoadout in victimLoadoutsQ.DefaultIfEmpty()

                        //join profile in dbContext.Profiles
                        //  on victimLoadout.Id equals profile.Id into victimProfilesQ
                        //from victimProfile in victimProfilesQ.DefaultIfEmpty()

                    select new FactionLoadoutSummaryRow()
                    {
                        //{
                        FactionId = attackerLoadout.FactionId,
                        LoadoutId = attackerLoadout.Id,
                        //ProfileTypeId = attackerProfile.ProfileTypeId,
                        //Name = attackerProfile.Name ?? "??",
                        Kills = (from kill in dbContext.Deaths
                                 where (kill.AttackerCharacterId == characterId
                                         || kill.CharacterId == characterId)
                                    && kill.AttackerFactionId != 4
                                    && kill.AttackerLoadoutId == attackerLoadout.Id
                                    && kill.DeathEventType == DeathEventType.Kill
                                    && kill.Timestamp >= startTime
                                 select kill).Count(),

                        Headshots = (from kill in dbContext.Deaths
                                     where (kill.AttackerCharacterId == characterId
                                             || kill.CharacterId == characterId)
                                     && kill.AttackerFactionId != 4
                                     && kill.AttackerLoadoutId == attackerLoadout.Id
                                     && kill.DeathEventType == DeathEventType.Kill
                                     && kill.IsHeadshot == true
                                     && kill.Timestamp >= startTime
                                     select kill).Count()

                        //LoadoutKills = new FactionLoadoutSummaryRow()
                        //{
                        //    FactionId = attackerLoadout.FactionId,
                        //    LoadoutId = attackerLoadout.Id,
                        //    ProfileTypeId = attackerProfile.ProfileTypeId,
                        //    Name = attackerProfile.Name,
                        //    Aggregates = new DeathEventAggregate()
                        //    {
                        //        Kills = (from kill in dbContext.Deaths
                        //                 where (kill.AttackerCharacterId == characterId
                        //                         || kill.CharacterId == characterId)
                        //                    && kill.AttackerLoadoutId == attackerLoadout.Id
                        //                    && kill.DeathEventType == DeathEventType.Kill
                        //                    && kill.Timestamp >= startTime
                        //                 select kill).Count(),

                        //        Headshots = (from kill in dbContext.Deaths
                        //                     where (kill.AttackerCharacterId == characterId
                        //                             || kill.CharacterId == characterId)
                        //                        && kill.AttackerLoadoutId == attackerLoadout.Id
                        //                        && kill.DeathEventType == DeathEventType.Kill
                        //                        && kill.IsHeadshot == true
                        //                        && kill.Timestamp >= startTime
                        //                     select kill).Count()
                        //    }
                        //},

                        //LoadoutDeaths = new FactionLoadoutSummaryRow()
                        //{
                        //    FactionId = victimLoadout.FactionId,
                        //    LoadoutId = victimLoadout.Id,
                        //    ProfileTypeId = victimProfile.ProfileTypeId,
                        //    Name = victimProfile.Name,
                        //    Aggregates = new DeathEventAggregate()
                        //    {
                        //        Deaths = (from kill in dbContext.Deaths
                        //                  where (kill.AttackerCharacterId == characterId
                        //                          || kill.CharacterId == characterId)
                        //                     && kill.CharacterLoadoutId == victimLoadout.Id
                        //                     && kill.DeathEventType == DeathEventType.Kill
                        //                     && kill.Timestamp >= startTime
                        //                  select kill).Count(),

                        //        HeadshotDeaths = (from kill in dbContext.Deaths
                        //                          where (kill.AttackerCharacterId == characterId
                        //                                  || kill.CharacterId == characterId)
                        //                             && kill.CharacterLoadoutId == victimLoadout.Id
                        //                             && kill.DeathEventType == DeathEventType.Kill
                        //                             && kill.IsHeadshot == true
                        //                             && kill.Timestamp >= startTime
                        //                          select kill).Count()
                        //    }
                        //}
                    };

                var allLoadoutResults = await query
                                                .AsNoTracking()
                                                .ToArrayAsync();

                //var playerSummary = new PlayerHourlyLoadoutsSummary()
                //{
                //    PlayerId = characterId,
                //    QueryStartTime = startTime,
                //    QueryNowUtc = nowUtc
                //};

                //var loadoutKills = allLoadoutResults
                //                    .Select(e => e.LoadoutKills)
                //                    .Where(e => e.FactionId == playerFactionId && e.LoadoutId != 0)
                //                    .ToArray();

                //var loadoutDeaths = allLoadoutResults
                //                    .Select(e => e.LoadoutDeaths)
                //                    .Where(e => e.FactionId != playerFactionId && e.LoadoutId != 0)
                //                    .ToArray();

                var testList = new List<FactionLoadoutSummaryRow>();
                var test = new FactionLoadoutSummaryRow()
                {
                    FactionId = 1,
                    LoadoutId = 15,
                    Kills = 111,
                    Headshots = 44
                };

                testList.Add(test);

                return new PlayerHourlyLoadoutsSummary()
                {
                    PlayerId = characterId,
                    QueryStartTime = startTime,
                    QueryNowUtc = nowUtc,
                    PlayerLoadouts = testList.ToArray() //allLoadoutResults //loadoutKills,
                    //EnemyFactionLoadouts = loadoutDeaths
                };

                //playerSummary.PlayerLoadouts = loadoutKills
                //                                .GroupBy(l => l.LoadoutId)
                //                                .Select(grp => grp.First())
                //                                .OrderByDescending(l => l.LoadoutId)
                //                                //.Where(l => l.FactionId == playerFactionId && l.LoadoutId != 0)
                //                                .ToArray();

                //playerSummary.EnemyFactionLoadouts = loadoutDeaths
                //                                        .GroupBy(l => new { l.FactionId, l.LoadoutId })
                //                                        .Select(grp => grp.First())
                //                                        .OrderByDescending(grp => grp.FactionId)
                //                                        .ThenByDescending(grp => grp.LoadoutId)
                //                                        //.Where(grp => grp.FactionId != playerFactionId)
                //                                        .ToArray();

                //playerSummary.EnemyLoadouts = loadoutDeaths
                //                                .GroupBy(l => l.ProfileTypeId)
                //                                .Select(grp => new ProfileTypeSummaryRow()
                //                                {
                //                                    ProfileTypeId = grp.Select(g => g.ProfileTypeId).First(),
                //                                    Name = grp.Select(g => g.Name).First(),
                //                                    Aggregates = grp.Select(g => g.Aggregates).First()
                //                                })
                //                                .OrderByDescending(grp => grp.ProfileTypeId)
                //                                .ToArray();

                ////playerSummary.PlayerLoadouts.ToList().ForEach(async l => l.Name = (await _profileService.GetProfileFromLoadoutIdAsync(l.LoadoutId)).Name);
                ////playerSummary.PlayerLoadouts.ToList().ForEach(async l => l.Name = (await _profileService.GetProfileFromLoadoutIdAsync(l.LoadoutId)).Name);

                //return playerSummary;

            }
        }

        [HttpGet("test/{characterId}")]
        public async Task<ActionResult<PlayerHourlyLoadoutsSummary>> GetTestLoadoutsAsync(string characterId)
        {
            var character = await _characterService.GetCharacterAsync(characterId);

            if (character == null)
            {
                return null;
            }

            var playerFactionId = character.FactionId;

            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<FactionLoadoutSummaryRow> query =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && (death.AttackerCharacterId == characterId
                            || death.CharacterId == characterId)
                       && death.DeathEventType == DeathEventType.Kill
                       && death.AttackerFactionId != 4

                    join loadout in dbContext.Loadouts
                      on death.AttackerLoadoutId equals loadout.Id into attackerLoadoutsQ
                    from attackerLoadout in attackerLoadoutsQ.DefaultIfEmpty()

                    select new FactionLoadoutSummaryRow()
                    {
                        FactionId = attackerLoadout.FactionId,
                        LoadoutId = attackerLoadout.Id,
                        //Name = attackerLoadout.CodeName,
                        Kills = 29, // (from kill in dbContext.Deaths
                                 //where (kill.AttackerCharacterId == characterId
                                 //        || kill.CharacterId == characterId)
                                 //   && kill.AttackerFactionId != 4
                                 //   && kill.AttackerLoadoutId == attackerLoadout.Id
                                 //   && kill.DeathEventType == DeathEventType.Kill
                                 //   && kill.Timestamp >= startTime
                                 //select kill).Count(),

                        Headshots = 17 //(from kill in dbContext.Deaths
                                     //where (kill.AttackerCharacterId == characterId
                                     //        || kill.CharacterId == characterId)
                                     //&& kill.AttackerFactionId != 4
                                     //&& kill.AttackerLoadoutId == attackerLoadout.Id
                                     //&& kill.DeathEventType == DeathEventType.Kill
                                     //&& kill.IsHeadshot == true
                                     //&& kill.Timestamp >= startTime
                                     //select kill).Count()

                        //LoadoutKills = new FactionLoadoutSummaryRow()
                        //{
                        //    FactionId = attackerLoadout.FactionId,
                        //    LoadoutId = attackerLoadout.Id,
                        //    ProfileTypeId = attackerProfile.ProfileTypeId,
                        //    Name = attackerProfile.Name,
                        //    Aggregates = new DeathEventAggregate()
                        //    {
                        //        Kills = (from kill in dbContext.Deaths
                        //                 where (kill.AttackerCharacterId == characterId
                        //                         || kill.CharacterId == characterId)
                        //                    && kill.AttackerLoadoutId == attackerLoadout.Id
                        //                    && kill.DeathEventType == DeathEventType.Kill
                        //                    && kill.Timestamp >= startTime
                        //                 select kill).Count(),

                        //        Headshots = (from kill in dbContext.Deaths
                        //                     where (kill.AttackerCharacterId == characterId
                        //                             || kill.CharacterId == characterId)
                        //                        && kill.AttackerLoadoutId == attackerLoadout.Id
                        //                        && kill.DeathEventType == DeathEventType.Kill
                        //                        && kill.IsHeadshot == true
                        //                        && kill.Timestamp >= startTime
                        //                     select kill).Count()
                        //    }
                        //},

                        //LoadoutDeaths = new FactionLoadoutSummaryRow()
                        //{
                        //    FactionId = victimLoadout.FactionId,
                        //    LoadoutId = victimLoadout.Id,
                        //    ProfileTypeId = victimProfile.ProfileTypeId,
                        //    Name = victimProfile.Name,
                        //    Aggregates = new DeathEventAggregate()
                        //    {
                        //        Deaths = (from kill in dbContext.Deaths
                        //                  where (kill.AttackerCharacterId == characterId
                        //                          || kill.CharacterId == characterId)
                        //                     && kill.CharacterLoadoutId == victimLoadout.Id
                        //                     && kill.DeathEventType == DeathEventType.Kill
                        //                     && kill.Timestamp >= startTime
                        //                  select kill).Count(),

                        //        HeadshotDeaths = (from kill in dbContext.Deaths
                        //                          where (kill.AttackerCharacterId == characterId
                        //                                  || kill.CharacterId == characterId)
                        //                             && kill.CharacterLoadoutId == victimLoadout.Id
                        //                             && kill.DeathEventType == DeathEventType.Kill
                        //                             && kill.IsHeadshot == true
                        //                             && kill.Timestamp >= startTime
                        //                          select kill).Count()
                        //    }
                        //}
                    };

                var allLoadoutResults = await query
                                                .AsNoTracking()
                                                .ToArrayAsync();

                //var playerSummary = new PlayerHourlyLoadoutsSummary()
                //{
                //    PlayerId = characterId,
                //    QueryStartTime = startTime,
                //    QueryNowUtc = nowUtc
                //};

                //var loadoutKills = allLoadoutResults
                //                    .Select(e => e.LoadoutKills)
                //                    .Where(e => e.FactionId == playerFactionId && e.LoadoutId != 0)
                //                    .ToArray();

                //var loadoutDeaths = allLoadoutResults
                //                    .Select(e => e.LoadoutDeaths)
                //                    .Where(e => e.FactionId != playerFactionId && e.LoadoutId != 0)
                //                    .ToArray();

                var testList = new List<FactionLoadoutSummaryRow>();
                var test = new FactionLoadoutSummaryRow()
                {
                    FactionId = 1,
                    LoadoutId = 15,
                    Kills = 111,
                    Headshots = 44
                };

                testList.Add(test);

                var testArray = testList.ToArray();

                return new PlayerHourlyLoadoutsSummary()
                {
                    PlayerId = characterId,
                    QueryStartTime = startTime,
                    QueryNowUtc = nowUtc,
                    PlayerLoadouts = testArray //testList.AsEnumerable() //.ToArray() //allLoadoutResults //loadoutKills,
                    //EnemyFactionLoadouts = loadoutDeaths
                };

                //playerSummary.PlayerLoadouts = loadoutKills
                //                                .GroupBy(l => l.LoadoutId)
                //                                .Select(grp => grp.First())
                //                                .OrderByDescending(l => l.LoadoutId)
                //                                //.Where(l => l.FactionId == playerFactionId && l.LoadoutId != 0)
                //                                .ToArray();

                //playerSummary.EnemyFactionLoadouts = loadoutDeaths
                //                                        .GroupBy(l => new { l.FactionId, l.LoadoutId })
                //                                        .Select(grp => grp.First())
                //                                        .OrderByDescending(grp => grp.FactionId)
                //                                        .ThenByDescending(grp => grp.LoadoutId)
                //                                        //.Where(grp => grp.FactionId != playerFactionId)
                //                                        .ToArray();

                //playerSummary.EnemyLoadouts = loadoutDeaths
                //                                .GroupBy(l => l.ProfileTypeId)
                //                                .Select(grp => new ProfileTypeSummaryRow()
                //                                {
                //                                    ProfileTypeId = grp.Select(g => g.ProfileTypeId).First(),
                //                                    Name = grp.Select(g => g.Name).First(),
                //                                    Aggregates = grp.Select(g => g.Aggregates).First()
                //                                })
                //                                .OrderByDescending(grp => grp.ProfileTypeId)
                //                                .ToArray();

                ////playerSummary.PlayerLoadouts.ToList().ForEach(async l => l.Name = (await _profileService.GetProfileFromLoadoutIdAsync(l.LoadoutId)).Name);
                ////playerSummary.PlayerLoadouts.ToList().ForEach(async l => l.Name = (await _profileService.GetProfileFromLoadoutIdAsync(l.LoadoutId)).Name);

                //return playerSummary;

            }
        }
    }
    */
        
        /*
        [HttpGet("loadouts1/{characterId}")]
        public async Task<ActionResult<PlayerHourlyLoadoutsSummary>> GetHourlyLoadoutsAsync1(string characterId)
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

                IQueryable<PlayerHourlyLoadoutsSummaryRow> query =
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

                    select new PlayerHourlyLoadoutsSummaryRow()
                    {
                        AttackerLoadoutId = attackerLoadouts.Id,
                        AttackerLoadoutName = attackerLoadouts.CodeName,
                        AttackerFactionId = attackerLoadouts.FactionId,
                        AttackerProfileId = attackerLoadouts.ProfileId,

                        VictimLoadoutId = victimLoadouts.Id,
                        VictimLoadoutName = victimLoadouts.CodeName,
                        VictimFactionId = victimLoadouts.FactionId,
                        VictimProfileId = victimLoadouts.ProfileId,


                        AttackerKills = (from kill in dbContext.Deaths
                                         where kill.AttackerCharacterId == characterId
                                            && kill.DeathEventType == DeathEventType.Kill
                                            && kill.AttackerLoadoutId == attackerLoadouts.Id
                                            && kill.CharacterLoadoutId == victimLoadouts.Id
                                            && kill.Timestamp >= startTime
                                         select kill).Count(),

                        AttackerHeadshots = (from kill in dbContext.Deaths
                                             where kill.AttackerCharacterId == characterId
                                                && kill.DeathEventType == DeathEventType.Kill
                                                && kill.AttackerLoadoutId == attackerLoadouts.Id
                                                && kill.CharacterLoadoutId == victimLoadouts.Id
                                                && kill.Timestamp >= startTime
                                                && kill.IsHeadshot == true
                                             select kill).Count(),

                        VictimKills = (from kill in dbContext.Deaths
                                       where kill.CharacterId == characterId
                                          && kill.DeathEventType == DeathEventType.Kill
                                          && kill.AttackerLoadoutId == victimLoadouts.Id
                                          && kill.CharacterLoadoutId == attackerLoadouts.Id
                                          && kill.Timestamp >= startTime
                                       select kill).Count(),

                        VictimHeadshots = (from kill in dbContext.Deaths
                                           where kill.CharacterId == characterId
                                              && kill.DeathEventType == DeathEventType.Kill
                                              && kill.AttackerLoadoutId == victimLoadouts.Id
                                              && kill.CharacterLoadoutId == attackerLoadouts.Id
                                              && kill.Timestamp >= startTime
                                              && kill.IsHeadshot == true
                                           select kill).Count()
                    };

                var allHeadToHeadPlayers = await query
                                                    .AsNoTracking()
                                                    .ToArrayAsync();

                var topByKills = allHeadToHeadPlayers
                                .GroupBy(p => new { p.AttackerLoadoutId, p.VictimLoadoutId })
                                .Select(grp => grp.First())
                                //.OrderByDescending(grp => grp.AttackerKills)
                                .OrderByDescending(grp => grp.AttackerLoadoutId)
                                .ThenByDescending(grp => grp.VictimLoadoutId)
                                .Where(grp => grp.AttackerFactionId == playerFactionId)
                                .ToArray();

                var topByDeaths = allHeadToHeadPlayers
                                .GroupBy(p => new { p.AttackerLoadoutId, p.VictimLoadoutId })
                                .Select(grp => grp.First())
                                //.OrderByDescending(grp => grp.AttackerKills)
                                .OrderByDescending(grp => grp.AttackerLoadoutId)
                                .ThenBy(grp => grp.VictimLoadoutId)
                                .Where(grp => grp.VictimFactionId == playerFactionId)
                                .ToArray();

                Debug.WriteLine($"{topByKills.Count()}");
                Debug.WriteLine($"{topByDeaths.Count()}");


                return new PlayerHourlyLoadoutsSummary()
                {
                    PlayerId = characterId,
                    QueryStartTime = startTime,
                    QueryNowUtc = nowUtc,
                    TopLoadoutsByKills = topByKills,
                    TopLoadoutsByDeaths = topByDeaths
                };
            }
        }
        */

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



                foreach (var row in loadoutVsLoadoutRows)
                {
                    Debug.WriteLine($"{row.AttackerLoadoutName} vs {row.VictimLoadoutName}");

                    Debug.WriteLine($"________________________________");
                }


                Debug.WriteLine($"====================================");

                var groupedLoadouts = loadoutVsLoadoutRows
                                                .GroupBy(vsRow => new { vsRow.AttackerLoadoutId, vsRow.VictimLoadoutId })
                                                .Select(grp => grp.First())
                                                //.Distinct()
                                                //.Where(grp => grp.AttackerFactionId == playerFactionId)
                                                .ToArray();

                var activePlayerLoadouts = GetActivePlayerLoadouts(loadoutVsLoadoutRows, playerFactionId);
                var activeEnemyLoadouts = GetActiveEnemyLoadouts(loadoutVsLoadoutRows, playerFactionId);
                var activeFactions = GetActiveFactions(loadoutVsLoadoutRows, playerFactionId);
                var activeFactionLoadouts = GetActiveFactionLoadouts(loadoutVsLoadoutRows, activeFactions);


                foreach (var group in groupedLoadouts) //.Where(g => g.AttackerFactionId == playerFactionId))
                {
                    var alId = group.AttackerLoadoutId;
                    Debug.WriteLine($"*{group.AttackerLoadoutName} [{group.AttackerLoadoutId}] vs {group.VictimLoadoutName} [{group.VictimLoadoutId}]* [Kills: {group.AttackerStats.Kills}  Deaths: {group.AttackerStats.Deaths}]");

                    //foreach (var victim in groupedLoadouts.Where(v => v.AttackerLoadoutId == alId))
                    //{
                    //    Debug.WriteLine($"   {victim.VictimLoadoutName} [{victim.VictimLoadoutId}]   Kills: {victim.AttackerDeathEventAggregate.Kills}  Deaths: {victim.AttackerDeathEventAggregate.Deaths}");
                    //}
                    Debug.WriteLine($"========================================");

                }


                var allFactionsLoadoutSummaries = new List<FactionLoadoutsSummary>();
                foreach (var factionId in activeFactions)
                {
                    /* Faction Loadouts Summary */
                    var factionLoadoutsSummary = new FactionLoadoutsSummary
                    {
                        Summary = new FactionSummary()
                        {
                            Details = new FactionDetails()
                            {
                                Id = factionId
                            }
                        }
                    };

                    Debug.WriteLine($"=== Faction {factionId} ===");

                    var factionLoadouts = activeFactionLoadouts.Where(f => f.FactionId == factionId).SelectMany(f => f.Loadouts);

                    var factionLoadoutSummaries = new List<EnemyLoadoutHeadToHeadSummary>();


                    foreach (var enemyLoadoutId in factionLoadouts)
                    {
                        var enemyLoadoutSummary = new LoadoutSummary
                        {
                            Details = new LoadoutDetails()
                            {
                                Id = enemyLoadoutId,
                                FactionId = factionId
                            }
                        };
                        var enemyLoadoutProfile = await _profileService.GetProfileFromLoadoutIdAsync(enemyLoadoutId);
                        enemyLoadoutSummary.Details.Name = enemyLoadoutProfile.Name ?? string.Empty;
                        Debug.WriteLine($"   Loadout {enemyLoadoutSummary.Details.Name}");

                        var enemyH2HSummary = new EnemyLoadoutHeadToHeadSummary()
                        {
                            Summary = enemyLoadoutSummary
                        };

                        var h2hPlayerLoadouts = new List<LoadoutSummary>();


                        foreach (var playerLoadoutId in activePlayerLoadouts)
                        {
                            /* EnemyLoadoutHeadToHeadSummary . PlayerLoadouts */
                            var playerLoadoutSummary = new LoadoutSummary
                            {
                                Details = new LoadoutDetails()
                                {
                                    Id = playerLoadoutId,
                                    FactionId = playerFactionId
                                }

                                /*
                                Stats = (groupedLoadouts
                                            .Where(grp => (grp.AttackerLoadoutId == playerLoadoutId && grp.VictimLoadoutId == enemyLoadoutId )
                                                           || (grp.VictimLoadoutId == playerLoadoutId && grp.AttackerLoadoutId == enemyLoadoutId))
                                            .Select(grp => grp.AttackerStats) //new DeathEventAggregate()
                                            //{
                                            //    Kills = grp.AttackerStats.Kills,
                                            //    Headshots = grp.AttackerStats.Headshots,
                                            //    Deaths = grp.AttackerStats.Deaths,
                                            //    HeadshotDeaths = grp.AttackerStats.HeadshotDeaths
                                            //})
                                            .FirstOrDefault()) ?? new DeathEventAggregate()
                                */
                            };
                            var playerLoadoutProfile = await _profileService.GetProfileFromLoadoutIdAsync(playerLoadoutId);
                            playerLoadoutSummary.Details.Name = playerLoadoutProfile.Name;


                            if (groupedLoadouts.Any(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimLoadoutId == enemyLoadoutId))
                            {
                                playerLoadoutSummary.Stats = groupedLoadouts
                                                                .Where(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimLoadoutId == enemyLoadoutId)
                                                                .Select(grp => grp.AttackerStats)
                                                                .FirstOrDefault();
                            }
                            else if (groupedLoadouts.Any(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerLoadoutId == enemyLoadoutId))
                            {
                                playerLoadoutSummary.Stats = groupedLoadouts
                                    .Where(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerLoadoutId == enemyLoadoutId)
                                    .Select(grp => grp.AttackerStats)
                                    .FirstOrDefault();
                            }
                            else
                            {
                                playerLoadoutSummary.Stats = new DeathEventAggregate();
                            }


                            if (playerLoadoutSummary.Stats == null)
                            {
                                playerLoadoutSummary.Stats = new DeathEventAggregate();
                                Debug.WriteLine($"      vs {playerLoadoutSummary.Details.Name} [null stats]   [Kills: {playerLoadoutSummary.Stats.Kills} Deaths: {playerLoadoutSummary.Stats.Deaths}]");
                            }
                            else
                            {
                                Debug.WriteLine($"      vs {playerLoadoutSummary.Details.Name}  [Kills: {playerLoadoutSummary.Stats.Kills} Deaths: {playerLoadoutSummary.Stats.Deaths}]");
                            }


                            h2hPlayerLoadouts.Add(playerLoadoutSummary);


                        }

                        enemyH2HSummary.PlayerLoadouts = h2hPlayerLoadouts;

                        enemyH2HSummary.Summary.Stats = new DeathEventAggregate()
                        {
                            //Kills = h2hPlayerLoadouts.Where(pl => pl.Stats != null).Sum(pl => pl.Stats.Kills),
                            //Headshots = h2hPlayerLoadouts.Where(pl => pl.Stats != null).Sum(pl => pl.Stats.Headshots),
                            //Deaths = h2hPlayerLoadouts.Where(pl => pl.Stats != null).Sum(pl => pl.Stats.Deaths),
                            //HeadshotDeaths = h2hPlayerLoadouts.Where(pl => pl.Stats != null).Sum(pl => pl.Stats.HeadshotDeaths)
                            Kills = h2hPlayerLoadouts.Sum(pl => pl.Stats.Kills),
                            Headshots = h2hPlayerLoadouts.Sum(pl => pl.Stats.Headshots),
                            Deaths = h2hPlayerLoadouts.Sum(pl => pl.Stats.Deaths),
                            HeadshotDeaths = h2hPlayerLoadouts.Sum(pl => pl.Stats.HeadshotDeaths)
                        };

                        Debug.WriteLine($"   Loadout {enemyLoadoutSummary.Details.Name}  [Kills: {enemyLoadoutSummary.Stats.Kills} Deaths: {enemyLoadoutSummary.Stats.Deaths}]");

                        factionLoadoutSummaries.Add(enemyH2HSummary);

                        Debug.WriteLine($"________________________________");
                    }

                    factionLoadoutsSummary.FactionLoadouts = factionLoadoutSummaries;


                    /* Faction vs Player Loadout summaries */
                    var factionVsPlayerLoadoutSummaries = new List<LoadoutSummary>();
                    foreach (var playerLoadoutId in activePlayerLoadouts)
                    {
                        var playerLoadoutSummary = new LoadoutSummary
                        {
                            Details = new LoadoutDetails()
                            {
                                Id = playerLoadoutId,
                                FactionId = playerFactionId
                            },

                            Stats = (groupedLoadouts
                                        .Where(grp => grp.AttackerLoadoutId == playerLoadoutId
                                                   && grp.VictimFactionId == factionId)
                                        .Select(grp => grp.AttackerStats) //new DeathEventAggregate()
                                        //{
                                        //    Kills = grp.AttackerStats.Kills,
                                        //    Headshots = grp.AttackerStats.Headshots,
                                        //    Deaths = grp.AttackerStats.Deaths,
                                        //    HeadshotDeaths = grp.AttackerStats.HeadshotDeaths
                                        //})
                                        .FirstOrDefault()) ?? new DeathEventAggregate()
                        };
                        var playerLoadoutProfile = await _profileService.GetProfileFromLoadoutIdAsync(playerLoadoutId);
                        playerLoadoutSummary.Details.Name = playerLoadoutProfile.Name;


                        if (groupedLoadouts.Any(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimFactionId == factionId))
                        {
                            var targetRows = groupedLoadouts.Where(grp => grp.AttackerLoadoutId == playerLoadoutId && grp.VictimFactionId == factionId).ToArray();
                            playerLoadoutSummary.Stats = new DeathEventAggregate()
                            {
                                Kills = targetRows.Sum(rows => rows.AttackerStats.Kills),
                                Headshots = targetRows.Sum(rows => rows.AttackerStats.Headshots),
                                Deaths = targetRows.Sum(rows => rows.AttackerStats.Deaths),
                                HeadshotDeaths = targetRows.Sum(rows => rows.AttackerStats.HeadshotDeaths),
                            };
                        }
                        else if (groupedLoadouts.Any(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerFactionId == factionId))
                        {
                            var targetRows = groupedLoadouts.Where(grp => grp.VictimLoadoutId == playerLoadoutId && grp.AttackerFactionId == factionId).ToArray();
                            playerLoadoutSummary.Stats = new DeathEventAggregate()
                            {
                                Kills = targetRows.Sum(rows => rows.AttackerStats.Kills),
                                Headshots = targetRows.Sum(rows => rows.AttackerStats.Headshots),
                                Deaths = targetRows.Sum(rows => rows.AttackerStats.Deaths),
                                HeadshotDeaths = targetRows.Sum(rows => rows.AttackerStats.HeadshotDeaths),
                            };
                        }
                        else
                        {
                            playerLoadoutSummary.Stats = new DeathEventAggregate();
                        }




                        Debug.WriteLine($"   vs {playerLoadoutSummary.Details.Name}  [Kills: {playerLoadoutSummary.Stats.Kills} Deaths: {playerLoadoutSummary.Stats.Deaths}]");

                        factionVsPlayerLoadoutSummaries.Add(playerLoadoutSummary);
                    }

                    factionLoadoutsSummary.PlayerLoadouts = factionVsPlayerLoadoutSummaries;

                    factionLoadoutsSummary.Summary.Stats = new DeathEventAggregate()
                    {
                        Kills = factionVsPlayerLoadoutSummaries.Sum(f => f.Stats.Kills),
                        Headshots = factionVsPlayerLoadoutSummaries.Sum(f => f.Stats.Headshots),
                        Deaths = factionVsPlayerLoadoutSummaries.Sum(f => f.Stats.Deaths),
                        HeadshotDeaths = factionVsPlayerLoadoutSummaries.Sum(f => f.Stats.HeadshotDeaths)
                        //Kills = factionVsPlayerLoadoutSummaries.Where(f => f.Stats != null).Sum(f => f.Stats.Kills),
                        //Headshots = factionVsPlayerLoadoutSummaries.Where(f => f.Stats != null).Sum(f => f.Stats.Headshots),
                        //Deaths = factionVsPlayerLoadoutSummaries.Where(f => f.Stats != null).Sum(f => f.Stats.Deaths),
                        //HeadshotDeaths = factionVsPlayerLoadoutSummaries.Where(f => f.Stats != null).Sum(f => f.Stats.HeadshotDeaths)
                    };

                    allFactionsLoadoutSummaries.Add(factionLoadoutsSummary);
                }
                Debug.WriteLine($"========================================");



                var playerLoadouts = new List<LoadoutSummary>();

                foreach (var loadoutId in activePlayerLoadouts)
                {
                    var playerLoadoutSummary = new LoadoutSummary()
                    {
                        Details = new LoadoutDetails()
                        {
                            Id = loadoutId,
                            FactionId = playerFactionId
                            //Name = await _profileService.GetProfileFromLoadoutIdAsync(loadoutId).Result.Name // loadoutId.ToString() //groupedLoadouts.Where(grp => grp.AttackerLoadoutId == loadoutId).Select(grp => grp.AttackerLoadoutName).First()
                                        //?? groupedLoadouts.Where(grp => grp.VictimLoadoutId == loadoutId).Select(grp => grp.VictimLoadoutName).FirstOrDefault(),
                            //ProfileId = groupedLoadouts.Where(grp => grp.AttackerLoadoutId == loadoutId || grp.VictimLoadoutId == loadoutId).Select(grp => grp.AttackerProfileId).FirstOrDefault()
                        }

                        //Stats = new DeathEventAggregate()
                        //{
                        //    Kills = groupedLoadouts.Where(grp => grp.AttackerLoadoutId == loadoutId).Sum(grp => grp.AttackerStats.Kills),
                        //    Headshots = groupedLoadouts.Where(grp => grp.AttackerLoadoutId == loadoutId).Sum(grp => grp.AttackerStats.Headshots),
                        //    Deaths = groupedLoadouts.Where(grp => grp.AttackerLoadoutId == loadoutId).Sum(grp => grp.AttackerStats.Deaths),
                        //    HeadshotDeaths = groupedLoadouts.Where(grp => grp.AttackerLoadoutId == loadoutId).Sum(grp => grp.AttackerStats.HeadshotDeaths),
                        //}
                    };
                    var playerLoadoutProfile = await _profileService.GetProfileFromLoadoutIdAsync(loadoutId);
                    playerLoadoutSummary.Details.Name = playerLoadoutProfile.Name;


                    if (groupedLoadouts.Any(grp => grp.AttackerLoadoutId == loadoutId))
                    {
                        var targetRows = groupedLoadouts.Where(grp => grp.AttackerLoadoutId == loadoutId).ToArray();
                        playerLoadoutSummary.Stats = new DeathEventAggregate()
                        {
                            Kills = targetRows.Sum(rows => rows.AttackerStats.Kills),
                            Headshots = targetRows.Sum(rows => rows.AttackerStats.Headshots),
                            Deaths = targetRows.Sum(rows => rows.AttackerStats.Deaths),
                            HeadshotDeaths = targetRows.Sum(rows => rows.AttackerStats.HeadshotDeaths)
                        };

                        //playerLoadoutSummary.Stats = groupedLoadouts.Where(grp => grp.AttackerLoadoutId == loadoutId).Select(grp => grp.AttackerStats).FirstOrDefault();
                    }
                    else if (groupedLoadouts.Any(grp => grp.VictimLoadoutId == loadoutId))
                    {
                        var targetRows = groupedLoadouts.Where(grp => grp.VictimLoadoutId == loadoutId).ToArray();
                        playerLoadoutSummary.Stats = new DeathEventAggregate()
                        {
                            Kills = targetRows.Sum(rows => rows.AttackerStats.Kills),
                            Headshots = targetRows.Sum(rows => rows.AttackerStats.Headshots),
                            Deaths = targetRows.Sum(rows => rows.AttackerStats.Deaths),
                            HeadshotDeaths = targetRows.Sum(rows => rows.AttackerStats.HeadshotDeaths)
                        };

                        //playerLoadoutSummary.Stats = groupedLoadouts.Where(grp => grp.VictimLoadoutId == loadoutId).Select(grp => grp.AttackerStats).FirstOrDefault();
                    }
                    else
                    {
                        playerLoadoutSummary.Stats = new DeathEventAggregate();
                    }

                    //playerLoadoutSummary.Details.Name = await _profileService.GetProfileFromLoadoutIdAsync(loadoutId).Result.Name;

                    //    (groupedLoadouts
                    //                .Where(grp => grp.AttackerLoadoutId == loadoutId)
                    //                .Select(grp => new DeathEventAggregate()
                    //                {
                    //                    Kills = grp.AttackerStats.Sum
                    //                }) //grp.AttackerStats)
                    //                .First()) ?? new DeathEventAggregate()
                    //};

                    playerLoadouts.Add(playerLoadoutSummary);
                }

                var playerStats = new DeathEventAggregate()
                {
                    //Kills = playerLoadouts.Where(pl => pl.Stats != null).Sum(pl => pl.Stats.Kills),
                    //Headshots = playerLoadouts.Where(pl => pl.Stats != null).Sum(pl => pl.Stats.Headshots),
                    //Deaths = playerLoadouts.Where(pl => pl.Stats != null).Sum(pl => pl.Stats.Deaths),
                    //HeadshotDeaths = playerLoadouts.Where(pl => pl.Stats != null).Sum(pl => pl.Stats.HeadshotDeaths)
                    Kills = playerLoadouts.Sum(pl => pl.Stats.Kills),
                    Headshots = playerLoadouts.Sum(pl => pl.Stats.Headshots),
                    Deaths = playerLoadouts.Sum(pl => pl.Stats.Deaths),
                    HeadshotDeaths = playerLoadouts.Sum(pl => pl.Stats.HeadshotDeaths)
                };
                Debug.WriteLine($"========================================");

                /*
                var playerLoadouts = groupedLoadouts
                                        .GroupBy(vsRow => vsRow.AttackerLoadoutId) //new { vsRow.AttackerLoadoutId, vsRow.VictimLoadoutId })
                                        .Select(grp => new LoadoutHeadToHeadSummary()
                                        {
                                            Summary = new LoadoutSummary()
                                            {
                                                Details = new LoadoutDetails()
                                                {
                                                    Id = grp.Key,
                                                    Name = grp.Select(g => g.AttackerLoadoutName).First(),
                                                    ProfileId = grp.Select(g => g.AttackerProfileId).First(),
                                                    FactionId = grp.Select(g => g.AttackerFactionId).First()

                                                },

                                                Stats = new DeathEventAggregate()
                                                {
                                                    Kills = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.Kills), //  .Where(vGrp => vGrp.  Where(g => g.AttackerLoadoutId == grp.Key).Select(g => g.AttackerDeathEventAggregate.Kills).Sum(),
                                                Headshots = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.Headshots),
                                                    Deaths = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.Deaths),
                                                    HeadshotDeaths = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.HeadshotDeaths)
                                                }

                                            },

                                            VictimFactions = grp.Select(g => new FactionLoadoutsSummary()
                                            {
                                                Summary = new FactionSummary()
                                                {
                                                    Details = new FactionDetails()
                                                    {
                                                        Id = g.VictimFactionId
                                                    },

                                                    Stats = new DeathEventAggregate()
                                                    {
                                                        Kills = grp.Where(a => a.AttackerLoadoutId == grp.Key && a.VictimFactionId == g.VictimFactionId).GroupBy(aggGrp => aggGrp.VictimFactionId).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.Kills),
                                                        Headshots = grp.Where(a => a.AttackerLoadoutId == grp.Key && a.VictimFactionId == g.VictimFactionId).GroupBy(aggGrp => aggGrp.VictimFactionId).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.Headshots),
                                                        Deaths = grp.Where(a => a.AttackerLoadoutId == grp.Key && a.VictimFactionId == g.VictimFactionId).GroupBy(aggGrp => aggGrp.VictimFactionId).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.Deaths),
                                                        HeadshotDeaths = grp.Where(a => a.AttackerLoadoutId == grp.Key && a.VictimFactionId == g.VictimFactionId).GroupBy(aggGrp => aggGrp.VictimFactionId).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerStats.HeadshotDeaths),
                                                    }
                                                }
                                            }).GroupBy(fls => fls.Summary.Details.Id).Select(flsGrp => flsGrp.First()),


                                            VictimLoadouts = grp.Select(g => new LoadoutSummary()
                                            {
                                                Details = new LoadoutDetails()
                                                {
                                                    Id = g.VictimLoadoutId,
                                                    Name = grp.Where(v => v.VictimLoadoutId == g.VictimLoadoutId).Select(v => v.VictimLoadoutName).First(),
                                                    ProfileId = grp.Where(v => v.VictimLoadoutId == g.VictimLoadoutId).Select(v => v.VictimProfileId).First(),
                                                    FactionId = grp.Where(v => v.VictimLoadoutId == g.VictimLoadoutId).Select(v => v.VictimFactionId).First(),
                                                },

                                                Stats = new DeathEventAggregate()
                                                {
                                                    Kills = grp.Where(a => a.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerStats.Kills), // Select(a => a.AttackerDeathEventAggregate.Kills).Sum(),
                                                Headshots = grp.Where(a => g.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerStats.Headshots), //Select(a => a.AttackerDeathEventAggregate.Headshots).Sum(),
                                                Deaths = grp.Where(a => g.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerStats.Deaths), //Select(a => a.AttackerDeathEventAggregate.Deaths).Sum(),
                                                HeadshotDeaths = grp.Where(a => g.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerStats.HeadshotDeaths) //Select(a => a.AttackerDeathEventAggregate.HeadshotDeaths).Sum()
                                            }
                                            }).GroupBy(vls => vls.Details.Id).Select(vlsGrp => vlsGrp.First())
                                        })
                                        .Where(grp => grp.Summary.Details.FactionId == playerFactionId) // FactionId == playerFactionId)
                                        .OrderBy(grp => grp.Summary.Details.Id)
                                        .ThenBy(grp => grp.VictimLoadouts.Select(g => g.Details.Id).First()) // Summary.Loadout.Id).Select(g => g.VictimLoadoutId).First())
                                        .Distinct()
                                        .ToArray();
                */


                /*
                foreach (var loadout in playerLoadouts) //loadoutsSummary.LoadoutAggregates) //.Where(m => m.FactionId != stats.FactionId))
                {
                    {
                        foreach (var victim in loadout.VictimLoadouts)
                        {
                            Debug.WriteLine($"{loadout.Summary.Details.Id} vs {victim.Details.Id}  [Kills: {victim.Stats.Kills}]");
                        }
                    }
                }
                */


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
                    //TopLoadoutsByKills = topByKills,
                    //TopLoadoutsByDeaths = topByDeaths
                };

                    //return groupedLoadouts;
            }
        }


        private IEnumerable<int> GetActivePlayerLoadouts(IEnumerable<LoadoutVsLoadoutSummaryRow> loadoutVsLoadoutRows, int playerFactionId)
        {
            var activePlayerAttackerLoadouts = GetActivePlayerAttackerLoadouts(loadoutVsLoadoutRows, playerFactionId);

            // get loadouts with deaths
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

            // get loadouts with deaths
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
    }

}
