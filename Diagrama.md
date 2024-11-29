# Diagrama de Funcionamiento del Backend

Este diagrama describe el ciclo de ejecución del backend de la aplicación, desde la inicialización hasta la respuesta al cliente.

## Ciclo de Ejecución

1. **Inicio de la Aplicación**
   - **Archivo:** `App.cs`
   - **Descripción:** 
     - Se configura y se inicia la aplicación ASP.NET Core.
     - Se configuran los servicios necesarios (como Entity Framework, Redis, etc.).
     - Se define el middleware para manejar las solicitudes HTTP.

2. **Recepción de Solicitudes**
   - **Middleware:** 
     - Se procesan las solicitudes entrantes.
     - Se aplican filtros de autorización y autenticación.

3. **Rutas y Controladores**
   - **Archivo:** `UsuariosController.cs`
   - **Descripción:**
     - Se definen las rutas para las API (por ejemplo, `GET`, `POST`, `PUT`, `DELETE`).
     - Los métodos del controlador manejan las solicitudes y responden con datos o mensajes.
     - Se interactúa con la base de datos y se maneja la lógica de negocio.

4. **Interacción con la Base de Datos**
   - **Archivo:** `AppDbContext.cs`
   - **Descripción:**
     - Se configura el contexto de la base de datos utilizando Entity Framework.
     - Se definen las entidades y sus relaciones.
     - Se realizan consultas y operaciones CRUD (Crear, Leer, Actualizar, Eliminar) en la base de datos.

5. **Modelos de Datos**
   - **Archivos:** `Usuario.cs`, `PasswordResetToken.cs`, etc.
   - **Descripción:**
     - Se definen las clases que representan las entidades de la base de datos.
     - Se utilizan para mapear los datos entre la base de datos y la aplicación.

6. **Caché**
   - **Uso de Redis:**
     - Se utiliza Redis para almacenar en caché los datos que se consultan frecuentemente.
     - Se mejora el rendimiento al evitar consultas repetidas a la base de datos.

7. **Respuesta al Cliente**
   - **Formato de Respuesta:**
     - Se devuelven los datos al cliente en formato JSON.
     - Se incluyen códigos de estado HTTP para indicar el resultado de la operación (éxito, error, etc.).

## Flujo General

App.cs → Middleware → UsuariosController.cs → AppDbContext.cs → Modelos (Usuario.cs, etc.) → Redis (caché) → Respuesta al Cliente

graph TD;
    A[App.cs] --> B[Middleware]
    B --> C[UsuariosController.cs]
    C --> D[AppDbContext.cs]
    D --> E[Modelos (Usuario.cs, etc.)]
    C --> F[Redis (caché)]
    F --> G[Respuesta al Cliente]
    E --> G[Respuesta al Cliente]

 ### Tabla de Flujo

| Paso                      | Descripción                                                                 |
|---------------------------|-----------------------------------------------------------------------------|
| **App.cs**                | Punto de entrada de la aplicación.                                          |
| **Middleware**            | Procesa las solicitudes antes de llegar a los controladores.                |
| **UsuariosController.cs** | Controlador que maneja las solicitudes relacionadas con los usuarios.       |
| **AppDbContext.cs**       | Contexto de la base de datos que maneja las operaciones de la base de datos.|
| **Modelos (Usuario.cs, etc.)** | Clases que representan las entidades de la base de datos.              |
| **Redis (caché)**         | Sistema de caché para almacenar datos temporalmente y mejorar el rendimiento.|
| **Respuesta al Cliente**  | La respuesta final que se envía al cliente.                                 |   