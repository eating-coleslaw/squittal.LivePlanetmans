using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Data.DataConfigurations
{
    public class CharacterTimeConfiguration : IEntityTypeConfiguration<CharacterTime>
    {
        public void Configure(EntityTypeBuilder<CharacterTime> builder)
        {
            builder.ToTable("CharacterTime");

            builder.HasKey(e => e.CharacterId);

            builder
                .HasOne(e => e.Character)
                .WithOne(e => e.Time)
                .HasForeignKey<CharacterTime>(e => e.CharacterId);
        }
    }
}
