using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Data.DataConfigurations
{
    public class ApplicationStartConfiguration : IEntityTypeConfiguration<ApplicationStart>
    {
        public void Configure(EntityTypeBuilder<ApplicationStart> builder)
        {
            builder.ToTable("ApplicationStart");

            builder.HasKey(e => new
            {
                e.ApplicationStartId//,
                //e.StartTimeUtc
            });
        }
    }
}
