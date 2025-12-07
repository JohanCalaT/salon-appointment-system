using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonAppointmentSystem.ApiService._3.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservaAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Reservas_EstacionId_FechaHora_Estado",
                table: "Reservas",
                newName: "IX_Reserva_EstacionId_FechaHora_Estado");

            // Agregar columna sin índice único primero
            migrationBuilder.AddColumn<string>(
                name: "CodigoReserva",
                table: "Reservas",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "TEMP0000");

            // Generar códigos únicos para reservas existentes
            migrationBuilder.Sql(@"
                UPDATE Reservas
                SET CodigoReserva = UPPER(
                    SUBSTRING(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''), 1, 8)
                )
                WHERE CodigoReserva = 'TEMP0000'
            ");

            migrationBuilder.AddColumn<string>(
                name: "CreadaPor",
                table: "Reservas",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RolCreador",
                table: "Reservas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Crear índice único después de que todos los registros tengan códigos únicos
            migrationBuilder.CreateIndex(
                name: "IX_Reservas_CodigoReserva",
                table: "Reservas",
                column: "CodigoReserva",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reservas_CodigoReserva",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "CodigoReserva",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "CreadaPor",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "RolCreador",
                table: "Reservas");

            migrationBuilder.RenameIndex(
                name: "IX_Reserva_EstacionId_FechaHora_Estado",
                table: "Reservas",
                newName: "IX_Reservas_EstacionId_FechaHora_Estado");
        }
    }
}
