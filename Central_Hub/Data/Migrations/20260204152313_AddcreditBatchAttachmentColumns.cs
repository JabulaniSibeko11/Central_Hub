using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Central_Hub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddcreditBatchAttachmentColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "CreditBatches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "CreditBatches",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "CreditBatches");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "CreditBatches");
        }
    }
}
