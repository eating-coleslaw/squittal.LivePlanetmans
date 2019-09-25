using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public interface IItemService
    {
        Task<Item> GetItem(int itemId);
        Task<IEnumerable<Item>> GetItemsByCategoryId(int categoryId);
        Task RefreshStore();
    }
}
