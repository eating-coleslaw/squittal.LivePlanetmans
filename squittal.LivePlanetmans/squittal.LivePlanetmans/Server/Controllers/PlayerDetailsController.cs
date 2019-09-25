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
        public async Task<ActionResult<IEnumerable<PlayerKillboardItem>>> GetPlayDeaths(string characterId) //, int rows = 20)
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
                        Kills = (from k in dbContext.Deaths
                                 where k.AttackerCharacterId == characterId
                                    && k.AttackerCharacterId != k.CharacterId
                                    && ( (k.AttackerFactionId != k.CharacterFactionId)
                                         || k.AttackerFactionId == 4 || k.CharacterFactionId == 4
                                         || k.CharacterFactionId == null || k.CharacterFactionId == 0) //Nanite Systems
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
                                        && h.AttackerCharacterId != h.CharacterId
                                        && ( (h.AttackerFactionId != h.CharacterFactionId)
                                             || h.AttackerFactionId == 4 || h.CharacterFactionId == 4
                                             || h.CharacterFactionId == null || h.CharacterFactionId == 0) //Nanite Systems
                                        && h.Timestamp >= startTime
                                        && h.WorldId == character.WorldId
                                     select h).Count(),
                        TeamKills = (from tk in dbContext.Deaths
                                     where tk.AttackerCharacterId == characterId
                                        && tk.AttackerCharacterId != tk.CharacterId
                                        && tk.AttackerFactionId == tk.CharacterFactionId
                                        && tk.Timestamp >= startTime
                                        && tk.WorldId == character.WorldId
                                     select tk).Count(),
                        Suicides = (from s in dbContext.Deaths
                                    where s.CharacterId == characterId
                                       && s.AttackerCharacterId == s.CharacterId
                                       && s.Timestamp >= startTime
                                       && s.WorldId == character.WorldId
                                    select s).Count()
                    };

                return await query.AsNoTracking().FirstOrDefaultAsync();
            }
        }
    }
}
