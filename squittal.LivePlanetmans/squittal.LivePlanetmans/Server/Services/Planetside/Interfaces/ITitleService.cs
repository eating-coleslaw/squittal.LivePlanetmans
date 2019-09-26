using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public interface ITitleService
    {
        Task<IEnumerable<Title>> GetAllTitlesAsync();
        Task<Title> GetTitleAsync(int titleId);
        Task RefreshStore();
    }
}
