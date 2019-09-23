﻿using System.Linq;
using System.Threading.Tasks;
using DaybreakGames.Census.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squittal.LivePlanetmans.Server.CensusServices;
using squittal.LivePlanetmans.Server.CensusServices.Models;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public class OutfitService : IOutfitService
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly CensusOutfit _censusOutfit;
        private readonly CensusCharacter _censusCharacter;
        private readonly ILogger<OutfitService> _logger;

        public OutfitService(IDbContextHelper dbContextHelper, CensusOutfit censusOutfit, CensusCharacter censusCharacter, ILogger<OutfitService> logger)
        {
            _dbContextHelper = dbContextHelper;
            _censusOutfit = censusOutfit;
            _censusCharacter = censusCharacter;
            _logger = logger;
        }

        public async Task<Outfit> GetOutfitAsync(string outfitId)
        {
            return await GetOutfitInternalAsync(outfitId);
        }

        public async Task<Outfit> GetOutfitByAlias(string alias)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.Outfits.AsNoTracking().FirstOrDefaultAsync(o => o.Alias.ToLower() == alias.ToLower());
            }
        }

        public async Task<OutfitMember> UpdateCharacterOutfitMembership(Character character)
        {
            OutfitMember outfitMember;
            CensusOutfitMemberModel membership;
            
            try
            {
                membership = await _censusCharacter.GetCharacterOutfitMembership(character.Id);
            }
            catch (CensusConnectionException)
            {
                return null;
            }

            if (membership == null)
            {
                await RemoveOutfitMemberAsync(character.Id);
                return null;
            }

            var outfit = await GetOutfitInternalAsync(membership.OutfitId, character);
            if (outfit == null)
            {
                _logger.LogError(84624, $"Unable to resolve outfit {membership.OutfitId} for character {character.Id}");
                return null;
            }

            outfitMember = new OutfitMember
            {
                OutfitId = membership.OutfitId,
                CharacterId = membership.CharacterId,
                MemberSinceDate = membership.MemberSinceDate,
                Rank = membership.Rank,
                RankOrdinal = membership.RankOrdinal
            };

            outfitMember = await UpsertOutfitMemberAsync(outfitMember);
            return outfitMember;
        }
        
        private async Task<Outfit> GetOutfitInternalAsync(string outfitId, Character member = null)
        {
            Outfit outfit;
            
            outfit = await GetKnownOutfitAsync(outfitId);

            if (outfit == null)
            {
                return null;
            }

            // These are null if outfit was retrieved from the census API
            if (outfit.WorldId == null || outfit.FactionId == null)
            {
                outfit = await ResolveOutfitDetailsAsync(outfit, member);
                await UpsertOutfitAsync(outfit);
            }

            return outfit;
        }

        // Returns the specified outfit from the database, if it exists. Otherwise,
        // queries for it in the DBG census.
        private async Task<Outfit> GetKnownOutfitAsync(string outfitId)
        {
            Outfit outfit;
            
            outfit = await GetDbOutfitAsync(outfitId);

            if (outfit == null)
            {
                try
                {
                    outfit = await GetCensusOutfit(outfitId);
                }
                catch (CensusConnectionException)
                {
                    return null;
                }
            }

            return outfit;
        }

        private async Task<Outfit> GetDbOutfitAsync(string outfitId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                return await dbContext.Outfits.AsNoTracking().FirstOrDefaultAsync(o => o.Id == outfitId);
            }
        }


        private async Task<Outfit> GetCensusOutfit(string outfitId)
        {
            var censusOutfit = await _censusOutfit.GetOutfitAsync(outfitId);

            return censusOutfit == null
                ? null
                : new Outfit
            {
                Id = censusOutfit.OutfitId,
                Alias = censusOutfit.Alias,
                Name = censusOutfit.Name,
                LeaderCharacterId = censusOutfit.LeaderCharacterId,
                CreatedDate = censusOutfit.TimeCreated,
                MemberCount = censusOutfit.MemberCount
            };
        }

        private async Task<Outfit> ResolveOutfitDetailsAsync(Outfit outfit, Character member)
        {
            if (member != null)
            {
                outfit.WorldId = member.WorldId;
                outfit.FactionId = member.FactionId;
            }
            else
            {
                var leader = await _censusCharacter.GetCharacter(outfit.LeaderCharacterId);
                outfit.WorldId = leader.WorldId;
                outfit.FactionId = member.FactionId;
            }

            return outfit;
        }

        private async Task<Outfit> UpsertOutfitAsync(Outfit newEntity)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storeEntity = await dbContext.Outfits.AsNoTracking().FirstOrDefaultAsync(o => o.Id == newEntity.Id);

                if (storeEntity == null)
                {
                    await dbContext.AddAsync(newEntity);
                }
                else
                {
                    storeEntity = newEntity;
                    dbContext.Update(storeEntity);
                }
                await dbContext.SaveChangesAsync();
            }
            return newEntity;
        }

        private async Task<OutfitMember> UpsertOutfitMemberAsync(OutfitMember newEntity)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storeEntity = await dbContext.OutfitMembers.AsNoTracking().FirstOrDefaultAsync(m => m.CharacterId == newEntity.CharacterId);

                if (storeEntity == null)
                {
                    await dbContext.AddAsync(newEntity);
                }
                else
                {
                    storeEntity = newEntity;
                    dbContext.Update(storeEntity);
                }
                await dbContext.SaveChangesAsync();
            }
            return newEntity;
        }

        private async Task RemoveOutfitMemberAsync(string characterId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storeMembership = await dbContext.OutfitMembers.FindAsync(characterId);

                if (storeMembership != null)
                {
                    dbContext.OutfitMembers.Remove(storeMembership);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
