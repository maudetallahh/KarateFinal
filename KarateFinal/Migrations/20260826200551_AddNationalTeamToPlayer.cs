using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarateFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddNationalTeamToPlayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNationalTeam",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NationalTeamStatus",
                table: "Players",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNationalTeam",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "NationalTeamStatus",
                table: "Players");
        }
    }
}
