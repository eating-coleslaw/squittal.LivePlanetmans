using DaybreakGames.Census;
using squittal.LivePlanetmans.Server.CensusServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.CensusServices
{
    public class CensusLoadout
    {
        private readonly ICensusQueryFactory _queryFactory;

        public CensusLoadout(ICensusQueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public async Task<IEnumerable<CensusLoadoutModel>> GetAllLoadoutsAsync()
        {
            var query = _queryFactory.Create("loadout");

            query.ShowFields("loadout_id", "profile_id", "faction_id", "code_name");

            return await query.GetBatchAsync<CensusLoadoutModel>();
        }
    }
}
