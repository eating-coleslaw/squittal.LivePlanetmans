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
    public class PlayerHourlyWeaponsController : ControllerBase
    {

        private readonly IDbContextHelper _dbContextHelper;

        public PlayerHourlyWeaponsController (IDbContextHelper dbContextHelper)
        {
            _dbContextHelper = dbContextHelper;
        }

        [HttpGet("weapons/kills/{characterId}")]
        public async Task<ActionResult<IEnumerable<HourlyWeaponSummaryRow>>> GetPlayerTopWeaponsByKillsAsync(string characterId)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<HourlyWeaponSummaryRow> query =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && death.AttackerCharacterId == characterId
                       && death.DeathEventType == DeathEventType.Kill
                    
                    join weapon in dbContext.Items
                      on (int)death.AttackerWeaponId equals weapon.Id into weaponsQ
                    from weapon in weaponsQ.DefaultIfEmpty()

                    select new HourlyWeaponSummaryRow()
                    {
                        WeaponId = (int)death.AttackerWeaponId,
                        WeaponName = weapon.Name,
                        FactionId = weapon.FactionId,

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
                                     select kill.AttackerWeaponId).Count()
                    };

                var topWeapons = await query
                                        .AsNoTracking()
                                        .ToArrayAsync();

                return topWeapons
                        .GroupBy(w => w.WeaponId)
                        .Select(grp => new HourlyWeaponSummaryRow()
                        {
                            WeaponId = grp.Key,
                            WeaponName = grp.Where(w => w.WeaponId == grp.Key).Select(w => w.WeaponName).FirstOrDefault(),
                            FactionId = grp.Where(w => w.WeaponId == grp.Key).Select(w => w.FactionId).FirstOrDefault(),
                            Kills = grp.Where(w => w.WeaponId == grp.Key).Select(w => w.Kills).FirstOrDefault(),
                            Headshots = grp.Where(w => w.WeaponId == grp.Key).Select(w => w.Headshots).FirstOrDefault()
                        })
                        .OrderByDescending(w => w.Kills)
                        .Where(w => w.Kills > 0 && w.WeaponId != 0)
                        .ToArray();
            }
        }

        [HttpGet("weapons/deaths/{characterId}")]
        public async Task<ActionResult<IEnumerable<HourlyWeaponSummaryRow>>> GetPlayerTopWeaponsByDeathsAsync(string characterId)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<HourlyWeaponSummaryRow> query =
                    from death in dbContext.Deaths

                    where death.Timestamp >= startTime
                       && death.CharacterId == characterId
                       //&& death.DeathEventType == DeathEventType.Kill

                    join weapon in dbContext.Items
                      on (int)death.AttackerWeaponId equals weapon.Id into weaponsQ
                    from weapon in weaponsQ.DefaultIfEmpty()

                    select new HourlyWeaponSummaryRow()
                    {
                        WeaponId = (int)death.AttackerWeaponId,
                        WeaponName = weapon.Name,
                        FactionId = weapon.FactionId,

                        Kills = (from kill in dbContext.Deaths
                                 where kill.CharacterId == characterId
                                    //&& kill.DeathEventType == DeathEventType.Kill
                                    && kill.AttackerWeaponId == weapon.Id
                                    && kill.Timestamp >= startTime
                                 select kill.AttackerWeaponId).Count(),

                        Headshots = (from kill in dbContext.Deaths
                                     where kill.CharacterId == characterId
                                        //&& kill.DeathEventType == DeathEventType.Kill
                                        && kill.IsHeadshot == true
                                        && kill.AttackerWeaponId == weapon.Id
                                        && kill.Timestamp >= startTime
                                     select kill.AttackerWeaponId).Count()
                    };

                var topWeapons = await query
                                        .AsNoTracking()
                                        .ToArrayAsync();

                return topWeapons
                        .GroupBy(w => w.WeaponId)
                        .Select(grp => new HourlyWeaponSummaryRow()
                        {
                            WeaponId = grp.Key,
                            WeaponName = grp.Where(w => w.WeaponId == grp.Key).Select(w => w.WeaponName).FirstOrDefault(),
                            FactionId = grp.Where(w => w.WeaponId == grp.Key).Select(w => w.FactionId).FirstOrDefault(),
                            Kills = grp.Where(w => w.WeaponId == grp.Key).Select(w => w.Kills).FirstOrDefault(),
                            Headshots = grp.Where(w => w.WeaponId == grp.Key).Select(w => w.Headshots).FirstOrDefault()
                        })
                        .OrderByDescending(w => w.Kills)
                        .Where(w => w.Kills > 0 && w.WeaponId != 0)
                        .ToArray();
            }
        }
    }
}