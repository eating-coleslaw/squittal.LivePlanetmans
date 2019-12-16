using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DaybreakGames.Census.Exceptions;
using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Server.CensusServices;
using squittal.LivePlanetmans.Server.CensusServices.Models;
using squittal.LivePlanetmans.Server.Data;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public class CharacterService : ICharacterService
    {
        private readonly IDbContextHelper _dbContextHelper;
        private readonly IOutfitService _outfitService;
        private readonly CensusCharacter _censusCharacter;

        public CharacterService(IDbContextHelper dbContextHelper, IOutfitService outfitService, CensusCharacter censusCharacter)
        {
            _dbContextHelper = dbContextHelper;
            _outfitService = outfitService;
            _censusCharacter = censusCharacter;
        }

        public async Task<Character> GetCharacterAsync(string characterId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storeCharacter = await dbContext.Characters
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(e => e.Id == characterId);

                if (storeCharacter != null)
                {
                    return storeCharacter;
                }

                var character = await _censusCharacter.GetCharacter(characterId);

                if (character == null)
                {
                    return null;
                }

                var censusEntity = ConvertToDbModel(character);

                dbContext.Add(censusEntity);
                await dbContext.SaveChangesAsync();

                return censusEntity;

            }
        }

        public async Task<Character> UpdateCharacterAsync(string characterId)
        {
            // Update Character Details
            CensusCharacterModel censusCharacter;

            try
            {
                censusCharacter = await _censusCharacter.GetCharacter(characterId);
            }
            catch (CensusConnectionException)
            {
                return null;
            }

            if (censusCharacter == null)
            {
                return null;
            }

            var character = await UpsertCharacterAsync(ConvertToDbModel(censusCharacter));
            Debug.WriteLine($"Updated character {character.Name} [{character.Id}]");


            // Update Outfit Membership
            await _outfitService.UpdateCharacterOutfitMembership(character);

            return character;
        }

        private async Task<Character> UpsertCharacterAsync(Character newEntity)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var storeEntity = await dbContext.Characters
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(o => o.Id == newEntity.Id);

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

        public async Task<OutfitMember> GetCharactersOutfitAsync(string characterId)
        {
            var character = await GetCharacterAsync(characterId);
            if (character == null)
            {
                return null;
            }

            return await _outfitService.UpdateCharacterOutfitMembership(character);
        }

        public static Character ConvertToDbModel(CensusCharacterModel censusModel)
        {
            return new Character
            {
                Id = censusModel.CharacterId,
                Name = censusModel.Name.First,
                FactionId = censusModel.FactionId,
                TitleId = censusModel.TitleId,
                WorldId = censusModel.WorldId,
                BattleRank = censusModel.BattleRank.Value,
                BattleRankPercentToNext = censusModel.BattleRank.PercentToNext,
                CertsEarned = censusModel.Certs.EarnedPoints,
                PrestigeLevel = censusModel.PrestigeLevel
            };
        }

        public async Task<CharacterTime> GetCharacterTimesAsync(string characterId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                var censusCharacterTimes = await _censusCharacter.GetCharacterTimes(characterId);

                if (censusCharacterTimes == null)
                {
                    return null;
                }

                return new CharacterTime
                {
                    CharacterId = characterId,
                    CreatedDate = censusCharacterTimes.CreationDate,
                    LastSaveDate = censusCharacterTimes.LastSaveDate,
                    LastLoginDate = censusCharacterTimes.LastLoginDate,
                    MinutesPlayed = censusCharacterTimes.MinutesPlayed
                };
            }
        }
    }
}
