using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Data.DataConfigurations
{
    public class TitleConfiguration : IEntityTypeConfiguration<Title>
    {
        public void Configure(EntityTypeBuilder<Title> builder)
        {
            builder.ToTable("Title");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedNever();
        }
    }
}
