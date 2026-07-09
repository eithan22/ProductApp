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

Las excepciones de dominio (`ValidacionDominioException`, `EstadoInvalidoException`, `PrecioInvalidoException`) se lanzan dentro de las entidades y se dejan burbujear hasta el controller, que las captura en el bloque `catch (Exception ex)`.

### Módulos

| Módulo | Entidades |
|--------|-----------|
| Usuarios | `Usuario`, `Cliente` |
| Productos | `Categoria`, `Producto`, `Inventario` |
| Ventas | `Orden`, `OrdenDetalle`, `Pago` |

**Orden → Pago es 1:N** (pagos parciales permitidos). Al pagar completamente: `Orden` pasa a `Pagada`, el stock de todos los productos del pedido se descuenta en una sola transacción atómica dentro de `PagoService.RegistrarPagoAsync`.

`Orden` tiene un diccionario de transiciones de estado válidas. Llamar `CambiarEstado()` con una transición inválida lanza `EstadoInvalidoException`.

### Patrones de resultado

- **`OperationResult`** — operaciones sin datos de retorno. `OperationResult.Success()` / `.Failure("msg")`
- **`OperationResultD<T>`** — operaciones con datos. `OperationResultD<T>.Success(data, "msg")` / `.Failure("msg")`
- **`ApiResponseT<T>`** / **`ApiResponse`** — envolturas de respuesta HTTP que los controllers devuelven al cliente

Los controllers nunca devuelven `OperationResult` directamente; siempre lo convierten a `ApiResponse*`.

### Inyección de dependencias (API)

Todo el DI está en `ProductApp/Extensions/`:
- `DependencyInjectionExtension.cs` → punto de entrada, llama a los tres módulos
- `InfraestructuraExtension.cs` → DbContext + JWT Bearer
- `Modulo Usuarios/UsuarioDependenciesExtension.cs`
- `Modulo Productos/ProductoDependenciesExtension.cs`
- `Modulo Ventas/VentasDependenciesExtension.cs`

`Program.cs` solo llama `builder.Services.AddProjectDependencies(builder.Configuration)`. Al agregar un nuevo servicio/repositorio/validator, registrarlo en la extension del módulo correspondiente, **no en Program.cs**.

### Capa Web (MVC)

Consume la API REST por HTTP usando `IBaseHttpServices` (GET, POST, PUT, DELETE genéricos). El JWT se almacena en sesión (`Session["TOKEN"]`) y `BaseHttpServices` lo inyecta automáticamente en cada petición.

Para agregar un nuevo módulo en Web se necesitan 4 piezas:
1. **Endpoint** (`Services/EndPoints/`) — URLs hardcodeadas de la API
2. **HttpService** (`Services/ServicesHttp/`) — métodos tipados usando `IBaseHttpServices`
3. **Models** (`Models/`) — ViewModels para las vistas
4. **Controller** + **Views** — patrón MVC estándar

Los 5 módulos (Usuarios, Categoría, Producto, Inventario, Orden) ya tienen controller + views en Web. Pago no tiene controller propio: sus acciones (registrar pago, ver pagos de una orden) están dentro de `OrdenController`.

### Repositorios

`GenericRepository<T>` implementa las operaciones base. Todos los `GetAllAsync` y `GetByIdAsync` filtran automáticamente `EstaEliminado == false` (soft delete). Los repositorios específicos heredan de `GenericRepository<T>` y añaden métodos especializados (e.g., `IProductoRepository.BuscarProductosAsync`, `IOrdenRepository.ObtenerPorUsuarioAsync`).

`AppDbContext.SaveChangesAsync` actualiza `ModificadoEn` automáticamente en entidades modificadas.

### Configuración

La API requiere en `appsettings.json`:
- `ConnectionStrings:DefaultConnection` — SQL Server con Integrated Security
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` — para la autenticación JWT (tokens expiran en 60 min)

La Web requiere:
- `ApiSettings:BaseUrl` — URL base de la API (e.g., `https://localhost:7001`)

### Estado del proyecto

El proyecto es educativo (ITLA). Los comments "aún no lo entiendo" en `GenericRepository` son notas personales del autor — no son código incorrecto.
