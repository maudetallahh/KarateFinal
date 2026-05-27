using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarateFinal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTournamentRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "TournamentRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "AttachedFile",
                table: "TournamentRegistrations",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachedFileName",
                table: "TournamentRegistrations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TournamentRegistrations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "TournamentRegistrations");

            migrationBuilder.DropColumn(
                name: "AttachedFile",
                table: "TournamentRegistrations");

            migrationBuilder.DropColumn(
                name: "AttachedFileName",
                table: "TournamentRegistrations");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TournamentRegistrations");
        }
    }
}
