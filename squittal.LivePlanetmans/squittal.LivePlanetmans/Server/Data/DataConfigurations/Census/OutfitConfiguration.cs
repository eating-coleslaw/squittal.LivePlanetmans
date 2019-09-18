using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Data.DataConfigurations

{
    public class OutfitConfiguration : IEntityTypeConfiguration<Outfit>
    {
        public void Configure(EntityTypeBuilder<Outfit> builder)
        {
            builder.ToTable("Outfit");

            builder.HasKey(e => e.Id);

            builder
                .Ignore(e => e.Faction)
                .Ignore(e => e.World)
                .Ignore(e => e.LeaderCharacter);
        }
    }
}
