using FluentAssertions;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Entitis
{
    public class CategoriaTests
    {
        private static Categoria CrearCategoria()
            => new Categoria("Categoria Test", "Descripcion de la categoria");

        [Fact]
        public void Desactivar_EnCategoriaActiva_LaMarcaComoEliminada()
        {
            var categoria = CrearCategoria();

            categoria.Desactivar();

            categoria.EstaEliminado.Should().BeTrue();
        }

        [Fact]
        public void Desactivar_EnCategoriaYaDesactivada_LanzaEstadoInvalidoException()
        {
            var categoria = CrearCategoria();
            categoria.Desactivar();

            var accion = () => categoria.Desactivar();

            accion.Should().Throw<EstadoInvalidoException>();
        }

        [Fact]
        public void Activar_EnCategoriaDesactivada_LaDejaActiva()
        {
            var categoria = CrearCategoria();
            categoria.Desactivar();

            categoria.Activar();

            categoria.EstaEliminado.Should().BeFalse();
        }

        [Fact]
        public void Activar_EnCategoriaYaActiva_LanzaEstadoInvalidoException()
        {
            var categoria = CrearCategoria();

            var accion = () => categoria.Activar();

            accion.Should().Throw<EstadoInvalidoException>();
        }
    }
}
