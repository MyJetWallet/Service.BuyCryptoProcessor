using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BuyCryptoProcessor.Postgres.Migrations
{
    public partial class AddedErrorCodes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentErrorCode",
                schema: "cryptobuy",
                table: "intentions",
                newName: "PaymentExecutionErrorCode");

            migrationBuilder.AddColumn<int>(
                name: "PaymentCreationErrorCode",
                schema: "cryptobuy",
                table: "intentions",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentCreationErrorCode",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.RenameColumn(
                name: "PaymentExecutionErrorCode",
                schema: "cryptobuy",
                table: "intentions",
                newName: "PaymentErrorCode");
        }
    }
}
