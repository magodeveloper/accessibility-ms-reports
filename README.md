# 📊 Accessibility Reports Service

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Tests](https://img.shields.io/badge/tests-445%2F458-brightgreen)](test-dashboard.html)
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

**Total: 16 endpoints disponibles**

## 🧪 Testing

### Estado de Cobertura

**Estado General:** ✅ 432/444 tests exitosos (97.3%)  
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
- **Tiempo de ejecución:** ~20s para 444 tests
- **Tasa de éxito:** 97.3% (432/444)

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

## 🐳 Deployment

### Docker

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

## 🛠️ Stack Tecnológico

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

## � License

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
**Last Update:** 09/10/2025
