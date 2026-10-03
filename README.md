# ProductApp 🛒

Sistema de gestión comercial (ventas, inventario y clientes) construido como **API REST en ASP.NET Core 10** bajo **Clean Architecture**, con una capa web MVC que la consume por HTTP. Incluye autenticación JWT, control de inventario, cobros con pagos parciales, generación de facturas en PDF, reportes exportables y una suite de más de 500 pruebas automatizadas.

---

## 📋 Descripción

ProductApp automatiza el ciclo completo de una operación de ventas: catálogo de productos y categorías, control de inventario con umbrales mínimos configurables, gestión de proveedores, órdenes con pagos parciales, facturación en PDF, reportes de negocio y administración de usuarios con roles.

La arquitectura separa estrictamente dominio, aplicación, infraestructura y presentación. Las entidades de dominio tienen encapsulación estricta (propiedades `private set`, invariantes validadas en el constructor, mutación solo por métodos explícitos) y las reglas de negocio se validan en una capa dedicada antes de tocar la base de datos.

---

## ✨ Módulos

| Módulo | Qué hace |
|---|---|
| 🔐 **Usuarios y Roles** | Autenticación JWT, cambio de contraseña obligatorio en el primer login, revocación de sesión inmediata al desactivar/eliminar un usuario o cambiarle el rol (re-verificación contra la base de datos en cada request), rate limiting de login (5 intentos por minuto por IP) |
| 📦 **Productos y Categorías** | Alta, edición y baja lógica de productos y categorías, validación de nombres/descripciones duplicados, importación y exportación masiva por CSV |
| 📊 **Inventario** | Control de stock por producto, cantidad mínima configurable, ajustes manuales auditados |
| 🚚 **Proveedores** | Registro y administración de proveedores asociados a productos |
| 🧾 **Órdenes** | Ciclo de vida de una orden con máquina de estados explícita (transiciones inválidas rechazadas) |
| 💳 **Pagos** | Pagos parciales (1:N contra una orden) dentro de una transacción serializable: el cobro, el cambio de estado de la orden y el descuento de inventario se aplican atómicamente o no se aplican |
| 📄 **Facturación PDF** | Generación de la factura de la orden en PDF con QuestPDF, emitida después del commit del cobro |
| 📈 **Reportes** | Ventas por fecha/producto/vendedor, inventario actual, productos más vendidos, ingresos totales — exportables a CSV |
| ⚙️ **Configuración** | Parámetros editables en caliente: nombre de la empresa, moneda, duración del JWT, cantidad mínima de inventario por defecto |
| 🔔 **Notificaciones** | Notificaciones in-app para eventos relevantes del sistema |

---

## 🛠️ Stack Tecnológico

| Capa | Tecnología |
|---|---|
| Lenguaje | C# / .NET 10 |
| Framework API | ASP.NET Core 10 Web API |
| Frontend | ASP.NET Core MVC (capa `Web`, consume la API por HTTP) |
| ORM | Entity Framework Core (SQL Server) |
| Autenticación | JWT Bearer, hashing de contraseñas con BCrypt |
| Validación | FluentValidation (forma del DTO) + validadores de negocio dedicados |
| PDF | QuestPDF (facturas) |
| Logging | Serilog (consola + archivo con rotación diaria, 30 días de retención) |
| Testing | xUnit — **583 pruebas** automatizadas (unitarias + integración) |
| CI/CD | GitHub Actions — build, test y despliegue automático a Azure App Service (API y Web) en cada push a `master` |

---

## 🏗️ Arquitectura

Solución .NET 10 con 5 proyectos:

```
ProductApp.Domian          → Domain Layer        (entidades, interfaces, enums, excepciones de dominio)
ProductApp.Aplication      → Application Layer   (DTOs, servicios, validadores de negocio, mappers)
ProductApp.Infraesctructura → Infrastructure      (EF Core, repositorios, migraciones)
ProductApp (Api)           → API REST            (controllers, inyección de dependencias, Program.cs)
Web                         → MVC Frontend        (consume la API por HTTP)
```

