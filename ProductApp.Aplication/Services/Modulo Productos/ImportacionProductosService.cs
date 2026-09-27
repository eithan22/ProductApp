using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Productos.ImportacionDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Common.Importacion;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System.Globalization;

namespace ProductApp.Aplication.Services
{
    public class ImportacionProductosService : IImportacionProductosService
    {
        // Tope duro del SDD 3.1: la importación corre síncrona dentro de la request porque el
        // proyecto no tiene background jobs. Por encima de esto se rechaza el archivo entero
        // con un mensaje claro, no se trunca en silencio. El tope se le pasa al lector para que
        // corte el parseo en cuanto lo supere: antes leía el archivo completo en memoria para
        // que el chequeo de abajo lo descartara entero.
        public const int MaximoFilasPorArchivo = 500;

        private const string ColumnaGeneral = "General";

        private readonly ILectorArchivoProductos _lectorArchivoProductos;
        private readonly IGeneradorArchivoProductos _generadorArchivoProductos;
        private readonly IProductoServices _productoServices;
        private readonly IProductoRepository _productoRepository;
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IValidator<ImportarProductosDto> _validatorImportarProductosDto;
        private readonly ILogger<ImportacionProductosService> _logger;

        public ImportacionProductosService(
            ILectorArchivoProductos lectorArchivoProductos,
            IGeneradorArchivoProductos generadorArchivoProductos,
            IProductoServices productoServices,
            IProductoRepository productoRepository,
            ICategoriaRepository categoriaRepository,
            IProveedorRepository proveedorRepository,
            IValidator<ImportarProductosDto> validatorImportarProductosDto,
            ILogger<ImportacionProductosService> logger)
        {
            _lectorArchivoProductos = lectorArchivoProductos;
            _generadorArchivoProductos = generadorArchivoProductos;
            _productoServices = productoServices;
            _productoRepository = productoRepository;
            _categoriaRepository = categoriaRepository;
            _proveedorRepository = proveedorRepository;
            _validatorImportarProductosDto = validatorImportarProductosDto;
            _logger = logger;
        }

        public OperationResultD<byte[]> ObtenerPlantilla()
        {
            var plantilla = _generadorArchivoProductos.GenerarPlantilla();
            return OperationResultD<byte[]>.Success(plantilla, "Plantilla generada correctamente");
        }

