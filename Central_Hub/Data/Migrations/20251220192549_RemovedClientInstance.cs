using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Central_Hub.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedClientInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditTransactions_ClientInstance_ClientInstanceId",
                table: "CreditTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_DemoRequests_ClientInstance_ClientInstanceId",
                table: "DemoRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LicenseRenewals_ClientInstance_ClientInstanceId",
                table: "LicenseRenewals");

            migrationBuilder.DropTable(
                name: "SyncLog");

            migrationBuilder.DropTable(
                name: "ClientInstance");

            migrationBuilder.DropIndex(
                name: "IX_LicenseRenewals_ClientInstanceId",
                table: "LicenseRenewals");

            migrationBuilder.DropIndex(
                name: "IX_DemoRequests_ClientInstanceId",
                table: "DemoRequests");

            migrationBuilder.DropIndex(
                name: "IX_CreditTransactions_ClientInstanceId",
                table: "CreditTransactions");

            migrationBuilder.DropColumn(
                name: "ClientInstanceId",
                table: "LicenseRenewals");

            migrationBuilder.DropColumn(
                name: "ClientInstanceId",
                table: "DemoRequests");

            migrationBuilder.DropColumn(
                name: "ClientInstanceId",
                table: "CreditTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientInstanceId",
                table: "LicenseRenewals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClientInstanceId",
                table: "DemoRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientInstanceId",
                table: "CreditTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ClientInstance",
                columns: table => new
                {
                    ClientInstanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdminEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdminName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdminPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CompanyAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentCreditBalance = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailDomain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InstanceServerUrl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LicenseIssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LicenseKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalCreditsPurchased = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientInstance", x => x.ClientInstanceId);
                });

            migrationBuilder.CreateTable(
                name: "SyncLog",
                columns: table => new
                {
                    SyncLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientInstanceId = table.Column<int>(type: "int", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    SyncDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncLog", x => x.SyncLogId);
                    table.ForeignKey(
                        name: "FK_SyncLog_ClientInstance_ClientInstanceId",
                        column: x => x.ClientInstanceId,
                        principalTable: "ClientInstance",
                        principalColumn: "ClientInstanceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicenseRenewals_ClientInstanceId",
                table: "LicenseRenewals",
                column: "ClientInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequests_ClientInstanceId",
                table: "DemoRequests",
                column: "ClientInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_ClientInstanceId",
                table: "CreditTransactions",
                column: "ClientInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLog_ClientInstanceId",
                table: "SyncLog",
                column: "ClientInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditTransactions_ClientInstance_ClientInstanceId",
                table: "CreditTransactions",
                column: "ClientInstanceId",
                principalTable: "ClientInstance",
                principalColumn: "ClientInstanceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DemoRequests_ClientInstance_ClientInstanceId",
                table: "DemoRequests",
                column: "ClientInstanceId",
                principalTable: "ClientInstance",
                principalColumn: "ClientInstanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_LicenseRenewals_ClientInstance_ClientInstanceId",
                table: "LicenseRenewals",
                column: "ClientInstanceId",
                principalTable: "ClientInstance",
                principalColumn: "ClientInstanceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
