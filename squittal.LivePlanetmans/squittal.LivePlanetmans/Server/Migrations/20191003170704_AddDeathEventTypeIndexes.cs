using Microsoft.EntityFrameworkCore.Migrations;

namespace squittal.LivePlanetmans.Server.Migrations
{
    public partial class AddDeathEventTypeIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DeathEvent_Timestamp_AttackerCharacterId_DeathEventType",
                table: "DeathEvent",
                columns: new[] { "Timestamp", "AttackerCharacterId", "DeathEventType" });

            migrationBuilder.CreateIndex(
                name: "IX_DeathEvent_Timestamp_CharacterId_DeathEventType",
                table: "DeathEvent",
                columns: new[] { "Timestamp", "CharacterId", "DeathEventType" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DeathEvent_Timestamp_AttackerCharacterId_DeathEventType",
                table: "DeathEvent");

            migrationBuilder.DropIndex(
                name: "IX_DeathEvent_Timestamp_CharacterId_DeathEventType",
                table: "DeathEvent");
        }
    }
}
