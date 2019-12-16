using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace squittal.LivePlanetmans.Shared.Models
{
    public class PlayerLoginMemoryCache
    {
        public MemoryCache Cache { get; set; }

        private static readonly string _cacheEntryPrefix = "_LoginKey";
        public PlayerLoginMemoryCache()
        {
            Cache = new MemoryCache(new MemoryCacheOptions
            {
                SizeLimit = 250 // 250 Player Logins
            });
        }

        public static string GetPlayerLoginKey(string characterId)
        {
            return $"{_cacheEntryPrefix}{characterId}";
        }
    }
}
