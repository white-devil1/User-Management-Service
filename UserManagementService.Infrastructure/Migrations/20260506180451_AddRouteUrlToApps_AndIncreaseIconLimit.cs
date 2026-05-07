using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteUrlToApps_AndIncreaseIconLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "apps",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RouteUrl",
                table: "apps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteUrl",
                table: "apps");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "apps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000,
                oldNullable: true);
        }
    }
}
