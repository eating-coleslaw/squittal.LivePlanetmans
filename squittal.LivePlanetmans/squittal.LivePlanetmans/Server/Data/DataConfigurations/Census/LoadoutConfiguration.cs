using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Data.DataConfigurations
{
    public class LoadoutConfiguration : IEntityTypeConfiguration<Loadout>
    {
        public void Configure(EntityTypeBuilder<Loadout> builder)
        {
            builder.ToTable("Loadout");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Ignore(e => e.Profile);
        }
    }
}
