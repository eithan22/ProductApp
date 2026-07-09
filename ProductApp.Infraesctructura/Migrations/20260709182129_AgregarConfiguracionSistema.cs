using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infraesctructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConfiguracionSistema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracionSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CantidadMinimaInventarioDefecto = table.Column<int>(type: "int", nullable: false),
                    DuracionTokenMinutos = table.Column<int>(type: "int", nullable: false),
                    NombreEmpresa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EstaEliminado = table.Column<bool>(type: "bit", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionSistema", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracionSistema");
        }
    }
}
