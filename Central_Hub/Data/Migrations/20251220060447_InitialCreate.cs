using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Central_Hub.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CentralUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentralUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientCompanies",
                columns: table => new
                {
                    CompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmailDomain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PhysicalAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyType = table.Column<int>(type: "int", nullable: false),
                    EstimatedEmployeeCount = table.Column<int>(type: "int", nullable: true),
                    LicenseKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LicenseIssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LicenseExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LicenseStatus = table.Column<int>(type: "int", nullable: false),
                    CurrentCreditBalance = table.Column<int>(type: "int", nullable: false),
                    TotalCreditsPurchased = table.Column<int>(type: "int", nullable: false),
                    TotalCreditsUsed = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCompanies", x => x.CompanyId);
                });

            migrationBuilder.CreateTable(
                name: "ClientInstance",
                columns: table => new
                {
                    ClientInstanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmailDomain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdminName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdminEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AdminPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AdminDepartment = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseKey = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LicenseIssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LicenseExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentCreditBalance = table.Column<int>(type: "int", nullable: false),
                    TotalCreditsPurchased = table.Column<int>(type: "int", nullable: false),
                    InstanceServerUrl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientInstance", x => x.ClientInstanceId);
                });

            migrationBuilder.CreateTable(
                name: "CompanyAdministrators",
                columns: table => new
                {
                    AdministratorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Surname = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPrimaryContact = table.Column<bool>(type: "bit", nullable: false),
                    ReceiveNotifications = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAdministrators", x => x.AdministratorId);
                    table.ForeignKey(
                        name: "FK_CompanyAdministrators_ClientCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "ClientCompanies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditTransactions",
                columns: table => new
                {
                    TransactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    CreditsAmount = table.Column<int>(type: "int", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientInstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_CreditTransactions_ClientCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "ClientCompanies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditTransactions_ClientInstance_ClientInstanceId",
                        column: x => x.ClientInstanceId,
                        principalTable: "ClientInstance",
                        principalColumn: "ClientInstanceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DemoRequests",
                columns: table => new
                {
                    DemoRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrganizationType = table.Column<int>(type: "int", nullable: false),
                    ContactPersonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrganizationEmailDomain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstimatedEmployeeCount = table.Column<int>(type: "int", nullable: true),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreferredDemoDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreferredTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    HearAboutUs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DemoScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DemoCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedSalesRep = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InternalNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConvertedToClient = table.Column<bool>(type: "bit", nullable: false),
                    ConversionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConvertedCompanyId = table.Column<int>(type: "int", nullable: true),
                    ClientInstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoRequests", x => x.DemoRequestId);
                    table.ForeignKey(
                        name: "FK_DemoRequests_ClientInstance_ClientInstanceId",
                        column: x => x.ClientInstanceId,
                        principalTable: "ClientInstance",
                        principalColumn: "ClientInstanceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LicenseRenewals",
                columns: table => new
                {
                    RenewalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PreviousExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProcessedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientInstanceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseRenewals", x => x.RenewalId);
                    table.ForeignKey(
                        name: "FK_LicenseRenewals_ClientCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "ClientCompanies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LicenseRenewals_ClientInstance_ClientInstanceId",
                        column: x => x.ClientInstanceId,
                        principalTable: "ClientInstance",
                        principalColumn: "ClientInstanceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyncLog",
                columns: table => new
                {
                    SyncLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientInstanceId = table.Column<int>(type: "int", nullable: false),
                    SyncDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncType = table.Column<int>(type: "int", nullable: false),
                    Success = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
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
                name: "IX_ClientCompanies_EmailDomain",
                table: "ClientCompanies",
                column: "EmailDomain");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompanies_LicenseKey",
                table: "ClientCompanies",
                column: "LicenseKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientInstance_LicenseKey",
                table: "ClientInstance",
                column: "LicenseKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAdministrators_CompanyId",
                table: "CompanyAdministrators",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAdministrators_Email",
                table: "CompanyAdministrators",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_ClientInstanceId",
                table: "CreditTransactions",
                column: "ClientInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTransactions_CompanyId",
                table: "CreditTransactions",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequests_ClientInstanceId",
                table: "DemoRequests",
                column: "ClientInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequests_Email",
                table: "DemoRequests",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_DemoRequests_Status",
                table: "DemoRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseRenewals_ClientInstanceId",
                table: "LicenseRenewals",
                column: "ClientInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_LicenseRenewals_CompanyId",
                table: "LicenseRenewals",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncLog_ClientInstanceId",
                table: "SyncLog",
                column: "ClientInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CentralUser");

            migrationBuilder.DropTable(
                name: "CompanyAdministrators");

            migrationBuilder.DropTable(
                name: "CreditTransactions");

            migrationBuilder.DropTable(
                name: "DemoRequests");

            migrationBuilder.DropTable(
                name: "LicenseRenewals");

            migrationBuilder.DropTable(
                name: "SyncLog");

            migrationBuilder.DropTable(
                name: "ClientCompanies");

            migrationBuilder.DropTable(
                name: "ClientInstance");
        }
    }
}
