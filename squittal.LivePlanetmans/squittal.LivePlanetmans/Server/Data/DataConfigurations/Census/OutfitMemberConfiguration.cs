using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Data.DataConfigurations
{
    public class OutfitMemberConfiguration : IEntityTypeConfiguration<OutfitMember>
    {
        public void Configure(EntityTypeBuilder<OutfitMember> builder)
        {
            builder.ToTable("OutfitMember");

            builder.HasKey(e => e.CharacterId);

            builder
                .HasOne(e => e.Character)
                .WithOne(e => e.OutfitMember)
                .HasForeignKey<OutfitMember>(e => e.CharacterId);

            builder
                .Ignore(e => e.Outfit);
        }
    }
}
