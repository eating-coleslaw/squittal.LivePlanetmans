using DaybreakGames.Census;
using squittal.LivePlanetmans.Server.CensusServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.CensusServices
{
    public class CensusItem
    {
        private readonly ICensusQueryFactory _queryFactory;

        public CensusItem(ICensusQueryFactory queryFactory)
        {
            _queryFactory = queryFactory;
        }

        public async Task<IEnumerable<CensusItemModel>> GetAllItems()
        {
            var query = _queryFactory.Create("item");
            query.SetLanguage("en");

            query.ShowFields("item_id", "item_type_id", "item_category_id", "is_vehicle_weapon", "name", "description", "faction_id", "max_stack_size", "image_id");

            return await query.GetBatchAsync<CensusItemModel>();
        }
    }
}
