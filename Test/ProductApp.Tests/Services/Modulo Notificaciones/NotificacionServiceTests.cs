using FluentAssertions;
using Moq;
using ProductApp.Aplication.Mappers.Modulo_Notificaciones;
using ProductApp.Aplication.Services;
using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using Xunit;

namespace ProductApp.Tests.Services.Modulo_Notificaciones
{
    // NotificacionService no depende de navegaciones cargadas por EF: solo llama metodos
    // planos del repositorio, asi que se prueba con Moq. El control de autorizacion de
    // MarcarComoLeidaAsync (IDOR) es el caso critico: se verifica ademas que el repositorio
    // de escritura NO se toque cuando la notificacion es de otro usuario.
    public class NotificacionServiceTests
    {
        private const string MensajeNoAutorizado = "No autorizado para modificar esta notificación";
        private const string MensajeNoEncontrada = "Notificación no encontrada";

        private static (NotificacionService Service,
                        Mock<INotificacionRepository> NotificacionRepo,
                        Mock<IUsuarioRepository> UsuarioRepo) Crear()
        {
            var notificacionRepo = new Mock<INotificacionRepository>();
            var usuarioRepo = new Mock<IUsuarioRepository>();

            // Moq devuelve null (no lista vacia) para Task<List<T>> sin setup, y el servicio
            // hace .Select sobre el resultado: sin estos defaults reventaria con NRE.
            notificacionRepo
                .Setup(r => r.ObtenerRecientesPorUsuarioAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Notificacion>());

            usuarioRepo
                .Setup(r => r.ObtenerIdsAdministradoresActivosAsync())
                .ReturnsAsync(new List<int>());

            var service = new NotificacionService(
                notificacionRepo.Object,
                usuarioRepo.Object,
                new NotificacionMapper());

            return (service, notificacionRepo, usuarioRepo);
        }

        // El Id lo asigna la base de datos; en un test unitario hay que forzarlo. La propiedad
        // se declara en BaseEntity, asi que hay que pedirla desde ese tipo.
        private static Notificacion CrearNotificacion(
            int id = 15,
            int usuarioId = 7,
            bool leida = false,
            TipoNotificacion tipo = TipoNotificacion.StockBajo,
            string mensaje = "Stock bajo: quedan 2 unidades.")
        {
            var notificacion = new Notificacion(usuarioId, tipo, mensaje);
            typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(notificacion, id);

            if (leida)
                notificacion.MarcarComoLeida();

            return notificacion;
        }

        private static void DevolverNotificacion(Mock<INotificacionRepository> repo, int id, Notificacion? notificacion)
            => repo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(notificacion);

        // ---------- MarcarComoLeidaAsync: autorizacion ----------

        [Fact]
        public async Task MarcarComoLeidaAsync_ConNotificacionDeOtroUsuario_DevuelveFailureYNoLaModifica()
        {
            var (service, notificacionRepo, _) = Crear();
            var ajena = CrearNotificacion(id: 15, usuarioId: 7);
            DevolverNotificacion(notificacionRepo, 15, ajena);

            var resultado = await service.MarcarComoLeidaAsync(15, usuarioId: 99);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeNoAutorizado);
            ajena.Leida.Should().BeFalse("una notificación ajena no puede quedar marcada");
            notificacionRepo.Verify(r => r.UpdateAsync(It.IsAny<Notificacion>()), Times.Never);
        }

        [Fact]
        public async Task MarcarComoLeidaAsync_ConNotificacionInexistente_DevuelveFailureYNoEscribe()
        {
            var (service, notificacionRepo, _) = Crear();
            DevolverNotificacion(notificacionRepo, 404, null);

            var resultado = await service.MarcarComoLeidaAsync(404, usuarioId: 7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeNoEncontrada);
            notificacionRepo.Verify(r => r.UpdateAsync(It.IsAny<Notificacion>()), Times.Never);
        }

        // Id inexistente y notificacion ajena devuelven mensajes distintos: es enumeracion de
        // ids, pero se documenta el comportamiento real en vez de taparlo.
        [Fact]
        public async Task MarcarComoLeidaAsync_DistingueEntreNoEncontradaYAjena_EnElMensajeDeError()
        {
            var (service, notificacionRepo, _) = Crear();
            DevolverNotificacion(notificacionRepo, 404, null);
            DevolverNotificacion(notificacionRepo, 15, CrearNotificacion(id: 15, usuarioId: 7));

            var inexistente = await service.MarcarComoLeidaAsync(404, usuarioId: 99);
            var ajena = await service.MarcarComoLeidaAsync(15, usuarioId: 99);

            inexistente.Message.Should().Be(MensajeNoEncontrada);
            ajena.Message.Should().Be(MensajeNoAutorizado);
            inexistente.Message.Should().NotBe(ajena.Message);
        }

