using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using squittal.LivePlanetmans.Shared.Models;

namespace squittal.LivePlanetmans.Server.Data.DataConfigurations
{
    public class CharacterLifetimeStatConfiguration : IEntityTypeConfiguration<CharacterLifetimeStat>
    {
        public void Configure(EntityTypeBuilder<CharacterLifetimeStat> builder)
        {
            builder.ToTable("CharacterLifetimeStat");

            builder.HasKey(e => e.CharacterId);

            builder
                .HasOne(e => e.Character)
                .WithOne(e => e.LifetimeStats)
                .HasForeignKey<CharacterLifetimeStat>(e => e.CharacterId);

            builder.Property(e => e.AchievementCount).HasDefaultValue(0);
            builder.Property(e => e.AssistCount).HasDefaultValue(0);
            builder.Property(e => e.FacilityDefendedCount).HasDefaultValue(0);
            builder.Property(e => e.MedalCount).HasDefaultValue(0);
            builder.Property(e => e.SkillPoints).HasDefaultValue(0);
            builder.Property(e => e.WeaponDeaths).HasDefaultValue(0);
            builder.Property(e => e.WeaponFireCount).HasDefaultValue(0);
            builder.Property(e => e.WeaponHitCount).HasDefaultValue(0);
            builder.Property(e => e.WeaponPlayTime).HasDefaultValue(0);
            builder.Property(e => e.WeaponScore).HasDefaultValue(0);
        }
    }
}
