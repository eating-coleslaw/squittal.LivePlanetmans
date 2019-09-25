using DaybreakGames.Census;
using squittal.LivePlanetmans.Server.CensusServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.CensusServices
{
    public class CensusZone
    {
        public readonly ICensusQueryFactory _queryFactory;
        
        public CensusZone(ICensusQueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public async Task<IEnumerable<CensusZoneModel>> GetAllZones()
        {
            var query = _queryFactory.Create("zone");
            query.SetLanguage("en");

            query.ShowFields("zone_id", "code", "name", "description", "hex_size");

            return await query.GetBatchAsync<CensusZoneModel>();
        }
    }
}
