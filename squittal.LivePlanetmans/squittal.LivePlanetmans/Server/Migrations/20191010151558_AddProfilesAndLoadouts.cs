using Microsoft.EntityFrameworkCore.Migrations;

namespace squittal.LivePlanetmans.Server.Migrations
{
    public partial class AddProfilesAndLoadouts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Loadout",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    ProfileId = table.Column<int>(nullable: false),
                    FactionId = table.Column<int>(nullable: false),
                    CodeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loadout", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profile",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileTypeId = table.Column<int>(nullable: false),
                    FactionId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ImageId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profile", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Loadout");

            migrationBuilder.DropTable(
                name: "Profile");
        }
    }
}
