using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApp.Infraesctructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class RefactorizacionDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Edad",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "IsDisable",
                table: "Usuarios",
                newName: "EstaEliminado");

            migrationBuilder.RenameColumn(
                name: "IsDisable",
                table: "Productos",
                newName: "EstaEliminado");

            migrationBuilder.RenameColumn(
                name: "IsDisable",
                table: "Pagos",
                newName: "EstaEliminado");

            migrationBuilder.RenameColumn(
                name: "IsDisable",
                table: "Ordenes",
                newName: "EstaEliminado");

            migrationBuilder.RenameColumn(
                name: "IsDisable",
                table: "Inventario",
                newName: "EstaEliminado");

            migrationBuilder.RenameColumn(
                name: "IsDisable",
                table: "DetalleOrden",
                newName: "EstaEliminado");

            migrationBuilder.RenameColumn(
                name: "IsDisable",
                table: "Clientes",
                newName: "EstaEliminado");

            migrationBuilder.RenameColumn(
                name: "IsDisable",
                table: "Categorias",
                newName: "EstaEliminado");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "Usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificadoEn",
                table: "Usuarios",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "Productos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificadoEn",
                table: "Productos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "Pagos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificadoEn",
                table: "Pagos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "Ordenes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificadoEn",
                table: "Ordenes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "Inventario",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificadoEn",
                table: "Inventario",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "DetalleOrden",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificadoEn",
                table: "DetalleOrden",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "Clientes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificadoEn",
                table: "Clientes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreadoEn",
                table: "Categorias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModificadoEn",
                table: "Categorias",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ModificadoEn",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ModificadoEn",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "ModificadoEn",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "ModificadoEn",
                table: "Ordenes");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "Inventario");

            migrationBuilder.DropColumn(
                name: "ModificadoEn",
                table: "Inventario");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "DetalleOrden");

            migrationBuilder.DropColumn(
                name: "ModificadoEn",
                table: "DetalleOrden");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "ModificadoEn",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "CreadoEn",
                table: "Categorias");

            migrationBuilder.DropColumn(
                name: "ModificadoEn",
                table: "Categorias");

            migrationBuilder.RenameColumn(
                name: "EstaEliminado",
                table: "Usuarios",
                newName: "IsDisable");

            migrationBuilder.RenameColumn(
                name: "EstaEliminado",
                table: "Productos",
                newName: "IsDisable");

            migrationBuilder.RenameColumn(
                name: "EstaEliminado",
                table: "Pagos",
                newName: "IsDisable");

            migrationBuilder.RenameColumn(
                name: "EstaEliminado",
                table: "Ordenes",
                newName: "IsDisable");

            migrationBuilder.RenameColumn(
                name: "EstaEliminado",
                table: "Inventario",
                newName: "IsDisable");

            migrationBuilder.RenameColumn(
                name: "EstaEliminado",
                table: "DetalleOrden",
                newName: "IsDisable");

            migrationBuilder.RenameColumn(
                name: "EstaEliminado",
                table: "Clientes",
                newName: "IsDisable");

            migrationBuilder.RenameColumn(
                name: "EstaEliminado",
                table: "Categorias",
                newName: "IsDisable");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "Edad",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
