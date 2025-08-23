# accessibility-ms-reports

🚀 **Microservicio de Reportes y Historial** para el sistema de accesibilidad web.

- API RESTful para gestión de reportes e historial de generación
- Endpoints para crear, consultar y eliminar reportes e historial
- Respuestas internacionalizadas (i18n) y manejo global de errores
- Validación robusta con FluentValidation
- Documentación OpenAPI/Swagger interactiva
- Pruebas de integración automatizadas con xUnit y base de datos InMemory
- Base de datos MySQL para producción/desarrollo, InMemory para tests
- Listo para despliegue en Docker y Docker Compose
- Configuración de entorno automática (Development/Production/TestEnvironment)

## 🏗️ Estructura del proyecto

```
.
├── docker-compose.yml
├── Dockerfile
├── .env.development
├── .env.production
├── Directory.Packages.props
├── README.md
├── Reports.sln
├── src/
│   ├── Reports.Api/           # API principal (Swagger, FluentValidation, i18n)
│   ├── Reports.Application/   # DTOs, validadores y servicios de aplicación
│   ├── Reports.Domain/        # Entidades y enums de dominio
│   ├── Reports.Infrastructure/# DbContext, ServiceRegistration, acceso a datos
│   └── Reports.Tests/         # Pruebas de integración con xUnit y TestWebApplicationFactory
│       └── Infrastructure/    # TestWebApplicationFactory para entorno de tests
```

## ⚙️ Configuración de Entornos

El proyecto utiliza **detección automática de entorno** basada en la variable `ASPNETCORE_ENVIRONMENT`:

| Entorno           | Base de Datos | Migraciones            | Propósito           |
| ----------------- | ------------- | ---------------------- | ------------------- |
| `Development`     | MySQL         | `MigrateAsync()`       | Desarrollo local    |
| `Production`      | MySQL         | `MigrateAsync()`       | Producción          |
| `TestEnvironment` | InMemory      | `EnsureCreatedAsync()` | Tests automatizados |

### Configuración de Variables de Entorno

Configura los archivos `.env.development` y `.env.production` para tus entornos:

```env
# .env.development
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8083
DB_NAME=reportsdb
DB_USER=root
DB_PASSWORD=yourpassword
API_HOST_PORT=8083
```

```env
# .env.production
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8083
DB_NAME=reportsdb
DB_USER=msuser
DB_PASSWORD=prodpassword
API_HOST_PORT=8083
```

> **Nota:** No es necesario definir `DB_HOST` ni `DB_PORT` en los archivos `.env`, ya que la comunicación interna entre contenedores Docker utiliza el nombre del servicio (`reports-mysql`) y el puerto por defecto (`3306`).

## 🐳 Uso con Docker Compose

```bash
# Desarrollo
docker compose --env-file .env.development up --build

# Producción
docker compose --env-file .env.production up --build

# Detener servicios
docker compose down

# Ver logs
docker compose logs -f reports-api
```

## 🔧 Desarrollo Local

### Prerequisitos

- .NET 9.0 SDK
- MySQL Server (para desarrollo local sin Docker)

### Compilación y Ejecución

```bash
# Restaurar dependencias
dotnet restore Reports.sln

# Compilar proyecto
dotnet build Reports.sln

# Ejecutar API (puerto 5000/5001)
dotnet run --project src/Reports.Api/Reports.Api.csproj

# Ejecutar con watch (recarga automática)
dotnet watch run --project src/Reports.Api/Reports.Api.csproj
```

### 🧪 Ejecutar Pruebas

```bash
# Ejecutar todas las pruebas
dotnet test Reports.sln --verbosity normal

# Ejecutar solo el proyecto de pruebas
dotnet test src/Reports.Tests/Reports.Tests.csproj

# Ejecutar con reporte de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

Las pruebas utilizan:

- **xUnit** como framework de testing
- **FluentAssertions** para aserciones expresivas
- **TestWebApplicationFactory** para pruebas de integración
- **Base de datos InMemory** para aislamiento y velocidad
- **Microsoft.AspNetCore.Mvc.Testing** para testing de APIs

## 📊 Arquitectura de Tests

### Configuración Automática de Entornos

El proyecto implementa **detección automática de entorno** en `ServiceRegistration.cs`:

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
{
    var environment = config["ASPNETCORE_ENVIRONMENT"];

    if (environment == "TestEnvironment")
    {
        // Usar InMemory database para tests
        services.AddDbContext<ReportsDbContext>(opt =>
        {
            opt.UseInMemoryDatabase("TestReportsDb");
        });
    }
    else
    {
        // Usar MySQL para desarrollo y producción
        var cs = config.GetConnectionString("Default") ?? "...";
        services.AddDbContext<ReportsDbContext>(opt =>
        {
            opt.UseMySql(cs, ServerVersion.AutoDetect(cs), ...);
        });
    }
}
```

