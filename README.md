# 📊 Reports Microservice

> **Microservicio de generación de reportes y gestión de historial** para el ecosistema de accesibilidad web empresarial. Construido con .NET 9.0, MySQL y Clean Architecture.

## 🚀 Características Principales

- **🎯 API RESTful Completa**: Gestión integral de reportes e historiales de accesibilidad
- **📊 Generación Multi-formato**: Reportes en PDF, HTML, JSON y CSV
- **🏗️ Clean Architecture**: Separación clara entre capas Domain, Application, Infrastructure y API
- **🗄️ Base de Datos MySQL**: Almacenamiento optimizado con Entity Framework Core 9.0
- **🌐 Internacionalización**: Soporte completo para español (ES) e inglés (EN)
- **🔍 Validación Avanzada**: FluentValidation con manejo global de errores
- **🧪 Testing Integral**: xUnit, InMemory DB y pruebas de integración
- **🐳 Docker Ready**: Containerización multi-stage optimizada
- **📖 Documentación OpenAPI**: Swagger/OpenAPI 3.0 interactivo
- **🔗 Integración Cross-Service**: Comunicación con accessibility-ms-analysis y accessibility-ms-users

---

## 📋 Tabla de Contenidos

### 🏗️ Arquitectura y Estructura

- [🏗️ Estructura del Proyecto](#️-estructura-del-proyecto)
- [📊 Modelos de Datos](#-modelos-de-datos)
- [🗄️ Base de Datos MySQL](#️-base-de-datos-mysql)
- [� Configuración](#-configuración)

### 🛠️ Desarrollo y Deployment

- [⚡ Inicio Rápido](#-inicio-rápido)
- [� Docker y Contenedores](#-docker-y-contenedores)
- [🧪 Testing](#-testing)
- [📊 Scripts de Gestión](#-scripts-de-gestión)

### 🌐 API y Funcionalidades

- [� API Endpoints](#-api-endpoints)
- [� Ejemplos de Uso](#-ejemplos-de-uso)
- [🌍 Internacionalización](#-internacionalización)
- [� Seguridad](#-seguridad)

### � Referencia y Soporte

- [� Troubleshooting](#-troubleshooting)
- [📖 Recursos Adicionales](#-recursos-adicionales)
- [🤝 Contribución](#-contribución)

---

## 🏗️ Estructura del Proyecto

```
📦 accessibility-ms-reports/
├── � coverlet.runsettings           # Configuración de cobertura de código
├── �📋 Directory.Packages.props       # Gestión centralizada de paquetes NuGet
├── 🐳 docker-compose.yml            # Orquestación completa de servicios
├── � Dockerfile                    # Imagen Docker multi-stage optimizada
├── �️ init-reports-db.sql           # Script de inicialización de base de datos
├── 🛠️ init-test-databases.ps1|sh     # Scripts para bases de datos de testing
├── 🧪 manage-tests.ps1               # Script de gestión de pruebas
├── 📦 package.json                   # Configuración Node.js para herramientas
├── 📖 README.md                      # Documentación completa del proyecto
├── 🔧 Reports.sln                    # Solución .NET 9.0 principal
├── � test-dashboard.html            # Dashboard de resultados de testing
└── 📁 src/
    ├── � Reports.Api/               # API Principal y Configuración
    │   ├── 🚀 Program.cs             # Punto de entrada y configuración DI
    │   ├── ⚙️ appsettings.json       # Configuración base de la aplicación
    │   ├── ⚙️ appsettings.Development.json # Configuración de desarrollo
    │   ├── 📦 Reports.Api.csproj     # Archivo de proyecto de la API
    │   ├── 🎮 Controllers/           # Controladores REST
    │   │   ├── ReportController.cs   # Gestión de reportes
    │   │   └── HistoryController.cs  # Gestión de historial
    │   ├── 🔧 Helpers/               # Utilidades y extensiones
    │   │   ├── LanguageHelper.cs     # Soporte de idiomas
    │   │   └── LocalizationHelper.cs # Configuración de localización
    │   └── 📁 Resources/             # Archivos de recursos multiidioma
    │       ├── messages.en.json      # Mensajes en inglés
    │       └── messages.es.json      # Mensajes en español
    │
    ├── 💼 Reports.Application/        # Lógica de Negocio y Servicios
    │   ├── 📋 DTOs/                  # Data Transfer Objects
    │   │   ├── ReportRequestDto.cs   # DTO para solicitudes de reporte
    │   │   ├── ReportResponseDto.cs  # DTO para respuestas de reporte
    │   │   ├── HistoryRequestDto.cs  # DTO para solicitudes de historial
    │   │   └── HistoryResponseDto.cs # DTO para respuestas de historial
    │   ├── 🛡️ Validators/            # Validadores FluentValidation
    │   │   ├── ReportRequestValidator.cs # Validaciones de reportes
    │   │   └── HistoryRequestValidator.cs # Validaciones de historial
    │   └── ⚙️ Services/              # Servicios de aplicación
    │       ├── IReportService.cs     # Interfaz del servicio de reportes
    │       ├── ReportService.cs      # Implementación del servicio de reportes
    │       ├── IHistoryService.cs    # Interfaz del servicio de historial
    │       └── HistoryService.cs     # Implementación del servicio de historial
    │
    ├── 🏛️ Reports.Domain/            # Entidades y Reglas de Dominio
    │   ├── 📊 Entities/              # Entidades del dominio
    │   │   ├── Report.cs             # Entidad principal de reporte
    │   │   └── History.cs            # Entidad de historial
    │   └── 📋 Enums/                 # Enumeraciones del dominio
    │       ├── ReportFormat.cs       # Formatos de reporte (PDF, HTML, JSON)
    │       ├── ReportStatus.cs       # Estados de reporte
    │       └── HistoryType.cs        # Tipos de historial
    │
    ├── 🔌 Reports.Infrastructure/     # Acceso a Datos y Servicios Externos
    │   ├── 🗃️ Data/                 # Configuración de Entity Framework
    │   │   ├── ReportsDbContext.cs   # Contexto principal de la base de datos
    │   │   └── Configurations/       # Configuraciones de entidades
    │   │       ├── ReportConfiguration.cs # Configuración de entidad Report
    │   │       └── HistoryConfiguration.cs # Configuración de entidad History
    │   ├── 🔄 Migrations/            # Migraciones de base de datos
    │   │   ├── 001_InitialCreate.cs  # Migración inicial
    │   │   └── [Timestamp]_*.cs      # Migraciones adicionales
    │   └── 📦 ServiceRegistration.cs # Registro de servicios de infraestructura
    │
    └── 🧪 Reports.Tests/             # Suite de Pruebas Automatizadas
        ├── 📦 Reports.Tests.csproj   # Archivo de proyecto de pruebas
        ├── 🌐 ReportsApiTests.cs     # Pruebas de la API principal
        ├── 🎮 Controllers/           # Pruebas de controladores
        │   ├── ReportControllerTests.cs # Tests del controlador de reportes
        │   └── HistoryControllerTests.cs # Tests del controlador de historial
        ├── 💼 Application/           # Pruebas de servicios de aplicación
        │   ├── ReportServiceTests.cs # Tests del servicio de reportes
        │   └── HistoryServiceTests.cs # Tests del servicio de historial
        ├── 🏛️ Domain/               # Pruebas de entidades del dominio
        │   └── DomainEntitiesTests.cs # Tests de entidades
        ├── � Dtos/                  # Pruebas de DTOs
        │   └── DtoInstantiationTests.cs # Tests de instanciación de DTOs
        ├── � Helpers/               # Pruebas de utilidades
        │   ├── LanguageHelperTests.cs # Tests de helper de idiomas
        │   └── LocalizationHelperTests.cs # Tests de localización
        ├── 🔌 Infrastructure/        # Pruebas de infraestructura
        │   ├── ReportsDbContextTests.cs # Tests del contexto de BD
        │   ├── EntityConfigurationTests.cs # Tests de configuraciones EF
        │   ├── MigrationsTests.cs    # Tests de migraciones
        │   ├── DatabasePerformanceTests.cs # Tests de rendimiento
        │   ├── ServiceRegistrationTests.cs # Tests de registro de servicios
        │   └── ReportsTestWebApplicationFactory.cs # Factory para tests
        ├── 🔗 IntegrationTests/      # Pruebas de integración
        │   └── ReportManagementIntegrationTests.cs # Tests end-to-end
        └── 🧪 UnitTests/             # Pruebas unitarias específicas
            ├── ExtendedLocalizationTests.cs # Tests de localización avanzada
            ├── ProgramUnitTests.cs   # Tests del punto de entrada
            └── Services/             # Tests unitarios de servicios
                ├── ReportServiceTests.cs # Tests unitarios de reportes
                └── HistoryServiceTests.cs # Tests unitarios de historial
```

## 📊 Modelos de Datos

### � Entidad Report

```csharp
public class Report
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public ReportFormat Format { get; set; }
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string CreatedBy { get; set; }
    public string FilePath { get; set; }
    public long FileSize { get; set; }
    public string Language { get; set; } // "es-ES" | "en-US"

    // Metadatos del reporte
    public string SourceAnalysisId { get; set; }
    public int TotalIssues { get; set; }
    public int CriticalIssues { get; set; }
    public int WarningIssues { get; set; }
    public int InfoIssues { get; set; }

    // Navegación
    public ICollection<History> Histories { get; set; }
}
```

### 📈 Entidad History

```csharp
public class History
{
    public int Id { get; set; }
    public int ReportId { get; set; }
    public HistoryType Type { get; set; }
    public string Action { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Details { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }

    // Navegación
    public Report Report { get; set; }
}
```

### 📋 Enumeraciones

#### ReportFormat

```csharp
public enum ReportFormat
{
    PDF = 1,     // Documento PDF optimizado
    HTML = 2,    // Página web interactiva
    JSON = 3,    // Datos estructurados API
    CSV = 4      // Datos tabulares Excel
}
```

#### ReportStatus

```csharp
public enum ReportStatus
{
    PENDING = 1,     // En cola de generación
    GENERATING = 2,  // Procesando
    COMPLETED = 3,   // Generado exitosamente
    FAILED = 4,      // Error en generación
    EXPIRED = 5      // Caducado (>30 días)
}
```

#### HistoryType

```csharp
public enum HistoryType
{
    GENERATION = 1,  // Generación de reporte
    DOWNLOAD = 2,    // Descarga de reporte
    DELETION = 3,    // Eliminación de reporte
    SHARING = 4      // Compartir reporte
}
```

## 🗄️ Base de Datos MySQL

### 📊 Esquema de Base de Datos

```sql
-- Tabla Reports
CREATE TABLE Reports (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(255) NOT NULL,
    Description TEXT,
    Format ENUM('PDF', 'HTML', 'JSON', 'CSV') NOT NULL DEFAULT 'PDF',
    Status ENUM('PENDING', 'GENERATING', 'COMPLETED', 'FAILED', 'EXPIRED') NOT NULL DEFAULT 'PENDING',
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CompletedAt DATETIME NULL,
    CreatedBy VARCHAR(100) NOT NULL,
    FilePath VARCHAR(500),
    FileSize BIGINT DEFAULT 0,
    Language VARCHAR(10) NOT NULL DEFAULT 'es-ES',
    SourceAnalysisId VARCHAR(100),
    TotalIssues INT DEFAULT 0,
    CriticalIssues INT DEFAULT 0,
    WarningIssues INT DEFAULT 0,
    InfoIssues INT DEFAULT 0,

    INDEX idx_reports_status (Status),
    INDEX idx_reports_created_by (CreatedBy),
    INDEX idx_reports_created_at (CreatedAt),
    INDEX idx_reports_language (Language),
    INDEX idx_reports_source_analysis (SourceAnalysisId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Tabla History
CREATE TABLE History (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ReportId INT NOT NULL,
    Type ENUM('GENERATION', 'DOWNLOAD', 'DELETION', 'SHARING') NOT NULL,
    Action VARCHAR(255) NOT NULL,
    Timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UserId VARCHAR(100) NOT NULL,
    UserName VARCHAR(255),
    Details TEXT,
    IpAddress VARCHAR(45),
    UserAgent VARCHAR(500),

    INDEX idx_history_report_id (ReportId),
    INDEX idx_history_user_id (UserId),
    INDEX idx_history_timestamp (Timestamp),
    INDEX idx_history_type (Type),

    FOREIGN KEY (ReportId) REFERENCES Reports(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

### 🔧 Configuración de Entity Framework

```csharp
// Reports.Infrastructure/Data/ReportsDbContext.cs
public class ReportsDbContext : DbContext
{
    public ReportsDbContext(DbContextOptions<ReportsDbContext> options) : base(options) { }

    public DbSet<Report> Reports { get; set; }
    public DbSet<History> History { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### 🚀 Migraciones

```bash
# Crear nueva migración
dotnet ef migrations add InitialCreate --project src/Reports.Infrastructure --startup-project src/Reports.Api

# Aplicar migraciones
dotnet ef database update --project src/Reports.Infrastructure --startup-project src/Reports.Api

# Generar script SQL
dotnet ef migrations script --project src/Reports.Infrastructure --startup-project src/Reports.Api
```

## 🔧 Configuración

### ⚙️ Variables de Entorno

```bash
# === APLICACIÓN ===
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5003

# === BASE DE DATOS ===
ConnectionStrings__DefaultConnection=Server=localhost;Port=3309;Database=reportsdb;Uid=reportsuser;Pwd=ReportsApp2025SecurePass;
DB_NAME=reportsdb
DB_USER=reportsuser
DB_PASSWORD=ReportsApp2025SecurePass
DB_ROOT_PASSWORD=cH9QM3YwWOJJZaZ3ZyYloMqU6dcDCWiN
DB_PORT=3309

# === CONFIGURACIÓN DEL SERVICIO ===
API_HOST_PORT=5003
API_VERSION=v1
ENABLE_SWAGGER=true

# === CROSS-MICROSERVICES ===
ANALYSIS_API_URL=http://accessibility-ms-analysis:5002
USERS_API_URL=http://accessibility-ms-users:5001

# === GENERACIÓN DE REPORTES ===
MAX_REPORT_SIZE_MB=50
REPORT_RETENTION_DAYS=30
CONCURRENT_REPORTS_LIMIT=10
DEFAULT_REPORT_FORMAT=PDF

# === INTERNACIONALIZACIÓN ===
DEFAULT_CULTURE=es-ES
SUPPORTED_CULTURES=es-ES,en-US
ENABLE_LOCALIZATION=true

# === LOGGING ===
SERILOG_MINIMUM_LEVEL=Information
SERILOG_FILE_PATH=/app/logs/reports-{Date}.log
SERILOG_RETENTION_DAYS=30
```

### 🏗️ Configuración de Desarrollo (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3309;Database=reportsdb;Uid=reportsuser;Pwd=ReportsApp2025SecurePass;"
  },
  "CrossMicroservices": {
    "AnalysisApiUrl": "http://localhost:5002",
    "UsersApiUrl": "http://localhost:5001",
    "TimeoutSeconds": 30
  },
  "ReportGeneration": {
    "MaxReportSizeMB": 50,
    "RetentionDays": 30,
    "ConcurrentReportsLimit": 10,
    "DefaultFormat": "PDF",
    "OutputPath": "./reports",
    "TemplatePath": "./templates"
  },
  "Localization": {
    "DefaultCulture": "es-ES",
    "SupportedCultures": ["es-ES", "en-US"],
    "ResourcePath": "Resources"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

### 🚀 Configuración de Producción (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=${DB_HOST};Port=${DB_PORT};Database=${DB_NAME};Uid=${DB_USER};Pwd=${DB_PASSWORD};"
  },
  "CrossMicroservices": {
    "AnalysisApiUrl": "${ANALYSIS_API_URL}",
    "UsersApiUrl": "${USERS_API_URL}",
    "TimeoutSeconds": 60
  },
  "ReportGeneration": {
    "MaxReportSizeMB": 100,
    "RetentionDays": 90,
    "ConcurrentReportsLimit": 20,
    "DefaultFormat": "PDF",
    "OutputPath": "/app/reports",
    "TemplatePath": "/app/templates"
  },
  "Localization": {
    "DefaultCulture": "es-ES",
    "SupportedCultures": ["es-ES", "en-US", "pt-BR", "fr-FR"],
    "ResourcePath": "Resources"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  "AllowedHosts": "*"
}
```

## ⚡ Inicio Rápido

### 🛠️ Prerrequisitos

- **.NET 9.0 SDK** - [Descargar](https://dotnet.microsoft.com/download/dotnet/9.0)
- **MySQL 8.4+** - [Descargar](https://dev.mysql.com/downloads/mysql/)
- **Docker Desktop** (opcional) - [Descargar](https://www.docker.com/products/docker-desktop)
- **Visual Studio 2022** o **VS Code** con extensión C#

### 🚀 Instalación Local

```bash
# 1. Clonar el repositorio
git clone <repository-url>
cd accessibility-ms-reports

dotnet restore

# 3. Configurar base de datos
# Editar appsettings.Development.json con tu configuración MySQL

# 4. Ejecutar migraciones
dotnet ef database update --project src/Reports.Infrastructure --startup-project src/Reports.Api

# 5. Ejecutar la aplicación
dotnet run --project src/Reports.Api
```

### 🐳 Instalación con Docker

```bash
# 1. Construir y ejecutar con Docker Compose
docker-compose up -d

# 2. Verificar que los servicios están funcionando
docker-compose ps

# 3. Ver logs
docker-compose logs reports-api

# 4. Acceder a la API
# http://localhost:5003/swagger
```

### 🌐 Verificación de Instalación

```bash
# Verificar estado de la API
curl http://localhost:5003/health

# Verificar endpoints principales
curl http://localhost:5003/api/v1/reports
curl http://localhost:5003/api/v1/history
```

## 🧪 Testing

### 🎯 Estrategia de Testing

```bash
# Ejecutar todas las pruebas
dotnet test

# Pruebas con cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Pruebas específicas por categoría
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"

# Pruebas de un proyecto específico
dotnet test src/Reports.Tests/
```

### 📊 Scripts de Gestión

#### PowerShell (Windows)

```powershell
# manage-tests.ps1
./manage-tests.ps1 -Action "coverage" -Format "html"
./manage-tests.ps1 -Action "run" -Filter "Integration"
./manage-tests.ps1 -Action "clean"
```

#### Bash (Linux/macOS)

```bash
# Ejecutar suite completa
./manage-tests.sh --action=coverage --format=html

# Pruebas específicas
./manage-tests.sh --action=run --filter="Unit"
```

### 🎯 Tipos de Testing Implementados

| Tipo            | Descripción               | Cobertura | Herramientas                 |
| --------------- | ------------------------- | --------- | ---------------------------- |
| **Unitarias**   | Lógica de negocio aislada | 95%+      | xUnit, Moq, FluentAssertions |
| **Integración** | API endpoints y DB        | 90%+      | TestServer, InMemory DB      |
| **Performance** | Rendimiento y carga       | 85%+      | NBomber, BenchmarkDotNet     |
| **E2E**         | Flujos completos          | 80%+      | TestWebApplicationFactory    |

## 📝 Ejemplos de Uso

### 🚀 Crear un Reporte

```bash
# POST /api/v1/reports
curl -X POST "http://localhost:5003/api/v1/reports" \
  -H "Content-Type: application/json" \
  -H "Accept-Language: es-ES" \
  -d '{
    "title": "Análisis de Accesibilidad Web",
    "description": "Reporte completo basado en WCAG 2.1",
    "format": "PDF",
    "language": "es-ES",
    "sourceAnalysisId": "analysis_123",
    "createdBy": "admin@empresa.com"
  }'
```

**Respuesta:**

```json
{
  "success": true,
  "message": "Reporte creado exitosamente",
  "data": {
    "id": 1,
    "title": "Análisis de Accesibilidad Web",
    "status": "PENDING",
    "format": "PDF",
    "createdAt": "2024-01-15T10:30:00Z"
  }
}
```

### 📊 Obtener Reportes

```bash
# GET /api/v1/reports
curl "http://localhost:5003/api/v1/reports?page=1&pageSize=10&status=COMPLETED"

# GET /api/v1/reports/{id}
curl "http://localhost:5003/api/v1/reports/1"
```

### 📈 Gestionar Historial

```bash
# GET /api/v1/history
curl "http://localhost:5003/api/v1/history?reportId=1&type=GENERATION"

# POST /api/v1/history
curl -X POST "http://localhost:5003/api/v1/history" \
  -H "Content-Type: application/json" \
  -d '{
    "reportId": 1,
    "type": "DOWNLOAD",
    "action": "Reporte descargado por usuario",
    "userId": "user123",
    "userName": "Juan Pérez"
  }'
```

## 🌍 Internacionalización

### 🗺️ Idiomas Soportados

| Idioma  | Código  | Estado      | Cobertura |
| ------- | ------- | ----------- | --------- |
| Español | `es-ES` | ✅ Completo | 100%      |
| Inglés  | `en-US` | ✅ Completo | 100%      |

### 📁 Archivos de Recursos

```
src/Reports.Api/Resources/
├── messages.es.json      # Mensajes en español
├── messages.en.json      # Mensajes en inglés
└── SharedLocalizer.cs    # Clase de localización
```

### 🔧 Configuración

```json
{
  "Localization": {
    "DefaultCulture": "es-ES",
    "SupportedCultures": ["es-ES", "en-US"],
    "ResourcePath": "Resources"
  }
}
```

## 🔒 Seguridad

### 🛡️ Medidas de Seguridad Implementadas

- **Validación de entrada**: FluentValidation en todos los endpoints
- **Rate limiting**: Límites por IP y usuario
- **CORS**: Configuración restrictiva por entorno
- **Logs de auditoría**: Registro completo de actividades
- **Sanitización**: Limpieza de nombres de archivo y rutas
- **HTTPS**: Obligatorio en producción

### 🔑 Variables de Entorno Seguras

```bash
# Usar variables de entorno para datos sensibles
DB_PASSWORD=${REPORTS_DB_PASSWORD}
JWT_SECRET=${REPORTS_JWT_SECRET}

# No incluir credenciales en código o logs
```

## 🔧 Troubleshooting

### ❌ Problemas Comunes

#### Error de conexión a base de datos

```bash
# Verificar estado del contenedor
docker-compose ps

# Ver logs de MySQL
docker-compose logs reports-db

# Probar conexión manualmente
mysql -h localhost -P 3309 -u reportsuser -p
```

#### Timeout en generación de reportes

```bash
# Aumentar timeout en configuración
REPORT_GENERATION_TIMEOUT_MS=120000

# Verificar memoria disponible
docker stats accessibility-reports-api
```

#### Error de permisos en archivos

```bash
# Verificar permisos del directorio de reportes
ls -la /app/reports

# Corregir permisos
chown -R www-data:www-data /app/reports
chmod -R 755 /app/reports
```

### 🔍 Comandos de Diagnóstico

```bash
# Health check
curl http://localhost:5003/health

# Verificar configuración
curl http://localhost:5003/api/v1/config

# Ver métricas
curl http://localhost:5003/metrics

# Logs de la aplicación
docker-compose logs -f reports-api
```

## 📖 Recursos Adicionales

### 📚 Documentación

- [Entity Framework Core](https://docs.microsoft.com/ef/core/) - ORM utilizado
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/) - Framework web
- [Docker](https://docs.docker.com/) - Containerización
- [MySQL](https://dev.mysql.com/doc/) - Base de datos

### 🛠️ Herramientas de Desarrollo

- **Visual Studio 2022** - IDE recomendado
- **Visual Studio Code** - Editor alternativo
- **Postman** - Testing de API
- **Docker Desktop** - Containerización local

### 🧪 Testing

- **xUnit** - Framework de testing
- **Moq** - Mocking library
- **FluentAssertions** - Assertions fluidas
- **TestContainers** - Testing con contenedores

## 🤝 Contribución

### 🚀 Cómo Contribuir

1. **Fork** del repositorio
2. **Crear branch** para nueva funcionalidad
3. **Implementar** cambios con tests
4. **Documentar** cambios realizados
5. **Crear Pull Request** con descripción detallada

### 📋 Estándares de Código

- **Clean Code**: Código limpio y legible
- **SOLID Principles**: Principios de diseño orientado a objetos
- **Clean Architecture**: Separación clara de responsabilidades
- **Unit Testing**: Cobertura mínima del 80%
- **Documentation**: Comentarios y README actualizados

### 🐛 Reportar Issues

- Usar **GitHub Issues** para reportar bugs
- Incluir **logs relevantes** y **pasos para reproducir**
- Especificar **versión** y **entorno**
- Usar **labels** apropiadas

---

> **📊 Reports Microservice** - Versión 1.0.0  
> Desarrollado con ❤️ usando .NET 9.0, MySQL y Clean Architecture  
> 📅 Última actualización: Enero 2025

---

dotnet restore

# 3. Configurar base de datos

# Editar appsettings.Development.json con tu configuración MySQL

# 4. Ejecutar migraciones

dotnet ef database update --project src/Reports.Infrastructure --startup-project src/Reports.Api

# 5. Ejecutar la aplicación

dotnet run --project src/Reports.Api

````

### 🐳 Instalación con Docker

```bash
# 1. Construir y ejecutar con Docker Compose
docker-compose up -d

# 2. Verificar que los servicios están funcionando
docker-compose ps

# 3. Ver logs
docker-compose logs reports-api

# 4. Acceder a la API
# http://localhost:5003/swagger
````

### 🌐 Verificación de Instalación

```bash
# Verificar estado de la API
curl http://localhost:5003/health

# Verificar endpoints principales
curl http://localhost:5003/api/v1/reports
curl http://localhost:5003/api/v1/history
```

```bash
# Restaurar dependencias NuGet
dotnet restore Reports.sln

# Compilar en modo desarrollo
dotnet build Reports.sln --configuration Debug

# Ejecutar con recarga automática
dotnet watch run --project src/Reports.Api --environment Development

# Ejecutar en puerto específico
dotnet run --project src/Reports.Api --urls "http://localhost:8083"
```

### 🏗️ Build optimizado

```bash
# Compilación optimizada para producción
dotnet build Reports.sln --configuration Release --no-restore

# Build con análisis de código
dotnet build Reports.sln -c Release --verbosity detailed

# Publicación optimizada
dotnet publish src/Reports.Api -c Release -o ./publish --self-contained false
```

### ✅ Ejecución de pruebas

```bash
# Todas las pruebas con output detallado
dotnet test Reports.sln --verbosity normal --configuration Release

# Solo pruebas de integración
dotnet test src/Reports.Tests --filter Category=Integration

# Pruebas con reporte de cobertura
dotnet test Reports.sln --collect:"XPlat Code Coverage" --results-directory TestResults

# Generar reporte HTML de cobertura
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html
```

## 🗄️ Base de datos y arquitectura

### 📊 Estructura de datos optimizada

El microservicio utiliza **MySQL 8.4** con Entity Framework Core y gestiona reportes e historial:

```
📊 REPORTS (tabla principal)
├── 📋 Relación con ANALYSIS (cross-microservice)
└── 📈 HISTORY (tracking de actividades)
    └── 👤 Relación con USERS (cross-microservice)
```

**Tablas principales:**

- **REPORTS** → Información de reportes generados con metadatos
- **HISTORY** → Historial de actividades y auditoría de reportes

### ⚡ Optimizaciones de rendimiento

**🔍 Índices especializados implementados:**

```sql
-- Consultas por análisis (más común)
CREATE INDEX idx_reports_analysis ON REPORTS(analysis_id);
CREATE INDEX idx_reports_format_date ON REPORTS(format, generation_date);
CREATE INDEX idx_reports_status ON REPORTS(status);

-- Historial por usuario y análisis
CREATE INDEX idx_history_user ON HISTORY(user_id);
CREATE INDEX idx_history_analysis ON HISTORY(analysis_id);
CREATE INDEX idx_history_type_date ON HISTORY(history_type, created_at);

-- Búsquedas por fecha (reportes y auditoría)
CREATE INDEX idx_reports_generation_date ON REPORTS(generation_date);
CREATE INDEX idx_history_created_at ON HISTORY(created_at);
```

**🔗 Integridad referencial cross-microservice:**

```sql
-- Relación con accessibility-ms-analysis
ALTER TABLE REPORTS ADD CONSTRAINT fk_reports_analysis
FOREIGN KEY (analysis_id) REFERENCES analysisdb.ANALYSIS(id) ON DELETE CASCADE;

-- Relación con accessibility-ms-users (a través de history)
ALTER TABLE HISTORY ADD CONSTRAINT fk_history_user
FOREIGN KEY (user_id) REFERENCES usersdb.USERS(id) ON DELETE CASCADE;

-- Cascada interna: Reports → History
ALTER TABLE HISTORY ADD CONSTRAINT fk_history_analysis
FOREIGN KEY (analysis_id) REFERENCES REPORTS(analysis_id) ON DELETE CASCADE;
```

### 🔄 Gestión de migraciones

```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update --project src/Reports.Infrastructure --startup-project src/Reports.Api

# Generar nueva migración
dotnet ef migrations add NombreMigracion --project src/Reports.Infrastructure --startup-project src/Reports.Api

# Generar script SQL para revisión
dotnet ef migrations script --project src/Reports.Infrastructure --startup-project src/Reports.Api

# Rollback a migración específica
dotnet ef database update NombreMigracionAnterior --project src/Reports.Infrastructure --startup-project src/Reports.Api
```

> ⚠️ **Prerequisito importante**: Las constraints cross-microservice requieren que **accessibility-ms-analysis** y **accessibility-ms-users** estén funcionando y sus bases de datos creadas.

### 🧪 Base de datos de test

Para las pruebas se crean automáticamente bases de datos temporales:

```yaml
# Test Configuration
services:
  database-test:
    image: mysql:8.4
    environment:
      MYSQL_ROOT_PASSWORD: fK7SP6bZYRMMbdB6azbrpPtX9gfGGZlQ
      MYSQL_USER: testuser
      MYSQL_PASSWORD: TestApp2025SecurePass
    ports:
      - "3310:3306"
    volumes:
      - ./init-test-databases.sql:/docker-entrypoint-initdb.d/init.sql
```

**🛠️ Scripts de inicialización disponibles:**

- `init-test-databases.ps1` (Windows PowerShell)
- `init-test-databases.sh` (Linux/macOS)

## 🌐 API endpoints y ejemplos

### 📊 Endpoints de reportes

| 🎯 Acción                 | Método   | Endpoint                               | Descripción                                    |
| ------------------------- | -------- | -------------------------------------- | ---------------------------------------------- |
| **Crear reporte**         | `POST`   | `/api/report`                          | Genera nuevo reporte de accesibilidad          |
| **Obtener reporte**       | `GET`    | `/api/report/{id}`                     | Recupera reporte específico con metadatos      |
| **Reportes por análisis** | `GET`    | `/api/report/by-analysis/{analysisId}` | Obtiene reportes de un análisis específico     |
| **Reportes por fecha**    | `GET`    | `/api/report/by-date/{date}`           | Obtiene reportes generados en fecha específica |
| **Reportes por formato**  | `GET`    | `/api/report/by-format/{format}`       | Filtra reportes por formato (PDF, HTML, JSON)  |
| **Eliminar reporte**      | `DELETE` | `/api/report/{id}`                     | Elimina reporte y archivos asociados           |

### 📈 Endpoints de historial

| 🎯 Acción                  | Método   | Endpoint                                | Descripción                                 |
| -------------------------- | -------- | --------------------------------------- | ------------------------------------------- |
| **Crear historial**        | `POST`   | `/api/history`                          | Registra nueva actividad en el historial    |
| **Historial por usuario**  | `GET`    | `/api/history/by-user/{userId}`         | Obtiene historial completo del usuario      |
| **Historial por análisis** | `GET`    | `/api/history/by-analysis/{analysisId}` | Obtiene historial de un análisis específico |
| **Eliminar historial**     | `DELETE` | `/api/history/{id}`                     | Elimina entrada específica del historial    |

### 📝 Ejemplos de uso completo

**🚀 Crear nuevo reporte:**

```bash
curl -X POST "https://api.accessibility.local/api/report" \
  -H "Content-Type: application/json" \
  -H "Accept-Language: es" \
  -d '{
    "analysisId": 456,
    "format": "PDF",
    "filePath": "/reports/accessibility-report-456.pdf",
    "generationDate": "2025-09-13T14:30:00Z",
    "templateType": "comprehensive",
    "includeCharts": true,
    "includeRecommendations": true
  }'
```

**📊 Respuesta de reporte creado:**

```json
{
  "message": "Reporte creado correctamente.",
  "success": true,
  "data": {
    "id": 789,
    "analysisId": 456,
    "format": "PDF",
    "filePath": "/reports/accessibility-report-456.pdf",
    "generationDate": "2025-09-13T14:30:00Z",
    "status": "PENDING",
    "fileSize": null,
    "downloadUrl": null,
    "expirationDate": "2025-10-13T14:30:00Z",
    "createdAt": "2025-09-13T14:30:00Z",
    "updatedAt": "2025-09-13T14:30:00Z"
  }
}
```

**✅ Reporte completado con detalles:**

```bash
curl "https://api.accessibility.local/api/report/789" \
  -H "Accept-Language: es"
```

```json
{
  "message": "Reporte obtenido exitosamente.",
  "success": true,
  "data": {
    "id": 789,
    "analysisId": 456,
    "format": "PDF",
    "filePath": "/reports/accessibility-report-456.pdf",
    "generationDate": "2025-09-13T14:30:00Z",
    "status": "COMPLETED",
    "fileSize": 2048576,
    "downloadUrl": "https://api.accessibility.local/api/report/789/download",
    "expirationDate": "2025-10-13T14:30:00Z",
    "metadata": {
      "pages": 24,
      "violationsCount": 8,
      "passesCount": 156,
      "templateVersion": "2.1",
      "generationTimeMs": 3420
    },
    "createdAt": "2025-09-13T14:30:00Z",
    "updatedAt": "2025-09-13T14:32:25Z"
  }
}
```

**📈 Crear registro de historial:**

```bash
curl -X POST "https://api.accessibility.local/api/history" \
  -H "Content-Type: application/json" \
  -H "Accept-Language: es" \
  -d '{
    "userId": 42,
    "analysisId": 456,
    "historyType": "GENERATION",
    "description": "Reporte PDF generado automáticamente",
    "metadata": {
      "reportId": 789,
      "format": "PDF",
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0..."
    }
  }'
```

**📊 Respuesta de historial creado:**

```json
{
  "message": "Historial creado correctamente.",
  "success": true,
  "data": {
    "id": 123,
    "userId": 42,
    "analysisId": 456,
    "historyType": "GENERATION",
    "description": "Reporte PDF generado automáticamente",
    "metadata": {
      "reportId": 789,
      "format": "PDF",
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0..."
    },
    "createdAt": "2025-09-13T14:32:30Z",
    "updatedAt": "2025-09-13T14:32:30Z"
  }
}
```

### 🔍 Consultas avanzadas

**Reportes por análisis específico:**

```bash
# Obtener todos los reportes de un análisis
curl "https://api.accessibility.local/api/report/by-analysis/456" \
  -H "Accept-Language: es"
```

**Reportes por formato:**

```bash
# Solo reportes PDF
curl "https://api.accessibility.local/api/report/by-format/PDF" \
  -H "Accept-Language: es"

# Solo reportes HTML interactivos
curl "https://api.accessibility.local/api/report/by-format/HTML" \
  -H "Accept-Language: es"
```

**Historial completo por usuario:**

```bash
# Historial de actividades del usuario
curl "https://api.accessibility.local/api/history/by-user/42?page=1&pageSize=20" \
  -H "Accept-Language: es"
```

### 🚨 Manejo de errores y respuestas

**Respuestas de error estandarizadas:**

```json
// Error 400: Parámetros inválidos
{
  "success": false,
  "error": "Formato de reporte no válido. Formatos soportados: PDF, HTML, JSON",
  "details": {
    "field": "format",
    "value": "XLSX",
    "allowedValues": ["PDF", "HTML", "JSON"]
  },
  "timestamp": "2025-09-13T14:30:00Z",
  "path": "/api/report"
}

// Error 404: Recurso no encontrado
{
  "success": false,
  "error": "Reporte con ID 999 no encontrado",
  "timestamp": "2025-09-13T14:30:00Z",
  "path": "/api/report/999"
}

// Error 409: Conflicto de recursos
{
  "success": false,
  "error": "Ya existe un reporte en formato PDF para el análisis 456",
  "details": {
    "conflictingResource": "Report",
    "analysisId": 456,
    "existingFormat": "PDF"
  },
  "timestamp": "2025-09-13T14:30:00Z",
  "path": "/api/report"
}
```

**🎯 Códigos de estado HTTP:**

- `200 OK` → Operación exitosa
- `201 Created` → Recurso creado correctamente
- `204 No Content` → Eliminación exitosa
- `400 Bad Request` → Parámetros inválidos o malformados
- `404 Not Found` → Recurso no encontrado
- `409 Conflict` → Conflicto de recursos existentes
- `500 Internal Server Error` → Error interno del servidor

## 🐳 Despliegue y containerización

### 🐳 Configuración de Docker

**📦 Docker Compose para desarrollo:**

```yaml
# docker-compose.dev.yml
services:
  reports-api:
    build:
      context: .
      dockerfile: Dockerfile
    image: magodeveloper/accessibility-ms-reports:dev
    container_name: accessibility-reports-dev
    ports:
      - "5003:8083"
      - "5103:8443" # HTTPS
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8083;https://+:8443
      - ConnectionStrings__Default=Server=reports-db;Port=3306;Database=reportsdb;Uid=reportsuser;Pwd=ReportsApp2025SecurePass;
      - ASPNETCORE_Kestrel__Certificates__Default__Password=dev-cert-password
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
    volumes:
      - ~/.aspnet/https:/https:ro
      - ./reports-storage:/app/reports
    depends_on:
      - reports-db
    networks:
      - accessibility-network

  reports-db:
    image: mysql:8.4
    container_name: accessibility-reports-db-dev
    ports:
      - "3309:3306"
    environment:
      MYSQL_ROOT_PASSWORD: cH9QM3YwWOJJZaZ3ZyYloMqU6dcDCWiN
      MYSQL_DATABASE: reportsdb
      MYSQL_USER: reportsuser
      MYSQL_PASSWORD: ReportsApp2025SecurePass
    volumes:
      - reports-db-data:/var/lib/mysql
      - ./init-reports-db.sql:/docker-entrypoint-initdb.d/init.sql
    networks:
      - accessibility-network

volumes:
  reports-db-data:
  reports-storage:

networks:
  accessibility-network:
    external: true
```

**🚀 Comandos de despliegue:**

```bash
# Construcción de imagen optimizada
docker build -t accessibility-ms-reports:latest .

# Desarrollo con recarga automática
docker-compose -f docker-compose.dev.yml up --build

# Producción con optimizaciones
docker-compose -f docker-compose.prod.yml up -d

# Logs en tiempo real
docker-compose logs -f reports-api

# Limpieza completa
docker-compose down -v && docker system prune -f
```

### ⚙️ Variables de entorno Docker

**🔧 Configuración avanzada (.env):**

```bash
# === APLICACIÓN ===
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:8443;http://+:8083

# === ALMACENAMIENTO DE REPORTES ===
REPORTS_STORAGE_PATH=/app/reports
REPORTS_BASE_URL=https://reports.accessibility.local
REPORTS_MAX_FILE_SIZE_MB=100
REPORTS_CLEANUP_INTERVAL_HOURS=24

# === GENERACIÓN DE REPORTES ===
PDF_TEMPLATE_PATH=/app/templates/pdf
HTML_TEMPLATE_PATH=/app/templates/html
DEFAULT_FONT_FAMILY=Arial, sans-serif
ENABLE_WATERMARK=true
WATERMARK_TEXT=Generated by Accessibility Reports

# === PERFORMANCE Y CACHING ===
ENABLE_RESPONSE_CACHING=true
CACHE_DURATION_MINUTES=30
MAX_CONCURRENT_GENERATIONS=5
REPORT_GENERATION_TIMEOUT_MS=60000

# === INTEGRACIÓN CON OTROS MICROSERVICIOS ===
ANALYSIS_SERVICE_TIMEOUT_MS=30000
USERS_SERVICE_TIMEOUT_MS=15000
ENABLE_SERVICE_DISCOVERY=true
```

### ⚡ Performance y benchmarks esperados

**🚀 Métricas de generación de reportes:**

| Formato  | Tamaño promedio | Tiempo generación | Límite concurrente | Memoria pico |
| -------- | --------------- | ----------------- | ------------------ | ------------ |
| **PDF**  | 2-5 MB          | 2-4 segundos      | 5 simultáneos      | 150-300 MB   |
| **HTML** | 500 KB - 1 MB   | 1-2 segundos      | 10 simultáneos     | 80-150 MB    |
| **JSON** | 100-500 KB      | 0.5-1 segundo     | 15 simultáneos     | 50-100 MB    |

**📈 Configuraciones de rendimiento recomendadas:**

```bash
# Para entornos de alta demanda
MAX_CONCURRENT_GENERATIONS=10
REPORT_GENERATION_TIMEOUT_MS=120000
CACHE_DURATION_MINUTES=60

# Para entornos con recursos limitados
MAX_CONCURRENT_GENERATIONS=3
REPORT_GENERATION_TIMEOUT_MS=45000
CACHE_DURATION_MINUTES=15
```

**🎯 Umbrales de monitoreo sugeridos:**

- **Tiempo de respuesta**: < 5 segundos para PDF, < 3 segundos para HTML/JSON
- **Memoria máxima**: < 500 MB por proceso de generación
- **CPU utilizada**: < 80% durante picos de generación
- **Tasa de éxito**: > 95% de reportes generados exitosamente
- **Storage cleanup**: Reportes > 30 días eliminados automáticamente

### 📊 Monitoreo y métricas

**🔍 Health checks implementados:**

- `/health` → Estado general del servicio
- `/health/ready` → Preparado para recibir tráfico
- `/health/live` → Servicio funcionando correctamente
- `/health/db` → Estado de conexión a base de datos

**📈 Métricas personalizadas disponibles:**

```csharp
// Métricas de generación de reportes
reports_generation_requests_total{format="PDF|HTML|JSON", status="success|error"}
reports_generation_duration_seconds{format="PDF|HTML|JSON"}
reports_active_generations_count
reports_storage_size_bytes

// Métricas de historial
history_events_total{type="GENERATION|DOWNLOAD|DELETION|SHARING"}
history_entries_count_by_user
history_retention_policy_cleanups_total

// Métricas de base de datos
database_reports_total_count
database_reports_by_status{status="PENDING|GENERATING|COMPLETED|FAILED|EXPIRED"}
database_history_entries_total
database_query_duration_seconds{operation="select|insert|update|delete"}

// Métricas de sistema
memory_usage_bytes
storage_usage_bytes{type="reports|templates|cache"}
http_requests_per_second{method="GET|POST|DELETE"}
```

### 🔗 Integración con ecosistema de microservicios

**🌐 Comunicación con accessibility-ms-analysis:**

```bash
# Obtener detalles de análisis para generar reporte
GET http://accessibility-ms-analysis:5002/api/analysis/{analysisId}

# Notificar que reporte ha sido generado
POST http://accessibility-ms-analysis:5002/api/analysis/{analysisId}/report-generated
{
  "reportId": 789,
  "format": "PDF",
  "downloadUrl": "https://reports.accessibility.local/api/report/789/download"
}
```

**👤 Comunicación con accessibility-ms-users:**

```bash
# Validar usuario antes de crear historial
GET http://accessibility-ms-users:5001/api/users/{userId}

# Registrar actividad de reporte en perfil de usuario
POST http://accessibility-ms-users:5001/api/users/{userId}/activity
{
  "activityType": "REPORT_GENERATED",
  "reportId": 789,
  "timestamp": "2025-09-13T14:32:30Z"
}
```

## 🌐 Internacionalización y localización

### 🗺️ Idiomas soportados

| Idioma        | Código  | Estado             | Cobertura |
| ------------- | ------- | ------------------ | --------- |
| **Español**   | `es-ES` | ✅ **Completo**    | 100%      |
| **Inglés**    | `en-US` | ✅ **Completo**    | 100%      |
| **Portugués** | `pt-BR` | 🔄 **Planificado** | 0%        |
| **Francés**   | `fr-FR` | 🔄 **Planificado** | 0%        |

### 🔧 Configuración de localización

**Headers de idioma soportados:**

```bash
# Español (por defecto)
Accept-Language: es-ES
Accept-Language: es

# Inglés
Accept-Language: en-US
Accept-Language: en

# Múltiples idiomas con prioridad
Accept-Language: en-US,en;q=0.9,es;q=0.8
```

**📝 Ejemplos de respuestas localizadas:**

```json
// Respuesta en español (es-ES)
{
  "message": "Reporte creado correctamente.",
  "success": true,
  "data": { /* ... */ }
}

// Respuesta en inglés (en-US)
{
  "message": "Report created successfully.",
  "success": true,
  "data": { /* ... */ }
}
```

### 📋 Archivos de recursos

```
📁 src/Reports.Api/Resources/
├── 🇪🇸 Messages.es.resx          # Mensajes en español
├── 🇺🇸 Messages.en.resx          # Mensajes en inglés
├── 🇪🇸 Validations.es.resx       # Validaciones en español
└── 🇺🇸 Validations.en.resx       # Validaciones en inglés
```

## 🚀 CI/CD y desarrollo

### 🔄 Pipeline automatizado

**GitHub Actions configurado para:**

✅ **Build y Tests automáticos**

- Compilación en .NET 9 con multiple targeting
- Ejecución de tests unitarios e integración
- Reporte de cobertura de código con Coverlet
- Análisis de calidad con SonarQube

✅ **Generación de reportes de prueba**

- Tests de generación PDF con bibliotecas reales
- Validación de plantillas HTML responsivas
- Pruebas de exportación JSON con esquemas
- Tests de performance para generación masiva

✅ **Despliegue automatizado**

- Build de imagen Docker multi-stage optimizada
- Push automático a Docker Hub y Azure Container Registry
- Deploy automático a entorno staging
- Deploy manual a producción con aprobaciones requeridas

✅ **Validaciones de seguridad**

- Escaneo de vulnerabilidades en dependencias NuGet
- Análisis SAST del código fuente C#
- Validación de configuraciones Docker y secretos

### 🛠️ Herramientas de desarrollo recomendadas

**IDEs y extensiones:**

- **Visual Studio 2022** con extensiones:
  - Entity Framework Core Power Tools
  - SonarLint para C#
  - Docker para Visual Studio
  - REST Client para pruebas de API
- **VS Code** con extensiones:
  - C# Dev Kit
  - REST Client
  - Docker y Docker Compose
  - GitLens para control de versiones

**🧪 Testing y depuración:**

```bash
# Tests con coverage detallado y filtros
dotnet test --collect:"XPlat Code Coverage" --filter Category!=Integration

# Generar reporte HTML de coverage con umbrales
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html -assemblyfilters:+Reports.*

# Tests de rendimiento específicos
dotnet test --filter Category=Performance --logger "console;verbosity=detailed"

# Depuración con logs estructurados
dotnet run --environment Development --verbosity diagnostic --property:EnableStructuredLogging=true

# 🎯 Tests específicos del dominio de reportes
dotnet test --filter "Category=ReportGeneration&Format=PDF" --logger trx --results-directory TestResults/Reports

# Tests de integración con servicios externos
dotnet test --filter "Category=Integration&Service=Analysis" --logger "console;verbosity=detailed"

# Tests de validación de formatos
dotnet test --filter "FullyQualifiedName~ReportFormatValidation" --logger json --results-directory TestResults/Validation

# Tests de performance por formato específico
dotnet test --filter "TestCategory=Performance&Format=HTML" --logger "trx;LogFileName=html-performance.trx"

# Tests de almacenamiento y cleanup
dotnet test --filter "TestCategory=Storage" --environment TEST_STORAGE_PATH=/tmp/test-reports

# Tests de historial y auditoría
dotnet test --filter "FullyQualifiedName~HistoryService" --collect:"Code Coverage" --settings coverage.runsettings

# Tests end-to-end de flujo completo
dotnet test --filter "Category=E2E" --logger "console;verbosity=normal" --results-directory TestResults/E2E
```

## 🔒 Consideraciones de seguridad

### 🛡️ Protecciones implementadas

✅ **Autenticación y autorización:**

- JWT tokens para autenticación de microservicios
- API Keys para servicios internos
- Rate limiting por endpoint y usuario
- CORS configurado específicamente por entorno

✅ **Validación y sanitización:**

- Validación estricta de formatos de reporte
- Sanitización de nombres de archivo y rutas
- Protección contra path traversal en storage
- Validación de tamaños de archivo y limits

✅ **Protección de datos:**

- Encriptación de URLs de descarga temporales
- Logs sanitizados sin información PII
- Almacenamiento seguro de reportes con TTL
- Configuración HTTPS obligatoria en producción

✅ **Auditoría y compliance:**

- Registro completo de actividades en historial
- Tracking de accesos y descargas de reportes
- Retention policies configurable por tipo de dato
- Logs de auditoría para compliance GDPR/CCPA

### ⚠️ Recomendaciones de producción

1. **🔐 Gestión de secretos**: Usar Azure Key Vault o equivalente para passwords y JWT secrets
2. **🌐 Red segura**: VPN/VPC para comunicación inter-microservicios sin exposición pública
3. **📊 Monitoreo activo**: Alertas automáticas para generaciones fallidas y accesos anómalos
4. **🔄 Respaldo**: Backup automático de reportes críticos y base de datos cada 6 horas
5. **📋 Auditoría**: Log estructurado de todas las operaciones con correlationId
6. **⏱️ Límites**: Timeouts apropiados y circuit breakers para evitar degradación
7. **🗑️ Limpieza**: Políticas de retention automático para reportes y logs antiguos

## �️ Troubleshooting y resolución de problemas

### 🚨 **Problemas comunes y soluciones**

#### **❌ Error: "Report generation timeout"**

```bash
# Síntoma: Reportes PDF fallan con timeout
Error: ReportGenerationException: Generation timeout after 60000ms

# Solución 1: Aumentar timeout en configuración
REPORT_GENERATION_TIMEOUT_MS=120000

# Solución 2: Verificar memoria disponible
docker stats accessibility-reports-api

# Solución 3: Reducir concurrencia
MAX_CONCURRENT_GENERATIONS=3
```

#### **🗄️ Error: "Database connection failed"**

```bash
# Síntoma: Cannot connect to MySQL
SqlException: Unable to connect to any of the specified MySQL hosts

# Solución 1: Verificar estado del contenedor de BD
docker-compose ps reports-db

# Solución 2: Verificar logs de MySQL
docker-compose logs reports-db

# Solución 3: Recrear base de datos
docker-compose down -v
docker-compose up -d reports-db
```

#### **🔒 Error: "Cross-microservice validation failed"**

```bash
# Síntoma: Analysis ID not found in external service
ValidationException: Analysis 456 not found in accessibility-ms-analysis

# Solución 1: Verificar conectividad entre servicios
curl http://accessibility-ms-analysis:5002/health

# Solución 2: Comprobar configuración de red Docker
docker network ls
docker network inspect accessibility-network

# Solución 3: Verificar variables de entorno de servicios
docker-compose config
```

#### **💾 Error: "Storage space exhausted"**

```bash
# Síntoma: Fallos en escritura de archivos de reporte
IOException: No space left on device

# Solución 1: Limpiar reportes antiguos manualmente
find /app/reports -name "*.pdf" -mtime +30 -delete

# Solución 2: Verificar y aumentar volumen Docker
docker system df
docker volume prune

# Solución 3: Configurar limpieza automática
REPORTS_CLEANUP_INTERVAL_HOURS=12
REPORTS_MAX_AGE_DAYS=15
```

### 🔍 **Comandos de diagnóstico útiles**

```bash
# 📊 Estado general del sistema
curl http://localhost:5003/health
curl http://localhost:5003/health/ready
curl http://localhost:5003/health/db

# 📈 Métricas de performance
curl http://localhost:5003/metrics

# 🔍 Logs estructurados con filtro
docker-compose logs reports-api | grep -i "error\|exception\|timeout"

# 📋 Verificar configuración activa
curl http://localhost:5003/api/config/active

# 🗄️ Prueba de conexión a base de datos
docker exec -it accessibility-reports-db-dev mysql -u reportsuser -p -e "SHOW TABLES;"

# 🌐 Test de conectividad entre microservicios
docker exec -it accessibility-reports-dev curl http://accessibility-ms-analysis:5002/health
docker exec -it accessibility-reports-dev curl http://accessibility-ms-users:5001/health
```

### 📋 **Checklist de resolución rápida**

✅ **Verificaciones básicas:**

- [ ] Contenedores en ejecución: `docker-compose ps`
- [ ] Logs sin errores críticos: `docker-compose logs --tail=50`
- [ ] Health checks respondan: `curl localhost:5003/health`
- [ ] Base de datos accesible: Conexión MySQL exitosa

✅ **Verificaciones de red:**

- [ ] Red Docker activa: `docker network inspect accessibility-network`
- [ ] Puertos expuestos correctamente: `netstat -tlnp | grep 5003`
- [ ] Servicios externos respondiendo: Health checks de otros microservicios

✅ **Verificaciones de performance:**

- [ ] Memoria suficiente: `docker stats` < 80% uso
- [ ] Espacio en disco: `df -h` > 2GB disponible
- [ ] CPU no saturada: Load average < número de cores

## �📚 Recursos adicionales

### 🔗 Enlaces útiles

- **[PDF Generation Best Practices](https://docs.microsoft.com/en-us/dotnet/core/extensions/pdf-generation)** → Guías para generación de PDF empresarial
- **[Entity Framework Core Docs](https://docs.microsoft.com/en-us/ef/core/)** → Documentación completa de EF Core 9.0
- **[ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)** → Mejores prácticas de seguridad
- **[Docker Multi-stage Builds](https://docs.docker.com/develop/dev-best-practices/)** → Optimización de imágenes Docker
- **[Localization in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization)** → Guía completa de i18n
- **[Health Checks in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)** → Implementación de health checks

### 📖 Documentación técnica

- `docs/report-templates.md` → Plantillas y formatos de reporte disponibles
- `docs/api-specification.yaml` → Especificación completa OpenAPI 3.0
- `docs/deployment-guide.md` → Guía detallada de despliegue empresarial
- `docs/troubleshooting.md` → Resolución de problemas comunes
- `docs/performance-tuning.md` → Optimizaciones de rendimiento
- `docs/localization-guide.md` → Guía para añadir nuevos idiomas
- `docs/database-migrations.md` → Gestión de migraciones y versionado

---

## 🤝 Contribución y soporte

### 👥 Equipo de desarrollo

- **Tech Lead**: Arquitectura y diseño de microservicios
- **Backend Developer**: Implementación de lógica de negocio y APIs
- **UI/UX Designer**: Diseño de plantillas de reportes y experiencia de usuario
- **DevOps Engineer**: CI/CD, containerización y despliegue
- **QA Engineer**: Testing automatizado y aseguramiento de calidad

### 🐛 Reportar issues

1. **Issues en GitHub**: Usar **[GitHub Issues](../../issues)** para bugs y feature requests
2. **Información requerida**:
   - Versión del microservicio y entorno
   - Logs relevantes con correlationId
   - Pasos detallados para reproducir
   - Ejemplos de payloads que causan problemas
3. **Labels**: Usar etiquetas apropiadas (bug, enhancement, documentation, etc.)
4. **Prioridad**: Indicar severidad (critical, high, medium, low)

### ✨ Contribuir al proyecto

1. **Fork** del repositorio en GitHub
2. **Branch** para nueva funcionalidad: `git checkout -b feature/nueva-funcionalidad`
3. **Desarrollo** siguiendo estándares de código y documentación
4. **Tests** obligatorios para nueva funcionalidad con cobertura >85%
5. **Pull Request** con:
   - Descripción detallada de cambios
   - Tests que validan la funcionalidad
   - Documentación actualizada
   - Screenshots o ejemplos si aplica

### 📋 Estándares de código

- **C# Coding Standards**: Seguir convenciones de Microsoft y StyleCop
- **API Design**: RESTful siguiendo estándares OpenAPI 3.0
- **Testing**: Tests unitarios y de integración obligatorios
- **Documentation**: README actualizado y documentación inline
- **Commit Messages**: Seguir [Conventional Commits](https://conventionalcommits.org/)
- **Versioning**: Semantic Versioning (SemVer) estricto

---

> 💡 **¿Necesitas ayuda?** Consulta nuestra documentación técnica completa, revisa los issues existentes, o abre un nuevo issue con detalles específicos. El microservicio está diseñado para ser escalable y mantenible siguiendo arquitectura de microservicios empresarial.

**🎯 Versión:** 1.0.0 | **📅 Última actualización:** Septiembre 2025 | **⚡ Estado:** Producción listo