        // ---------- MarcarComoLeidaAsync: camino feliz e idempotencia ----------

        [Fact]
        public async Task MarcarComoLeidaAsync_ConNotificacionPropia_LaMarcaLeidaYLaPersiste()
        {
            var (service, notificacionRepo, _) = Crear();
            var propia = CrearNotificacion(id: 15, usuarioId: 7);
            DevolverNotificacion(notificacionRepo, 15, propia);

            var resultado = await service.MarcarComoLeidaAsync(15, usuarioId: 7);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().BeTrue();
            resultado.Message.Should().Be("Notificación marcada como leída");
            propia.Leida.Should().BeTrue();
            notificacionRepo.Verify(r => r.UpdateAsync(propia), Times.Once);
        }

        // Notificacion.MarcarComoLeida() hace early return si Leida ya es true: es idempotente,
        // no lanza. El servicio igual llama a UpdateAsync (escritura sin cambios, ver hallazgo).
        [Fact]
        public async Task MarcarComoLeidaAsync_SobreUnaNotificacionYaLeida_EsIdempotenteYNoCambiaLaFechaDeModificacion()
        {
            var (service, notificacionRepo, _) = Crear();
            var yaLeida = CrearNotificacion(id: 15, usuarioId: 7, leida: true);
            var modificadoAntes = yaLeida.ModificadoEn;
            DevolverNotificacion(notificacionRepo, 15, yaLeida);

            var resultado = await service.MarcarComoLeidaAsync(15, usuarioId: 7);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            yaLeida.Leida.Should().BeTrue();
            yaLeida.ModificadoEn.Should().Be(modificadoAntes);
        }

        // ---------- MarcarTodasComoLeidasAsync ----------

        [Fact]
        public async Task MarcarTodasComoLeidasAsync_DelegaEnElRepositorioConElUsuarioRecibido()
        {
            var (service, notificacionRepo, _) = Crear();

            var resultado = await service.MarcarTodasComoLeidasAsync(usuarioId: 7);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().BeTrue();
            notificacionRepo.Verify(r => r.MarcarTodasComoLeidasAsync(7), Times.Once);
            notificacionRepo.Verify(r => r.MarcarTodasComoLeidasAsync(It.Is<int>(id => id != 7)), Times.Never);
        }

        // ---------- NotificarUsuarioAsync ----------