### TestWebApplicationFactory

Las pruebas utilizan una fábrica personalizada que configura automáticamente el entorno de tests:

```csharp
public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment"
                })
                .Build();

            services.AddSingleton<IConfiguration>(configuration);
        });

        builder.UseEnvironment("TestEnvironment");
    }
}
```

## 🚀 Dockerización y Despliegue

Este proyecto está preparado para ejecutarse fácilmente en contenedores Docker.

### Archivos de Configuración

- **Dockerfile**: Imagen multi-etapa optimizada para producción
- **docker-compose.yml**: Orquestación de servicios (API + MySQL)
- **.env.development/.env.production**: Variables de entorno por ambiente

### Personalización de la Imagen

```yaml
# docker-compose.yml
services:
  reports-api:
    image: magodeveloper/accessibility-ms-reports:latest
    # ...
```

### Flujo de Despliegue Recomendado

1. **Desarrollo:**

   ```bash
   docker compose --env-file .env.development up --build
   ```

2. **Producción:**

   ```bash
   docker compose --env-file .env.production up --build
   ```

3. **Acceso:** La API estará disponible en `http://localhost:${API_HOST_PORT}`

## 📋 Endpoints Principales

### 🏷️ Reportes

| Método   | Endpoint                               | Descripción                              |
| -------- | -------------------------------------- | ---------------------------------------- |
| `GET`    | `/api/report/by-analysis/{analysisId}` | Obtiene reportes por ID de análisis      |
| `GET`    | `/api/report/by-date/{date}`           | Obtiene reportes por fecha de generación |
| `GET`    | `/api/report/by-format/{format}`       | Obtiene reportes por formato             |
| `POST`   | `/api/report`                          | Crea un nuevo reporte                    |
| `DELETE` | `/api/report/{id}`                     | Elimina un reporte por ID                |

#### Crear Reporte

**Endpoint:** `POST /api/report`

**Payload ejemplo:**

```json
{
  "analysisId": 1,
  "format": "PDF",
  "filePath": "reporte-2025.pdf",
  "generationDate": "2025-08-19T00:00:00Z"
}
```

**Respuesta 200:**

```json
{
  "message": "Reporte creado correctamente.",
  "data": {
    "id": 10,
    "analysisId": 1,
    "format": "PDF",
    "filePath": "reporte-2025.pdf",
    "generationDate": "2025-08-19T00:00:00Z",
    "createdAt": "2025-08-19T00:00:00Z",
    "updatedAt": "2025-08-19T00:00:00Z"
  }
}
```

### 📊 Historial

| Método   | Endpoint                                | Descripción                          |
| -------- | --------------------------------------- | ------------------------------------ |
| `GET`    | `/api/history/by-user/{userId}`         | Obtiene historial por ID de usuario  |
| `GET`    | `/api/history/by-analysis/{analysisId}` | Obtiene historial por ID de análisis |
| `POST`   | `/api/history`                          | Crea un nuevo historial              |
| `DELETE` | `/api/history/{id}`                     | Elimina un historial por ID          |

#### Crear Historial

**Endpoint:** `POST /api/history`

**Payload ejemplo:**

```json
{
  "userId": 42,
  "analysisId": 1
}
```

**Respuesta 200:**

```json
{
  "message": "Historial creado correctamente.",
  "data": {
    "id": 5,
    "userId": 42,
    "analysisId": 1,
    "createdAt": "2025-08-19T00:00:00Z",
    "updatedAt": "2025-08-19T00:00:00Z"
  }
}
```

## 📚 Documentación API

### Swagger/OpenAPI

La documentación interactiva está disponible en `/swagger` cuando la API se ejecuta en modo desarrollo.

**Características:**

