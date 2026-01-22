# Documentación Técnica - Proyecto VERTEX Backend

## 📋 Información General
- **Proyecto:** VERTEX - Sistema de Gestión de CV Profesionales
- **Framework:** .NET 9.0
- **Arquitectura:** Clean Architecture (Onion Architecture)
- **Fecha:** Enero 22, 2026

---

## 🏗️ Arquitectura del Proyecto

El proyecto sigue los principios de **Clean Architecture**, separando las responsabilidades en 4 capas independientes:

```
Vertex/
├── src/
│   ├── Vertex.Domain          (Capa de Dominio - Núcleo)
│   ├── Vertex.Application     (Capa de Aplicación)
│   ├── Vertex.Infrastructure  (Capa de Infraestructura)
│   └── Vertex.API             (Capa de Presentación)
└── Vertex.sln
```

### Reglas de Dependencia (Onion)
1. **Domain** → No tiene dependencias externas
2. **Application** → Depende solo de **Domain**
3. **Infrastructure** → Depende de **Application** y **Domain**
4. **API** → Depende de **Application** e **Infrastructure**

---

## 📦 Paso 1: Creación de la Solución y Proyectos

### Comandos Ejecutados

```bash
# Crear solución
dotnet new sln -n Vertex

# Crear proyectos
dotnet new classlib -n Vertex.Domain -o src/Vertex.Domain
dotnet new classlib -n Vertex.Application -o src/Vertex.Application
dotnet new classlib -n Vertex.Infrastructure -o src/Vertex.Infrastructure
dotnet new webapi -n Vertex.API -o src/Vertex.API

# Agregar proyectos a la solución
dotnet sln add src/Vertex.Domain/Vertex.Domain.csproj
dotnet sln add src/Vertex.Application/Vertex.Application.csproj
dotnet sln add src/Vertex.Infrastructure/Vertex.Infrastructure.csproj
dotnet sln add src/Vertex.API/Vertex.API.csproj
```

### Referencias entre Proyectos

```bash
# Application → Domain
cd src/Vertex.Application
dotnet add reference ../Vertex.Domain/Vertex.Domain.csproj

# Infrastructure → Domain + Application
cd ../Vertex.Infrastructure
dotnet add reference ../Vertex.Domain/Vertex.Domain.csproj
dotnet add reference ../Vertex.Application/Vertex.Application.csproj

# API → Application + Infrastructure
cd ../Vertex.API
dotnet add reference ../Vertex.Application/Vertex.Application.csproj
dotnet add reference ../Vertex.Infrastructure/Vertex.Infrastructure.csproj
```

### Paquetes NuGet Instalados

**Domain:**
```bash
dotnet add package Microsoft.Extensions.Identity.Stores --version 9.0.1
```

