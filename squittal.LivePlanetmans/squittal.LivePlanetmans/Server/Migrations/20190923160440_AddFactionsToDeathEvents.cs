using Microsoft.EntityFrameworkCore.Migrations;

namespace squittal.LivePlanetmans.Server.Migrations
{
    public partial class AddFactionsToDeathEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttackerFactionId",
                table: "DeathEvent",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CharacterFactionId",
                table: "DeathEvent",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttackerFactionId",
                table: "DeathEvent");

            migrationBuilder.DropColumn(
                name: "CharacterFactionId",
                table: "DeathEvent");
        }
    }
}