- Ejemplos de request/response automáticos
- Validaciones en tiempo real
- Esquemas de datos detallados
- Interfaz interactiva para probar endpoints

**URLs de acceso:**

- Desarrollo: `http://localhost:8083/swagger`
- Docker: `http://localhost:${API_HOST_PORT}/swagger`

### Respuestas de Error

Todas las respuestas de error siguen el formato estándar:

```json
{
  "error": "Mensaje de error internacionalizado"
}
```

**Códigos de estado comunes:**

- `200` - Éxito
- `201` - Recurso creado
- `400` - Error de validación
- `404` - Recurso no encontrado
- `409` - Conflicto (recurso duplicado)
- `500` - Error interno del servidor

## 🌐 Internacionalización (i18n)

El microservicio soporta múltiples idiomas:

**Idiomas soportados:**

- Español (`es`) - Por defecto
- Inglés (`en`)

**Configuración:**

- Header: `Accept-Language: en` o `Accept-Language: es`
- Fallback automático a español si no se especifica idioma

**Archivos de recursos:**

- `src/Reports.Api/Resources/`
- Mensajes de error y respuestas automáticamente localizados

## 🛠️ Tecnologías y Paquetes

### Backend

- **.NET 9.0** - Framework principal
- **Entity Framework Core 9.0** - ORM para acceso a datos
- **MySQL/Pomelo** - Base de datos para producción/desarrollo
- **InMemory Database** - Base de datos para tests
- **FluentValidation** - Validación de DTOs
- **AutoMapper** - Mapeo entre entidades y DTOs

### Testing

- **xUnit** - Framework de testing
- **FluentAssertions** - Aserciones expresivas
- **Microsoft.AspNetCore.Mvc.Testing** - Testing de integración
- **TestWebApplicationFactory** - Factory personalizada para tests

### Documentación

- **OpenAPI/Swagger** - Documentación interactiva de API
- **Swashbuckle** - Generación automática de documentación

### Infraestructura

- **Docker & Docker Compose** - Contenerización
- **MySQL** - Base de datos relacional
- **Central Package Management** - Gestión centralizada de paquetes NuGet

## 🔒 Consideraciones de Seguridad

### Variables de Entorno

- Credenciales de base de datos en archivos `.env`
- Configuración específica por ambiente
- No incluir archivos `.env` en control de versiones

### Validación

- Validación automática de DTOs con FluentValidation
- Manejo global de errores y excepciones
- Responses sanitizadas sin exposición de detalles internos

## 📈 Monitoreo y Logs

### Logging

- Logging estructurado con ILogger
- Logs automáticos de Entity Framework
- Información de requests/responses en desarrollo

### Health Checks

El microservicio expone información básica de salud:

- Estado de conexión a base de datos
- Información de versión de API

## 🚀 Próximas Mejoras

### En desarrollo

- [ ] Health checks endpoint (`/health`)
- [ ] Métricas de Prometheus
- [ ] Rate limiting
- [ ] Autenticación JWT
- [ ] Logs estructurados con Serilog
- [ ] Tests de carga automatizados

### Pipeline CI/CD

- [ ] GitHub Actions para tests automáticos
- [ ] Build y push automático de imágenes Docker
- [ ] Deploy automático a staging/producción

## 🤝 Contribución

### Desarrollo Local

1. Fork del repositorio
2. Crear branch de feature: `git checkout -b feature/nueva-funcionalidad`
3. Commit de cambios: `git commit -am 'Add nueva funcionalidad'`
4. Push al branch: `git push origin feature/nueva-funcionalidad`
5. Crear Pull Request

### Estándares de Código

- Seguir convenciones de C# y .NET
- Tests unitarios para nuevas funcionalidades
- Documentación actualizada en README
- Versionado semántico

---

## 📞 Contacto y Soporte

**Desarrollado por:** magodeveloper  
**Año:** 2025  
**Licencia:** MIT

**Enlaces útiles:**

- 📋 [Issues](../../issues)
- 🔄 [Pull Requests](../../pulls)
- 📖 [Wiki](../../wiki)

---

_Este microservicio forma parte del ecosistema de accesibilidad web, trabajando en conjunto con `accessibility-ms-analysis` y `accessibility-ms-users` para proporcionar una solución completa de análisis y reporting de accesibilidad._
