using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infraesctructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class FixDetalleOrdenRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleOrden_Productos_ProductoId",
                table: "DetalleOrden");

            migrationBuilder.DropIndex(
                name: "IX_DetalleOrden_ProductoId",
                table: "DetalleOrden");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "DetalleOrden");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleOrden_ProductId",
                table: "DetalleOrden",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleOrden_Productos_ProductId",
                table: "DetalleOrden",
                column: "ProductId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetalleOrden_Productos_ProductId",
                table: "DetalleOrden");

            migrationBuilder.DropIndex(
                name: "IX_DetalleOrden_ProductId",
                table: "DetalleOrden");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "DetalleOrden",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleOrden_ProductoId",
                table: "DetalleOrden",
                column: "ProductoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DetalleOrden_Productos_ProductoId",
                table: "DetalleOrden",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
