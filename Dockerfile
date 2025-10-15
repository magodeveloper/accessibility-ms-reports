# ============================================================================
# Multi-stage Dockerfile para Microservicio Reports
# ============================================================================
# STAGE 1: Build - Compila la aplicación
# STAGE 2: Runtime - Imagen optimizada para producción
# ============================================================================

# ============ STAGE 1: build ============
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar global.json para asegurar versión correcta del SDK
COPY global.json ./

# Copiar archivos de solución y gestión de paquetes
COPY Reports.sln ./
COPY Directory.Packages.props ./

# Copiar archivos .csproj (optimiza cache de Docker - solo se invalida si cambian las dependencias)
COPY src/Reports.Api/Reports.Api.csproj                      src/Reports.Api/
COPY src/Reports.Application/Reports.Application.csproj      src/Reports.Application/
COPY src/Reports.Domain/Reports.Domain.csproj                src/Reports.Domain/
COPY src/Reports.Infrastructure/Reports.Infrastructure.csproj src/Reports.Infrastructure/
COPY src/Reports.Tests/Reports.Tests.csproj                  src/Reports.Tests/

# Restaurar dependencias (capa independiente para aprovechar cache)
RUN dotnet restore Reports.sln

# Copiar el resto del código fuente
COPY src/ src/

# Publicar aplicación (--no-restore evita restore duplicado)
RUN dotnet publish ./src/Reports.Api/Reports.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ============ STAGE 2: runtime ============
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Instalar curl para health checks (necesario para Docker HEALTHCHECK) y crear usuario no-root para seguridad
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    rm -rf /var/lib/apt/lists/* && \
    groupadd -r appuser && useradd -r -g appuser appuser

# Copiar binarios publicados desde stage de build
COPY --from=build /app/publish .

# Cambiar permisos y usuario
RUN chown -R appuser:appuser /app
USER appuser

# Exponer puerto
EXPOSE 8083

# Health check integrado
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
    CMD curl -f http://localhost:8083/health || exit 1

# Punto de entrada
ENTRYPOINT ["dotnet", "Reports.Api.dll", "--urls", "http://0.0.0.0:8083"]