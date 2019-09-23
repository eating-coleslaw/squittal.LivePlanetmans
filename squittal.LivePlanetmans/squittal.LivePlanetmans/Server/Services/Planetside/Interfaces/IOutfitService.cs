using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public interface IOutfitService
    {
        Task<Outfit> GetOutfitAsync(string outfitId);
        Task<Outfit> GetOutfitByAlias(string alias);
        Task<OutfitMember> UpdateCharacterOutfitMembership(Character character);

    }
}
