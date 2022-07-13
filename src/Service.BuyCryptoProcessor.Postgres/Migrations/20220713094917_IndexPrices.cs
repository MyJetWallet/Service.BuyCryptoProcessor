using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BuyCryptoProcessor.Postgres.Migrations
{
    public partial class IndexPrices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BuyAssetIndexPrice",
                schema: "cryptobuy",
                table: "intentions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BuyFeeAssetIndexPrice",
                schema: "cryptobuy",
                table: "intentions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaymentAssetIndexPrice",
                schema: "cryptobuy",
                table: "intentions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SwapFeeAmountConverted",
                schema: "cryptobuy",
                table: "intentions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SwapFeeAssetConverted",
                schema: "cryptobuy",
                table: "intentions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SwapFeeAssetIndexPrice",
                schema: "cryptobuy",
                table: "intentions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyAssetIndexPrice",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.DropColumn(
                name: "BuyFeeAssetIndexPrice",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.DropColumn(
                name: "PaymentAssetIndexPrice",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.DropColumn(
                name: "SwapFeeAmountConverted",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.DropColumn(
                name: "SwapFeeAssetConverted",
                schema: "cryptobuy",
                table: "intentions");

            migrationBuilder.DropColumn(
                name: "SwapFeeAssetIndexPrice",
                schema: "cryptobuy",
                table: "intentions");
        }
    }
}
