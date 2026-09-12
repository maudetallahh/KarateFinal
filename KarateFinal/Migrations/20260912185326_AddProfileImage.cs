using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarateFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "Players",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfileImageUpdatedAt",
                table: "Players",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImage",
                table: "Officials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfileImageUpdatedAt",
                table: "Officials",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LogoUpdatedAt",
                table: "Clubs",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ProfileImageUpdatedAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ProfileImage",
                table: "Officials");

            migrationBuilder.DropColumn(
                name: "ProfileImageUpdatedAt",
                table: "Officials");

            migrationBuilder.DropColumn(
                name: "LogoUpdatedAt",
                table: "Clubs");
        }
    }
}
