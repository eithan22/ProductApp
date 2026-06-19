using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infraesctructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class PagosRelacion1N : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pagos_OrdenId",
                table: "Pagos");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_OrdenId",
                table: "Pagos",
                column: "OrdenId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pagos_OrdenId",
                table: "Pagos");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_OrdenId",
                table: "Pagos",
                column: "OrdenId",
                unique: true);
        }
    }
}