**Infrastructure:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.1
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.1
```

**API:**
```bash
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.1
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 9.0.1
dotnet add package Swashbuckle.AspNetCore --version 7.2.0
```

---

## 🎯 Paso 2: Capa de Dominio (Vertex.Domain)

### Responsabilidad
Contiene las **entidades de negocio puras** (POCOs - Plain Old CLR Objects). No tiene dependencias de frameworks ni tecnologías externas.

### Entidades Creadas

#### 1. `OnboardingProcess.cs`
**Ubicación:** `src/Vertex.Domain/Entities/OnboardingProcess.cs`

**Propósito:** Representa el estado del proceso de onboarding multi-paso de un usuario.

**Propiedades:**
- `Id` (Guid): Identificador único
- `UserId` (string): ID del usuario del sistema de identidad
- `CurrentStep` (int): Paso actual del formulario (default: 1)
- `SerializedData` (string): Datos del formulario en formato JSON
- `UpdatedAt` (DateTime): Fecha de última actualización
- `IsCompleted` (bool): Indica si el onboarding está completo

**Regla de Negocio:** Solo puede existir un proceso activo por usuario (índice único).

#### 2. `ProfessionalProfile.cs`
**Ubicación:** `src/Vertex.Domain/Entities/ProfessionalProfile.cs`

**Propósito:** Representa el perfil profesional final (CV) generado al completar el onboarding.

**Propiedades:**
- `Id` (Guid): Identificador único
- `UserId` (string): ID del usuario propietario
- `FullName` (string): Nombre completo del profesional
- `Summary` (string): Resumen profesional o biografía
- `SkillsJson` (string): Habilidades serializadas en JSON
- `CreatedAt` (DateTime): Fecha de creación
- `UpdatedAt` (DateTime): Fecha de última actualización

---

## 🔧 Paso 3: Capa de Aplicación (Vertex.Application)

### Responsabilidad
Define los **contratos (interfaces)** y **objetos de transferencia de datos (DTOs)** para desacoplar el dominio de la infraestructura.

### Interfaces Creadas

#### `IOnboardingRepository.cs`
**Ubicación:** `src/Vertex.Application/Interfaces/IOnboardingRepository.cs`

**Métodos:**
- `GetByUserIdAsync(string userId)`: Obtiene el proceso de onboarding de un usuario
- `SaveOrUpdateAsync(OnboardingProcess process)`: Upsert - Guarda o actualiza un proceso

### DTOs Creados

#### 1. `SaveProgressDto.cs`
**Ubicación:** `src/Vertex.Application/DTOs/SaveProgressDto.cs`

**Propósito:** Recibe datos del frontend al guardar progreso.

**Propiedades:**
- `UserId` (string): ID del usuario
- `CurrentStep` (int): Paso actual
- `SerializedData` (string): Datos en JSON
- `IsCompleted` (bool): Estado de completitud

#### 2. `OnboardingStatusDto.cs`
**Ubicación:** `src/Vertex.Application/DTOs/OnboardingStatusDto.cs`

**Propósito:** Respuesta al frontend con el estado actual.

**Propiedades:**
- `CurrentStep` (int): Paso actual
- `SerializedData` (string): Datos en JSON
- `IsCompleted` (bool): Estado
- `UpdatedAt` (DateTime): Última actualización

---

## 💾 Paso 4: Capa de Infraestructura (Vertex.Infrastructure)

### Responsabilidad
Implementa la **persistencia de datos** usando Entity Framework Core y SQL Server.

### Componentes Creados

#### 1. `VertexDbContext.cs`
**Ubicación:** `src/Vertex.Infrastructure/Data/VertexDbContext.cs`

**Características:**
- Hereda de `IdentityDbContext` para soporte futuro de autenticación
- Define `DbSet` para `OnboardingProcesses` y `ProfessionalProfiles`
- Configura restricciones en `OnModelCreating`:
  - Índice único en `OnboardingProcess.UserId`
  - Tipo de columna `nvarchar(max)` para campos JSON
  - Valores por defecto para `CurrentStep` (1) e `IsCompleted` (false)

#### 2. `OnboardingRepository.cs`
**Ubicación:** `src/Vertex.Infrastructure/Repositories/OnboardingRepository.cs`

**Implementación del patrón Repository:**
- **GetByUserIdAsync:** Consulta el proceso activo de un usuario
- **SaveOrUpdateAsync:** 
  - **Regla Crítica:** Verifica si existe un proceso previo
  - Si existe → Actualiza los campos (`CurrentStep`, `SerializedData`, `IsCompleted`, `UpdatedAt`)
  - Si no existe → Crea un nuevo registro con `Id` generado
  - **Previene duplicados** para el mismo usuario

---

## 🌐 Paso 5: Capa de API (Vertex.API)

### Responsabilidad
Expone los **endpoints REST** para que el frontend interactúe con el backend.

### Componentes Creados

#### 1. `OnboardingController.cs`
**Ubicación:** `src/Vertex.API/Controllers/OnboardingController.cs`

**Endpoints:**

##### POST `/api/Onboarding/save`
- **Propósito:** Guardar el progreso del onboarding
- **Request Body:** `SaveProgressDto`
- **Response:** `OnboardingStatusDto` (200 OK) o BadRequest (400)
- **Lógica:**
  1. Valida que el `UserId` no esté vacío
  2. Mapea el DTO a la entidad `OnboardingProcess`
  3. Llama al repositorio para guardar/actualizar
  4. Retorna el estado actualizado

**Nota:** El `UserId` actualmente se recibe del DTO. En producción, se extraerá del Token JWT.

##### GET `/api/Onboarding/resume?userId={userId}`
- **Propósito:** Recuperar el estado actual del onboarding
- **Query Parameter:** `userId` (string)
- **Response:** `OnboardingStatusDto` (200 OK) o NotFound (404)
- **Lógica:**
  1. Valida que el `UserId` no esté vacío
  2. Busca el proceso en el repositorio
  3. Si existe, retorna el estado; si no, retorna 404

#### 2. `Program.cs`
**Ubicación:** `src/Vertex.API/Program.cs`

**Configuración de Servicios:**
- **DbContext:** Configurado con SQL Server usando la cadena de conexión `DefaultConnection`
- **Inyección de Dependencias:** Registra `IOnboardingRepository` → `OnboardingRepository` con scope transient
- **Swagger/OpenAPI:** Configurado para documentación interactiva
- **CORS:** Configurado para permitir orígenes de frontend (localhost:3000 y localhost:5173)

**Configuración del Pipeline HTTP:**
- Habilita Swagger en modo Development
- Aplica CORS con política `AllowFrontend`
- Mapea controladores

#### 3. `appsettings.json`
**Ubicación:** `src/Vertex.API/appsettings.json`

**Configuración de Conexión a Base de Datos:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=VertexDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Nota:** Esta es una cadena de conexión para desarrollo local con Windows Authentication. Para producción, se debe usar un usuario y contraseña específicos.

---

## 🚀 Pasos Siguientes (Pendientes)

### 1. Crear Migraciones de Base de Datos
```bash
cd src/Vertex.API
dotnet ef migrations add InitialCreate --project ../Vertex.Infrastructure
dotnet ef database update
```

### 2. Implementar Autenticación con JWT
- Instalar `Microsoft.AspNetCore.Authentication.JwtBearer`
- Configurar políticas de autorización
- Reemplazar `UserId` hardcodeado por extracción desde `ClaimsPrincipal`

### 3. Implementar Validaciones
- Instalar `FluentValidation.AspNetCore`
- Crear validadores para DTOs
- Agregar middleware de manejo de errores global

### 4. Testing
- Crear proyecto de pruebas unitarias para la lógica de negocio
- Crear pruebas de integración para los endpoints

---

## 📊 Diagrama de Dependencias

```
┌─────────────────┐
│   Vertex.API    │  ← Capa de Presentación (Controladores HTTP)
└────────┬────────┘
         │
         ├──────────────────┐
         ↓                  ↓
