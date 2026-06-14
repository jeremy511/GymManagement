using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipTypesAndClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Memberships");

            migrationBuilder.RenameColumn(
                name: "Start",
                table: "Memberships",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Memberships",
                newName: "PricePaid");

            migrationBuilder.RenameColumn(
                name: "End",
                table: "Memberships",
                newName: "EndDate");

            migrationBuilder.AddColumn<Guid>(
                name: "MembershipTypeId",
                table: "Memberships",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MembershipTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GymId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipTypes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MembershipTypes");

            migrationBuilder.DropColumn(
                name: "MembershipTypeId",
                table: "Memberships");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Memberships",
                newName: "Start");

            migrationBuilder.RenameColumn(
                name: "PricePaid",
                table: "Memberships",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Memberships",
                newName: "End");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
