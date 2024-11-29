# Usar la imagen base de .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar el archivo .csproj y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar el resto del código y compilar
COPY . ./
RUN dotnet publish -c Release -o out

# Usar la imagen base de .NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Exponer el puerto
EXPOSE 80

# Comando para ejecutar la aplicación
ENTRYPOINT ["dotnet", "dataqboxbackend.dll"]
