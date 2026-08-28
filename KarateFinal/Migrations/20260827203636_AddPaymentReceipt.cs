using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KarateFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE TABLE IF NOT EXISTS ""PaymentReceipts"" (
            ""Id"" SERIAL PRIMARY KEY,
            ""PlayerId"" INT NOT NULL,
            ""ClubId"" INT NOT NULL,
            ""Year"" INT NOT NULL,
            ""Month"" INT NOT NULL,
            ""Amount"" DECIMAL NOT NULL,
            ""PaidDate"" TIMESTAMP NOT NULL,
            ""Notes"" TEXT,
            ""CreatedBy"" TEXT NOT NULL DEFAULT ''
        )
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
