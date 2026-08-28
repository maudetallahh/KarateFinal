using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarateFinal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Clubs_ClubId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ClubId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ClubId",
                table: "Messages",
                newName: "SenderPlayerId");

            migrationBuilder.AddColumn<bool>(
                name: "IsGroupMessage",
                table: "Messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReceiverClubId",
                table: "Messages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiverPlayerId",
                table: "Messages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiverRole",
                table: "Messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SenderClubId",
                table: "Messages",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SenderOfficialId",
                table: "Messages",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGroupMessage",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReceiverClubId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReceiverPlayerId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ReceiverRole",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SenderClubId",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "SenderOfficialId",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "SenderPlayerId",
                table: "Messages",
                newName: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ClubId",
                table: "Messages",
                column: "ClubId");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Clubs_ClubId",
                table: "Messages",
                column: "ClubId",
                principalTable: "Clubs",
                principalColumn: "Id");
        }
    }
}
