using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViridiscaUi.Migrations
{
    /// <inheritdoc />
    public partial class MakeDateOfBirthNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_birth",
                table: "persons",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "uid",
                keyValue: new Guid("aaaabbbb-1111-1111-1111-111111111111"),
                column: "password_hash",
                value: "$2a$11$CgwKf1JhcTMiOw.bDm/cWOpHTZ2RrCeVTZekuhWApzUdeozIyLWeq");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "uid",
                keyValue: new Guid("bbbbbbbb-1111-1111-1111-111111111111"),
                column: "password_hash",
                value: "$2a$11$fvl0awRPrGMYgHufeKGUC.XAyHM6hX2Kg6O7q6kzQo.s/LZycBg9G");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "uid",
                keyValue: new Guid("cccccccc-1111-1111-1111-111111111111"),
                column: "password_hash",
                value: "$2a$11$LNUbX4mJMKl0SDhEOfNDGezODA2gYwRBMhbn2uBJQUhsdc4SoG6e.");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "uid",
                keyValue: new Guid("dddddddd-1111-1111-1111-111111111111"),
                column: "password_hash",
                value: "$2a$11$JESJ6UhVtZCcLQkGe69vserhozcTlHppW/h8bZ9/JWF5vSWJlIAU6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "date_of_birth",
                table: "persons",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "uid",
                keyValue: new Guid("aaaabbbb-1111-1111-1111-111111111111"),
                column: "password_hash",
                value: "$2a$11$k0ODYBCbIng48AKxSkhd8..4baR4aTkTME4d0hgiDHjMvmzlJteBu");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "uid",
                keyValue: new Guid("bbbbbbbb-1111-1111-1111-111111111111"),
                column: "password_hash",
                value: "$2a$11$h5g50RMYfnJyebkKM.g4GerEoAaAP2Lwip/PrsXlSd9MybB4600GC");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "uid",
                keyValue: new Guid("cccccccc-1111-1111-1111-111111111111"),
                column: "password_hash",
                value: "$2a$11$Z7iP6z0MgMUyCWa6BEeL5.6ij6XyLea7l8V4t3O8QoDJXrT9gsgEe");

            migrationBuilder.UpdateData(
                table: "accounts",
                keyColumn: "uid",
                keyValue: new Guid("dddddddd-1111-1111-1111-111111111111"),
                column: "password_hash",
                value: "$2a$11$k.ystkXxXPTB4yZy6mGvXOuVVLLk9al9UFRftBG6GDBC6K2g/tYEe");
        }
    }
}
