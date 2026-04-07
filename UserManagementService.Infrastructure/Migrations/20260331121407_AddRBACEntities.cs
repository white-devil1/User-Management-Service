using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRBACEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "pages");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "pages");

            migrationBuilder.RenameIndex(
                name: "IX_rolepermissions_PermissionId",
                table: "rolepermissions",
                newName: "ix_rolepermissions_permissionid");

            migrationBuilder.RenameIndex(
                name: "IX_permissions_PageId",
                table: "permissions",
                newName: "ix_permissions_pageid");

            migrationBuilder.RenameIndex(
                name: "IX_permissions_ActionId",
                table: "permissions",
                newName: "ix_permissions_actionid");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "rolepermissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "rolepermissions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedAt",
                table: "rolepermissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<string>(
                name: "AssignedBy",
                table: "rolepermissions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "rolepermissions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "rolepermissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "rolepermissions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "rolepermissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "rolepermissions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "refreshtokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "refreshtokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "refreshtokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "refreshtokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "refreshtokens",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "permissions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "permissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "permissions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "permissions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "permissions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "pages",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "pages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "pages",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "pages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "pages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "pages",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "otpverifications",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "aspnetusers",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "aspnetusers",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "aspnetroles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "aspnetroles",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "aspnetroles",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "aspnetroles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "aspnetroles",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "aspnetroles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "aspnetroles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "aspnetroles",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "appactions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "appactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "appactions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "appactions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "appactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "appactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "appactions",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_rolepermissions_isdeleted",
                table: "rolepermissions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "ix_rolepermissions_roleid",
                table: "rolepermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_appid",
                table: "permissions",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_isdeleted",
                table: "permissions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_isenabled",
                table: "permissions",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "ix_pages_appid",
                table: "pages",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "ix_pages_isactive",
                table: "pages",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "ix_pages_isdeleted",
                table: "pages",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "ix_aspnetroles_isdeleted",
                table: "aspnetroles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "ix_appactions_isactive",
                table: "appactions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "ix_appactions_isdeleted",
                table: "appactions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "ix_appactions_pageid",
                table: "appactions",
                column: "PageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_rolepermissions_isdeleted",
                table: "rolepermissions");

            migrationBuilder.DropIndex(
                name: "ix_rolepermissions_roleid",
                table: "rolepermissions");

            migrationBuilder.DropIndex(
                name: "ix_permissions_appid",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "ix_permissions_isdeleted",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "ix_permissions_isenabled",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "ix_pages_appid",
                table: "pages");

            migrationBuilder.DropIndex(
                name: "ix_pages_isactive",
                table: "pages");

            migrationBuilder.DropIndex(
                name: "ix_pages_isdeleted",
                table: "pages");

            migrationBuilder.DropIndex(
                name: "ix_aspnetroles_isdeleted",
                table: "aspnetroles");

            migrationBuilder.DropIndex(
                name: "ix_appactions_isactive",
                table: "appactions");

            migrationBuilder.DropIndex(
                name: "ix_appactions_isdeleted",
                table: "appactions");

            migrationBuilder.DropIndex(
                name: "ix_appactions_pageid",
                table: "appactions");

            migrationBuilder.DropColumn(
                name: "AssignedAt",
                table: "rolepermissions");

            migrationBuilder.DropColumn(
                name: "AssignedBy",
                table: "rolepermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "rolepermissions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "rolepermissions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "rolepermissions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "rolepermissions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "rolepermissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "refreshtokens");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "refreshtokens");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "refreshtokens");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "refreshtokens");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "refreshtokens");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "pages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "pages");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "pages");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "pages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "pages");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "pages");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "aspnetroles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "aspnetroles");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "aspnetroles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "aspnetroles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "aspnetroles");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "aspnetroles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "aspnetroles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "appactions");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "appactions");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "appactions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "appactions");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "appactions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "appactions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "appactions");

            migrationBuilder.RenameIndex(
                name: "ix_rolepermissions_permissionid",
                table: "rolepermissions",
                newName: "IX_rolepermissions_PermissionId");

            migrationBuilder.RenameIndex(
                name: "ix_permissions_pageid",
                table: "permissions",
                newName: "IX_permissions_PageId");

            migrationBuilder.RenameIndex(
                name: "ix_permissions_actionid",
                table: "permissions",
                newName: "IX_permissions_ActionId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "rolepermissions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "rolepermissions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<bool>(
                name: "IsEnabled",
                table: "permissions",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "pages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "pages",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "otpverifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                table: "aspnetusers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "aspnetusers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "aspnetroles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
