# 🌟 VERTEX - Sistema de Gestión de CV Profesionales

Backend desarrollado en .NET 9 siguiendo **Clean Architecture** para el sistema de onboarding y gestión de perfiles profesionales.

---

## 🏗️ Arquitectura

Este proyecto implementa **Clean Architecture (Onion Architecture)** con 4 capas claramente definidas:

- **Vertex.Domain**: Entidades de negocio (núcleo)
- **Vertex.Application**: Interfaces y DTOs (contratos)
- **Vertex.Infrastructure**: Implementación de persistencia con EF Core
- **Vertex.API**: Endpoints REST y configuración de servicios

---

## 🚀 Inicio Rápido

### Prerrequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB o instancia completa)
- Visual Studio 2022 / VS Code / Rider

### 1. Clonar el Repositorio

```bash
cd "Proyecto VERTEX"
```

### 2. Restaurar Dependencias

```bash
dotnet restore
```

### 3. Crear la Base de Datos

```bash
cd src/Vertex.API
dotnet ef migrations add InitialMigrationWithIdentity --project ../Vertex.Infrastructure
dotnet ef database update
```

### 4. Ejecutar la Aplicación

```bash
dotnet run --project src/Vertex.API/Vertex.API.csproj
```

La API estará disponible en:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

---

## 📡 Endpoints Disponibles

### 🔐 Autenticación

#### **POST** `/api/Auth/register`
Registra un nuevo usuario.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123",
  "fullName": "John Doe"
}
```

**Response:** `200 OK`
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "email": "user@example.com",
  "fullName": "John Doe",
  "expiresAt": "2026-01-22T11:30:00Z"
}
```

#### **POST** `/api/Auth/login`
Inicia sesión y genera un token JWT.

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123"
}
```

**Response:** `200 OK` o `401 Unauthorized`

---

### 📝 Onboarding (Requiere Autenticación)

> **Nota:** Todos los endpoints de onboarding requieren el header:
> ```
> Authorization: Bearer {token}
> ```

#### **POST** `/api/Onboarding/save`
Guarda el progreso del onboarding del usuario autenticado.

**Request Body:**
```json
{
  "currentStep": 2,
  "serializedData": "{\"name\":\"John Doe\"}",
  "isCompleted": false
}
```
> ⚠️ **Seguridad:** El `userId` se extrae automáticamente del token JWT

**Response:** `200 OK`
```json
{
  "currentStep": 2,
  "serializedData": "{\"name\":\"John Doe\"}",
  "isCompleted": false,
  "updatedAt": "2026-01-22T10:30:00Z"
}
```

#### **GET** `/api/Onboarding/resume`
Recupera el estado actual del onboarding del usuario autenticado.

**Response:** `200 OK` o `404 Not Found`

---

## 🗄️ Base de Datos

### Cadena de Conexión (Desarrollo)

Definida en `src/Vertex.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VertexDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Tablas Principales

- **OnboardingProcesses**: Almacena el progreso del onboarding
- **ProfessionalProfiles**: Almacena los perfiles profesionales finales
- **AspNetUsers**: Tabla de identidad (preparada para autenticación)

---

## 🧪 Testing

```bash
# Compilar la solución
dotnet build

# Ejecutar pruebas (cuando estén implementadas)
dotnet test
```

---

## 📚 Documentación Adicional

Para más detalles sobre la arquitectura, implementación y decisiones técnicas, consulta:

📄 [**docs/documentacion.md**](docs/documentacion.md)

---

## 🔐 Seguridad

⚠️ **IMPORTANTE:**
- El sistema actualmente NO implementa autenticación JWT.
- El `UserId` se recibe del frontend sin validación.
- **NO USAR EN PRODUCCIÓN** sin implementar seguridad.

### Próximos Pasos de Seguridad:
1. Implementar JWT Bearer Authentication
2. Agregar validación de entrada con FluentValidation
3. Implementar Rate Limiting
4. Configurar HTTPS obligatorio en producción

---

## 📦 Estructura del Proyecto

```
Vertex/
├── src/
│   ├── Vertex.Domain/
│   │   └── Entities/
│   │       ├── OnboardingProcess.cs
│   │       └── ProfessionalProfile.cs
│   │
│   ├── Vertex.Application/
│   │   ├── Interfaces/
│   │   │   └── IOnboardingRepository.cs
│   │   └── DTOs/
│   │       ├── SaveProgressDto.cs
│   │       └── OnboardingStatusDto.cs
│   │
│   ├── Vertex.Infrastructure/
│   │   ├── Data/
│   │   │   └── VertexDbContext.cs
│   │   └── Repositories/
│   │       └── OnboardingRepository.cs
│   │
│   └── Vertex.API/
│       ├── Controllers/
│       │   └── OnboardingController.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── docs/
│   └── documentacion.md
├── Vertex.sln
└── README.md
```

---

## 🛠️ Tecnologías Utilizadas

- **.NET 9**: Framework principal
- **Entity Framework Core 10.0**: ORM para persistencia
- **SQL Server**: Base de datos
- **ASP.NET Core Identity**: Preparado para autenticación
- **Swashbuckle/Swagger**: Documentación de API
- **Clean Architecture**: Patrón arquitectónico

---

## 👥 Contribución

Este es un proyecto académico/profesional. Para contribuir:

1. Fork el proyecto
2. Crea una rama (`git checkout -b feature/nueva-funcionalidad`)
3. Commit tus cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

---

## 📄 Licencia

Este proyecto es de uso educativo y profesional.

---

## ✅ Estado del Proyecto

**Build Status:** ✅ Compilación Exitosa

**Funcionalidades Implementadas:**
- ✅ Estructura de Clean Architecture
- ✅ Entidades de dominio
- ✅ Repositorio con patrón Upsert
- ✅ Endpoints REST para onboarding
- ✅ Configuración de EF Core con SQL Server
- ✅ Swagger UI para testing

**Pendiente:**
- ⏳ Autenticación JWT
- ⏳ Migraciones de base de datos
- ⏳ Validación de entrada
- ⏳ Pruebas unitarias
- ⏳ Frontend integration

---

Desarrollado con ❤️ siguiendo las mejores prácticas de .NET