        public async Task<OperationResultD<ImportacionProductosResultadoDto>> ImportarAsync(
            ImportarProductosDto dto, int usuarioSolicitanteId)
        {
            var dtoValidator = await _validatorImportarProductosDto.ValidateAsync(dto);

            if (!dtoValidator.IsValid)
            {
                var errors = string.Join("; ", dtoValidator.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ImportacionProductosResultadoDto>.Failure($"Error de validación: {errors}");
            }

            var lectura = _lectorArchivoProductos.Leer(dto.Contenido, dto.NombreArchivo, MaximoFilasPorArchivo);

            if (!lectura.FormatoSoportado)
                return OperationResultD<ImportacionProductosResultadoDto>.Failure(
                    "El formato del archivo no está soportado. Solo se aceptan archivos .xlsx y .csv.");

            if (lectura.ColumnasFaltantes.Count > 0)
                return OperationResultD<ImportacionProductosResultadoDto>.Failure(
                    $"El archivo no tiene las columnas esperadas. Faltan: {string.Join(", ", lectura.ColumnasFaltantes)}. " +
                    "Descarga la plantilla para ver los encabezados exactos.");

            if (lectura.Filas.Count == 0)
                return OperationResultD<ImportacionProductosResultadoDto>.Failure(
                    "El archivo no tiene filas para importar.");

            if (lectura.Filas.Count > MaximoFilasPorArchivo)
                return OperationResultD<ImportacionProductosResultadoDto>.Failure(
                    $"El archivo tiene más de {MaximoFilasPorArchivo} filas y ese es el máximo permitido. " +
                    "Divide el archivo en partes más pequeñas.");

            // Los tres catálogos se traen UNA vez y se comparan en memoria. Consultarlos por
            // fila serían 1.500 viajes a la base para un archivo de 500 filas.
            var categorias = new IndiceNombres<Categoria>(
                (await _categoriaRepository.GetAllAsync()).ToList(), c => c.Nombre);

            var proveedores = new IndiceNombres<Proveedor>(
                await _proveedorRepository.BuscarProveedoresAsync(null, incluirInactivos: false), p => p.Nombre);

            // incluirInactivos: true a propósito. Un producto desactivado sigue ocupando su
            // nombre (ValidatorBusinessProducto no filtra por estado), así que ignorarlo haría
            // que la fila se intentara crear y fallara con un mensaje confuso.
            var catalogo = (await _productoRepository.BuscarProductosAsync(null, null, incluirInactivos: true))
                .Select(p => ClaveCatalogo(p.Nombre, p.CategoriaId))
                .ToHashSet();

            var errores = new List<FilaErroneaArchivoProductos>();
            var advertencias = new List<AdvertenciaArchivoProductos>();
            var duplicadas = new List<FilaDuplicadaImportacionDto>();
            var creadas = 0;

            foreach (var fila in lectura.Filas)
            {
                if (string.IsNullOrWhiteSpace(fila.Nombre))
                {
                    errores.Add(Error(fila, "Nombre", "Campo obligatorio vacío."));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fila.Descripcion))
                {
                    errores.Add(Error(fila, "Descripcion", "Campo obligatorio vacío."));
                    continue;
                }

                // --- Categoría: obligatoria, nunca se crea sola ---

                if (string.IsNullOrWhiteSpace(fila.Categoria))
                {
                    errores.Add(Error(fila, "Categoria", "Campo obligatorio vacío."));
                    continue;
                }

                if (!categorias.TryResolver(fila.Categoria, out var categoria, out var categoriaNormalizada))
                {
                    errores.Add(Error(fila, "Categoria",
                        $"La categoría '{fila.Categoria}' no existe en el sistema. Créala primero o corrige el nombre."));
                    continue;
                }

                // Coincidió solo después de quitar tildes/mayúsculas: se usa la existente, pero
                // se avisa. Crear una categoría nueva automáticamente llenaría el catálogo de
                // duplicados por errores de tipeo.
                if (categoriaNormalizada)
                    advertencias.Add(new AdvertenciaArchivoProductos
                    {
                        NumeroFila = fila.NumeroFila,
                        Columna = "Categoria",
                        Mensaje = $"'{fila.Categoria}' se interpretó como la categoría existente '{categoria!.Nombre}'."
                    });

                // --- Proveedor: opcional. Si viene y no matchea (ni normalizado), el producto
                // se crea igual sin proveedor asignado — a diferencia de Categoría, ProveedorId
                // es nullable por diseño y "sin proveedor" es un estado válido y común. La fila
                // no se rechaza: se avisa de forma visible para que el usuario lo asocie después.

                int? proveedorId = null;

                if (!string.IsNullOrWhiteSpace(fila.Proveedor))
                {
                    if (proveedores.TryResolver(fila.Proveedor, out var proveedor, out var proveedorNormalizado))
                    {
                        if (proveedorNormalizado)
                            advertencias.Add(new AdvertenciaArchivoProductos
                            {
                                NumeroFila = fila.NumeroFila,
                                Columna = "Proveedor",
                                Mensaje = $"'{fila.Proveedor}' se interpretó como el proveedor existente '{proveedor!.Nombre}'."
                            });

                        proveedorId = proveedor!.Id;
                    }
                    else
                    {
                        advertencias.Add(new AdvertenciaArchivoProductos
                        {
                            NumeroFila = fila.NumeroFila,
                            Columna = "Proveedor",
                            Mensaje = $"No existe un proveedor llamado '{fila.Proveedor}'. El producto se creó sin proveedor asignado."
                        });
                    }
                }

                // --- Montos ---

                if (!TryParseMonto(fila.Precio, out var precio))
                {
                    errores.Add(Error(fila, "Precio", MotivoMontoInvalido(fila.Precio)));
                    continue;
                }

                if (!TryParseMonto(fila.Costo, out var costo))
                {
                    errores.Add(Error(fila, "Costo", MotivoMontoInvalido(fila.Costo)));
                    continue;
                }

                // --- Duplicado: Nombre + Categoría ya en el catálogo ---

                var clave = ClaveCatalogo(fila.Nombre, categoria!.Id);

                if (catalogo.Contains(clave))
                {
                    duplicadas.Add(new FilaDuplicadaImportacionDto
                    {
                        NumeroFila = fila.NumeroFila,
                        Nombre = fila.Nombre.Trim(),
                        Categoria = categoria.Nombre
                    });
                    continue;
                }

                // --- Alta: exactamente el mismo camino que el alta manual ---

                var createDto = new CreateProductoDto
                {
                    Nombre = fila.Nombre.Trim(),
                    Descripcion = fila.Descripcion.Trim(),
                    Precio = precio,
                    Costo = costo,
                    CategoriaId = categoria.Id,
                    ProveedorId = proveedorId
                };

                OperationResultD<ProductoResponseDto> resultado;

                try
                {
                    // Se reusa ProductoServices.CreateAsync tal cual: FluentValidation, reglas de
                    // negocio y la creación del Inventario asociado. Un producto importado queda
                    // idéntico a uno cargado a mano.
                    resultado = await _productoServices.CreateAsync(createDto);
                }
                catch (DomainException ex)
                {
                    // Único try/catch del flujo, y es deliberado: una invariante de dominio que
                    // salte en UNA fila no puede tumbar la importación entera, que es justamente
                    // lo que la feature promete (importación parcial). Las excepciones que no son
                    // de dominio sí se dejan propagar: esas son errores de programación.
                    errores.Add(Error(fila, ColumnaGeneral, ex.Message));
                    continue;
                }

                if (!resultado.IsSuccess)
                {
                    errores.Add(Error(fila, ColumnaGeneral, resultado.Message));
                    continue;
                }

                // Se indexa lo recién creado para que dos filas idénticas dentro del MISMO
                // archivo no generen dos productos: la segunda cae como duplicada.
                catalogo.Add(clave);
                creadas++;
            }

            var resultadoDto = new ImportacionProductosResultadoDto
            {
                NombreArchivo = dto.NombreArchivo,
                TotalFilas = lectura.Filas.Count,
                FilasCreadas = creadas,
                FilasConError = errores.Count,
                FilasOmitidas = duplicadas.Count,
                Duplicadas = duplicadas,
                Errores = errores.Select(e => new FilaErrorImportacionDto
                {
                    NumeroFila = e.Fila.NumeroFila,
                    Nombre = e.Fila.Nombre ?? string.Empty,
                    Columna = e.Columna,
                    Motivo = e.Motivo
                }).ToList(),
                Advertencias = advertencias.Select(a => new AdvertenciaImportacionDto
                {
                    NumeroFila = a.NumeroFila,
                    Columna = a.Columna,
                    Mensaje = a.Mensaje
                }).ToList()
            };

            if (errores.Count > 0)
            {
                var archivo = _generadorArchivoProductos.GenerarReporteErrores(errores, advertencias);
                resultadoDto.ArchivoErroresBase64 = Convert.ToBase64String(archivo);
                resultadoDto.NombreArchivoErrores =
                    $"errores-importacion-{DateTime.Now:yyyy-MM-dd-HHmm}.xlsx";
            }

            _logger.LogInformation(
                "Importación masiva de productos desde {NombreArchivo}: {TotalFilas} filas, {FilasCreadas} creadas, " +
                "{FilasConError} con error, {FilasOmitidas} omitidas por duplicado, por el usuario {UsuarioSolicitanteId}",
                dto.NombreArchivo, lectura.Filas.Count, creadas, errores.Count, duplicadas.Count, usuarioSolicitanteId);

            return OperationResultD<ImportacionProductosResultadoDto>.Success(
                resultadoDto, $"Importación procesada: {creadas} productos agregados al catálogo");
        }

