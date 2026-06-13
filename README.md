- E-Commerce Microservicios - Trabajo Práctico CAI

Este proyecto es una plataforma de E-Commerce desarrollada bajo una arquitectura de microservicios utilizando .NET Core 8. El sistema está compuesto por 5 APIs REST independientes que se comunican entre sí para gestionar el ciclo de vida completo de las ventas.

- Arquitectura y Microservicios

El sistema está dividido por dominios de negocio:

1. Products.API: Gestión del catálogo de productos e inventario.
2. Cart.API: Gestión de carritos de compras. Se comunica con Products.API.
3. Orders.API: Motor de compras. Se integra con Products.API y Users.API.
4. Users.API: Gestión de identidad, registro y login con seguridad.
5. Notifications.API: Servicio de alertas, integrado con Users.API.

- Tecnologías y Patrones

* Framework: .NET 8.0.
* Persistencia: SQLite + Dapper (Micro-ORM).
* Integración: Comunicación HTTP asíncrona (Typed Clients).
* Documentación: Swagger / OpenAPI.
* Manejo de Errores: IExceptionHandler global con formato Problem Details.
* Observabilidad: Health Checks y Logs estructurados (Serilog).

- Estructura del Repositorio

La solución respeta la estructura exigida por la cátedra:

ecommerce-microservicios/
├── docs/
├── src/
│   ├── Cart.API/
│   ├── Notifications.API/
│   ├── Orders.API/
│   ├── Products.API/
│   └── Users.API/
├── ECommerce.sln
├── .gitignore
└── README.md

- Cómo ejecutar el proyecto

- Requisitos Previos

* .NET 8 SDK.
* Visual Studio 2022 o IDE compatible.

- Pasos para la ejecución local

1. Clonar el repositorio: git clone https://github.com/tu-usuario/ecommerce-microservicios.git
2. Abrir el archivo ECommerce.sln con Visual Studio.
3. Clic derecho en la Solución > "Configurar proyectos de inicio...".
4. Seleccionar "Proyectos de inicio múltiples" y marcar los 5 proyectos para que inicien.
5. Ejecutar (F5). Se lanzarán las interfaces de Swagger automáticamente.

(Nota: La persistencia es automática; los archivos .db se crean al iniciar por primera vez).

