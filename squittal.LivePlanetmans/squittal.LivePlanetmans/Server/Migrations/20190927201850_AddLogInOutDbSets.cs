using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace squittal.LivePlanetmans.Server.Migrations
{
    public partial class AddLogInOutDbSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerLoginEvent",
                columns: table => new
                {
                    CharacterId = table.Column<string>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    WorldId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerLoginEvent", x => new { x.Timestamp, x.CharacterId });
                });

            migrationBuilder.CreateTable(
                name: "PlayerLogoutEvent",
                columns: table => new
                {
                    CharacterId = table.Column<string>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    WorldId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerLogoutEvent", x => new { x.Timestamp, x.CharacterId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerLoginEvent");

            migrationBuilder.DropTable(
                name: "PlayerLogoutEvent");
        }
    }
}
