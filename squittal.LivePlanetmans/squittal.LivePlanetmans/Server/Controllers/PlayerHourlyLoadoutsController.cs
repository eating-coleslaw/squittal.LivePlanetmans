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

        public PlayerHourlyLoadoutsController(IDbContextHelper dbContextHelper, IProfileService profileService, ICharacterService characterService)
        {
            _dbContextHelper = dbContextHelper;
            _profileService = profileService;
            _characterService = characterService;
        }

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

                /*IQueryable<FactionLoadoutSummaryRow>*/ var query = 
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && (death.AttackerCharacterId == characterId
                            || death.CharacterId == characterId)
                       && death.DeathEventType == DeathEventType.Kill

                    join loadout in dbContext.Loadouts
                      on death.AttackerLoadoutId equals loadout.Id into attackerLoadoutsQ
                    from attackerLoadout in attackerLoadoutsQ.DefaultIfEmpty()

                    join profile in dbContext.Profiles
                      on attackerLoadout.Id equals profile.Id into attackerProfilesQ
                    from attackerProfile in attackerProfilesQ.DefaultIfEmpty()

                    join loadout in dbContext.Loadouts
                      on death.CharacterLoadoutId equals loadout.Id into victimLoadoutsQ
                    from victimLoadout in victimLoadoutsQ.DefaultIfEmpty()

                    join profile in dbContext.Profiles
                      on victimLoadout.Id equals profile.Id into victimProfilesQ
                    from victimProfile in victimProfilesQ.DefaultIfEmpty()

                    select new
                    {
                        LoadoutKills = new FactionLoadoutSummaryRow()
                        {
                            FactionId = (int)death.AttackerFactionId,
                            LoadoutId = attackerLoadout.Id,
                            ProfileTypeId = attackerProfile.ProfileTypeId,
                            Name = attackerProfile.Name,
                            Aggregates = new DeathEventAggregate()
                            {
                                Kills = (from kill in dbContext.Deaths
                                         where (kill.AttackerCharacterId == characterId
                                                 || kill.CharacterId == characterId)
                                            && kill.AttackerLoadoutId == attackerLoadout.Id
                                            && kill.DeathEventType == DeathEventType.Kill
                                            && kill.Timestamp >= startTime
                                         select kill).Count(),

                                Headshots = (from kill in dbContext.Deaths
                                             where (kill.AttackerCharacterId == characterId
                                                     || kill.CharacterId == characterId)
                                                && kill.AttackerLoadoutId == attackerLoadout.Id
                                                && kill.DeathEventType == DeathEventType.Kill
                                                && kill.IsHeadshot == true
                                                && kill.Timestamp >= startTime
                                             select kill).Count()
                            }
                        },

                        LoadoutDeaths = new FactionLoadoutSummaryRow()
                        {
                            FactionId = (int)death.CharacterFactionId,
                            LoadoutId = victimLoadout.Id,
                            ProfileTypeId = victimProfile.ProfileTypeId,
                            Name = victimProfile.Name,
                            Aggregates = new DeathEventAggregate()
                            {
                                Deaths = (from kill in dbContext.Deaths
                                          where (kill.AttackerCharacterId == characterId
                                                  || kill.CharacterId == characterId)
                                             && kill.CharacterLoadoutId == victimLoadout.Id
                                             && kill.DeathEventType == DeathEventType.Kill
                                             && kill.Timestamp >= startTime
                                          select kill).Count(),

                                HeadshotDeaths = (from kill in dbContext.Deaths
                                                  where (kill.AttackerCharacterId == characterId
                                                          || kill.CharacterId == characterId)
                                                     && kill.CharacterLoadoutId == victimLoadout.Id
                                                     && kill.DeathEventType == DeathEventType.Kill
                                                     && kill.IsHeadshot == true
                                                     && kill.Timestamp >= startTime
                                                  select kill).Count()
                            }
                        }
                    };

                var allLoadoutResults = await query
                                                .AsNoTracking()
                                                .ToArrayAsync();

                var playerSummary = new PlayerHourlyLoadoutsSummary()
                {
                    PlayerId = characterId,
                    QueryStartTime = startTime,
                    QueryNowUtc = nowUtc
                };

                var loadoutKills = allLoadoutResults.Select(e => e.LoadoutKills).Where(e => e.FactionId == playerFactionId && e.LoadoutId != 0);
                var loadoutDeaths = allLoadoutResults.Select(e => e.LoadoutDeaths).Where(e => e.FactionId != playerFactionId && e.LoadoutId != 0);

                playerSummary.PlayerLoadouts = loadoutKills
                                                .GroupBy(l => l.LoadoutId)
                                                .Select(grp => grp.First())
                                                .OrderByDescending(l => l.LoadoutId)
                                                //.Where(l => l.FactionId == playerFactionId && l.LoadoutId != 0)
                                                .ToArray();

                playerSummary.EnemyFactionLoadouts = loadoutDeaths
                                                        .GroupBy(l => new { l.FactionId, l.LoadoutId })
                                                        .Select(grp => grp.First())
                                                        .OrderByDescending(grp => new { grp.FactionId, grp.LoadoutId })
                                                        //.Where(grp => grp.FactionId != playerFactionId)
                                                        .ToArray();

                playerSummary.EnemyLoadouts = loadoutDeaths
                                                .GroupBy(l => l.ProfileTypeId)
                                                .Select(grp => new ProfileTypeSummaryRow()
                                                {
                                                    ProfileTypeId = grp.Select(g => g.ProfileTypeId).First(),
                                                    Name = grp.Select(g => g.Name).First(),
                                                    Aggregates = grp.Select(g => g.Aggregates).First()
                                                })
                                                .OrderByDescending(grp => grp.ProfileTypeId)
                                                .ToArray();

                //playerSummary.PlayerLoadouts.ToList().ForEach(async l => l.Name = (await _profileService.GetProfileFromLoadoutIdAsync(l.LoadoutId)).Name);
                //playerSummary.PlayerLoadouts.ToList().ForEach(async l => l.Name = (await _profileService.GetProfileFromLoadoutIdAsync(l.LoadoutId)).Name);

                return playerSummary;

            }
        }
    }
}
