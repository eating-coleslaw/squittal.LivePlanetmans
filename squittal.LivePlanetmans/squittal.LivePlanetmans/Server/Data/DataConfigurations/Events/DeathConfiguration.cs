using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Data.DataConfigurations
{
    public class DeathConfiguration : IEntityTypeConfiguration<Death>
    {
        public void Configure(EntityTypeBuilder<Death> builder)
        {
            builder.ToTable("DeathEvent");

            builder.HasKey(e => new
            {
                e.Timestamp,
                e.CharacterId,
                e.AttackerCharacterId
            });

            builder
                .Ignore(e => e.Character)
                .Ignore(e => e.AttackerCharacter)
                .Ignore(e => e.CharacterOutfit)
                .Ignore(e => e.AttackerOutfit)
                //.Ignore(e => e.AttackerVehicle)
                .Ignore(e => e.AttackerWeapon);
        }
    }
}
