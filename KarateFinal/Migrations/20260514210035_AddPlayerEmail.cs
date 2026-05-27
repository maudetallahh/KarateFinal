using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarateFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Players",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Players");
        }
    }
}
