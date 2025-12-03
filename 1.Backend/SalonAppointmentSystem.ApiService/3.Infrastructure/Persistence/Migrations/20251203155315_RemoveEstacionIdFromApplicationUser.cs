using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonAppointmentSystem.ApiService._3.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEstacionIdFromApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Estaciones_EstacionId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Estaciones_BarberoId",
                table: "Estaciones");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_EstacionId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EstacionId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Estaciones_BarberoId",
                table: "Estaciones",
                column: "BarberoId",
                unique: true,
                filter: "[BarberoId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Estaciones_AspNetUsers_BarberoId",
                table: "Estaciones",
                column: "BarberoId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estaciones_AspNetUsers_BarberoId",
                table: "Estaciones");

            migrationBuilder.DropIndex(
                name: "IX_Estaciones_BarberoId",
                table: "Estaciones");

            migrationBuilder.AddColumn<int>(
                name: "EstacionId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estaciones_BarberoId",
                table: "Estaciones",
                column: "BarberoId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_EstacionId",
                table: "AspNetUsers",
                column: "EstacionId",
                unique: true,
                filter: "[EstacionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Estaciones_EstacionId",
                table: "AspNetUsers",
                column: "EstacionId",
                principalTable: "Estaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
