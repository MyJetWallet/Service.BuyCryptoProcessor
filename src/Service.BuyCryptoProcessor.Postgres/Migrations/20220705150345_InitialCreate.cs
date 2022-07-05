using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BuyCryptoProcessor.Postgres.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cryptobuy");

            migrationBuilder.CreateTable(
                name: "intentions",
                schema: "cryptobuy",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    BrandId = table.Column<string>(type: "text", nullable: true),
                    BrokerId = table.Column<string>(type: "text", nullable: true),
                    WalletId = table.Column<string>(type: "text", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WorkflowState = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    RetriesCount = table.Column<int>(type: "integer", nullable: false),
                    ServiceWalletId = table.Column<string>(type: "text", nullable: true),
                    DepositProfileId = table.Column<string>(type: "text", nullable: true),
                    PaymentMethod = table.Column<int>(type: "integer", nullable: false),
                    PaymentDetails = table.Column<string>(type: "text", nullable: true),
                    PaymentAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentAsset = table.Column<string>(type: "text", nullable: true),
                    ProvidedCryptoAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    ProvidedCryptoAsset = table.Column<string>(type: "text", nullable: true),
                    CircleDepositId = table.Column<long>(type: "bigint", nullable: false),
                    DepositOperationId = table.Column<string>(type: "text", nullable: true),
                    DepositTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)),
                    DepositIntegration = table.Column<string>(type: "text", nullable: true),
                    CardId = table.Column<string>(type: "text", nullable: true),
                    CardLast4 = table.Column<string>(type: "text", nullable: true),
                    DepositCheckoutLink = table.Column<string>(type: "text", nullable: true),
                    SwapProfileId = table.Column<string>(type: "text", nullable: true),
                    BuyFeeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BuyFeeAsset = table.Column<string>(type: "text", nullable: true),
                    BuyAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    BuyAsset = table.Column<string>(type: "text", nullable: true),
                    FeeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    FeeAsset = table.Column<string>(type: "text", nullable: true),
                    SwapFeeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    SwapFeeAsset = table.Column<string>(type: "text", nullable: true),
                    PreviewQuoteId = table.Column<string>(type: "text", nullable: true),
                    PreviewConvertTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)),
                    QuotePrice = table.Column<decimal>(type: "numeric", nullable: false),
                    ExecuteQuoteId = table.Column<string>(type: "text", nullable: true),
                    ExecuteTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intentions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_intentions_ClientId",
                schema: "cryptobuy",
                table: "intentions",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_intentions_Status",
                schema: "cryptobuy",
                table: "intentions",
                column: "Status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "intentions",
                schema: "cryptobuy");
        }
    }
}