        // --- Helpers ---

        private static FilaErroneaArchivoProductos Error(FilaArchivoProductos fila, string columna, string motivo) =>
            new() { Fila = fila, Columna = columna, Motivo = motivo };

        private static string MotivoMontoInvalido(string? valor) =>
            string.IsNullOrWhiteSpace(valor)
                ? "Campo obligatorio vacío."
                : $"Valor '{valor}' no es un número. Escribe solo dígitos con punto decimal: 185.00";

        // Cultura invariante: el punto decimal es el mismo criterio que ya usa GeneradorCsv
        // para exportar, así el archivo no depende de la cultura del servidor.
        // Sin AllowThousands a propósito: .NET no valida el tamaño de los grupos, así que
        // "185,00" (coma decimal dominicana) se leería como 18500. Mejor rechazar la fila
        // y que el usuario corrija el formato, que importar un monto 100 veces mayor.
        private static bool TryParseMonto(string? valor, out decimal monto)
        {
            monto = 0;

            return !string.IsNullOrWhiteSpace(valor)
                && decimal.TryParse(
                    valor.Trim(),
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign
                        | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
                    CultureInfo.InvariantCulture,
                    out monto);
        }

        // Duplicado = Nombre + Categoría. Solo trim + minúsculas, SIN quitar tildes: en nombres
        // de producto "Piña" y "Pina" pueden ser dos artículos distintos, y borrar esa diferencia
        // haría que el segundo se omitiera sin que el usuario entienda por qué.
        private static string ClaveCatalogo(string? nombre, int categoriaId) =>
            $"{(nombre ?? string.Empty).Trim().ToLowerInvariant()}|{categoriaId}";

