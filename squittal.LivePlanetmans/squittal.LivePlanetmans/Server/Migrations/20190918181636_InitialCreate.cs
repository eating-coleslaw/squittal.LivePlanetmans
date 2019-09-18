using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace squittal.LivePlanetmans.Server.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Character",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    FactionId = table.Column<int>(nullable: false),
                    TitleId = table.Column<int>(nullable: false),
                    WorldId = table.Column<int>(nullable: false),
                    BattleRank = table.Column<int>(nullable: false),
                    BattleRankPercentToNext = table.Column<int>(nullable: false),
                    CertsEarned = table.Column<int>(nullable: false),
                    PrestigeLevel = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Character", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeathEvent",
                columns: table => new
                {
                    CharacterId = table.Column<string>(nullable: false),
                    AttackerCharacterId = table.Column<string>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    WorldId = table.Column<int>(nullable: false),
                    ZoneId = table.Column<int>(nullable: false),
                    CharacterLoadoutId = table.Column<int>(nullable: true),
                    AttackerFireModeId = table.Column<int>(nullable: true),
                    AttackerLoadoutId = table.Column<int>(nullable: true),
                    AttackerVehicleId = table.Column<int>(nullable: true),
                    AttackerWeaponId = table.Column<int>(nullable: true),
                    VehicleId = table.Column<int>(nullable: true),
                    IsHeadshot = table.Column<bool>(nullable: false),
                    IsCritical = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeathEvent", x => new { x.Timestamp, x.CharacterId, x.AttackerCharacterId });
                });

            migrationBuilder.CreateTable(
                name: "Faction",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ImageId = table.Column<int>(nullable: true),
                    CodeTag = table.Column<string>(nullable: true),
                    UserSelectable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faction", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Outfit",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Alias = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    LeaderCharacterId = table.Column<string>(nullable: true),
                    MemberCount = table.Column<int>(nullable: false),
                    FactionId = table.Column<int>(nullable: true),
                    WorldId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outfit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "World",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_World", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CharacterLifetimeStat",
                columns: table => new
                {
                    CharacterId = table.Column<string>(nullable: false),
                    AchievementCount = table.Column<int>(nullable: true, defaultValue: 0),
                    AssistCount = table.Column<int>(nullable: true, defaultValue: 0),
                    FacilityDefendedCount = table.Column<int>(nullable: true, defaultValue: 0),
                    MedalCount = table.Column<int>(nullable: true, defaultValue: 0),
                    SkillPoints = table.Column<int>(nullable: true, defaultValue: 0),
                    WeaponDeaths = table.Column<int>(nullable: true, defaultValue: 0),
                    WeaponFireCount = table.Column<int>(nullable: true, defaultValue: 0),
                    WeaponHitCount = table.Column<int>(nullable: true, defaultValue: 0),
                    WeaponPlayTime = table.Column<int>(nullable: true, defaultValue: 0),
                    WeaponScore = table.Column<int>(nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterLifetimeStat", x => x.CharacterId);
                    table.ForeignKey(
                        name: "FK_CharacterLifetimeStat_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterTime",
                columns: table => new
                {
                    CharacterId = table.Column<string>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    LastSaveDate = table.Column<DateTime>(nullable: false),
                    LastLoginDate = table.Column<DateTime>(nullable: false),
                    MinutesPlayed = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterTime", x => x.CharacterId);
                    table.ForeignKey(
                        name: "FK_CharacterTime_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OutfitMember",
                columns: table => new
                {
                    CharacterId = table.Column<string>(nullable: false),
                    OutfitId = table.Column<string>(nullable: false),
                    MemberSinceDate = table.Column<DateTime>(nullable: true),
                    Rank = table.Column<string>(nullable: true),
                    RankOrdinal = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutfitMember", x => x.CharacterId);
                    table.ForeignKey(
                        name: "FK_OutfitMember_Character_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Character",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterLifetimeStat");

            migrationBuilder.DropTable(
                name: "CharacterTime");

            migrationBuilder.DropTable(
                name: "DeathEvent");

            migrationBuilder.DropTable(
                name: "Faction");

            migrationBuilder.DropTable(
                name: "Outfit");

            migrationBuilder.DropTable(
                name: "OutfitMember");

            migrationBuilder.DropTable(
                name: "World");

            migrationBuilder.DropTable(
                name: "Character");
        }
    }
}
