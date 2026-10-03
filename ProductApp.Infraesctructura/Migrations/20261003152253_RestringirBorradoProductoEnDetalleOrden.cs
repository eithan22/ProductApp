using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infraesctructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class RestringirBorradoProductoEnDetalleOrden : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleOrden_Productos_ProductId",
                table: "DetalleOrden");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleOrden_Productos_ProductId",
                table: "DetalleOrden",
                column: "ProductId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleOrden_Productos_ProductId",
                table: "DetalleOrden");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleOrden_Productos_ProductId",
                table: "DetalleOrden",
                column: "ProductId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
