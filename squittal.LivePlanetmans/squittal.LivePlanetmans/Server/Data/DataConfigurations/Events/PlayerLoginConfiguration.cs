using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Data.DataConfigurations
{
    public class PlayerLoginConfiguration : IEntityTypeConfiguration<PlayerLogin>
    {
        public void Configure(EntityTypeBuilder<PlayerLogin> builder)
        {
            builder.ToTable("PlayerLoginEvent");

            builder.HasKey(e => new
            {
                e.Timestamp,
                e.CharacterId
            });
        }
    }
}
