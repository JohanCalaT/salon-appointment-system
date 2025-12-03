using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalonAppointmentSystem.ApiService._3.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstacionIdToHorariosAndUsaHorarioGenericoToEstaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Estaciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UsaHorarioGenerico",
                table: "Estaciones",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "ConfiguracionHorarios",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstacionId",
                table: "ConfiguracionHorarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "ConfiguracionHorarios",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Estaciones_Activa_BarberoId",
                table: "Estaciones",
                columns: new[] { "Activa", "BarberoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionHorarios_EstacionId",
                table: "ConfiguracionHorarios",
                column: "EstacionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionHorarios_EstacionId_DiaSemana_Activo",
                table: "ConfiguracionHorarios",
                columns: new[] { "EstacionId", "DiaSemana", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionHorarios_EstacionId_Tipo_Activo",
                table: "ConfiguracionHorarios",
                columns: new[] { "EstacionId", "Tipo", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionHorarios_Tipo",
                table: "ConfiguracionHorarios",
                column: "Tipo");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionHorarios_Estaciones_EstacionId",
                table: "ConfiguracionHorarios",
                column: "EstacionId",
                principalTable: "Estaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionHorarios_Estaciones_EstacionId",
                table: "ConfiguracionHorarios");

            migrationBuilder.DropIndex(
                name: "IX_Estaciones_Activa_BarberoId",
                table: "Estaciones");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionHorarios_EstacionId",
                table: "ConfiguracionHorarios");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionHorarios_EstacionId_DiaSemana_Activo",
                table: "ConfiguracionHorarios");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionHorarios_EstacionId_Tipo_Activo",
                table: "ConfiguracionHorarios");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionHorarios_Tipo",
                table: "ConfiguracionHorarios");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Estaciones");

            migrationBuilder.DropColumn(
                name: "UsaHorarioGenerico",
                table: "Estaciones");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "ConfiguracionHorarios");

            migrationBuilder.DropColumn(
                name: "EstacionId",
                table: "ConfiguracionHorarios");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "ConfiguracionHorarios");
        }
    }
}
