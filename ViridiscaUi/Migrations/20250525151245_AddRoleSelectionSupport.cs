using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleSelectionSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "uid",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "password_hash",
                value: "$2a$11$8EPP7eDbOSFPG6YcVEWzsu81jRo550.5.b9INI7muBAMpfwa3ftcS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "uid",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "password_hash",
                value: "$2a$11$uKGNJ6wGN2KAHRxZKGVMhOJjKJkp85JqL4v6O5CcKGi9JWgKGeLMa");
        }
    }
}
