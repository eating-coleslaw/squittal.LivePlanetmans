using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public interface IWorldService : IUpdateable
    {
        Task<IEnumerable<World>> GetAllWorldsAsync();
        Task<World> GetWorld(int worldId);
    }
}
