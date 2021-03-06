﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Server.Services.Planetside;
using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerHourlyWeaponsController : ControllerBase
    {

        private readonly IDbContextHelper _dbContextHelper;
        private readonly ICharacterService _characterService;

        public PlayerHourlyWeaponsController (IDbContextHelper dbContextHelper, ICharacterService characterService)
        {
            _dbContextHelper = dbContextHelper;
            _characterService = characterService;
        }

        [HttpGet("weapons/{characterId}")]
        public async Task<ActionResult<HourlyPlayerWeaponsReport>> GetPlayerHourlyWeaponsSummary(string characterId)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            var characterEntity = await _characterService.GetCharacterAsync(characterId);

            if (characterEntity == null)
            {
                return null;
            }

            var playerFactionId = characterEntity.FactionId;

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();


                IQueryable<WeaponSummaryRow> query =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && (death.AttackerCharacterId == characterId
                          || death.CharacterId == characterId)
                       && death.DeathEventType != DeathEventType.Suicide

                    join weapon in dbContext.Items
                      on (int)death.AttackerWeaponId equals weapon.Id into weaponsQ
                    from weapon in weaponsQ.DefaultIfEmpty()

                    select new WeaponSummaryRow()
                    {
                        WeaponId = (int)death.AttackerWeaponId,
                        WeaponName = weapon.Name,
                        FactionId = (int)weapon.FactionId,

                        WeaponStats = new DeathEventAggregate()
                        {
                            Kills = (from kill in dbContext.Deaths
                                     where kill.AttackerCharacterId == characterId
                                        && kill.DeathEventType == DeathEventType.Kill
                                        && kill.AttackerWeaponId == weapon.Id
                                        && kill.Timestamp >= startTime
                                     select kill.AttackerWeaponId).Count(),

                            Headshots = (from kill in dbContext.Deaths
                                         where kill.AttackerCharacterId == characterId
                                            && kill.DeathEventType == DeathEventType.Kill
                                            && kill.IsHeadshot == true
                                            && kill.AttackerWeaponId == weapon.Id
                                            && kill.Timestamp >= startTime
                                         select kill.AttackerWeaponId).Count(),

                            Deaths = (from kill in dbContext.Deaths
                                      where kill.CharacterId == characterId
                                         && kill.DeathEventType != DeathEventType.Suicide
                                         && kill.AttackerWeaponId == weapon.Id
                                         && kill.Timestamp >= startTime
                                      select kill.AttackerWeaponId).Count(),

                            HeadshotDeaths = (from kill in dbContext.Deaths
                                              where kill.CharacterId == characterId
                                                 && kill.DeathEventType != DeathEventType.Suicide
                                                 && kill.IsHeadshot == true
                                                 && kill.AttackerWeaponId == weapon.Id
                                                 && kill.Timestamp >= startTime
                                              select kill.AttackerWeaponId).Count(),
                        }
                    };

                var allWeaponSummaryRows = await query
                                                    .AsNoTracking()
                                                    .ToArrayAsync();

                var topWeaponsByKills = allWeaponSummaryRows
                                            .GroupBy(w => w.WeaponId)
                                            .Select(grp => grp.First())
                                            .Where(grp => grp.WeaponStats.Kills > 0)
                                            .OrderByDescending(grp => grp.WeaponStats.Kills)
                                            .ThenBy(grp => grp.WeaponName)
                                            .ToArray();

                var topWeaponsByDeaths = allWeaponSummaryRows
                                            .GroupBy(w => w.WeaponId)
                                            .Select(grp => grp.First())
                                            .Where(grp => grp.WeaponStats.Deaths > 0)
                                            .OrderByDescending(grp => grp.WeaponStats.Deaths)
                                            .ThenBy(grp => grp.WeaponName)
                                            .ToArray();

                return new HourlyPlayerWeaponsReport()
                {
                    PlayerId = characterId,
                    PlayerFactionId = playerFactionId,
                    QueryStartTime = startTime,
                    QueryNowUtc = nowUtc,
                    TopWeaponsByKills = topWeaponsByKills,
                    TopWeaponsByDeaths = topWeaponsByDeaths
                };
            }
        }
    }
}