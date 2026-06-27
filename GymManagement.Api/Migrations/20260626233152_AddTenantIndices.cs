using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🏢 CRITICAL: Índices en GymId para filtrado de tenant en todas las queries
            // Sin estos, cada query que filtra por GymId es O(n) - NECESARIO para producción
            migrationBuilder.CreateIndex(name: "IX_Payments_GymId", table: "Payments", column: "GymId");
            migrationBuilder.CreateIndex(name: "IX_Members_GymId", table: "Members", column: "GymId");
            migrationBuilder.CreateIndex(name: "IX_Memberships_GymId", table: "Memberships", column: "GymId");
            migrationBuilder.CreateIndex(name: "IX_Classes_GymId", table: "Classes", column: "GymId");
            migrationBuilder.CreateIndex(name: "IX_Reservations_GymId", table: "Reservations", column: "GymId");
            migrationBuilder.CreateIndex(name: "IX_MembershipTypes_GymId", table: "MembershipTypes", column: "GymId");
            migrationBuilder.CreateIndex(name: "IX_Users_GymId", table: "Users", column: "GymId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Payments_GymId", table: "Payments");
            migrationBuilder.DropIndex(name: "IX_Members_GymId", table: "Members");
            migrationBuilder.DropIndex(name: "IX_Memberships_GymId", table: "Memberships");
            migrationBuilder.DropIndex(name: "IX_Classes_GymId", table: "Classes");
            migrationBuilder.DropIndex(name: "IX_Reservations_GymId", table: "Reservations");
            migrationBuilder.DropIndex(name: "IX_MembershipTypes_GymId", table: "MembershipTypes");
            migrationBuilder.DropIndex(name: "IX_Users_GymId", table: "Users");
        }
    }
}
