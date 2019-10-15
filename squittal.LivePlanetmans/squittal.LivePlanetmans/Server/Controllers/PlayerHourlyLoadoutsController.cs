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

                        AttackerDeathEventAggregate = new DeathEventAggregate()
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
                                                            Kills = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerDeathEventAggregate.Kills), //  .Where(vGrp => vGrp.  Where(g => g.AttackerLoadoutId == grp.Key).Select(g => g.AttackerDeathEventAggregate.Kills).Sum(),
                                                            Headshots = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerDeathEventAggregate.Headshots),
                                                            Deaths = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerDeathEventAggregate.Deaths),
                                                            HeadshotDeaths = grp.Where(g => g.AttackerLoadoutId == grp.Key).GroupBy(aggGrp => new { aggGrp.AttackerLoadoutId, aggGrp.VictimLoadoutId }).Select(aggGrp => aggGrp.First()).Sum(aggGrp => aggGrp.AttackerDeathEventAggregate.HeadshotDeaths)
                                                        },

                                                        LoadoutVsLoadoutAggregates = grp.Select(g => new VictimLoadoutSummary()
                                                        {
                                                            VictimLoadoutId = g.VictimLoadoutId,
                                                            VictimLoadoutName = grp.Where(v => v.VictimLoadoutId == g.VictimLoadoutId).Select(v => v.VictimLoadoutName).First(),
                                                            VictimProfileId = grp.Where(v => v.VictimLoadoutId == g.VictimLoadoutId).Select(v => v.VictimProfileId).First(),
                                                            VictimFactionId = grp.Where(v => v.VictimLoadoutId == g.VictimLoadoutId).Select(v => v.VictimFactionId).First(),

                                                            AttackerDeathEventAggregate = new DeathEventAggregate()
                                                            {
                                                                Kills = grp.Where(a => a.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerDeathEventAggregate.Kills), // Select(a => a.AttackerDeathEventAggregate.Kills).Sum(),
                                                                Headshots = grp.Where(a => g.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerDeathEventAggregate.Headshots), //Select(a => a.AttackerDeathEventAggregate.Headshots).Sum(),
                                                                Deaths = grp.Where(a => g.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerDeathEventAggregate.Deaths), //Select(a => a.AttackerDeathEventAggregate.Deaths).Sum(),
                                                                HeadshotDeaths = grp.Where(a => g.AttackerLoadoutId == grp.Key && a.VictimLoadoutId == g.VictimLoadoutId).GroupBy(vAggGrp => vAggGrp.VictimLoadoutId).Select(vg => vg.First()).Sum(vg => vg.AttackerDeathEventAggregate.HeadshotDeaths) //Select(a => a.AttackerDeathEventAggregate.HeadshotDeaths).Sum()
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

                foreach (var al in attackerLoadoutAggregates)
                {
                    Debug.WriteLine($"*{al.LoadoutName} [{al.LoadoutId}]* [Kills: {al.LoadoutDeathEventAggregate.Kills}  Deaths: {al.LoadoutDeathEventAggregate.Deaths}]");

                    foreach (var vl in al.LoadoutVsLoadoutAggregates)
                    {
                        Debug.WriteLine($"   {vl.VictimLoadoutName} [{vl.VictimLoadoutId}]   Kills: {vl.AttackerDeathEventAggregate.Kills}  Deaths: {vl.AttackerDeathEventAggregate.Deaths}");
                    }
                    Debug.WriteLine($"________________________________");

                }



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
    }
}