        // Índice de nombres en dos niveles: primero busca la coincidencia literal y solo si
        // falla recurre a la normalizada. Así, si existen "Electrónica" y "Electronica" como
        // dos categorías distintas, escribir cualquiera de las dos da la correcta.
        private sealed class IndiceNombres<T> where T : class
        {
            private readonly Dictionary<string, T> _porNombre;
            private readonly Dictionary<string, T> _porNombreNormalizado;

            public IndiceNombres(IReadOnlyList<T> elementos, Func<T, string> obtenerNombre)
            {
                _porNombre = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
                _porNombreNormalizado = new Dictionary<string, T>(StringComparer.Ordinal);

                foreach (var elemento in elementos)
                {
                    var nombre = obtenerNombre(elemento)?.Trim() ?? string.Empty;
                    if (nombre.Length == 0)
                        continue;

                    // TryAdd y no indexador: si dos registros colapsan a la misma clave gana el
                    // primero, en vez de reventar con "duplicate key".
                    _porNombre.TryAdd(nombre, elemento);
                    _porNombreNormalizado.TryAdd(NormalizadorTexto.Normalizar(nombre), elemento);
                }
            }

            public bool TryResolver(string? nombre, out T? elemento, out bool coincidioNormalizando)
            {
                elemento = null;
                coincidioNormalizando = false;

                if (string.IsNullOrWhiteSpace(nombre))
                    return false;

                if (_porNombre.TryGetValue(nombre.Trim(), out elemento))
                    return true;

                if (_porNombreNormalizado.TryGetValue(NormalizadorTexto.Normalizar(nombre), out elemento))
                {
                    coincidioNormalizando = true;
                    return true;
                }

                return false;
            }
        }
    }
}
