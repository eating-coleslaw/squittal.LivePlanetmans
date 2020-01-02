using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public interface IWorldService
    {
        Task<IEnumerable<World>> GetAllWorldsAsync();
        Task<World> GetWorldAsync(int worldId);
        Task RefreshStore();
    }
}
