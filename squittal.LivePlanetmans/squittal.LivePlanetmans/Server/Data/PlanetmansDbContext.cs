using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.Data
{
    public class PlanetmansDbContext : DbContext
    {
        public PlanetmansDbContext(DbContextOptions<PlanetmansDbContext> options)
            : base(options)
        {
        }

        #region Census DbSets
        public DbSet<Character> Characters { get; set; }
        public DbSet<CharacterLifetimeStat> CharacterLifetimeStats { get; set; }
        public DbSet<CharacterTime> CharacterTimes { get; set; }
        public DbSet<Faction> Factions { get; set; }
        public DbSet<Outfit> Outfits { get; set; }
        public DbSet<OutfitMember> OutfitMembers { get; set; }
        public DbSet<World> Worlds { get; set; }
        #endregion

        #region Stream Event DbSets
        public DbSet<Death> DeathEvents { get; set; }
        #endregion
    }
}