        [Fact]
        public async Task NotificarUsuarioAsync_CreaUnaNotificacionNoLeidaConElTipoYElMensaje()
        {
            var (service, notificacionRepo, _) = Crear();
            Notificacion? creada = null;
            notificacionRepo
                .Setup(r => r.CreateAsync(It.IsAny<Notificacion>()))
                .Callback<Notificacion>(n => creada = n)
                .ReturnsAsync((Notificacion n) => n);

            await service.NotificarUsuarioAsync(7, TipoNotificacion.PagoRegistrado, "La orden #3 fue pagada completamente.");

            creada.Should().NotBeNull();
            creada!.UsuarioId.Should().Be(7);
            creada.Tipo.Should().Be(TipoNotificacion.PagoRegistrado);
            creada.Mensaje.Should().Be("La orden #3 fue pagada completamente.");
            creada.Leida.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task NotificarUsuarioAsync_ConMensajeVacio_LanzaValidacionDominioExceptionYNoCrea(string mensaje)
        {
            var (service, notificacionRepo, _) = Crear();

            var acto = async () => await service.NotificarUsuarioAsync(7, TipoNotificacion.StockBajo, mensaje);

            await acto.Should().ThrowAsync<ValidacionDominioException>();
            notificacionRepo.Verify(r => r.CreateAsync(It.IsAny<Notificacion>()), Times.Never);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task NotificarUsuarioAsync_ConUsuarioIdInvalido_LanzaValidacionDominioExceptionYNoCrea(int usuarioId)
        {
            var (service, notificacionRepo, _) = Crear();

            var acto = async () => await service.NotificarUsuarioAsync(usuarioId, TipoNotificacion.StockBajo, "Stock bajo.");

            await acto.Should().ThrowAsync<ValidacionDominioException>();
            notificacionRepo.Verify(r => r.CreateAsync(It.IsAny<Notificacion>()), Times.Never);
        }

        // ---------- NotificarAdministradoresAsync ----------

        [Fact]
        public async Task NotificarAdministradoresAsync_CreaUnaNotificacionPorCadaAdministradorActivo()
        {
            var (service, notificacionRepo, usuarioRepo) = Crear();
            usuarioRepo.Setup(r => r.ObtenerIdsAdministradoresActivosAsync()).ReturnsAsync(new List<int> { 1, 2, 3 });
            var creadas = new List<Notificacion>();
            notificacionRepo
                .Setup(r => r.CreateAsync(It.IsAny<Notificacion>()))
                .Callback<Notificacion>(creadas.Add)
                .ReturnsAsync((Notificacion n) => n);

            await service.NotificarAdministradoresAsync(TipoNotificacion.StockBajo, "Stock bajo en Teclado.");

            creadas.Should().HaveCount(3);
            creadas.Select(n => n.UsuarioId).Should().BeEquivalentTo(new[] { 1, 2, 3 });
            creadas.Should().OnlyContain(n => n.Tipo == TipoNotificacion.StockBajo && !n.Leida);
        }

        [Fact]
        public async Task NotificarAdministradoresAsync_ConExcluirUsuarioId_NoNotificaDosVecesAEseUsuario()
        {
            var (service, notificacionRepo, usuarioRepo) = Crear();
            usuarioRepo.Setup(r => r.ObtenerIdsAdministradoresActivosAsync()).ReturnsAsync(new List<int> { 1, 2, 3 });
            var creadas = new List<Notificacion>();
            notificacionRepo
                .Setup(r => r.CreateAsync(It.IsAny<Notificacion>()))
                .Callback<Notificacion>(creadas.Add)
                .ReturnsAsync((Notificacion n) => n);

            await service.NotificarAdministradoresAsync(TipoNotificacion.PagoRegistrado, "Pago registrado.", excluirUsuarioId: 2);

            creadas.Select(n => n.UsuarioId).Should().BeEquivalentTo(new[] { 1, 3 });
        }

        [Fact]
        public async Task NotificarAdministradoresAsync_SinAdministradoresActivos_NoCreaNingunaNotificacion()
        {
            var (service, notificacionRepo, usuarioRepo) = Crear();
            usuarioRepo.Setup(r => r.ObtenerIdsAdministradoresActivosAsync()).ReturnsAsync(new List<int>());

            await service.NotificarAdministradoresAsync(TipoNotificacion.StockBajo, "Stock bajo en Teclado.");

            notificacionRepo.Verify(r => r.CreateAsync(It.IsAny<Notificacion>()), Times.Never);
        }

        // ---------- ObtenerResumenAsync ----------

        [Fact]
        public async Task ObtenerResumenAsync_DevuelveLasRecientesMapeadasYElConteoDeNoLeidas()
        {
            var (service, notificacionRepo, _) = Crear();
            notificacionRepo
                .Setup(r => r.ObtenerRecientesPorUsuarioAsync(7, It.IsAny<int>()))
                .ReturnsAsync(new List<Notificacion>
                {
                    CrearNotificacion(id: 2, usuarioId: 7, tipo: TipoNotificacion.PagoRegistrado, mensaje: "Pago registrado."),
                    CrearNotificacion(id: 1, usuarioId: 7, leida: true, tipo: TipoNotificacion.StockBajo, mensaje: "Stock bajo.")
                });
            notificacionRepo.Setup(r => r.ContarNoLeidasPorUsuarioAsync(7)).ReturnsAsync(1);

            var resultado = await service.ObtenerResumenAsync(7);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.NoLeidas.Should().Be(1);
            resultado.Data.Recientes.Should().HaveCount(2);
            resultado.Data.Recientes[0].Id.Should().Be(2);
            resultado.Data.Recientes[0].Tipo.Should().Be("PagoRegistrado");
            resultado.Data.Recientes[0].Mensaje.Should().Be("Pago registrado.");
            resultado.Data.Recientes[0].Leida.Should().BeFalse();
            resultado.Data.Recientes[1].Leida.Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerResumenAsync_SinCantidadExplicita_PideLasUltimas10()
        {
            var (service, notificacionRepo, _) = Crear();

            await service.ObtenerResumenAsync(7);

            notificacionRepo.Verify(r => r.ObtenerRecientesPorUsuarioAsync(7, 10), Times.Once);
        }

        [Fact]
        public async Task ObtenerResumenAsync_ConCantidadExplicita_LaPasaAlRepositorio()
        {
            var (service, notificacionRepo, _) = Crear();

            await service.ObtenerResumenAsync(7, cantidad: 3);

            notificacionRepo.Verify(r => r.ObtenerRecientesPorUsuarioAsync(7, 3), Times.Once);
        }

        // El tope de 100 replica el de los listados paginados (pageSize): una cantidad fuera
        // de rango se rechaza, no se recorta en silencio ni llega al Take() de EF.
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(101)]
        public async Task ObtenerResumenAsync_ConCantidadFueraDeRango_DevuelveFailureYNoConsultaElRepositorio(int cantidad)
        {
            var (service, notificacionRepo, _) = Crear();

            var resultado = await service.ObtenerResumenAsync(7, cantidad);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("cantidad debe estar entre 1 y 100");
            notificacionRepo.Verify(r => r.ObtenerRecientesPorUsuarioAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
