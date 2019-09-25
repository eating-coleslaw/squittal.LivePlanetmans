using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public interface IZoneService
    {
        Task<IEnumerable<Zone>> GetAllZonesAsync();
        Task<Zone> GetZoneAsync(int zoneId);
        Task RefreshStore();
    }
}
