# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Flujo de trabajo obligatorio

Antes de hacer cualquier cambio en el código, siempre seguir este orden:
1. Explicar qué se va a cambiar, en qué archivo(s) y por qué
2. Mostrar el código exacto del cambio (antes/después o el bloque nuevo)
3. Pedir permiso al usuario antes de ejecutar cualquier edición

No hacer ningún cambio sin aprobación explícita del usuario.

---

## Comandos esenciales

```bash
# Construir toda la solución
dotnet build ProductApp.sln

# Correr la API (proyecto principal)
dotnet run --project ProductApp/ProductApp.Api.csproj



# Correr la capa Web (MVC frontend)
dotnet run --project Web/Web.csproj

# Migraciones EF Core — siempre desde la raíz de la solución
dotnet ef migrations add <NombreMigracion> --project ProductApp.Infraesctructura --startup-project ProductApp
dotnet ef database update --project ProductApp.Infraesctructura --startup-project ProductApp
```

No hay capa de tests todavía. La carpeta `Test` existe en la solución pero está vacía.

---

## Arquitectura

Solución .NET 10 con 5 proyectos:

```
ProductApp.Domian          → Domain Layer       (entidades, interfaces, enums, excepciones)
ProductApp.Aplication      → Application Layer  (DTOs, servicios, mappers, validators)
ProductApp.Infraesctructura → Infrastructure     (EF Core, repositorios, migraciones)
ProductApp (Api)           → API REST           (controllers, extensions DI, Program.cs)
Web                        → MVC Frontend       (consume la API por HTTP)
```

> El nombre "Domian" (sin 'e') es un typo original del proyecto — **no corregirlo** en los namespaces, los csproj ya tienen ese nombre.

### Flujo de una petición en la API

```
Controller → Service → [FluentValidator + BusinessValidator] → Repository → EF Core → SQL Server
```

Cada servicio sigue este orden estricto:
1. Validar el DTO con FluentValidation
2. Ejecutar reglas de negocio con el `IValidatorBusiness*` correspondiente
3. Ejecutar la operación en el repositorio
4. Retornar `OperationResultD<T>` (con datos) u `OperationResult` (sin datos)

Los controllers traducen el resultado a `ApiResponseT<T>` o `ApiResponse` antes de enviarlo al cliente.

### Capa Domain

Entidades con **encapsulación estricta** — propiedades `private set`, constructor parametrizado que valida invariantes. Las mutaciones se hacen por métodos explícitos del dominio (e.g., `producto.DesactivarProducto()`, `orden.CambiarEstado()`). Nunca asignar propiedades directamente desde afuera.

`BaseEntity` provee: `Id`, `EstaEliminado` (soft delete), `CreadoEn`, `ModificadoEn`, `Eliminar()`, `ActualizarFechaModificacion()`.

Las excepciones de dominio (`ValidacionDominioException`, `EstadoInvalidoException`, `PrecioInvalidoException`, todas heredan de `DomainException`) se lanzan dentro de las entidades y burbujean sin captura local hasta `GlobalExceptionHandler` (`ProductApp/Filters/GlobalExceptionHandler.cs`, vía `IExceptionHandler`), registrado en `Program.cs` con `AddExceptionHandler<T>()` + `app.UseExceptionHandler()`. Este handler distingue: `DomainException` → 400 con el mensaje real; cualquier otra excepción → 500 con mensaje genérico (no filtra detalles internos). Los controllers **no** tienen bloques try/catch.

### Módulos

| Módulo | Entidades |
|--------|-----------|
| Usuarios | `Usuario`, `Cliente` |
| Productos | `Categoria`, `Producto`, `Inventario` |
| Ventas | `Orden`, `OrdenDetalle`, `Pago` |
| Configuración | `ConfiguracionSistema` (singleton editable: nombre de empresa, moneda, duración de JWT, cantidad mínima de inventario por defecto) |
| Reportes | Sin entidad propia — agrega consultas de solo lectura sobre Orden/Producto/Inventario (ventas por fecha, por producto, por vendedor, inventario actual, productos más vendidos, ingresos totales) |

**Orden → Pago es 1:N** (pagos parciales permitidos). Al pagar completamente: `Orden` pasa a `Pagada`, el stock de todos los productos del pedido se descuenta en una sola transacción atómica dentro de `PagoService.RegistrarPagoAsync`.

`Orden` tiene un diccionario de transiciones de estado válidas. Llamar `CambiarEstado()` con una transición inválida lanza `EstadoInvalidoException`.

### Patrones de resultado

