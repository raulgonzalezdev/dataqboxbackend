# Proyecto DataQBoxBackend

Este proyecto es una API construida con ASP.NET Core. A continuación, se detallan los pasos para configurar el ambiente de desarrollo, ejecutar la aplicación y desplegarla en producción.

## 1. Configuración del Ambiente de Desarrollo

### Requisitos Previos

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) o [Visual Studio Code](https://code.visualstudio.com/)
- [.NET SDK 6.0 o superior](https://dotnet.microsoft.com/download)
- [Redis](https://redis.io/download) (opcional, si se utiliza Redis)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (o cualquier otro motor de base de datos que estés utilizando)

### Instalación de Dependencias

1. Clona el repositorio:

   ```bash
   git clone https://github.com/tu_usuario/dataqboxbackend.git
   cd dataqboxbackend
   ```

 Reenombra el arcvivo: appsettingsSAMPLE.json  por appsettings.json   y actualiza los datos de la base de datos y tus claves de API 

2. Restaura las dependencias del proyecto:

   ```bash
   dotnet restore
   ```

3. Asegúrate de que Redis y SQL Server estén en funcionamiento.

## 2. Ejecución en Desarrollo

Para ejecutar la aplicación en modo de desarrollo, utiliza el siguiente comando:

```bash
dotnet run
```

La aplicación estará disponible en `http://localhost:5000` (o el puerto que hayas configurado).

### Ejecución en Producción

Para ejecutar la aplicación en producción, asegúrate de que la configuración de producción esté establecida en `appsettings.Production.json`. Luego, puedes usar el siguiente comando:

```bash
dotnet publish -c Release
```

Esto generará los archivos necesarios en la carpeta `bin/Release/net6.0/publish`. Puedes ejecutar la aplicación desde esa carpeta:

```bash
cd bin/Release/net6.0/publish
dotnet dataqboxbackend.dll
```

## 3. Dependencias

Las dependencias del proyecto están definidas en el archivo `.csproj`. Puedes verlas en el archivo `dataqboxbackend.csproj`.

## 4. Scripts de Instalación

### Windows (.bat)

Crea un archivo llamado `install.bat` con el siguiente contenido:

```bat
@echo off
echo Instalando dependencias...
dotnet restore
echo Dependencias instaladas.
```

### Linux/Mac (.sh)

Crea un archivo llamado `install.sh` con el siguiente contenido:

```bash
#!/bin/bash
echo "Instalando dependencias..."
dotnet restore
echo "Dependencias instaladas."
```

No olvides dar permisos de ejecución al script:

```bash
chmod +x install.sh
```

## 5. Docker

### Dockerfile

Crea un archivo llamado `Dockerfile` en la raíz del proyecto con el siguiente contenido:

```dockerfile
# Usar la imagen base de .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copiar el archivo .csproj y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y compilar
COPY . ./
RUN dotnet publish -c Release -o out

# Usar la imagen base de .NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/out .

# Exponer el puerto
EXPOSE 80

# Comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "dataqboxbackend.dll"]
```

### docker-compose.yml

Crea un archivo llamado `docker-compose.yml` en la raíz del proyecto con el siguiente contenido:

```yaml
version: '3.4'

services:
  app:
    image: dataqboxbackend
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

## 6. Despliegue con Docker

Para construir y ejecutar la aplicación en un contenedor Docker, utiliza el siguiente comando:

```bash
docker-compose up --build
```

La aplicación estará disponible en `http://localhost:5000`.

## Contribuciones

Si deseas contribuir a este proyecto, por favor abre un issue o envía un pull request.

## Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo LICENSE para más detalles.
