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

        [HttpGet("weaponKills/{characterId}")]
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
                    
                    //group death by (int)death.AttackerWeaponId into weaponsGroup

                    join weapon in dbContext.Items
                      on (int)death.AttackerWeaponId equals weapon.Id into weaponsQ
                      //on(int)death.AttackerWeaponId equals weapon.Id into weaponsQ
                    from weapon in weaponsQ.DefaultIfEmpty()
                    

                    //group death by death.AttackerWeaponId into weaponsGroup

                    //from death in weaponsGroup //.DefaultIfEmpty()

                    //join weapon in dbContext.Items
                    //  //on new { GroupKey = (int?)weaponsGroup.Key.Value } equals new { WeaponId = (int?)weapon.Id } into weaponsQ
                    //  on death.AttackerWeaponId equals weapon.Id
                    //from weapon in weaponsQ //.DefaultIfEmpty()
                    //on weaponsGroup.Key equals weapon.Id into weaponsQ

                    //on death.AttackerWeaponId equals weapon.Id into weaponsQ

                    select new HourlyWeaponSummaryRow()
                    {
                        WeaponId = (int)death.AttackerWeaponId, // ?? 0,
                        WeaponName = weapon.Name, //"hello",
                        FactionId = weapon.FactionId, //3,
                        Kills = (from kill in dbContext.Deaths
                                 where kill.AttackerCharacterId == characterId
                                    && kill.DeathEventType == DeathEventType.Kill
                                    && kill.AttackerWeaponId == weapon.Id
                                    && kill.Timestamp >= startTime
                                 //group kill by kill.AttackerWeaponId into weaponGroup
                                 select kill.AttackerWeaponId).Count(), //GroupBy(k => k.AttackerWeaponId).Count(),
                        ///Count(d => d.AttackerWeaponId == weaponsGroup.Key), //15,
                        
                        Headshots = (from kill in dbContext.Deaths
                                     where kill.AttackerCharacterId == characterId
                                        && kill.DeathEventType == DeathEventType.Kill
                                        && kill.IsHeadshot == true
                                        && kill.AttackerWeaponId == weapon.Id
                                        && kill.Timestamp >= startTime
                                     //group kill by kill.AttackerWeaponId into weaponGroup
                                     select kill.AttackerWeaponId).Count(), //GroupBy(k => k.AttackerWeaponId).Count()

                        //weaponsGroup.Count(d => d.AttackerWeaponId == weaponsGroup.Key && d.IsHeadshot == true) //7

                        //WeaponId = (int)weaponsGroup.Key, //death.AttackerWeaponId, // Id, /*weaponsGroup.Key ?? 0,*/
                        //WeaponName = weapon.Name, //Where(w => (int)weaponsGroup.Key == w.Id).Select(w => w.Name).FirstOrDefault(),//weaponsQ.FirstOrDefault().Name,
                        //FactionId = weapon.FactionId, //weaponsQ.Where(w => (int)weaponsGroup.Key == w.Id).Select(w => w.FactionId).FirstOrDefault(),//weaponsQ.FirstOrDefault().FactionId,
                        //Kills = weapon.Id,//weaponsGroup.Count(kill => kill.AttackerCharacterId == characterId
                        ////&& kill.DeathEventType == DeathEventType.Kill
                        ////&& (int)kill.AttackerWeaponId == (int)weaponsGroup.Key
                        ////&& kill.Timestamp >= startTime),
                        ////weaponsGroup.Where(kill => kill.AttackerCharacterId == characterId
                        ////                                && kill.DeathEventType == DeathEventType.Kill
                        ////                                && kill.AttackerWeaponId == weaponsGroup.Key
                        ////                                && kill.Timestamp >= startTime).Count(),
                        ////(from kill in dbContext.Deaths
                        //// where kill.AttackerCharacterId == characterId
                        ////    && kill.DeathEventType == DeathEventType.Kill
                        ////    && (int)kill.AttackerWeaponId == (int)weaponsGroup.Key
                        ////    && kill.Timestamp >= startTime
                        //// select kill).Count(),
                        //Headshots = weapon.Id / 2//weaponsGroup.Count(kill => kill.AttackerCharacterId == characterId
                        //            //&& kill.DeathEventType == DeathEventType.Kill
                        //            //&& (int)kill.AttackerWeaponId == (int)weaponsGroup.Key
                        //            //&& kill.Timestamp >= startTime
                        //            //&& kill.IsHeadshot == true)
                        //            //weaponsGroup.Where(kill => kill.AttackerCharacterId == characterId
                        //            //                                && kill.DeathEventType == DeathEventType.Kill
                        //            //                                && kill.AttackerWeaponId == weaponsGroup.Key
                        //            //                                && kill.Timestamp >= startTime
                        //            //                                && kill.IsHeadshot == true).Count()
                        ////(from kill in dbContext.Deaths
                        //// where kill.AttackerCharacterId == characterId
                        ////    && kill.DeathEventType == DeathEventType.Kill
                        ////    && kill.IsHeadshot == true
                        ////    && (int)kill.AttackerWeaponId == (int)weaponsGroup.Key
                        ////    && kill.Timestamp >= startTime
                        //// select kill).Count()
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
                        .Where(w => w.Kills > 0)
                        .ToArray();
            }
        }
    }
}