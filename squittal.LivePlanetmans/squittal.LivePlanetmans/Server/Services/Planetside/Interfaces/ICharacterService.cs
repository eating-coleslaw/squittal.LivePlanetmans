using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Services.Planetside
{
    public interface ICharacterService
    {
        Task<Character> GetCharacterAsync(string characterId);
        Task<OutfitMember> GetCharactersOutfitAsync(string characterId);
        Task<CharacterTime> GetCharacterTimesAsync(string characterId);
        Task<Character> UpdateCharacterAsync(string characterId);
    }
}
