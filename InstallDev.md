# Guía de Instalación del Ambiente de Desarrollo para .NET

Esta guía proporciona un paso a paso para configurar un ambiente de desarrollo para trabajar con aplicaciones .NET en una máquina nueva.

## 1. Instalación de Visual Studio Code

1. **Descargar Visual Studio Code**:
   - Ve a la página oficial de [Visual Studio Code](https://code.visualstudio.com/).
   - Haz clic en el botón de descarga para tu sistema operativo (Windows, macOS o Linux).

2. **Instalar Visual Studio Code**:
   - Ejecuta el instalador descargado y sigue las instrucciones en pantalla para completar la instalación.

## 2. Instalación del .NET SDK

1. **Descargar el .NET SDK**:
   - Ve a la página oficial de [.NET](https://dotnet.microsoft.com/download).
   - Selecciona la versión más reciente del SDK de .NET y descárgala.

2. **Instalar el .NET SDK**:
   - Ejecuta el instalador descargado y sigue las instrucciones en pantalla para completar la instalación.

3. **Verificar la instalación**:
   - Abre una terminal (Command Prompt o PowerShell en Windows, Terminal en macOS o Linux).
   - Ejecuta el siguiente comando para verificar que .NET se haya instalado correctamente:

   ```bash
   dotnet --version
   ```

   Deberías ver la versión del SDK instalada.

## 3. Instalación de Redis (opcional)

Si tu aplicación utiliza Redis, sigue estos pasos:

1. **Descargar Redis**:
   - Ve a la página oficial de [Redis](https://redis.io/download).
   - Sigue las instrucciones para instalar Redis en tu sistema operativo.

2. **Iniciar Redis**:
   - Asegúrate de que Redis esté en funcionamiento. Puedes iniciar el servidor Redis ejecutando el siguiente comando en la terminal:

   ```bash
   redis-server
   ```

## 4. Instalación de SQL Server (opcional)

Si tu aplicación utiliza SQL Server, sigue estos pasos:

1. **Descargar SQL Server**:
   - Ve a la página oficial de [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).
   - Descarga la versión adecuada para tu sistema operativo.

2. **Instalar SQL Server**:
   - Ejecuta el instalador y sigue las instrucciones en pantalla para completar la instalación.

3. **Configurar SQL Server**:
   - Asegúrate de que el servicio de SQL Server esté en funcionamiento.

## 5. Instalación de Extensiones para Visual Studio Code

1. **Abrir Visual Studio Code**:
   - Inicia Visual Studio Code.

2. **Instalar las extensiones necesarias**:
   - Haz clic en el icono de extensiones en la barra lateral izquierda (o presiona `Ctrl+Shift+X`).
   - Busca e instala las siguientes extensiones:

   - **C#**: Proporciona soporte para el desarrollo de aplicaciones .NET.
   - **C# Extensions**: Mejora la experiencia de desarrollo con características adicionales.
   - **NuGet Package Manager**: Facilita la gestión de paquetes NuGet.
   - **Docker**: Si planeas usar Docker, esta extensión es útil para gestionar contenedores.

3. **Configurar la extensión de C#**:
   - Después de instalar la extensión de C#, es posible que se te pida instalar el SDK de .NET si no está presente. Asegúrate de seguir las instrucciones.

## 6. Clonar el Repositorio

1. **Clonar el repositorio del proyecto**:
   - Abre una terminal y navega a la carpeta donde deseas clonar el proyecto.
   - Ejecuta el siguiente comando:

   ```bash
   git clone https://github.com/tu_usuario/dataqboxbackend.git
   cd dataqboxbackend
   ```

## 7. Restaurar Dependencias

1. **Restaurar las dependencias del proyecto**:
   - En la terminal, ejecuta el siguiente comando:

   ```bash
   dotnet restore
   ```

## 8. Compilar y Ejecutar la Aplicación

### Compilación

Para compilar la aplicación en modo de producción, utiliza el siguiente comando:

```bash
dotnet publish -c Release
```

Esto generará los archivos necesarios en la carpeta `bin/Release/net6.0/publish`.

### Ejecución en Desarrollo

Para ejecutar la aplicación en modo de desarrollo, utiliza el siguiente comando:

```bash
dotnet run
```

La aplicación estará disponible en `http://localhost:5000`.

### Ejecución en Producción

Para ejecutar la aplicación en producción con parámetros específicos, utiliza el siguiente comando:

```bash
dotnet C:\Users\Dell\Dropbox\BackendNetCore\dataqboxbackend\bin\Release\net9.0\dataqboxbackend.dll --urls "https://localhost:7174;http://localhost:5088"
```

Esto iniciará la aplicación en los puertos especificados.

## Conclusión

Siguiendo estos pasos, deberías tener un ambiente de desarrollo completamente funcional para trabajar con aplicaciones .NET en tu máquina. Si tienes alguna pregunta o necesitas más ayuda, no dudes en consultar la documentación oficial de .NET o abrir un issue en el repositorio.
