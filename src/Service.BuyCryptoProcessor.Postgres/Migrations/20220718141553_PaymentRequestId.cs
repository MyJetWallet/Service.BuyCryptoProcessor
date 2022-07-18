using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BuyCryptoProcessor.Postgres.Migrations
{
    public partial class PaymentRequestId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CircleRequestId",
                schema: "cryptobuy",
                table: "intentions",
                newName: "PaymentProcessorRequestId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentProcessorRequestId",
                schema: "cryptobuy",
                table: "intentions",
                newName: "CircleRequestId");
        }
    }
}
