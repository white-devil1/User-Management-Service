using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToApps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "apps",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "apps",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "apps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "apps",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "apps",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_apps_displayorder",
                table: "apps",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "ix_apps_isactive",
                table: "apps",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "ix_apps_isdeleted",
                table: "apps",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_apps_displayorder",
                table: "apps");

            migrationBuilder.DropIndex(
                name: "ix_apps_isactive",
                table: "apps");

            migrationBuilder.DropIndex(
                name: "ix_apps_isdeleted",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "apps");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "apps");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "apps",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "apps",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
