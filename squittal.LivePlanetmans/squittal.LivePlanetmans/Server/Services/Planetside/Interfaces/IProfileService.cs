using squittal.LivePlanetmans.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public interface IProfileService
    {
        Task<IEnumerable<Profile>> GetAllProfilesAsync();
        Task<IEnumerable<Loadout>> GetAllLoadoutsAsync();
        Task<Profile> GetProfileFromLoadoutIdAsync(int loadoutId);
        Task<Dictionary<int, Profile>> GetLoadoutMapping();
        Task RefreshStore();
    }
}
