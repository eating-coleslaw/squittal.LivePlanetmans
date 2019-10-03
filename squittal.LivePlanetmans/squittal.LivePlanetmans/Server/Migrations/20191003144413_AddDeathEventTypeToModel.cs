using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.IO;

namespace squittal.LivePlanetmans.Server.Migrations
{
    public partial class AddDeathEventTypeToModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeathEventType",
                table: "DeathEvent",
                nullable: false,
                defaultValue: -1);

            var sqlFile = "Data/SQL/MigrationHelpers/Backfill_DeathEventType.sql";
            var basePath = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
            var filePath = Path.Combine(basePath, sqlFile);
            migrationBuilder.Sql(File.ReadAllText(filePath));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeathEventType",
                table: "DeathEvent");
        }
    }
}
