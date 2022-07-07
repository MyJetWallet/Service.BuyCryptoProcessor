using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.BuyCryptoProcessor.Postgres.Migrations
{
    public partial class CircleOperationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CircleRequestId",
                schema: "cryptobuy",
                table: "intentions",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CircleRequestId",
                schema: "cryptobuy",
                table: "intentions");
        }
    }
}