- **`OperationResult`** — operaciones sin datos de retorno. `OperationResult.Success()` / `.Failure("msg")`
- **`OperationResultD<T>`** — operaciones con datos. `OperationResultD<T>.Success(data, "msg")` / `.Failure("msg")`
- **`ApiResponseT<T>`** / **`ApiResponse`** — envolturas de respuesta HTTP que los controllers devuelven al cliente

Los controllers nunca devuelven `OperationResult` directamente; siempre lo convierten a `ApiResponse*`.

### Paginación

Los listados de Producto, Categoria, Cliente, Usuario e Inventario usan `PagedResult<T>` (`ProductApp.Aplication/Common/PagedResult.cs`): `Items`, `PageNumber`, `PageSize`, `TotalCount`, `TotalPages` calculado. La capa Web consume esta respuesta paginada con controles Anterior/Siguiente.

### Cambio de contraseña obligatorio

`Usuario.DebeCambiarPassword` se activa al crear un usuario, resetear su contraseña, o marcarla como temporal (`MarcarPasswordComoTemporal` / `ConfirmarCambioPassword`), y se propaga como claim JWT. `RequiereCambioPasswordFilter` (`ProductApp/Filters/`) bloquea con 403 cualquier endpoint autenticado mientras ese claim sea `true`, salvo los marcados con `[PermitirConPasswordPendiente]`.

### Logging y auditoría

`AuthServices` registra intentos de login exitosos y fallidos. Las operaciones administrativas sensibles (cambio de rol, reseteo de contraseña, registro de pago, cancelación de orden, ajuste manual de inventario) generan logs de auditoría con `ILogger` que incluyen el id del usuario autenticado que ejecutó la acción.

### Inyección de dependencias (API)

Todo el DI está en `ProductApp/Extensions/`:
- `DependencyInjectionExtension.cs` → punto de entrada, llama a los cinco módulos
- `InfraestructuraExtension.cs` → DbContext + JWT Bearer
- `Modulo Usuarios/UsuarioDependenciesExtension.cs`
- `Modulo Productos/ProductoDependenciesExtension.cs`
- `Modulo Ventas/VentasDependenciesExtension.cs`
- `Modulo Reportes/ReportesDependenciesExtension.cs`
- `Modulo Configuracion/ConfiguracionDependenciesExtension.cs`

`Program.cs` solo llama `builder.Services.AddProjectDependencies(builder.Configuration)`. Al agregar un nuevo servicio/repositorio/validator, registrarlo en la extension del módulo correspondiente, **no en Program.cs**.

### Capa Web (MVC)

Consume la API REST por HTTP usando `IBaseHttpServices` (GET, POST, PUT, DELETE genéricos). El JWT se almacena en sesión (`Session["TOKEN"]`) y `BaseHttpServices` lo inyecta automáticamente en cada petición.

Para agregar un nuevo módulo en Web se necesitan 4 piezas:
1. **Endpoint** (`Services/EndPoints/`) — URLs hardcodeadas de la API
2. **HttpService** (`Services/ServicesHttp/`) — métodos tipados usando `IBaseHttpServices`
3. **Models** (`Models/`) — ViewModels para las vistas
4. **Controller** + **Views** — patrón MVC estándar

Los módulos Usuarios, Categoría, Producto, Inventario, Orden, Configuración y Reportes ya tienen controller + views en Web. Pago no tiene controller propio: sus acciones (registrar pago, ver pagos de una orden) están dentro de `OrdenController`.

### Repositorios

`GenericRepository<T>` implementa las operaciones base. Todos los `GetAllAsync` y `GetByIdAsync` filtran automáticamente `EstaEliminado == false` (soft delete). Los repositorios específicos heredan de `GenericRepository<T>` y añaden métodos especializados (e.g., `IProductoRepository.BuscarProductosAsync`, `IOrdenRepository.ObtenerPorUsuarioAsync`).

`AppDbContext.SaveChangesAsync` actualiza `ModificadoEn` automáticamente en entidades modificadas.

### Configuración

La API requiere en `appsettings.json`:
- `ConnectionStrings:DefaultConnection` — SQL Server con Integrated Security
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` — para la autenticación JWT

La duración del JWT y la cantidad mínima de inventario por defecto se leen de `ConfiguracionSistema` en base de datos (editable desde la app), con 60 minutos / 5 unidades como valores de respaldo si no hay configuración cargada — ya no son fijos en `appsettings.json`.

La Web requiere:
- `ApiSettings:BaseUrl` — URL base de la API (e.g., `https://localhost:7001`)

### Estado del proyecto

El proyecto es educativo (ITLA). Los comments "aún no lo entiendo" en `GenericRepository` son notas personales del autor — no son código incorrecto.
