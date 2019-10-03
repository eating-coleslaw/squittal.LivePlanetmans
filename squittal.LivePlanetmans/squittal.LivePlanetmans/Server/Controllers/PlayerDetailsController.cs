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
    public class PlayerDetailsController : ControllerBase
    {
        private readonly ICharacterService _characterService;
        private readonly IDbContextHelper _dbContextHelper;
        private readonly ILogger<PlayerDetailsController> _logger;

        public PlayerDetailsController(ICharacterService characterService, IDbContextHelper dbContextHelper, ILogger<PlayerDetailsController> logger)
        {
            _characterService = characterService;
            _dbContextHelper = dbContextHelper;
            _logger = logger;
        }

        [HttpGet("{characterId}")]
        public async Task<ActionResult<Character>> GetPlayerDetailsAsync(string characterId)
        {

            return await _characterService.GetCharacterAsync(characterId);
        }

        [HttpGet("kills/{characterId}")]
        public async Task<ActionResult<IEnumerable<PlayerKillboardItem>>> GetPlayerDeaths(string characterId) //, int rows = 20)
        {
            int rows = 50;

            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<PlayerKillboardItem> query =
                    from death in dbContext.Deaths

                    join weapon in dbContext.Items
                      on death.AttackerWeaponId equals weapon.Id into weaponsQ
                    from weapon in weaponsQ.DefaultIfEmpty()

                    join zone in dbContext.Zones
                      on death.ZoneId equals zone.Id into zonesQ
                    from zone in zonesQ.DefaultIfEmpty()

                    where death.Timestamp >= startTime
                       && ( death.AttackerCharacterId == characterId
                            || death.CharacterId == characterId)
                    select new PlayerKillboardItem()
                    {
                        VictimId = death.CharacterId,
                        VictimName = (from c in dbContext.Characters
                                      where c.Id == death.CharacterId
                                      select c.Name).FirstOrDefault(),
                        VictimFactionId = death.CharacterFactionId,
                        VictimOutfitAlias = (from o in dbContext.Outfits
                                             where o.Id == death.CharacterOutfitId
                                             select o.Alias).FirstOrDefault(),
                        VictimBattleRank = (from c in dbContext.Characters
                                            where c.Id == death.CharacterId
                                            select c.BattleRank).FirstOrDefault(),
                        VictimPrestigeLevel = (from c in dbContext.Characters
                                               where c.Id == death.CharacterId
                                               select c.PrestigeLevel).FirstOrDefault(),
                        VictimWorldId = (from c in dbContext.Characters
                                         where c.Id == death.CharacterId
                                         select c.WorldId).FirstOrDefault(),
                        IsHeadshot = death.IsHeadshot,
                        VictimLoadoutId = death.CharacterLoadoutId,
                        AttackerId = death.AttackerCharacterId,
                        AttackerName = (from c in dbContext.Characters
                                        where c.Id == death.AttackerCharacterId
                                        select c.Name).FirstOrDefault(),
                        AttackerFactionId = death.AttackerFactionId,
                        AttackerOutfitAlias = (from o in dbContext.Outfits
                                             where o.Id == death.AttackerOutfitId
                                             select o.Alias).FirstOrDefault(),
                        AttackerBattleRank = (from c in dbContext.Characters
                                            where c.Id == death.AttackerCharacterId
                                            select c.BattleRank).FirstOrDefault(),
                        AttackerPrestigeLevel = (from c in dbContext.Characters
                                                 where c.Id == death.AttackerCharacterId
                                                 select c.PrestigeLevel).FirstOrDefault(),
                        AttackerLoadoutId = death.AttackerLoadoutId,
                        AttackerWeaponId = death.AttackerWeaponId,
                        AttackerWeaponName = weapon.Name,
                        IsAttackerWeaponVehicle = weapon.IsVehicleWeapon,
                        ZoneId = zone.Id,
                        ZoneName = zone.Name,
                        KillTimestamp = death.Timestamp
                    };

                return await query
                    .AsNoTracking()
                    .OrderByDescending(k => k.KillTimestamp)
                    //.Take(rows)
                    .ToArrayAsync();
            }
        }

        [HttpGet("stats/{characterId}")]
        public async Task<ActionResult<PlayerHourlyStatsData>> GetHourlyCharacterStats(string characterId)
        {
            DateTime nowUtc = DateTime.UtcNow;
            DateTime startTime = nowUtc - TimeSpan.FromHours(1);

            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<PlayerHourlyStatsData> query =
                    //from death in dbContext.Deaths
                    from character in dbContext.Characters

                      join outfitMember in dbContext.OutfitMembers
                        on character.Id equals outfitMember.CharacterId into outfitMemberQ
                      from outfitMember in outfitMemberQ.DefaultIfEmpty()

                      join outfit in dbContext.Outfits
                        on outfitMember.OutfitId equals outfit.Id into outfitQ
                      from outfit in outfitQ.DefaultIfEmpty()

                      join faction in dbContext.Factions
                        on character.FactionId equals faction.Id

                      join title in dbContext.Titles
                        on character.TitleId equals title.Id into titleQ
                      from title in titleQ.DefaultIfEmpty()

                      join world in dbContext.Worlds
                        on character.WorldId equals world.Id

                      //join login in dbContext.PlayerLogins
                      //  on character.Id equals login.CharacterId into loginQ
                      //from login in loginQ.DefaultIfEmpty()

                      //join logout in dbContext.PlayerLogouts
                      //  on character.Id equals logout.CharacterId into logoutQ
                      //from logout in logoutQ.DefaultIfEmpty()

                    where character.Id == characterId
                    //where death.Timestamp >= startTime
                       //&& ( death.AttackerCharacterId == characterId
                            //|| death.CharacterId == characterId)
                    //group death by death.AttackerCharacterId into playerGroup
                    select new PlayerHourlyStatsData()
                    {
                        PlayerId = characterId,
                        PlayerName = character.Name,
                        OutfitAlias = outfit.Alias, // ?? string.Empty,
                        OutfitName = outfit.Name, // ?? string.Empty,
                        OutfitRankName = outfitMember.Rank, // ?? string.Empty,
                        FactionId = character.FactionId,
                        FactionName = faction.Name,
                        BattleRank = character.BattleRank,
                        PrestigeLevel = character.PrestigeLevel,
                        TitleName = title.Name,
                        WorldId = world.Id,
                        WorldName = world.Name,

                        LatestLoginTime = (from login in dbContext.PlayerLogins
                                           where login.CharacterId == characterId
                                           orderby login.Timestamp descending
                                           select login.Timestamp).FirstOrDefault(),

                        LatestLogoutTime = (from logout in dbContext.PlayerLogouts
                                            where logout.CharacterId == characterId
                                            orderby logout.Timestamp descending
                                            select logout.Timestamp).FirstOrDefault(),

                        LatestDeathEventTime = (from death in dbContext.Deaths
                                           where (death.AttackerCharacterId == characterId
                                                 || death.CharacterId == characterId)
                                              && death.WorldId == character.WorldId
                                           orderby death.Timestamp
                                           select death.Timestamp).FirstOrDefault(),

                        QueryStartTime = startTime,

                        QueryNowUtc = nowUtc,

                        Kills = (from k in dbContext.Deaths
                                 where k.AttackerCharacterId == characterId
                                    && k.DeathEventType == DeathEventType.Kill
                                    //&& k.AttackerCharacterId != k.CharacterId
                                    //&& ((k.AttackerFactionId != k.CharacterFactionId)
                                    //     || k.AttackerFactionId == 4 || k.CharacterFactionId == 4
                                    //     || k.CharacterFactionId == null || k.CharacterFactionId == 0) //Nanite Systems
                                    && k.Timestamp >= startTime
                                    && k.WorldId == character.WorldId
                                 select k).Count(),
                        Deaths = (from d in dbContext.Deaths
                                  where d.CharacterId == characterId
                                     && d.Timestamp >= startTime
                                     && d.WorldId == character.WorldId
                                  select d).Count(),
                        Headshots = (from h in dbContext.Deaths
                                     where h.IsHeadshot == true
                                        && h.AttackerCharacterId == characterId
                                        && h.DeathEventType == DeathEventType.Kill
                                        //&& h.AttackerCharacterId != h.CharacterId
                                        //&& ( (h.AttackerFactionId != h.CharacterFactionId)
                                        //     || h.AttackerFactionId == 4 || h.CharacterFactionId == 4
                                        //     || h.CharacterFactionId == null || h.CharacterFactionId == 0) //Nanite Systems
                                        && h.Timestamp >= startTime
                                        && h.WorldId == character.WorldId
                                     select h).Count(),
                        TeamKills = (from tk in dbContext.Deaths
                                     where tk.AttackerCharacterId == characterId
                                        && tk.DeathEventType == DeathEventType.Teamkill
                                        //&& tk.AttackerCharacterId != tk.CharacterId
                                        //&& tk.AttackerFactionId == tk.CharacterFactionId
                                        && tk.Timestamp >= startTime
                                        && tk.WorldId == character.WorldId
                                     select tk).Count(),
                        Suicides = (from s in dbContext.Deaths
                                    where s.CharacterId == characterId
                                       && s.DeathEventType == DeathEventType.Suicide
                                       //&& s.AttackerCharacterId == s.CharacterId
                                       && s.Timestamp >= startTime
                                       && s.WorldId == character.WorldId
                                    select s).Count()
                    };

                var playerStats = await query.AsNoTracking().FirstOrDefaultAsync();

                if (playerStats.LatestLoginTime != null)
                {

                    //IQueryable<SessionKillCount> sessionKillsQuery = //(from k in dbContext.Deaths
                    ////                                     where k.AttackerCharacterId == characterId
                    ////                                        && k.AttackerCharacterId != k.CharacterId
                    ////                                        && ((k.AttackerFactionId != k.CharacterFactionId)
                    ////                                             || k.AttackerFactionId == 4 || k.CharacterFactionId == 4
                    ////                                             || k.CharacterFactionId == null || k.CharacterFactionId == 0) //Nanite Systems
                    ////                                        && k.Timestamp >= playerStats.LatestLoginTime
                    ////                                     select k).Count();

                    //from death in dbContext.Deaths
                    //where death.AttackerCharacterId == characterId
                    //select new SessionKillCount()
                    //{
                    //    SessionKills = (from k in dbContext.Deaths
                    //                    where k.AttackerCharacterId == characterId
                    //                       && k.AttackerCharacterId != k.CharacterId
                    //                       && ((k.AttackerFactionId != k.CharacterFactionId)
                    //                            || k.AttackerFactionId == 4 || k.CharacterFactionId == 4
                    //                            || k.CharacterFactionId == null || k.CharacterFactionId == 0) //Nanite Systems
                    //                       && k.Timestamp >= playerStats.LatestLoginTime
                    //                    select k).Count()
                    //};

                    //playerStats.SessionKills = await sessionKillsQuery.AsNotTracking().FirstOrDefaultAsync();

                    DateTime sessionStartTime = (playerStats.LatestLoginTime ?? startTime); //(playerStats.LatestLoginTime != null) ? (playerStats.LatestLoginTime ?? startTime) : startTime;
                    DateTime sessionEndTime = (playerStats.LatestLogoutTime ?? nowUtc); // (playerStats.LatestLogoutTime != null) ? (playerStats.LatestLogoutTime ?? nowUtc) : nowUtc;

                    if (sessionEndTime <= sessionStartTime)
                    {
                        sessionEndTime = nowUtc;
                    }


                    //if (playerStats.IsOnline == true)
                    //{
                    //    sessionEndTime = nowUtc;
                    //    sessionStartTime = playerStats.LatestLoginTime ?? startTime;
                    //}
                    //else
                    //{
                    //    if (playerStats.LatestLogoutTime >= playerStats.LatestLoginTime && playerStats.LatestLogoutTime != null)
                    //    {
                    //        sessionEndTime = playerStats.LatestLogoutTime ?? nowUtc;
                    //        sessionStartTime = playerStats.LatestLoginTime ?? startTime;

                    //    }
                    //    else
                    //    {
                    //        sessionEndTime = 
                    //    }

                    //    sessionStartTime 
                    //}



                    playerStats.SessionKills = await dbContext.Deaths.CountAsync(death => death.AttackerCharacterId == characterId
                                                                                       && death.DeathEventType == DeathEventType.Kill
                                                                                       //&& death.CharacterId != characterId
                                                                                       //&& ((death.AttackerFactionId != death.CharacterFactionId)
                                                                                       //    || death.AttackerFactionId == 4 || death.CharacterFactionId == 4
                                                                                       //    || death.CharacterFactionId == null || death.CharacterFactionId == 0) //Nanite Systems
                                                                                       && death.Timestamp >= sessionStartTime
                                                                                       && death.Timestamp <= sessionEndTime);
                }

                //return await query.AsNoTracking().FirstOrDefaultAsync();
                return playerStats;
            }

        }

        private class SessionKillCount
        {
            public int SessionKills { get; set; }
        }
    }
}
