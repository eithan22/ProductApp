using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infraesctructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class agregandonuevacolumnaInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventarios_Productos_ProductoId",
                table: "Inventarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventarios",
                table: "Inventarios");

            migrationBuilder.RenameTable(
                name: "Inventarios",
                newName: "Inventario");

            migrationBuilder.RenameIndex(
                name: "IX_Inventarios_ProductoId",
                table: "Inventario",
                newName: "IX_Inventario_ProductoId");

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaActualizacion",
                table: "Inventario",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventario",
                table: "Inventario",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventario_Productos_ProductoId",
                table: "Inventario",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventario_Productos_ProductoId",
                table: "Inventario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventario",
                table: "Inventario");

            migrationBuilder.DropColumn(
                name: "UltimaActualizacion",
                table: "Inventario");

            migrationBuilder.RenameTable(
                name: "Inventario",
                newName: "Inventarios");

            migrationBuilder.RenameIndex(
                name: "IX_Inventario_ProductoId",
                table: "Inventarios",
                newName: "IX_Inventarios_ProductoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventarios",
                table: "Inventarios",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventarios_Productos_ProductoId",
                table: "Inventarios",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
