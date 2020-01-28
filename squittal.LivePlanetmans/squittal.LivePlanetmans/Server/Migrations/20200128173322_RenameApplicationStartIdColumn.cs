using Microsoft.EntityFrameworkCore.Migrations;

namespace squittal.LivePlanetmans.Server.Migrations
{
    public partial class RenameApplicationStartIdColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationStart",
                table: "ApplicationStart");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ApplicationStart");

            migrationBuilder.AddColumn<int>(
                name: "ApplicationStartId",
                table: "ApplicationStart",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationStart",
                table: "ApplicationStart",
                columns: new[] { "ApplicationStartId", "StartTimeUtc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationStart",
                table: "ApplicationStart");

            migrationBuilder.DropColumn(
                name: "ApplicationStartId",
                table: "ApplicationStart");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ApplicationStart",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationStart",
                table: "ApplicationStart",
                columns: new[] { "Id", "StartTimeUtc" });
        }
    }
}
