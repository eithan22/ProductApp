using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infraesctructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AgregarIndicesUnicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true,
                filter: "[EstaEliminado] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Usuarios_Username",
                table: "Usuarios",
                column: "Username",
                unique: true,
                filter: "[EstaEliminado] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Proveedores_Correo",
                table: "Proveedores",
                column: "Correo",
                unique: true,
                filter: "[EstaEliminado] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Proveedores_Nombre",
                table: "Proveedores",
                column: "Nombre",
                unique: true,
                filter: "[EstaEliminado] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Productos_Nombre_CategoriaId",
                table: "Productos",
                columns: new[] { "Nombre", "CategoriaId" },
                unique: true,
                filter: "[EstaEliminado] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Clientes_Cedula",
                table: "Clientes",
                column: "Cedula",
                unique: true,
                filter: "[EstaEliminado] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Clientes_Correo",
                table: "Clientes",
                column: "Correo",
                unique: true,
                filter: "[EstaEliminado] = 0");

            migrationBuilder.CreateIndex(
                name: "UX_Categorias_Nombre",
                table: "Categorias",
                column: "Nombre",
                unique: true,
                filter: "[EstaEliminado] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "UX_Usuarios_Username",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "UX_Proveedores_Correo",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "UX_Proveedores_Nombre",
                table: "Proveedores");

            migrationBuilder.DropIndex(
                name: "UX_Productos_Nombre_CategoriaId",
                table: "Productos");

            migrationBuilder.DropIndex(
                name: "UX_Clientes_Cedula",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "UX_Clientes_Correo",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "UX_Categorias_Nombre",
                table: "Categorias");
        }
    }
}
