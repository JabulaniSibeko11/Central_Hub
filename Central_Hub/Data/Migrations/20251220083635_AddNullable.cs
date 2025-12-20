using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Central_Hub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemoRequests_ClientInstance_ClientInstanceId",
                table: "DemoRequests");

            migrationBuilder.AlterColumn<int>(
                name: "ClientInstanceId",
                table: "DemoRequests",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_DemoRequests_ClientInstance_ClientInstanceId",
                table: "DemoRequests",
                column: "ClientInstanceId",
                principalTable: "ClientInstance",
                principalColumn: "ClientInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DemoRequests_ClientInstance_ClientInstanceId",
                table: "DemoRequests");

            migrationBuilder.AlterColumn<int>(
                name: "ClientInstanceId",
                table: "DemoRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DemoRequests_ClientInstance_ClientInstanceId",
                table: "DemoRequests",
                column: "ClientInstanceId",
                principalTable: "ClientInstance",
                principalColumn: "ClientInstanceId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
