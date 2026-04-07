using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagementService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_aspnetroles_scope",
                table: "aspnetroles",
                column: "Scope");

            migrationBuilder.CreateIndex(
                name: "ix_aspnetroles_tenantid_name_unique",
                table: "aspnetroles",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_aspnetroles_scope",
                table: "aspnetroles");

            migrationBuilder.DropIndex(
                name: "ix_aspnetroles_tenantid_name_unique",
                table: "aspnetroles");
        }
    }
}
