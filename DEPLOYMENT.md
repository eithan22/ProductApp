# Despliegue de una instancia nueva

ProductApp es **single-tenant**: cada cliente tiene su propia base de datos y su propio
par de App Services (API + Web). No hay `TenantId` en ninguna tabla — es una decisión
intencional, no una limitación a corregir. Este documento describe el proceso manual
para dar de alta un cliente nuevo.

## 1. Recursos de Azure a crear

Por cada cliente nuevo:

| Recurso | Cantidad | Notas |
|---|---|---|
| Resource Group | 1 | Uno por cliente, para poder borrar todo junto si hace falta |
| App Service Plan | 1 | Puede compartirse entre los dos App Services del mismo cliente |
| App Service (API) | 1 | .NET 10, corre `ProductApp.Api` |
| App Service (Web) | 1 | .NET 10, corre `Web` (MVC) |
| Azure SQL Database | 1 | Vacía — las migraciones la pueblan en el primer arranque |
| Storage Account | 1 | Con dos contenedores: `productos-imagenes` (privado, se sirve con SAS) y `facturas` |

## 2. Variables de entorno / secrets por instancia

### App Service de la API

| Variable | Ejemplo | Para qué |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | `Server=tcp:<servidor>.database.windows.net,1433;Database=<db>;User ID=<user>;Password=<pass>;Encrypt=True;` | Cadena de conexión a la SQL Database del cliente |
| `ConnectionStrings__AzureStorage` | `DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net` | Conexión al Storage Account del cliente |
| `Jwt__Key` | (secreto aleatorio, 32+ caracteres) | Firma de los JWT — debe ser distinto por cliente |
| `Jwt__Issuer` | `ProductApp` | Puede quedar igual en todas las instancias |
| `Jwt__Audience` | `ProductAppUsers` | Puede quedar igual en todas las instancias |

Las tres variables siguientes **solo son necesarias en el primer arranque** (la tabla
`Usuarios` está vacía). `DbInitializer` las exige y corta el arranque con un error claro
si faltan; una vez que el admin existe, se pueden dejar o quitar sin efecto:

| Variable | Ejemplo | Para qué |
|---|---|---|
| `Seed__AdminUsername` | `admin` | Usuario administrador inicial |
| `Seed__AdminEmail` | `admin@cliente.com` | Email del administrador inicial |
| `Seed__AdminPassword` | (contraseña temporal) | El sistema la marca como `DebeCambiarPassword` — se fuerza el cambio en el primer login |

### App Service Web

| Variable | Ejemplo | Para qué |
|---|---|---|
| `ApiSettings__BaseUrl` | `https://<api-del-cliente>.azurewebsites.net/api/` | URL de la API de ese mismo cliente |

## 3. Migraciones

**No requieren un paso manual.** `Program.cs` llama `context.Database.MigrateAsync()`
en el arranque de la API: si la base está vacía, crea todo el esquema desde cero; si ya
tiene migraciones aplicadas, solo corre las que falten. Alcanza con que
`ConnectionStrings__DefaultConnection` apunte a la base nueva antes del primer arranque.

## 4. Seed inicial

Tampoco requiere un paso manual, corre junto con las migraciones (`DbInitializer.SeedAsync`):

- Usuario administrador (con las variables `Seed__*` de arriba)
- `ConfiguracionSistema` con valores por defecto (5 unidades de stock mínimo, 60 minutos de JWT, moneda USD) — editable después desde la app
- Cliente "Consumidor Final" reservado, para las ventas de mostrador

## 5. Checklist de alta

1. Crear los recursos de Azure (manual o con `scripts/alta-cliente.ps1`, ver abajo)
2. Configurar las variables de ambos App Services
3. Arrancar la API primero (corre migraciones + seed) y confirmar en los logs que
   terminó sin errores
4. Arrancar la Web
5. Entrar con el usuario admin inicial y cambiar la contraseña temporal
6. Si se usó `scripts/alta-cliente.ps1`, queda pendiente conectar un pipeline de CI/CD
   propio para ese cliente — el `.github/workflows/ci.yml` actual despliega a los nombres
   fijos `ProductApp-eithan` / `ProductApp-eithan-Web` y no sirve para una instancia nueva
   sin adaptarlo
