# Capa de Servicios - VERTEX Backend

## 📋 Índice
1. [Introducción](#introducción)
2. [Arquitectura](#arquitectura)
3. [Ventajas de usar Service Layer](#ventajas)
4. [Implementación](#implementación)
5. [Flujo de Datos](#flujo-de-datos)
6. [Responsabilidades](#responsabilidades)
7. [Buenas Prácticas](#buenas-prácticas)

---

## Introducción

La **Capa de Servicios** (Service Layer) es un patrón arquitectónico que separa la lógica de negocio de los controladores, promoviendo el principio de **Responsabilidad Única** (Single Responsibility Principle) y facilitando el mantenimiento, testing y escalabilidad del código.

### ¿Por qué usar Services?

En la arquitectura original, los controladores tenían **demasiadas responsabilidades**:
- Extraer datos del token JWT
- Validar datos de entrada
- Mapear DTOs a entidades
- Ejecutar lógica de negocio
- Manejar excepciones
- Registrar logs
- Devolver respuestas HTTP

Esto viola el principio **SRP** (Single Responsibility Principle).

---

## Arquitectura

### Antes: Sin Service Layer

```
┌─────────────┐
│  Controller │  ← Demasiadas responsabilidades
│             │     - HTTP Request/Response
│             │     - Validación
│             │     - Lógica de negocio
│             │     - Mapeo de datos
│             │     - Logging
└──────┬──────┘
       │
       ▼
┌─────────────┐
│ Repository  │  ← Solo acceso a datos
└─────────────┘
```

### Después: Con Service Layer ✅

```
┌─────────────┐
│  Controller │  ← Solo HTTP Concerns
│             │     - Extraer claims JWT
│             │     - Devolver respuestas HTTP
└──────┬──────┘
       │
       ▼
┌─────────────┐
│   Service   │  ← Lógica de Negocio
│             │     - Validación
│             │     - Reglas de negocio
│             │     - Mapeo DTO ↔ Entity
│             │     - Logging
│             │     - Manejo de errores
└──────┬──────┘
       │
       ▼
┌─────────────┐
│ Repository  │  ← Solo acceso a datos
└─────────────┘
```

---

## Ventajas

### 1. **Separación de Responsabilidades**
Cada capa tiene una responsabilidad bien definida.

### 2. **Reutilización de Código**
La lógica de negocio en el servicio puede ser reutilizada por múltiples controladores o incluso por servicios externos (gRPC, SignalR, etc.).

### 3. **Facilita el Testing**
Los servicios pueden ser testeados de forma **unitaria** sin necesidad de simular requests HTTP.

```csharp
// Test unitario del servicio (sin HTTP)
[Fact]
public async Task SaveProgressAsync_WithValidData_ReturnsSuccess()
{
    // Arrange
    var mockRepo = new Mock<IOnboardingRepository>();
    var service = new OnboardingService(mockRepo.Object, logger);
    
    // Act
    var result = await service.SaveProgressAsync("userId123", dto);
    
    // Assert
    Assert.True(result.Success);
}
```

### 4. **Inyección de Dependencias**
Los servicios se registran en el contenedor de DI, facilitando el cambio de implementaciones.

### 5. **Escalabilidad**
Si necesitamos agregar lógica compleja (caché, validaciones adicionales, eventos), lo hacemos en el servicio sin tocar el controlador.

---

## Implementación

### 1. Crear la Interfaz del Servicio

**Ubicación:** `Vertex.Application/Interfaces/IOnboardingService.cs`

```csharp
public interface IOnboardingService
{
    Task<ApiResponse<OnboardingStatusDto>> SaveProgressAsync(
        string userId, 
        SaveProgressDto dto);
    
    Task<ApiResponse<OnboardingStatusDto>> GetProgressAsync(string userId);
}
```

**¿Por qué una interfaz?**
- Permite **mockear** el servicio en tests
- Facilita el cambio de implementación sin modificar el controlador
- Cumple con el **Principio de Inversión de Dependencias** (DIP)

---

### 2. Implementar el Servicio

**Ubicación:** `Vertex.Application/Services/OnboardingService.cs`

```csharp
public class OnboardingService : IOnboardingService
{
    private readonly IOnboardingRepository _repository;
    private readonly ILogger<OnboardingService> _logger;

    public OnboardingService(
        IOnboardingRepository repository,
        ILogger<OnboardingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ApiResponse<OnboardingStatusDto>> SaveProgressAsync(
        string userId, 
        SaveProgressDto dto)
    {
        try
        {
            // 1. VALIDACIÓN
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ApiResponse<OnboardingStatusDto>.ErrorResponse(
                    "El UserId no puede estar vacío", 400);
            }

            if (dto.CurrentStep < 0 || dto.CurrentStep > 10)
            {
                return ApiResponse<OnboardingStatusDto>.ErrorResponse(
                    "El paso actual debe estar entre 0 y 10", 400);
            }

            // 2. MAPEO DTO → ENTIDAD
            var process = new OnboardingProcess
            {
                UserId = userId,
                CurrentStep = dto.CurrentStep,
                SerializedData = dto.SerializedData,
                IsCompleted = dto.IsCompleted
            };

            // 3. LLAMADA AL REPOSITORIO
            var savedProcess = await _repository.SaveOrUpdateAsync(process);

            // 4. MAPEO ENTIDAD → DTO
            var response = new OnboardingStatusDto
            {
                CurrentStep = savedProcess.CurrentStep,
                SerializedData = savedProcess.SerializedData,
                IsCompleted = savedProcess.IsCompleted,
                UpdatedAt = savedProcess.UpdatedAt
            };

            // 5. LOGGING
            _logger.LogInformation(
                "Progreso guardado para usuario {UserId}, paso {Step}",
                userId, dto.CurrentStep);

            // 6. RETORNAR RESPUESTA
            return ApiResponse<OnboardingStatusDto>.SuccessResponse(
                response, "Progreso guardado exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar progreso");
            return ApiResponse<OnboardingStatusDto>.ErrorResponse(
                "Error interno del servidor", 500, 
                new List<string> { ex.Message });
        }
    }
}
```

---

### 3. Actualizar el Controlador

**Ubicación:** `Vertex.API/Controllers/OnboardingController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _onboardingService; // ← Inyectamos el servicio
    private readonly ILogger<OnboardingController> _logger;

    public OnboardingController(
        IOnboardingService onboardingService,
        ILogger<OnboardingController> logger)
    {
        _onboardingService = onboardingService;
        _logger = logger;
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<OnboardingStatusDto>>> SaveProgress(
        [FromBody] SaveProgressDto dto)
    {
        // 1. EXTRAER USERID DEL TOKEN JWT (responsabilidad del controlador)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<OnboardingStatusDto>.ErrorResponse(
                "Usuario no autenticado", 401));
        }

        // 2. DELEGAR AL SERVICIO (toda la lógica de negocio)
        var result = await _onboardingService.SaveProgressAsync(userId, dto);
        
        // 3. DEVOLVER RESPUESTA HTTP APROPIADA
        return result.StatusCode switch
        {
            200 => Ok(result),
            400 => BadRequest(result),
            500 => StatusCode(500, result),
            _ => StatusCode(result.StatusCode, result)
        };
    }
}
```

**Observa cómo el controlador es ahora "delgado" (thin controller):**
- Solo extrae el `userId` del token JWT
- Delega toda la lógica al servicio
- Devuelve la respuesta HTTP apropiada

---

### 4. Registrar el Servicio en DI

**Ubicación:** `Vertex.API/Program.cs`

```csharp
// Inyección de Dependencias: Repositorios
builder.Services.AddScoped<IOnboardingRepository, OnboardingRepository>();

// Inyección de Dependencias: Servicios de Aplicación
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
```

**¿Por qué AddScoped?**
- `AddScoped`: Una instancia por request HTTP (recomendado para servicios que usan DbContext)
- `AddTransient`: Una instancia por cada inyección (para servicios stateless ligeros)
- `AddSingleton`: Una única instancia para toda la aplicación (para servicios sin estado)

---

## Flujo de Datos

### Request Completo

```
┌──────────────────────────────────────────────────────────────┐
│  1. Cliente (Postman/Frontend)                               │
│     POST /api/onboarding/save                                │
│     Authorization: Bearer <JWT>                              │
│     Body: { "currentStep": 3, "serializedData": "..." }     │
└────────────────────────┬─────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────┐
│  2. OnboardingController                                     │
│     - Extrae UserId del JWT: User.FindFirstValue(...)       │
│     - Valida autenticación                                   │
└────────────────────────┬─────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────┐
│  3. OnboardingService                                        │
│     - Valida datos de entrada (currentStep, userId)         │
│     - Mapea DTO → Entity                                     │
│     - Aplica reglas de negocio                               │
│     - Registra logs                                          │
└────────────────────────┬─────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────┐
│  4. OnboardingRepository                                     │
│     - Ejecuta query a la base de datos                       │
│     - SaveOrUpdateAsync()                                    │
└────────────────────────┬─────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────┐
│  5. SQL Server Database                                      │
│     INSERT/UPDATE en tabla OnboardingProcesses               │
└────────────────────────┬─────────────────────────────────────┘
                         │
                         ▼ (Respuesta de vuelta)
┌──────────────────────────────────────────────────────────────┐
│  6. OnboardingService                                        │
│     - Mapea Entity → DTO                                     │
│     - Construye ApiResponse<OnboardingStatusDto>             │
└────────────────────────┬─────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────┐
│  7. OnboardingController                                     │
│     - Devuelve HTTP 200 OK con ApiResponse                   │
└────────────────────────┬─────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────┐
│  8. Cliente (Postman/Frontend)                               │
│     Response:                                                │
│     {                                                        │
│       "success": true,                                       │
│       "message": "Progreso guardado exitosamente",           │
│       "data": { ... },                                       │
│       "statusCode": 200                                      │
│     }                                                        │
└──────────────────────────────────────────────────────────────┘
```

---

## Responsabilidades

### Controller (OnboardingController)
✅ **SÍ debe hacer:**
- Extraer datos del request (claims JWT, body, query params)
- Validar autenticación/autorización a nivel HTTP
- Devolver códigos de estado HTTP apropiados
- Manejar respuestas HTTP (Ok, BadRequest, Unauthorized, etc.)

❌ **NO debe hacer:**
- Validar lógica de negocio (ej: "currentStep debe estar entre 0 y 10")
- Mapear DTOs a entidades
- Ejecutar lógica de negocio compleja
- Acceder directamente al repositorio

---

### Service (OnboardingService)
✅ **SÍ debe hacer:**
- Validar reglas de negocio
- Ejecutar lógica de aplicación
- Mapear DTOs ↔ Entidades
- Coordinar múltiples repositorios si es necesario
- Registrar logs de negocio
- Manejar excepciones de lógica de negocio
- Retornar respuestas estandarizadas (ApiResponse)

❌ **NO debe hacer:**
- Acceder al DbContext directamente
- Ejecutar queries SQL manualmente
- Manejar requests/responses HTTP
- Extraer claims JWT

---

### Repository (OnboardingRepository)
✅ **SÍ debe hacer:**
- Ejecutar queries a la base de datos
- Mapear entidades EF Core
- Aplicar filtros LINQ
- Manejar transacciones si es necesario

❌ **NO debe hacer:**
- Validar reglas de negocio
- Mapear a DTOs
- Registrar logs de negocio (solo logs de datos)

---

## Buenas Prácticas

### 1. **Usar Interfaces para Servicios**
```csharp
// ✅ BIEN
public interface IOnboardingService { ... }
public class OnboardingService : IOnboardingService { ... }

// ❌ MAL
public class OnboardingService { ... } // Sin interfaz
```

### 2. **Un Servicio por Agregado de Dominio**
```csharp
// ✅ BIEN
IOnboardingService  → OnboardingProcess
IUserService        → ApplicationUser
IResumeService      → Resume

// ❌ MAL
IGeneralService     → Hace TODO
```

### 3. **Servicios Stateless**
Los servicios NO deben mantener estado entre requests.

```csharp
// ❌ MAL
public class OnboardingService
{
    private string _currentUserId; // ¡Estado compartido!
}

// ✅ BIEN
public class OnboardingService
{
    // Sin campos de estado
    public async Task<ApiResponse> SaveAsync(string userId, ...) { }
}
```

### 4. **Servicios Pequeños y Cohesivos**
Si un servicio tiene más de 10 métodos, probablemente debas dividirlo.

```csharp
// ❌ MAL
public interface IOnboardingService
{
    Task SaveAsync(...);
    Task GetAsync(...);
    Task DeleteAsync(...);
    Task SendEmailAsync(...);      // ← Esto debería estar en IEmailService
    Task GenerateReportAsync(...); // ← Esto debería estar en IReportService
}

// ✅ BIEN
public interface IOnboardingService
{
    Task SaveProgressAsync(...);
    Task GetProgressAsync(...);
}

public interface IEmailService
{
    Task SendEmailAsync(...);
}
```

### 5. **Logging en Servicios, No en Controladores**
```csharp
// ✅ BIEN
public class OnboardingService
{
    public async Task SaveAsync(...)
    {
        _logger.LogInformation("Guardando progreso para usuario {UserId}", userId);
        // ...
    }
}

// ❌ MAL - Logging en el controlador
public class OnboardingController
{
    public async Task<IActionResult> SaveAsync(...)
    {
        _logger.LogInformation("Guardando progreso"); // ← No
        await _service.SaveAsync(...);
    }
}
```

### 6. **Retornar Tipos Tipados, No IActionResult**
Los servicios NO deben retornar tipos HTTP.

```csharp
// ✅ BIEN
public async Task<ApiResponse<OnboardingStatusDto>> SaveAsync(...)

// ❌ MAL
public async Task<IActionResult> SaveAsync(...) // ← Esto es HTTP, no pertenece al servicio
```

---

## Resumen

| Capa       | Responsabilidad                          | Ejemplo                           |
|------------|------------------------------------------|-----------------------------------|
| Controller | HTTP Requests/Responses, Autenticación   | Extraer userId del JWT            |
| Service    | Lógica de Negocio, Validación, Mapeo     | Validar currentStep entre 0 y 10  |
| Repository | Acceso a Datos                           | SaveOrUpdateAsync()               |

### Flujo Ideal
```
HTTP Request → Controller → Service → Repository → Database
                     ↓           ↓           ↓
              JWT Claims   Validación   Query SQL
                                ↓
                           ApiResponse
```

---

## Conclusión

La implementación de la **Service Layer** en VERTEX Backend mejora significativamente la arquitectura al:
- ✅ Separar responsabilidades (SRP)
- ✅ Facilitar testing unitario
- ✅ Permitir reutilización de lógica
- ✅ Mejorar mantenibilidad y escalabilidad
- ✅ Cumplir con los principios SOLID

Esta arquitectura en capas es fundamental para proyectos que crecen en complejidad y es una de las mejores prácticas en desarrollo backend moderno con .NET.

---

**Documento generado para el Proyecto VERTEX - Backend con Clean Architecture**  
**Fecha:** Enero 2025  
**Autor:** Equipo VERTEX
