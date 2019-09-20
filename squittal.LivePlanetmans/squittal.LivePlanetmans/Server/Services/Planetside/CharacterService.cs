using System.Linq;
using System.Threading.Tasks;
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
        private readonly CensusCharacter _censusCharacter;

        public CharacterService(IDbContextHelper dbContextHelper, CensusCharacter censusCharacter)
        {
            _dbContextHelper = dbContextHelper;
            _censusCharacter = censusCharacter;
        }

        public async Task<Character> GetCharacterAsync(string characterId)
        {
            using (var factory = _dbContextHelper.GetFactory())
            {
                var dbContext = factory.GetDbContext();

                IQueryable<Character> characterQuery = dbContext.Characters.Where(e => e.Id == characterId);//.ToListAsync();

                var storeCharacter =  await characterQuery
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync();

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
    }
}
