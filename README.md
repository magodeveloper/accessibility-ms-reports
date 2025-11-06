# 📊 Accessibility Reports Service

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-434%2F437-brightgreen)](test-dashboard.html)
[![Coverage](https://img.shields.io/badge/coverage-94.12%25-brightgreen)](coverage-report/index.html)
[![License](https://img.shields.io/badge/license-Proprietary-red)](LICENSE)

> **Microservicio de generación de reportes de accesibilidad web y gestión de historial desarrollado en .NET 9 con Clean Architecture. Proporciona generación multi-formato, almacenamiento persistente y trazabilidad completa.**

> ⚡ **Nota:** Este microservicio forma parte de un ecosistema donde el **Gateway** gestiona rate limiting, caching (Redis), circuit breaker y load balancing. El microservicio se enfoca en su lógica de dominio específica.

## � Descripción

Microservicio empresarial para:

- **Generación de reportes** en múltiples formatos (PDF, HTML, JSON, CSV)
- **Almacenamiento persistente** de reportes con MySQL 8.0
- **Gestión de historial** con trazabilidad completa de operaciones
- **Consultas avanzadas** por análisis, fecha, formato y usuario
- **i18n integrado** con soporte multiidioma (es, en)

## ✨ Características

### 📊 Gestión de Reportes

- **Generación multi-formato** (PDF, HTML, JSON, CSV)
- Consulta avanzada por análisis, fecha, formato
- Métricas de rendimiento y tasas de éxito
- Eliminación individual y masiva
- Almacenamiento persistente en MySQL

### 📋 Gestión de Historial

- **Trazabilidad completa** de operaciones
- Filtrado por usuario, análisis, fechas
- Control de acceso por permisos
- Auditoría integrada
- Registro detallado de acciones

### 🔒 Seguridad & Autenticación

- **Autenticación JWT obligatoria** en todos los endpoints
- Tokens JWT validados con firma digital
- Control de acceso basado en roles (User/Admin)
- Gateway Secret para comunicación entre servicios
- Validación con FluentValidation
- Protección contra acceso no autorizado

### � i18n & Localización

- Soporte multiidioma (es, en)
- Detección automática vía `Accept-Language`
- Mensajes de error localizados
- Content negotiation automático
- Sistema extensible para nuevos idiomas

### 🏥 Health Checks & Observabilidad

- Database connectivity check
- Application health monitoring
- Memory usage tracking
- Métricas Prometheus integradas
- Logging estructurado con Serilog

## 🏗️ Arquitectura

```
┌───────────────────────────────────────────────────┐
│         📊 REPORTS MICROSERVICE API               │
│                (Port 5003)                        │
│                                                   │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────┐ │
│  │ Controllers │  │  Middleware │  │  Health  │ │
│  │  (2 APIs)   │  │  (Context)  │  │  Checks  │ │
│  └─────────────┘  └─────────────┘  └──────────┘ │
│         │                │               │       │
│         └────────────────┴───────────────┘       │
│                      │                           │
│              ┌───────▼───────┐                   │
│              │  APPLICATION  │                   │
│              │   Services    │                   │
│              │ Localization  │                   │
│              └───────┬───────┘                   │
│                      │                           │
│              ┌───────▼───────┐                   │
│              │    DOMAIN     │                   │
│              │   Entities    │                   │
│              │  Interfaces   │                   │
│              └───────┬───────┘                   │
│                      │                           │
│              ┌───────▼───────┐                   │
│              │INFRASTRUCTURE │                   │
│              │   EF Core     │                   │
│              │   Repositories│                   │
│              └───────┬───────┘                   │
└──────────────────────┼───────────────────────────┘
                       │
                       ▼
               ┌──────────────┐
               │  MySQL DB    │
               │(reports_db)  │
               └──────────────┘
```

**Clean Architecture con 4 capas:**

- **API:** Controllers, Middleware, Health Checks
- **Application:** Services, DTOs, Localization, Use Cases
- **Domain:** Entities (Report, History), Interfaces, Business Logic
- **Infrastructure:** EF Core, Repositories, MySQL

## 🚀 Quick Start

### Requisitos

- .NET 9.0 SDK
- MySQL 8.0+
- Docker & Docker Compose (opcional)

### Instalación Local

```bash
# Clonar repositorio
git clone https://github.com/magodeveloper/accessibility-ms-reports.git
cd accessibility-ms-reports

# Configurar base de datos
mysql -u root -p < init-reports-db.sql

# Configurar variables de entorno
cp .env.example .env
# Editar .env con tus credenciales de MySQL

# Restaurar dependencias
dotnet restore

# Compilar
dotnet build --configuration Release

# Ejecutar
dotnet run --project src/Reports.Api/Reports.Api.csproj
```

### Uso con Docker Compose

```bash
# Levantar todos los servicios
docker-compose up -d

# Ver logs
docker-compose logs -f reports-api

# Verificar estado
docker-compose ps

# Detener servicios
docker-compose down
```

### Verificación

```bash
# Health check (no requiere autenticación)
curl http://localhost:5003/health

# Obtener token JWT del microservicio de usuarios
curl -X POST http://localhost:8081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123!"}'

# Crear reporte (requiere autenticación JWT)
curl -X POST http://localhost:5003/api/report \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{"analysisId":"test-123","format":"PDF","userId":"user-1"}'
```

> ⚠️ **Nota:** Todos los endpoints de la API requieren autenticación JWT excepto los health checks.

## 📡 API Endpoints

> 🔐 **Todos los endpoints requieren autenticación JWT** mediante el header `Authorization: Bearer {token}`

### 📊 Reports (/api/report)

| Método | Endpoint                         | Descripción                     | Auth |
| ------ | -------------------------------- | ------------------------------- | ---- |
| GET    | `/api/report`                    | Listar todos los reportes       | ✅   |
| GET    | `/api/report/by-analysis/{id}`   | Buscar reportes por análisis ID | ✅   |
| GET    | `/api/report/by-date/{date}`     | Buscar reportes por fecha       | ✅   |
| GET    | `/api/report/by-format/{format}` | Buscar reportes por formato     | ✅   |
| POST   | `/api/report`                    | Crear nuevo reporte             | ✅   |
| DELETE | `/api/report/{id}`               | Eliminar reporte por ID         | ✅   |
| DELETE | `/api/report/all`                | Eliminar todos los reportes     | ✅   |

### 📋 History (/api/history)

| Método | Endpoint                        | Descripción                               | Auth     |
| ------ | ------------------------------- | ----------------------------------------- | -------- |
| GET    | `/api/history`                  | Listar historial del usuario actual       | ✅       |
| GET    | `/api/history/by-user/{id}`     | Buscar historial por usuario (Admin)      | ✅ Admin |
| GET    | `/api/history/by-analysis/{id}` | Buscar historial por análisis ID          | ✅       |
| POST   | `/api/history`                  | Crear registro de historial               | ✅       |
| DELETE | `/api/history/{id}`             | Eliminar registro de historial por ID     | ✅       |
| DELETE | `/api/history/all`              | Eliminar todos los registros de historial | ✅       |

### 🏥 Health (/health)

| Método | Endpoint        | Descripción          |
| ------ | --------------- | -------------------- |
| GET    | `/health`       | Health check general |
| GET    | `/health/ready` | Readiness probe      |
| GET    | `/health/live`  | Liveness probe       |

### 📊 Metrics (/metrics)

| Método | Endpoint   | Descripción                |
| ------ | ---------- | -------------------------- |
| GET    | `/metrics` | Métricas de Prometheus.NET |

**Total: 17 endpoints disponibles**

## 🧪 Testing

### Estado de Cobertura

**Estado General:** ✅ 434/437 tests exitosos (99.3%)  
**Cobertura Total:** 94.12% (769/817 líneas cubiertas)

| Capa                       | Cobertura | Tests            | Estado |
| -------------------------- | --------- | ---------------- | ------ |
| **Reports.Api**            | 93.96%    | Controllers + MW | ✅     |
| ReportController           | 95%+      | CRUD Reportes    | ✅     |
| HistoryController          | 92%+      | CRUD Historial   | ✅     |
| **Reports.Application**    | 94.28%    | Services + DTOs  | ✅     |
| ReportService              | 95%+      | Lógica Reportes  | ✅     |
| HistoryService             | 93%+      | Lógica Historial | ✅     |
| **Reports.Domain**         | 100%      | Entities         | ✅     |
| Report Entity              | 100%      | Validaciones     | ✅     |
| History Entity             | 100%      | Validaciones     | ✅     |
| **Reports.Infrastructure** | 0%        | Excluido         | ⚠️     |

**Métricas detalladas:**

- **Cobertura de líneas:** 94.12% (769/817)
- **Cobertura de ramas:** 81.87%
- **Tiempo de ejecución:** ~2s para 437 tests
- **Tasa de éxito:** 99.3% (434/437, 3 skipped)

### Comandos de Testing

```bash
# Todos los tests con cobertura
.\manage-tests.ps1 -GenerateCoverage -OpenReport

# Solo tests unitarios
.\manage-tests.ps1 -TestType Unit

# Tests de integración
.\manage-tests.ps1 -TestType Integration

# Ver dashboard interactivo
Start-Process .\test-dashboard.html
```

### Categorías de Tests

**Unit Tests:**

- Validación de entidades (Report, History)
- Lógica de servicios (ReportService, HistoryService)
- DTOs y mappers
- Validadores de dominio
- Localización y mensajes

**Integration Tests:**

- Controllers con base de datos en memoria
- Repositorios con MySQL real
- Health checks completos
- Middleware de contexto
- Generación de reportes multi-formato

**E2E Tests:**

- Flujos completos de generación de reportes
- Consultas avanzadas por múltiples criterios
- Gestión de historial de operaciones
- Eliminación en cascada

## � Observabilidad & Métricas

### Prometheus Metrics

El microservicio expone métricas en `/metrics` usando **Prometheus.NET**.

#### Métricas HTTP Estándar

```promql
# Request rate
rate(http_requests_received_total[5m])

# Request duration
histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))

# Requests in progress
http_requests_in_progress
```

#### Métricas de Negocio

**Reportes:**

```csharp
// Contador de reportes generados
reports_created_total{format="PDF|HTML|JSON|CSV"}

// Tamaño de reportes generados
reports_size_bytes{format="PDF|HTML|JSON|CSV"}

// Histograma de tiempo de generación
report_generation_duration_seconds{format="PDF|HTML|JSON|CSV"}

// Reportes activos
reports_active_total

// Tasa de éxito en generación
reports_success_rate{format="PDF|HTML|JSON|CSV"}
```

**Historial:**

```csharp
// Entradas de historial creadas
history_entries_created_total{action="CREATE|UPDATE|DELETE"}

// Historial por usuario
history_entries_by_user{user_id="X"}

// Historial por análisis
history_entries_by_analysis{analysis_id="X"}
```

**Database:**

```csharp
// Queries ejecutadas
db_queries_total{operation="SELECT|INSERT|UPDATE|DELETE"}

// Duración de queries
db_query_duration_seconds{table="reports|history"}

// Conexiones activas
db_connections_active
```

### Health Checks

Endpoints disponibles:

- **`/health`** - Health check completo (incluye DB)
- **`/health/live`** - Liveness probe (sin dependencias)
- **`/health/ready`** - Readiness probe (con verificación DB)

Ejemplo de uso:

```bash
curl http://localhost:5003/health
# Response: {"status":"Healthy","totalDuration":"00:00:00.0234567"}
```

La configuración incluye verificación de conexión a MySQL con timeout de 30 segundos.

### Logging con Serilog

El proyecto usa **Serilog** para logging estructurado:

- **Console**: Output formateado para desarrollo
- **File**: Logs rotativos diarios (retención 7 días)
- **Niveles**: Information (default), Warning (Microsoft/EF Core)

Ejemplos de logs estructurados:

```csharp
_logger.LogInformation("Report generated: {ReportId}, size: {Size} bytes", reportId, size);
_logger.LogError(ex, "Failed to generate report for analysis {AnalysisId}", analysisId);
```

### Grafana Dashboards

**Queries PromQL principales:**

```promql
# Tasa de generación de reportes por formato
sum(rate(reports_created_total[5m])) by (format)

# Tiempo de generación P95
histogram_quantile(0.95, sum(rate(report_generation_duration_seconds_bucket[5m])) by (format, le))

# Request rate por endpoint
sum(rate(http_requests_received_total{job="reports-api"}[5m])) by (method, endpoint)

# Error rate 5xx
sum(rate(http_requests_received_total{code=~"5.."}[5m])) / sum(rate(http_requests_received_total[5m])) * 100
```

### Alertas Recomendadas

```yaml
# Alta tasa de errores (>5%)
- alert: HighReportGenerationErrorRate
  expr: (sum(rate(reports_created_total{status="error"}[5m])) / sum(rate(reports_created_total[5m]))) > 0.05
  for: 5m

# Generación lenta (P95 > 10s)
- alert: SlowReportGeneration
  expr: histogram_quantile(0.95, rate(report_generation_duration_seconds_bucket[5m])) > 10
  for: 5m

# Database no disponible
- alert: DatabaseDown
  expr: up{job="reports-mysql"} == 0
  for: 1m
```

## 🔒 Arquitectura de Seguridad

### Flujo de Autenticación

```
┌─────────────┐         ┌──────────────┐         ┌──────────────┐
│   Cliente   │         │   Gateway    │         │   Reports    │
│   (JWT)     │         │   (Port 80)  │         │  (Port 5003) │
└──────┬──────┘         └──────┬───────┘         └──────┬───────┘
       │                       │                        │
       │ 1. POST /api/report   │                        │
       │    Authorization:     │                        │
       │    Bearer eyJ...      │                        │
       ├──────────────────────>│                        │
       │                       │                        │
       │                       │ 2. Valida JWT          │
       │                       │    Extrae claims:      │
       │                       │    - UserId            │
       │                       │    - Email             │
       │                       │    - Role              │
       │                       │                        │
       │                       │ 3. Agrega headers:     │
       │                       │    X-User-Id: 123      │
       │                       │    X-User-Email: ...   │
       │                       │    X-User-Role: admin  │
       │                       │    X-Gateway-Secret    │
       │                       ├───────────────────────>│
       │                       │                        │
       │                       │                    4. Middleware
       │                       │                       Valida Gateway
       │                       │                       Secret ✓
       │                       │                        │
       │                       │                    5. Middleware
       │                       │                       Extrae headers
       │                       │                       Popula UserContext
       │                       │                        │
       │                       │                    6. Controller
       │                       │                       if (!IsAuthenticated)
       │                       │                       return Unauthorized();
       │                       │                        │
       │                       │                    7. Ejecuta lógica
       │                       │                       de negocio
       │                       │                        │
       │                       │ 8. Response 201       │
       │                       │<───────────────────────┤
       │                       │                        │
       │ 9. Response 201       │                        │
       │<──────────────────────┤                        │
       │                       │                        │
```

### Stack de Middleware

```csharp
app.UseHttpsRedirection();                          // 1. HTTPS enforcement
app.UseRouting();                                   // 2. Routing
app.UseAuthentication();                            // 3. JWT validation ([AllowAnonymous] permite bypass)
app.UseMiddleware<GatewaySecretValidationMiddleware>(); // 4. Gateway secret validation
app.UseMiddleware<UserContextMiddleware>();         // 5. User context population
app.UseAuthorization();                             // 6. Authorization policies
app.MapControllers();                               // 7. Endpoint execution
app.MapHealthChecks("/health");                     // 8. Health checks
app.MapMetrics("/metrics");                         // 9. Prometheus metrics
```

**Orden crítico:**

1. **UseAuthentication()** valida JWT pero `[AllowAnonymous]` permite bypass
2. **GatewaySecretValidationMiddleware** valida comunicación entre servicios
3. **UserContextMiddleware** extrae headers y popula contexto de usuario
4. **Controller validation** verifica `if (!_userContext.IsAuthenticated)`

### IUserContext Interface

La interfaz `IUserContext` proporciona acceso al contexto del usuario autenticado:

```csharp
public interface IUserContext
{
    int UserId { get; }
    string Email { get; }
    string Role { get; }
    bool IsAuthenticated { get; }  // true cuando UserId > 0
    bool IsAdmin { get; }           // true cuando Role == "admin"
}
```

**Ubicación:** `src/Reports.Application/Services/UserContext/`

### UserContextMiddleware

Middleware que extrae información del usuario de los headers del Gateway:

**Prioridades de autenticación:**

1. **Headers del Gateway** (`X-User-*`) - Producción
2. **Claims del JWT** - Acceso directo (sin Gateway)
3. **Sin autenticación** - UserId = 0, IsAuthenticated = false

**Ubicación:** `src/Reports.Api/Middleware/UserContextMiddleware.cs`### Patrón de Autenticación en Controllers

Los controllers utilizan `[AllowAnonymous]` con validación custom:

```csharp
[AllowAnonymous]  // Bypass JWT framework, permite headers del Gateway
[HttpPost]
public async Task<IActionResult> Create([FromBody] ReportCreateDto dto)
{
    if (!_userContext.IsAuthenticated)  // Validación custom
        return Unauthorized(new { message = "Authentication required" });

    var result = await _service.CreateAsync(dto, _userContext.UserId);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

**¿Por qué `[AllowAnonymous]`?**

- Permite llamadas del Gateway sin JWT directo
- Gateway valida JWT y propaga headers `X-User-*`
- Middleware extrae headers y popula `UserContext`
- Controller valida `IsAuthenticated` (true cuando UserId > 0)
- Facilita testing con mocks

### Flujos de Autenticación

**Producción (vía Gateway):**

```
Gateway valida JWT → Agrega X-User-* headers → Middleware extrae headers →
UserId = 123 → IsAuthenticated = true → Controller permite acceso ✓
```

**Unit Tests:**

```csharp
var mockUserContext = new Mock<IUserContext>();
mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);
// Test verifica que retorna Unauthorized ✓
```

**Integration Tests:**

```csharp
client.DefaultRequestHeaders.Add("X-User-Id", "1");
client.DefaultRequestHeaders.Add("X-User-Email", "test@test.com");
// Middleware popula contexto → IsAuthenticated = true ✓
```

### Validación de Gateway Secret

El middleware `GatewaySecretValidationMiddleware` valida la comunicación entre servicios:

- Verifica header `X-Gateway-Secret` en todas las requests
- Permite acceso sin validación a `/health` y `/metrics`
- Retorna `403 Forbidden` si el secret no coincide

**Ubicación:** `src/Reports.Api/Middleware/GatewaySecretValidationMiddleware.cs`

### Configuración de JWT

**appsettings.json:**

```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-min-64-chars",
    "Issuer": "https://api.accessibility.company.com/users",
    "Audience": "https://accessibility.company.com",
    "ExpiryHours": 24
  },
  "GatewaySecret": "your-gateway-secret-key"
}
```

⚠️ **IMPORTANTE:** La configuración JWT debe ser idéntica en todos los microservicios (Users, Reports, Analysis, Gateway).

**Scripts de gestión:**

```powershell
# Generar secret key segura
.\Generate-JwtSecretKey.ps1 -Type Special -Length 64

# Validar configuración
.\Validate-JwtConfig.ps1
```

## 🛠️ Scripts & Utilidades

### PowerShell Scripts

El proyecto incluye scripts para automatizar tareas comunes:

**manage-tests.ps1** - Gestión completa de tests y cobertura

```powershell
# Ejecutar todos los tests con cobertura y abrir reporte
.\manage-tests.ps1 -GenerateCoverage -OpenReport

# Ejecutar solo tests unitarios o de integración
.\manage-tests.ps1 -TestType Unit|Integration

# Limpiar resultados anteriores
.\manage-tests.ps1 -Clean
```

**Generate-JwtSecretKey.ps1** - Generación de claves JWT seguras

```powershell
# Generar clave segura (mínimo 64 caracteres)
.\Generate-JwtSecretKey.ps1 -Type Special -Length 64
```

**Validate-JwtConfig.ps1** - Validación de configuración JWT

```powershell
# Verificar que la configuración JWT es correcta
.\Validate-JwtConfig.ps1
```

### SQL Scripts

**init-reports-db.sql** - Script de inicialización de base de datos

Crea las tablas necesarias (`reports`, `history`) con sus índices y configuración UTF-8.

```bash
# Ejecutar script de inicialización
mysql -u root -p < init-reports-db.sql
```

**init-test-databases.ps1** - Configuración de base de datos para tests

```powershell
# Crear base de datos de test
.\init-test-databases.ps1
```

### Utilidades de Testing

**test-dashboard.html** - Dashboard interactivo de resultados

Visualiza métricas de tests, cobertura por capa, y tendencias históricas.

```powershell
Start-Process .\test-dashboard.html
```

## 🐳 Deployment

```dockerfile
# Build image
docker build -t accessibility-reports:latest .

# Run standalone
docker run -d \
  --name reports-api \
  -p 5003:8080 \
  -e ConnectionStrings__ReportsDb="Server=mysql;Database=accessibility_reports;..." \
  -e GatewaySecret="your-secret-key" \
  accessibility-reports:latest
```

### Docker Compose

```yaml
version: "3.8"

services:
  reports-api:
    image: accessibility-reports:latest
    ports:
      - "5003:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__ReportsDb=Server=mysql-reports;Database=accessibility_reports;Uid=root;Pwd=password
      - JwtSettings__SecretKey=your-secure-jwt-secret-key-min-64-chars
      - JwtSettings__Issuer=https://api.accessibility.company.com/users
      - JwtSettings__Audience=https://accessibility.company.com
      - JwtSettings__ExpiryHours=24
      - GatewaySecret=your-gateway-secret
      - DefaultLanguage=es
    depends_on:
      - mysql-reports
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s

  mysql-reports:
    image: mysql:8.0
    ports:
      - "3308:3306"
    environment:
      - MYSQL_ROOT_PASSWORD=password
      - MYSQL_DATABASE=accessibility_reports
    volumes:
      - mysql-reports-data:/var/lib/mysql
      - ./init-reports-db.sql:/docker-entrypoint-initdb.d/init.sql

volumes:
  mysql-reports-data:
```

## ⚙️ Configuración

### Variables de Entorno

```bash
# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production|Development
ASPNETCORE_URLS=http://+:8080

# Base de Datos
ConnectionStrings__ReportsDb=Server=localhost;Database=accessibility_reports;Uid=root;Pwd=password

# JWT Configuration (REQUERIDO)
JwtSettings__SecretKey=your-super-secret-key-min-64-chars-for-production
JwtSettings__Issuer=https://api.accessibility.company.com/users
JwtSettings__Audience=https://accessibility.company.com
JwtSettings__ExpiryHours=24

# Gateway Secret (para comunicación entre servicios)
GatewaySecret=your-super-secret-gateway-key

# Localization
DefaultLanguage=es
SupportedLanguages=es,en

# Logging
Serilog__MinimumLevel=Information
Serilog__WriteTo__Console=true

# Report Generation
Reports__MaxSizeInMB=50
Reports__AllowedFormats=PDF,HTML,JSON,CSV
Reports__StoragePath=/app/reports

# Health Checks
HealthChecks__TimeoutSeconds=30
```

### Configuración de Base de Datos

```sql
-- Crear base de datos
CREATE DATABASE accessibility_reports CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Ejecutar script de inicialización
SOURCE init-reports-db.sql;
```

### Configuración de JWT

**⚠️ IMPORTANTE:** Todos los endpoints requieren autenticación JWT.

#### Generar Secret Key Segura

```powershell
# Generar nueva secret key
.\Generate-JwtSecretKey.ps1 -Type Special -Length 64

# Validar configuración JWT
.\Validate-JwtConfig.ps1
```

#### Configurar Secret Key

**Desarrollo (User Secrets):**

```bash
dotnet user-secrets set "JwtSettings:SecretKey" "your-generated-key"
```

**Docker / Producción (.env):**

```bash
JwtSettings__SecretKey=your-generated-key-min-64-chars
```

#### Obtener Token JWT

Para usar la API, primero obtenga un token del microservicio de usuarios:

```bash
# Login y obtener token
curl -X POST http://localhost:8081/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123!"}'

# Usar token en requests
curl -X GET http://localhost:8083/api/report \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Nota:** La configuración JWT debe ser **idéntica** en todos los microservicios (Users, Reports, Analysis) y el Gateway.

## � Stack Tecnológico

- **Runtime:** .NET 9.0
- **Framework:** ASP.NET Core Web API
- **ORM:** Entity Framework Core 9.0
- **Database:** MySQL 8.0+
- **Authentication:** JWT Bearer
- **Validation:** FluentValidation
- **Logging:** Serilog
- **Metrics:** Prometheus.NET
- **Testing:** xUnit + Moq + FluentAssertions
- **Coverage:** Coverlet + ReportGenerator
- **Container:** Docker + Docker Compose

## 📄 License

**Proprietary Software License v1.0**

Copyright (c) 2025 Geovanny Camacho. All rights reserved.

**IMPORTANT:** This software and associated documentation files (the "Software") are the exclusive property of Geovanny Camacho and are protected by copyright laws and international treaty provisions.

### TERMS AND CONDITIONS

1. **OWNERSHIP**: The Software is licensed, not sold. Geovanny Camacho retains all right, title, and interest in and to the Software, including all intellectual property rights.

2. **RESTRICTIONS**: You may NOT:

   - Copy, modify, or create derivative works of the Software
   - Distribute, transfer, sublicense, lease, lend, or rent the Software
   - Reverse engineer, decompile, or disassemble the Software
   - Remove or alter any proprietary notices or labels on the Software
   - Use the Software for any commercial purpose without explicit written permission
   - Share access credentials or allow unauthorized access to the Software

3. **CONFIDENTIALITY**: The Software contains trade secrets and confidential information. You agree to maintain the confidentiality of the Software and not disclose it to any third party.

4. **TERMINATION**: This license is effective until terminated. Your rights under this license will terminate automatically without notice if you fail to comply with any of its terms.

5. **NO WARRANTY**: THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.

6. **LIMITATION OF LIABILITY**: IN NO EVENT SHALL GEOVANNY CAMACHO BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

7. **GOVERNING LAW**: This license shall be governed by and construed in accordance with the laws of the jurisdiction in which Geovanny Camacho resides, without regard to its conflict of law provisions.

8. **ENTIRE AGREEMENT**: This license constitutes the entire agreement between you and Geovanny Camacho regarding the Software and supersedes all prior or contemporaneous understandings.

**FOR LICENSING INQUIRIES:**  
Geovanny Camacho  
Email: fgiocl@outlook.com

**By using this Software, you acknowledge that you have read this license, understand it, and agree to be bound by its terms and conditions.**

---

**Author:** Geovanny Camacho (fgiocl@outlook.com)  
**Last Update:** 05/11/2025
