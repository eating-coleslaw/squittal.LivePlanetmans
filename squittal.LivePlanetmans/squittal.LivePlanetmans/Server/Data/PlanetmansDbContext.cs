using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Server.Data.DataConfigurations;
using squittal.LivePlanetmans.Shared.Models;

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
        public DbSet<Item> Items { get; set; }
        public DbSet<ItemCategory> ItemCategories { get; set; }
        public DbSet<Outfit> Outfits { get; set; }
        public DbSet<OutfitMember> OutfitMembers { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<World> Worlds { get; set; }
        public DbSet<Zone> Zones { get; set; }
        #endregion

        #region Stream Event DbSets
        public DbSet<Death> Deaths { get; set; }
        public DbSet<PlayerLogin> PlayerLogins { get; set; }
        public DbSet<PlayerLogout> PlayerLogouts { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region Census Configuration
            builder.ApplyConfiguration(new CharacterConfiguration());
            builder.ApplyConfiguration(new CharacterLifetimeStatConfiguration());
            builder.ApplyConfiguration(new CharacterTimeConfiguration());
            builder.ApplyConfiguration(new FactionConfiguration());
            builder.ApplyConfiguration(new ItemConfiguration());
            builder.ApplyConfiguration(new ItemCategoryConfiguration());
            builder.ApplyConfiguration(new OutfitConfiguration());
            builder.ApplyConfiguration(new OutfitMemberConfiguration());
            builder.ApplyConfiguration(new TitleConfiguration());
            builder.ApplyConfiguration(new WorldConfiguration());
            builder.ApplyConfiguration(new ZoneConfiguration());
            #endregion

            #region Stream Configuration
            builder.ApplyConfiguration(new DeathConfiguration());
            builder.ApplyConfiguration(new PlayerLoginConfiguration());
            builder.ApplyConfiguration(new PlayerLogoutConfiguration());
            #endregion
        }
    }
}
