using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BuyCryptoProcessor.Postgres.Migrations
{
    public partial class PaymentErrorCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentDetails",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.AddColumn<int>(
                name: "PaymentErrorCode",
                schema: "cryptobuy",
                table: "intentions",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentErrorCode",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.AddColumn<string>(
                name: "PaymentDetails",
                schema: "cryptobuy",
                table: "intentions",
                type: "text",
                nullable: true);
        }
    }
}
