using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarateFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddClubFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FoundedYear",
                table: "Clubs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Clubs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "FoundedYear",
                table: "Clubs");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Clubs");
        }
    }
}
