using Microsoft.EntityFrameworkCore.Migrations;

namespace squittal.LivePlanetmans.Server.Migrations
{
    public partial class AddItemDbSets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeathEvent_Outfit_AttackerOutfitId",
                table: "DeathEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_DeathEvent_Outfit_CharacterOutfitId",
                table: "DeathEvent");

            migrationBuilder.DropIndex(
                name: "IX_DeathEvent_AttackerOutfitId",
                table: "DeathEvent");

            migrationBuilder.DropIndex(
                name: "IX_DeathEvent_CharacterOutfitId",
                table: "DeathEvent");

            migrationBuilder.AlterColumn<string>(
                name: "CharacterOutfitId",
                table: "DeathEvent",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttackerOutfitId",
                table: "DeathEvent",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ItemTypeId = table.Column<int>(nullable: true),
                    ItemCategoryId = table.Column<int>(nullable: true),
                    IsVehicleWeapon = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    FactionId = table.Column<int>(nullable: true),
                    MaxStackSize = table.Column<int>(nullable: true),
                    ImageId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemCategory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategory", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Item");

            migrationBuilder.DropTable(
                name: "ItemCategory");

            migrationBuilder.AlterColumn<string>(
                name: "CharacterOutfitId",
                table: "DeathEvent",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttackerOutfitId",
                table: "DeathEvent",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeathEvent_AttackerOutfitId",
                table: "DeathEvent",
                column: "AttackerOutfitId");

            migrationBuilder.CreateIndex(
                name: "IX_DeathEvent_CharacterOutfitId",
                table: "DeathEvent",
                column: "CharacterOutfitId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeathEvent_Outfit_AttackerOutfitId",
                table: "DeathEvent",
                column: "AttackerOutfitId",
                principalTable: "Outfit",
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
    }
}
