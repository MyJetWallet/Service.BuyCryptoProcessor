using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BuyCryptoProcessor.Postgres.Migrations
{
    public partial class UpdatedErrorCodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentExecutionErrorCode",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.AddColumn<int>(
                name: "PaymentErrorCode",
                schema: "cryptobuy",
                table: "intentions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentErrorCode",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.AddColumn<int>(
                name: "PaymentExecutionErrorCode",
                schema: "cryptobuy",
                table: "intentions",
                type: "integer",
                nullable: true);
        }
    }
}
