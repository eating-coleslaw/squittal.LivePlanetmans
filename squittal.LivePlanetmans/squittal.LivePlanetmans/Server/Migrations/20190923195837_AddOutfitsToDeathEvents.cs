using Microsoft.EntityFrameworkCore.Migrations;

namespace squittal.LivePlanetmans.Server.Migrations
{
    public partial class AddOutfitsToDeathEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttackerOutfitId",
                table: "DeathEvent",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CharacterOutfitId",
                table: "DeathEvent",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeathEvent_AttackerFactionId",
                table: "DeathEvent",
                column: "AttackerFactionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeathEvent_AttackerOutfitId",
                table: "DeathEvent",
                column: "AttackerOutfitId");

            migrationBuilder.CreateIndex(
                name: "IX_DeathEvent_CharacterFactionId",
                table: "DeathEvent",
                column: "CharacterFactionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeathEvent_CharacterOutfitId",
                table: "DeathEvent",
                column: "CharacterOutfitId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeathEvent_Faction_AttackerFactionId",
                table: "DeathEvent",
                column: "AttackerFactionId",
                principalTable: "Faction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeathEvent_Outfit_AttackerOutfitId",
                table: "DeathEvent",
                column: "AttackerOutfitId",
                principalTable: "Outfit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeathEvent_Faction_CharacterFactionId",
                table: "DeathEvent",
                column: "CharacterFactionId",
                principalTable: "Faction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DeathEvent_Outfit_CharacterOutfitId",
                table: "DeathEvent",
                column: "CharacterOutfitId",
                principalTable: "Outfit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeathEvent_Faction_AttackerFactionId",
                table: "DeathEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_DeathEvent_Outfit_AttackerOutfitId",
                table: "DeathEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_DeathEvent_Faction_CharacterFactionId",
                table: "DeathEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_DeathEvent_Outfit_CharacterOutfitId",
                table: "DeathEvent");

            migrationBuilder.DropIndex(
                name: "IX_DeathEvent_AttackerFactionId",
                table: "DeathEvent");

            migrationBuilder.DropIndex(
                name: "IX_DeathEvent_AttackerOutfitId",
                table: "DeathEvent");

            migrationBuilder.DropIndex(
                name: "IX_DeathEvent_CharacterFactionId",
                table: "DeathEvent");

            migrationBuilder.DropIndex(
                name: "IX_DeathEvent_CharacterOutfitId",
                table: "DeathEvent");

            migrationBuilder.DropColumn(
                name: "AttackerOutfitId",
                table: "DeathEvent");

            migrationBuilder.DropColumn(
                name: "CharacterOutfitId",
                table: "DeathEvent");
        }
    }
}
