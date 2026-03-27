using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infraesctructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class actualizarModelos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Clientes",
                newName: "Correo");

            migrationBuilder.AddColumn<bool>(
                name: "IsDisable",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisable",
                table: "Productos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisable",
                table: "Pagos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisable",
                table: "Ordenes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisable",
                table: "Inventarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisable",
                table: "DetalleOrden",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisable",
                table: "Clientes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisable",
                table: "Categorias",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisable",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IsDisable",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "IsDisable",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "IsDisable",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "IsDisable",
                table: "Inventarios");

            migrationBuilder.DropColumn(
                name: "IsDisable",
                table: "DetalleOrden");

            migrationBuilder.DropColumn(
                name: "IsDisable",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "IsDisable",
                table: "Categorias");

            migrationBuilder.RenameColumn(
                name: "Correo",
                table: "Clientes",
                newName: "Email");
        }
    }
}