Flujo de una petición en la API:

```
Controller → Service → [Validación FluentValidation + Reglas de negocio] → Repository → EF Core → SQL Server
```

Las excepciones de dominio burbujean sin captura local hasta un `GlobalExceptionHandler` centralizado: una excepción de dominio responde 400 con el mensaje real, cualquier otra responde 500 sin filtrar detalles internos. Los controllers no tienen bloques try/catch.

---

## 🔒 Seguridad

- **JWT con fail-fast** — si falta la llave JWT o la cadena de conexión al arrancar, la aplicación falla de inmediato con un mensaje explicando qué configurar, en vez de un error críptico más adelante.
- **Sesión re-verificada en cada petición** — no hay denylist de tokens ni refresh tokens; en su lugar, cada request autenticado reconfirma contra la base de datos que el usuario sigue activo y que su rol no cambió, por lo que desactivar o eliminar una cuenta tiene efecto inmediato aunque el JWT siga técnicamente vigente.
- **Rate limiting en login** — máximo 5 intentos por minuto por IP, sin cola de espera.
- **Cambio de contraseña obligatorio** — se activa al crear un usuario, resetear su contraseña o marcarla como temporal; bloquea con 403 cualquier otro endpoint hasta que se cumple.
- **Auditoría** — operaciones administrativas sensibles (cambio de rol, reseteo de contraseña, registro de pago, cancelación de orden, ajuste de inventario) quedan registradas con el id del usuario que las ejecutó.
- **Transacciones serializables** — el cobro de una orden (pago + cambio de estado + descuento de inventario) corre en una única transacción; dos peticiones simultáneas sobre la misma orden no pueden cobrarla dos veces.

---

## 🚀 Instalación

Requisitos: .NET 10 SDK · SQL Server

```bash
git clone https://github.com/Eithan22/ProductApp.git
cd ProductApp
dotnet restore ProductApp.sln

# Migraciones (desde la raíz de la solución)
dotnet ef database update --project ProductApp.Infraesctructura --startup-project ProductApp

# Correr la API
dotnet run --project ProductApp/ProductApp.Api.csproj

# Correr la capa Web (en otra terminal)
dotnet run --project Web/Web.csproj
```

Swagger disponible en: `https://localhost:{puerto}/swagger`

---

## ⚙️ Configuración

La configuración sensible (llave JWT, cadena de conexión, credenciales del admin inicial) no se guarda en `appsettings.json`. En desarrollo se configura con el Secret Manager de .NET:

```bash
dotnet user-secrets init --project ProductApp/ProductApp.Api.csproj
dotnet user-secrets set "Jwt:Key" "<llave-aleatoria-de-al-menos-32-caracteres>" --project ProductApp/ProductApp.Api.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<tu-connection-string>" --project ProductApp/ProductApp.Api.csproj
```

En producción se configuran como variables de entorno (Azure App Service → Configuration, u otro proveedor), reemplazando `:` por `__`:

```
ConnectionStrings__DefaultConnection
Jwt__Key
Jwt__Issuer
Jwt__Audience
```

La duración del JWT y la cantidad mínima de inventario por defecto ya no son valores fijos: se leen de `ConfiguracionSistema` en base de datos y son editables desde la propia aplicación.

---

## 🧪 Testing y CI/CD

```bash
dotnet test ProductApp.sln
```

La suite cubre entidades de dominio, servicios de aplicación, validadores de negocio e integración. Cada push dispara el workflow de GitHub Actions (`.github/workflows/ci.yml`): restaura, compila y corre toda la suite; en `master`, si todo pasa, despliega automáticamente la API y la capa Web a Azure App Service.

---

## 👨‍💻 Autor

**Eithan** — Santo Domingo, República Dominicana 🇩🇴
🎓 Desarrollo de Software @ ITLA · 📧 eithanread1@gmail.com
[LinkedIn](https://linkedin.com/in/eithan-r) · [GitHub](https://github.com/Eithan22)

---

*MIT License*
