using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Central_Hub.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedCreditLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCreditBalance",
                table: "ClientCompanies");

            migrationBuilder.DropColumn(
                name: "TotalCreditsPurchased",
                table: "ClientCompanies");

            migrationBuilder.DropColumn(
                name: "TotalCreditsUsed",
                table: "ClientCompanies");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "CreditTransactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CreditTransactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "CreditTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CreditBatches",
                columns: table => new
                {
                    BatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    OriginalAmount = table.Column<int>(type: "int", nullable: false),
                    RemainingAmount = table.Column<int>(type: "int", nullable: false),
                    LoadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditBatches", x => x.BatchId);
                    table.ForeignKey(
                        name: "FK_CreditBatches_ClientCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "ClientCompanies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_BatchId",
                table: "CreditTransactions",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditBatches_CompanyId",
                table: "CreditBatches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditBatches_ExpiryDate",
                table: "CreditBatches",
                column: "ExpiryDate");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditTransactions_CreditBatches_BatchId",
                table: "CreditTransactions",
                column: "BatchId",
                principalTable: "CreditBatches",
                principalColumn: "BatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditTransactions_CreditBatches_BatchId",
                table: "CreditTransactions");

            migrationBuilder.DropTable(
                name: "CreditBatches");

            migrationBuilder.DropIndex(
                name: "IX_CreditTransactions_BatchId",
                table: "CreditTransactions");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "CreditTransactions");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "CreditTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "CreditTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentCreditBalance",
                table: "ClientCompanies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCreditsPurchased",
                table: "ClientCompanies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCreditsUsed",
                table: "ClientCompanies",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
