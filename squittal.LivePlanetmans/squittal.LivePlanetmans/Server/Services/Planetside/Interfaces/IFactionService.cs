using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public interface IFactionService
    {
        Task<IEnumerable<Faction>> GetAllFactionsAsync();
        Task<Faction> GetFactionAsync(int factionId);
        Task RefreshStore();
    }
}
