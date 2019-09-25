﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using squittal.LivePlanetmans.Server.Data;

namespace squittal.LivePlanetmans.Server.Migrations
{
    [DbContext(typeof(PlanetmansDbContext))]
    partial class PlanetmansDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0-rc1.19456.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.Character", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("BattleRank")
                        .HasColumnType("int");

                    b.Property<int>("BattleRankPercentToNext")
                        .HasColumnType("int");

                    b.Property<int>("CertsEarned")
                        .HasColumnType("int");

                    b.Property<int>("FactionId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PrestigeLevel")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int>("TitleId")
                        .HasColumnType("int");

                    b.Property<int>("WorldId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Character");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.CharacterLifetimeStat", b =>
                {
                    b.Property<string>("CharacterId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("AchievementCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("AssistCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("FacilityDefendedCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("MedalCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("SkillPoints")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("WeaponDeaths")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("WeaponFireCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("WeaponHitCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("WeaponPlayTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.Property<int?>("WeaponScore")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(0);

                    b.HasKey("CharacterId");

                    b.ToTable("CharacterLifetimeStat");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.CharacterTime", b =>
                {
                    b.Property<string>("CharacterId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastLoginDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastSaveDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("MinutesPlayed")
                        .HasColumnType("int");

                    b.HasKey("CharacterId");

                    b.ToTable("CharacterTime");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.Death", b =>
                {
                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("CharacterId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AttackerCharacterId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("AttackerFactionId")
                        .HasColumnType("int");

                    b.Property<int?>("AttackerFireModeId")
                        .HasColumnType("int");

                    b.Property<int?>("AttackerLoadoutId")
                        .HasColumnType("int");

                    b.Property<string>("AttackerOutfitId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("AttackerVehicleId")
                        .HasColumnType("int");

                    b.Property<int?>("AttackerWeaponId")
                        .HasColumnType("int");

                    b.Property<int?>("CharacterFactionId")
                        .HasColumnType("int");

                    b.Property<int?>("CharacterLoadoutId")
                        .HasColumnType("int");

                    b.Property<string>("CharacterOutfitId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsCritical")
                        .HasColumnType("bit");

                    b.Property<bool>("IsHeadshot")
                        .HasColumnType("bit");

                    b.Property<int?>("VehicleId")
                        .HasColumnType("int");

                    b.Property<int>("WorldId")
                        .HasColumnType("int");

                    b.Property<int>("ZoneId")
                        .HasColumnType("int");

                    b.HasKey("Timestamp", "CharacterId", "AttackerCharacterId");

                    b.HasIndex("AttackerFactionId");

                    b.HasIndex("CharacterFactionId");

                    b.ToTable("DeathEvent");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.Faction", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("CodeTag")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ImageId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("UserSelectable")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Faction");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.Item", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("FactionId")
                        .HasColumnType("int");

                    b.Property<int?>("ImageId")
                        .HasColumnType("int");

                    b.Property<bool>("IsVehicleWeapon")
                        .HasColumnType("bit");

                    b.Property<int?>("ItemCategoryId")
                        .HasColumnType("int");

                    b.Property<int?>("ItemTypeId")
                        .HasColumnType("int");

                    b.Property<int?>("MaxStackSize")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Item");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.ItemCategory", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ItemCategory");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.Outfit", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Alias")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("FactionId")
                        .HasColumnType("int");

                    b.Property<string>("LeaderCharacterId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("MemberCount")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("WorldId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Outfit");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.OutfitMember", b =>
                {
                    b.Property<string>("CharacterId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime?>("MemberSinceDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OutfitId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Rank")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("RankOrdinal")
                        .HasColumnType("int");

                    b.HasKey("CharacterId");

                    b.ToTable("OutfitMember");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.World", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("World");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.Zone", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Code")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("HexSize")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Zone");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.CharacterLifetimeStat", b =>
                {
                    b.HasOne("squittal.LivePlanetmans.Shared.Models.Character", "Character")
                        .WithOne("LifetimeStats")
                        .HasForeignKey("squittal.LivePlanetmans.Shared.Models.CharacterLifetimeStat", "CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.CharacterTime", b =>
                {
                    b.HasOne("squittal.LivePlanetmans.Shared.Models.Character", "Character")
                        .WithOne("Time")
                        .HasForeignKey("squittal.LivePlanetmans.Shared.Models.CharacterTime", "CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.Death", b =>
                {
                    b.HasOne("squittal.LivePlanetmans.Shared.Models.Faction", "AttackerFaction")
                        .WithMany()
                        .HasForeignKey("AttackerFactionId");

                    b.HasOne("squittal.LivePlanetmans.Shared.Models.Faction", "CharacterFaction")
                        .WithMany()
                        .HasForeignKey("CharacterFactionId");
                });

            modelBuilder.Entity("squittal.LivePlanetmans.Shared.Models.OutfitMember", b =>
                {
                    b.HasOne("squittal.LivePlanetmans.Shared.Models.Character", "Character")
                        .WithOne("OutfitMember")
                        .HasForeignKey("squittal.LivePlanetmans.Shared.Models.OutfitMember", "CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
