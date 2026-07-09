# ProductApp 🛒

A multi-layered **Sales Management REST API** built with **ASP.NET Core 10** following **Clean Architecture** principles. Manages products, categories, users, and clients with business validation, secure authentication, and strict separation of concerns across four independent layers.

---

## 📋 Description

ProductApp is a backend system providing a RESTful API to automate and optimize commercial business processes. It implements domain-driven design with explicit separation between domain rules, application logic, infrastructure, and the API surface.

The system handles full product and category lifecycle management, user authentication with BCrypt password hashing, client management, and enforces business rules through a dedicated validation layer before any persistence operation.

---

## ✨ Features

- 🔐 **User Authentication** — Login with BCrypt password hashing, forced password change on first login, and soft-delete support
- 📦 **Product Management** — Create, update, and disable products with duplicate-name/description validation
- 🗂️ **Category Management** — Full category lifecycle with business rule enforcement
- 👥 **Client Management** — Client registration with existence validation
- 🧪 **OperationResult Pattern** — All business operations return structured success/failure results
- ♻️ **Soft Delete** — Records disabled via IsDisable flag, never permanently deleted
- 📄 **Swagger UI** — Interactive API documentation via OpenAPI

---

## 🛠️ Technologies

| Layer | Technology |
|-------|-----------|
| Language | C# 12 |
| Framework | ASP.NET Core 8 Web API |
| Architecture | Clean Architecture |
| Security | BCrypt.Net (password hashing) |
| API Docs | Swagger / OpenAPI |
| DI Container | .NET 8 Built-in DI |
| Pattern | Repository Pattern |

---

## 🏗️ Architecture

```
ProductApp/
├── Domian.ProductApp/            Domain Layer
│   ├── Entities/                 Producto, Categoria, Usuario, Cliente
│   └── Interfaces/               IProductoRepository, IUsuarioRepository
│
├── ProductApp.Aplication/        Application Layer
│   ├── BusinessValidator/
│   │   ├── Modulo Productos/     ValidatorBusinessProducto, ValidatorBusinessCategoria
│   │   └── Modulo Usuarios/      ValidatorBusinessAuth, ValidatorBusinessUsuarios
│   ├── Dtos/                     CreateDto, UpdateDto, ResponseDto per entity
│   ├── Helper/                   PasswordHelper (BCrypt wrapper)
│   ├── Result/                   OperationResult pattern
│   └── Services/                 Application services
│
├── ProductApp.Infraestructure/   Infrastructure Layer
│   └── Persistencia/             DbContext, repository implementations
│
└── ProductApp.Api/               Presentation Layer
    ├── Controllers/              HTTP endpoints
    ├── Program.cs                DI registration, middleware pipeline
    └── appsettings.json          Configuration
```

---

## 🔑 Key Design Patterns

| Pattern | Implementation |
|---------|----------------|
| Repository Pattern | IProductoRepository, IUsuarioRepository abstract all data access |
| Business Validator Pattern | Dedicated validator per module enforces rules before persistence |
| OperationResult Pattern | Returns Success() or Failure("reason") instead of throwing exceptions |
| DTO Pattern | Separate CreateDto, UpdateDto, ResponseDto per entity |
| Dependency Injection | All services and repositories injected via constructor |

---

## 📦 Business Rules Enforced

### Authentication
- Passwords must match confirmation on register
- Duplicate email or username rejected
- Login validates existence, active status, and BCrypt password hash
- Disabled users cannot authenticate

### Products
- Duplicate name rejected on create and update
- Duplicate description rejected
- Cannot disable an already-disabled product

---

## 🚀 Installation

Prerequisites: .NET 8 SDK · SQL Server · Visual Studio 2022

```bash
git clone https://github.com/Eithan22/ProductApp.git
cd ProductApp
dotnet restore
dotnet ef database update --project ProductApp.Infraestructure --startup-project ProductApp.Api
dotnet run --project ProductApp.Api
```

Swagger available at: `https://localhost:{port}/swagger`

---

## ⚙️ Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProductAppDb;Trusted_Connection=True;"
  }
}
```

The JWT signing key is not stored in `appsettings.json`. Set it locally with the .NET Secret Manager before running the API:

```bash
dotnet user-secrets init --project ProductApp/ProductApp.Api.csproj
dotnet user-secrets set "Jwt:Key" "<random-key-at-least-32-characters>" --project ProductApp/ProductApp.Api.csproj
```

On first run, if the `Usuarios` table is empty, the API seeds an initial Administrador using the following configuration keys (also set via Secret Manager):

```bash
dotnet user-secrets set "Seed:AdminUsername" "admin" --project ProductApp/ProductApp.Api.csproj
dotnet user-secrets set "Seed:AdminEmail" "admin@example.com" --project ProductApp/ProductApp.Api.csproj
dotnet user-secrets set "Seed:AdminPassword" "<a-secure-password>" --project ProductApp/ProductApp.Api.csproj
```

The seeded admin is created with a temporary password flag, so it must be changed via `/api/Usuario/CambiarPassword` on first login.

---

## 📡 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/auth/login | Authenticate and validate credentials |
| GET | /api/productos | List all products |
| POST | /api/productos | Create a new product |
| PUT | /api/productos/{id} | Update a product |
| PATCH | /api/productos/{id}/disable | Soft-delete a product |
| GET | /api/categorias | List all categories |
| GET | /api/clientes | List all clients |

---

## 💡 Skills Demonstrated

- ✅ **Clean Architecture** — 4 independent project layers with zero circular dependencies
- ✅ **SOLID Principles** — Single Responsibility in validators, Open/Closed via interfaces
- ✅ **Repository Pattern** — Complete data access abstraction
- ✅ **Security** — BCrypt password hashing, soft delete, status validation
- ✅ **RESTful API Design** — Proper HTTP methods and status codes
- ✅ **ASP.NET Core 8** — Modern LTS framework with built-in DI and Swagger

---

## 🔮 Future Improvements

- [ ] JWT Bearer token authentication
- [ ] EF Core migrations
- [ ] Unit tests for business validators
- [ ] Pagination on list endpoints
- [ ] FluentValidation for DTO validation

---

## 👨‍💻 Author

**Eithan** — Backend Developer · Santo Domingo, Dominican Republic 🇩🇴  
🎓 Software Development @ ITLA · 📧 eithanread1@gmail.com  
[LinkedIn](https://linkedin.com/in/eithan-r) · [GitHub](https://github.com/Eithan22)

---

*MIT License*

<!-- cache-refresh-marker 2026-07-06 12:15 -->
