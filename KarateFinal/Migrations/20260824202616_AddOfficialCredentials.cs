using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarateFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficialCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogin",
                table: "Officials",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LicenseExpiry",
                table: "Officials",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseFile",
                table: "Officials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "Officials",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Officials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Officials",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLogin",
                table: "Officials");

            migrationBuilder.DropColumn(
                name: "LicenseExpiry",
                table: "Officials");

            migrationBuilder.DropColumn(
                name: "LicenseFile",
                table: "Officials");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Officials");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Officials");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Officials");
        }
    }
}