┌──────────────────┐  ┌─────────────────────┐
│ Vertex.Application│  │ Vertex.Infrastructure│
│   (Interfaces)    │  │   (Implementación)   │
└────────┬─────────┘  └──────────┬───────────┘
         │                       │
         └───────────┬───────────┘
                     ↓
            ┌────────────────┐
            │ Vertex.Domain  │  ← Núcleo (Entidades)
            └────────────────┘
```

---

## 🔐 Consideraciones de Seguridad

1. **Autenticación:** Pendiente implementar JWT Bearer Tokens
2. **Autorización:** Cada usuario solo debe acceder a sus propios datos
3. **Validación de Entrada:** Los DTOs deben validarse antes de procesarse
4. **SQL Injection:** EF Core previene esto automáticamente con consultas parametrizadas
5. **CORS:** Restringido a orígenes específicos del frontend

---

## 📝 Conclusión

La infraestructura backend del proyecto VERTEX ha sido completamente implementada siguiendo los principios de Clean Architecture. El sistema está listo para:

1. ✅ Crear las migraciones de base de datos
2. ✅ Ejecutar la aplicación y probar los endpoints
3. ✅ Conectar con el frontend
4. ⏳ Implementar autenticación y autorización (siguiente fase)

**Estado del Build:** ✅ Compilación Exitosa (0 errores, 0 warnings)
