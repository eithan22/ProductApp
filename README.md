# ProductApp - Sistema de Gestión de Ventas 🛒

ProductApp es un sistema integral de gestión de ventas diseñado para automatizar y optimizar los procesos comerciales de pequeñas y medianas empresas. Su objetivo principal es erradicar el descontrol de inventario, los errores de cálculo y la falta de historial financiero mediante una plataforma segura, organizada y escalable.

---

## 🎯 Objetivos del Proyecto

* Automatizar el proceso de ventas para reducir errores humanos.
* Mejorar el control y la actualización automática del inventario.
* Facilitar la toma de decisiones mediante la generación de reportes.
* Garantizar la seguridad de la información con control de acceso basado en roles.

---

## ✨ Características Principales

El sistema está compuesto por los siguientes módulos clave:

* **Gestión de Usuarios y Seguridad:** Autenticación segura con contraseñas encriptadas y control de acceso estricto mediante roles (Administrador y Vendedor).
* **Gestión de Clientes:** Registro completo de datos, historial de compras, totales acumulados y control de estado (activo/inactivo).
* **Catálogo de Productos y Categorías:** Administración de artículos, actualización de precios, asignación de categorías y control de visibilidad.
* **Control de Inventario Automatizado:** Descuento automático de stock al confirmar pagos y sistema de alertas para inventario mínimo.
* **Gestión de Órdenes:** Creación de pedidos con múltiples productos, cálculo automático de subtotales/totales y manejo de estados (Pendiente, Pagada, Cancelada, Entregada).
* **Control de Pagos:** Registro de transacciones (efectivo, transferencia, tarjeta), manejo de pagos parciales y actualización automática del estado de la orden.
* **Reportes y Analíticas:** Generación de métricas clave de ventas por fecha, producto, vendedor e ingresos totales.

---

## 🏛️ Arquitectura del Sistema

ProductApp está desarrollado bajo una **Arquitectura en Capas** (Clean Architecture/N-Tier) para garantizar un código organizado, mantenible y escalable a futuro. Las capas implementadas son:

1. **Capa de Presentación:** Interfaz de usuario y controladores (MVC/API).
2. **Capa de Aplicación:** Lógica de negocio, DTOs y casos de uso.
3. **Capa de Dominio:** Entidades principales y reglas de negocio puras.
4. **Capa de Infraestructura:** Acceso a datos, DbContext y servicios externos.

---

## 🗄️ Modelo de Datos (Entidades Principales)

| Entidad | Descripción |
| :--- | :--- |
| **Usuario** | Gestiona el acceso al sistema (Credenciales, Rol, Estado). |
| **Cliente** | Almacena los datos de los compradores y su información de contacto. |
| **Producto** | Representa los artículos en venta con sus precios y costos. |
| **Categoría** | Agrupación lógica para clasificar los productos. |
| **Inventario** | Controla el stock actual, stock mínimo y última actualización. |
| **Orden** | Transacción comercial principal que agrupa al cliente y los productos. |
| **DetalleOrden** | Desglose individual de cada producto, cantidad y subtotal dentro de una orden. |
| **Pago** | Registro financiero de los abonos realizados a una orden específica. |

---

## 🔄 Flujo Principal de la Aplicación

1. El usuario (Vendedor/Admin) inicia sesión en el sistema.
2. Se registra un nuevo cliente o se selecciona uno existente.
3. Se crea una nueva orden de venta.
4. Se agregan los productos deseados al detalle de la orden.
5. El sistema calcula el subtotal y el total de forma automática.
6. Se registra el pago (total o parcial).
7. Al completarse el pago, el sistema descuenta el stock del inventario automáticamente.
8. Los datos quedan disponibles para la generación de reportes.        
