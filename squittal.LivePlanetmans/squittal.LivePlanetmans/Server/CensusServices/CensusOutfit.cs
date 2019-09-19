using DaybreakGames.Census;
using squittal.LivePlanetmans.Server.CensusServices.Models;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.CensusServices
{
    public class CensusOutfit
    {
        private readonly ICensusQueryFactory _queryFactory;

        public CensusOutfit(ICensusQueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public async Task<CensusOutfitModel> GetOutfit(string outfitId)
        {
            var query = _queryFactory.Create("outfit");

            query.ShowFields("outfit_id", "name", "alias", "time_created", "leader_character_id", "member_count");

            query.Where("outfit_id").Equals(outfitId);

            return await query.GetAsync<CensusOutfitModel>();
        }
    }
}
