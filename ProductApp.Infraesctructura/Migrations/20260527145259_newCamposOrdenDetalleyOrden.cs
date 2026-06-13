using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infraesctructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class newCamposOrdenDetalleyOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "DetalleOrden",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "DetalleOrden");
        }
    }
}
