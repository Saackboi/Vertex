# 📘 VERTEX API - Documentación Completa de Contexto

> **Objetivo**: Este documento proporciona toda la información necesaria para que un agente/desarrollador frontend pueda trabajar con el API backend de VERTEX sin necesidad de explorar el código fuente.

---

## 📑 Tabla de Contenidos

1. [Arquitectura General](#arquitectura-general)
2. [Tecnologías y Stack](#tecnologías-y-stack)
3. [Configuración y Conexión](#configuración-y-conexión)
4. [Autenticación y Seguridad](#autenticación-y-seguridad)
5. [Endpoints del API](#endpoints-del-api)
6. [Modelos de Datos y DTOs](#modelos-de-datos-y-dtos)
7. [SignalR - Notificaciones en Tiempo Real](#signalr---notificaciones-en-tiempo-real)
8. [Flujo de Onboarding](#flujo-de-onboarding)
9. [Manejo de Errores](#manejo-de-errores)
10. [Base de Datos](#base-de-datos)

---

## 🏗️ Arquitectura General

### Clean Architecture (Capas)

El proyecto sigue el patrón de **Clean Architecture** dividido en 4 capas:

```
Vertex.API/              → Capa de Presentación (Controllers, Hubs)
Vertex.Application/      → Capa de Aplicación (Services, DTOs, Interfaces)
Vertex.Domain/           → Capa de Dominio (Entities, Value Objects)
Vertex.Infrastructure/   → Capa de Infraestructura (DB, Repositories, Services externos)
```

### Principios Aplicados

- ✅ **Separación de Responsabilidades**: Controladores solo manejan HTTP, servicios tienen la lógica de negocio
- ✅ **Inyección de Dependencias**: Todo configurado en `Program.cs`
- ✅ **Unit of Work Pattern**: Transacciones controladas
- ✅ **Repository Pattern**: Acceso a datos abstracto
- ✅ **DTOs**: No se exponen entidades de dominio al frontend

---

## 🛠️ Tecnologías y Stack

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| **.NET** | 9.0 | Framework principal |
| **ASP.NET Core** | 9.0 | Web API |
| **Entity Framework Core** | 9.0 | ORM para SQL Server |
| **SQL Server** | 2022+ | Base de datos |
| **ASP.NET Core Identity** | 9.0 | Sistema de usuarios y autenticación |
| **JWT Bearer** | Estándar | Tokens de autenticación |
| **SignalR** | 9.0 | WebSockets para notificaciones en tiempo real |
| **Swagger/OpenAPI** | 3.0 | Documentación interactiva |

---

## 🔌 Configuración y Conexión

### URL Base del API

```
Development: https://localhost:7295
            http://localhost:5019
```

### Endpoints Principales

```
API Base:     /api
Auth:         /api/auth
Onboarding:   /api/onboarding
SignalR Hub:  /hubs/notifications
Swagger:      /swagger
```

### CORS Configurado

El API acepta peticiones desde:
- `http://localhost:4200` (Angular)
- `http://localhost:3000` (React)
- `http://localhost:5173` (Vite)

**Configuración CORS**:
- ✅ AllowAnyHeader
- ✅ AllowAnyMethod
- ✅ AllowCredentials (necesario para SignalR)

### Connection String

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=VertexDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

---

## 🔐 Autenticación y Seguridad

### Sistema de Autenticación

**Proveedor**: ASP.NET Core Identity + JWT Bearer Tokens

### Configuración JWT

```json
"JwtSettings": {
  "Key": "VertexSecureKey2026-ThisIsAVerySecureKeyForProduction",
  "Issuer": "VertexAPI",
  "Audience": "VertexClient",
  "DurationInMinutes": 60
}
```

### Políticas de Contraseña

- ✅ Requiere dígito
- ✅ Requiere minúscula
- ✅ Requiere mayúscula
- ❌ NO requiere carácter especial
- ✅ Longitud mínima: 6 caracteres
- ✅ Email único

### Política de Bloqueo de Cuenta

- **Intentos máximos**: 5 intentos fallidos
- **Duración del bloqueo**: 5 minutos

### Cómo Usar el Token JWT

#### 1. Registro/Login

Al hacer login/registro, el API devuelve:

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Login exitoso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "usuario@example.com",
    "fullName": "Juan Pérez",
    "expiresAt": "2026-01-24T12:00:00Z"
  }
}
```

#### 2. Peticiones Autenticadas

Incluir el token en el header `Authorization`:

```http
GET /api/onboarding/resume HTTP/1.1
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### 3. Claims en el Token

El token JWT contiene los siguientes claims:

```csharp
ClaimTypes.NameIdentifier  → UserId (Guid del usuario)
ClaimTypes.Email           → Email del usuario
ClaimTypes.Name            → FullName del usuario
JwtRegisteredClaimNames.Jti → Token ID único
```

**⚠️ IMPORTANTE**: El `UserId` se extrae del token en el backend, **NUNCA** se envía desde el frontend por seguridad.

---

## 🌐 Endpoints del API

### 1. AuthController (`/api/auth`)

#### 🔵 POST `/api/auth/register`

Registra un nuevo usuario en el sistema.

**Request Body**:
```json
{
  "email": "usuario@example.com",
  "password": "Password123",
  "fullName": "Juan Pérez"
}
```

**Response 201 (Created)**:
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Usuario registrado exitosamente",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "usuario@example.com",
    "fullName": "Juan Pérez",
    "expiresAt": "2026-01-24T12:00:00Z"
  },
  "errors": []
}
```

**Errores Posibles**:
- `400 Bad Request`: Email ya existe, contraseña débil, datos inválidos
- `500 Internal Server Error`: Error del servidor

---

#### 🔵 POST `/api/auth/login`

Inicia sesión en el sistema.

**Request Body**:
```json
{
  "email": "usuario@example.com",
  "password": "Password123"
}
```

**Response 200 (OK)**:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Login exitoso",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "email": "usuario@example.com",
    "fullName": "Juan Pérez",
    "expiresAt": "2026-01-24T12:00:00Z"
  },
  "errors": []
}
```

**Errores Posibles**:
- `401 Unauthorized`: Credenciales incorrectas
- `400 Bad Request`: Datos inválidos
- `500 Internal Server Error`: Error del servidor

---

### 2. OnboardingController (`/api/onboarding`)

**🔒 Todos los endpoints requieren autenticación (JWT Bearer Token)**

#### 🔵 POST `/api/onboarding/save`

Guarda el progreso del onboarding del usuario.

**Headers**:
```http
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body**:
```json
{
  "currentStep": 2,
  "isCompleted": false,
  "data": {
    "fullName": "Juan Pérez",
    "email": "juan@example.com",
    "summary": "Desarrollador con 5 años de experiencia...",
    "skills": ["C#", "Angular", "SQL Server"],
    "experiences": [
      {
        "company": "Tech Corp",
        "role": "Senior Developer",
        "dateRange": {
          "start": "2020-01-01T00:00:00Z",
          "end": null
        }
      }
    ],
    "educations": [
      {
        "institution": "Universidad XYZ",
        "degree": "Ingeniería en Sistemas",
        "dateRange": {
          "start": "2015-01-01T00:00:00Z",
          "end": "2019-12-01T00:00:00Z"
        }
      }
    ]
  }
}
```

**Response 200 (OK)**:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Progreso guardado exitosamente",
  "data": {
    "currentStep": 2,
    "isCompleted": false,
    "updatedAt": "2026-01-23T10:30:00Z",
    "data": {
      "fullName": "Juan Pérez",
      "email": "juan@example.com",
      "summary": "Desarrollador con 5 años de experiencia...",
      "skills": ["C#", "Angular", "SQL Server"],
      "experiences": [...],
      "educations": [...]
    }
  },
  "errors": []
}
```

**Errores Posibles**:
- `401 Unauthorized`: Token inválido o expirado
- `400 Bad Request`: Datos inválidos (ej: currentStep < 1)
- `500 Internal Server Error`: Error del servidor

**⚠️ IMPORTANTE**: 
- El `UserId` se extrae automáticamente del token JWT
- **NO** enviar `userId` en el request body
- Al guardar progreso, se envía una notificación SignalR al usuario

---

#### 🔵 GET `/api/onboarding/resume`

Recupera el estado actual del onboarding del usuario autenticado.

**Headers**:
```http
Authorization: Bearer {token}
```

**Response 200 (OK)**:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Progreso recuperado exitosamente",
  "data": {
    "currentStep": 2,
    "isCompleted": false,
    "updatedAt": "2026-01-23T10:30:00Z",
    "data": {
      "fullName": "Juan Pérez",
      "email": "juan@example.com",
      "summary": "Desarrollador con 5 años de experiencia...",
      "skills": ["C#", "Angular", "SQL Server"],
      "experiences": [...],
      "educations": [...]
    }
  },
  "errors": []
}
```

**Response 404 (Not Found)**:
```json
{
  "success": false,
  "statusCode": 404,
  "message": "No se encontró un proceso de onboarding para este usuario",
  "data": null,
  "errors": []
}
```

**Uso Típico**:
- Al cargar la aplicación, verificar si hay progreso guardado
- Si hay progreso, redirigir al usuario al paso `currentStep`
- Si no hay progreso (404), iniciar desde el paso 1

---

#### 🔵 POST `/api/onboarding/complete`

Completa el proceso de onboarding y convierte los datos en un perfil profesional relacional.

**Headers**:
```http
Authorization: Bearer {token}
```

**Request Body**: No requiere body (usa el onboarding guardado)

**Response 201 (Created)**:
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Onboarding completado exitosamente",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userId": "a1b2c3d4-...",
    "summary": "Desarrollador con 5 años de experiencia...",
    "skills": [
      {
        "id": "...",
        "name": "C#",
        "level": "Avanzado"
      }
    ],
    "experiences": [
      {
        "id": "...",
        "company": "Tech Corp",
        "position": "Senior Developer",
        "startDate": "2020-01-01",
        "endDate": null,
        "isCurrent": true
      }
    ],
    "educations": [...]
  },
  "errors": []
}
```

**Errores Posibles**:
- `401 Unauthorized`: Token inválido
- `404 Not Found`: No existe un proceso de onboarding para completar
- `400 Bad Request`: El onboarding ya fue completado previamente
- `500 Internal Server Error`: Error del servidor

**⚠️ IMPORTANTE**:
- Al completar, se envía una notificación SignalR: **"Onboarding completado exitosamente"**
- Los datos temporales de `OnboardingProcess` se convierten en entidades relacionales (`ProfessionalProfile`, `WorkExperience`, `Education`, `ProfileSkill`)
- El `OnboardingProcess` se marca como `IsCompleted = true`

---

## 📦 Modelos de Datos y DTOs

### DTOs (Data Transfer Objects)

#### RegisterDto
```csharp
{
  "email": "string (required, email format)",
  "password": "string (required, min 6 chars)",
  "fullName": "string (required)"
}
```

#### LoginDto
```csharp
{
  "email": "string (required)",
  "password": "string (required)"
}
```

#### AuthResponseDto
```csharp
{
  "token": "string (JWT)",
  "email": "string",
  "fullName": "string",
  "expiresAt": "DateTime (UTC)"
}
```

#### SaveProgressDto
```csharp
{
  "currentStep": "int (1-n)",
  "isCompleted": "bool",
  "data": OnboardingData
}
```

#### OnboardingStatusDto
```csharp
{
  "currentStep": "int",
  "isCompleted": "bool",
  "updatedAt": "DateTime (UTC)",
  "data": OnboardingData
}
```

#### OnboardingData (Value Object)
```csharp
{
  "fullName": "string",
  "email": "string",
  "summary": "string",
  "skills": ["string"],
  "experiences": [WorkEntry],
  "educations": [EducationEntry]
}
```

#### WorkEntry
```csharp
{
  "company": "string",
  "role": "string",
  "dateRange": {
    "start": "DateTime",
    "end": "DateTime? (nullable)"
  }
}
```

#### EducationEntry
```csharp
{
  "institution": "string",
  "degree": "string",
  "dateRange": {
    "start": "DateTime",
    "end": "DateTime? (nullable)"
  }
}
```

#### ApiResponse<T> (Wrapper Universal)

**Todas las respuestas del API usan este formato**:

```csharp
{
  "success": "bool",
  "statusCode": "int (200, 201, 400, 401, 404, 500)",
  "message": "string (mensaje descriptivo)",
  "data": "T (objeto de respuesta, puede ser null)",
  "errors": ["string"] // Lista de errores detallados
}
```

**Ejemplo de Éxito**:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Operación exitosa",
  "data": { /* contenido */ },
  "errors": []
}
```

**Ejemplo de Error**:
```json
{
  "success": false,
  "statusCode": 400,
  "message": "Error en la validación",
  "data": null,
  "errors": [
    "El email ya está registrado",
    "La contraseña debe tener al menos 6 caracteres"
  ]
}
```

---

## 🔔 SignalR - Notificaciones en Tiempo Real

### URL del Hub

```
wss://localhost:7295/hubs/notifications
ws://localhost:5019/hubs/notifications
```

### Autenticación SignalR

SignalR requiere el token JWT para conectarse. Hay dos formas:

#### Opción 1: Query String (Recomendado)
```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7295/hubs/notifications', {
    accessTokenFactory: () => localStorage.getItem('token')
  })
  .build();
```

#### Opción 2: Header (Alternativo)
```typescript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7295/hubs/notifications', {
    headers: { 'Authorization': `Bearer ${token}` }
  })
  .build();
```

### Métodos del Hub (Servidor → Cliente)

El servidor puede enviar estos eventos al cliente:

#### 1. `ReceiveOnboardingProgress`

Se dispara cuando se guarda progreso en el onboarding.

**Parámetros**:
```typescript
(message: string, currentStep: number) => void
```

**Ejemplo**:
```typescript
connection.on('ReceiveOnboardingProgress', (message, currentStep) => {
  console.log(`📬 ${message} - Paso ${currentStep}`);
  // Ejemplo: "Progreso guardado en el paso 2 - Paso 2"
});
```

#### 2. `ReceiveOnboardingCompletion`

Se dispara cuando se completa el onboarding.

**Parámetros**:
```typescript
(message: string) => void
```

**Ejemplo**:
```typescript
connection.on('ReceiveOnboardingCompletion', (message) => {
  console.log(`🎉 ${message}`);
  // Ejemplo: "Onboarding completado exitosamente"
});
```

#### 3. `Pong`

Respuesta al método `Ping` (útil para keep-alive).

**Parámetros**:
```typescript
(timestamp: Date) => void
```

### Métodos Invocables (Cliente → Servidor)

#### 1. `Ping()`

Envía un ping al servidor para verificar conectividad.

```typescript
await connection.invoke('Ping');
```

#### 2. `JoinGroup(groupName: string)`

Une al usuario a un grupo específico.

```typescript
await connection.invoke('JoinGroup', 'admin');
```

#### 3. `LeaveGroup(groupName: string)`

Saca al usuario de un grupo.

```typescript
await connection.invoke('LeaveGroup', 'admin');
```

### Ciclo de Vida de la Conexión

```typescript
// Iniciar conexión
await connection.start();
console.log('✅ SignalR conectado');

// Evento: desconexión
connection.onclose((error) => {
  console.error('❌ SignalR desconectado:', error);
  // Reintentar conexión
});

// Detener conexión (al cerrar sesión)
await connection.stop();
```

### Grupos de Usuario

Cuando un usuario se conecta al hub:
- ✅ Se une automáticamente a un grupo con su `UserId`
- ✅ Las notificaciones se envían a ese grupo personalizado
- ✅ Solo el usuario autenticado recibe sus notificaciones

---

## 🔄 Flujo de Onboarding

### Diagrama de Flujo

```
┌─────────────┐
│  Register/  │
│    Login    │
└──────┬──────┘
       │ (JWT Token)
       v
┌─────────────┐
│ GET /resume │ ← Verificar si hay progreso guardado
└──────┬──────┘
       │
       ├─── 404 (No hay progreso) ──→ Iniciar desde Step 1
       │
       └─── 200 (Hay progreso) ────→ Reanudar desde currentStep
                │
                v
       ┌─────────────────┐
       │   Paso 1, 2, 3  │
       │  (Frontend UI)  │
       └────────┬────────┘
                │
                │ Cada paso guarda progreso
                v
       ┌─────────────────┐
       │  POST /save     │ → SignalR: "Progreso guardado en paso X"
       └────────┬────────┘
                │
                │ Continuar hasta el último paso
                v
       ┌─────────────────┐
       │ POST /complete  │ → SignalR: "Onboarding completado exitosamente"
       └────────┬────────┘
                │
                v
       ┌─────────────────┐
       │ Perfil Creado   │
       │   (Entidades    │
       │   Relacionales) │
       └─────────────────┘
```

### Paso a Paso (Implementación Frontend)

#### 1. Al Cargar la Aplicación (App Init)

```typescript
// Si el usuario está autenticado, verificar progreso
if (hasToken()) {
  const response = await http.get('/api/onboarding/resume');
  
  if (response.statusCode === 200) {
    // Hay progreso guardado
    const { currentStep, data, isCompleted } = response.data;
    
    if (isCompleted) {
      router.navigate('/dashboard'); // Ya completó
    } else {
      router.navigate(`/onboarding/step${currentStep}`); // Reanudar
    }
  } else if (response.statusCode === 404) {
    // No hay progreso, iniciar desde step 1
    router.navigate('/onboarding/step1');
  }
}
```

#### 2. Al Completar Cada Paso

```typescript
async function saveStep(stepNumber: number, formData: OnboardingData) {
  const response = await http.post('/api/onboarding/save', {
    currentStep: stepNumber,
    isCompleted: false,
    data: formData
  });
  
  if (response.success) {
    // Progreso guardado, avanzar al siguiente paso
    router.navigate(`/onboarding/step${stepNumber + 1}`);
  }
}
```

#### 3. Al Finalizar el Último Paso

```typescript
async function completeOnboarding() {
  // Primero guardar el último paso
  await saveStep(3, finalFormData);
  
  // Luego completar el onboarding
  const response = await http.post('/api/onboarding/complete');
  
  if (response.statusCode === 201) {
    // Onboarding completado, redirigir al dashboard
    router.navigate('/success');
  }
}
```

#### 4. Escuchar Notificaciones SignalR

```typescript
// En el servicio de notificaciones
connection.on('ReceiveOnboardingProgress', (message, step) => {
  showToast('info', message);
});

connection.on('ReceiveOnboardingCompletion', (message) => {
  showToast('success', message);
  confetti(); // 🎉
});
```

---

## ⚠️ Manejo de Errores

### Códigos de Estado HTTP

| Código | Significado | Cuándo Ocurre |
|--------|-------------|---------------|
| **200** | OK | Operación exitosa (GET, PUT, etc.) |
| **201** | Created | Recurso creado exitosamente (POST register, complete) |
| **400** | Bad Request | Datos inválidos, validación fallida |
| **401** | Unauthorized | Token inválido, expirado o ausente |
| **404** | Not Found | Recurso no encontrado (ej: no hay progreso de onboarding) |
| **500** | Internal Server Error | Error del servidor (bug, DB down, etc.) |

### Estructura de Error

```json
{
  "success": false,
  "statusCode": 400,
  "message": "Error en la validación",
  "data": null,
  "errors": [
    "El email ya está registrado",
    "La contraseña debe tener al menos 6 caracteres"
  ]
}
```

### Manejo Recomendado en Frontend

```typescript
async function handleApiCall<T>(request: Promise<ApiResponse<T>>): Promise<T> {
  try {
    const response = await request;
    
    if (response.success) {
      return response.data;
    } else {
      // Manejar errores del API
      if (response.statusCode === 401) {
        // Token expirado, redirigir al login
        logout();
        router.navigate('/login');
      } else if (response.statusCode === 400) {
        // Mostrar errores de validación
        showErrors(response.errors);
      } else {
        // Error genérico
        showToast('error', response.message);
      }
      throw new Error(response.message);
    }
  } catch (error) {
    // Error de red o excepción
    showToast('error', 'Error de conexión');
    throw error;
  }
}
```

---

## 🗄️ Base de Datos

### SQL Server

**Nombre de la BD**: `VertexDB`

### Migraciones Aplicadas

```
20260122152144_InitialMigrationWithIdentity
20260122174026_RefactorToRelationalModel
20260123030050_ChangeJsonToTypedObject
```

### Tablas Principales

#### 1. **AspNetUsers** (Identity)
```sql
Id (GUID) PK
Email (UNIQUE)
PasswordHash
FullName
EmailConfirmed
LockoutEnd
AccessFailedCount
-- ...más campos de Identity
```

#### 2. **OnboardingProcesses**
```sql
Id (GUID) PK
UserId (GUID) FK → AspNetUsers
CurrentStep (INT)
Data (NVARCHAR(MAX)) -- JSON serializado como objeto tipado
IsCompleted (BIT)
UpdatedAt (DATETIME2)
```

**Estructura del campo `Data` (JSON)**:
```json
{
  "FullName": "string",
  "Email": "string",
  "Summary": "string",
  "Skills": ["string"],
  "Experiences": [
    {
      "Company": "string",
      "Role": "string",
      "DateRange": {
        "Start": "DateTime",
        "End": "DateTime?"
      }
    }
  ],
  "Educations": [
    {
      "Institution": "string",
      "Degree": "string",
      "DateRange": {
        "Start": "DateTime",
        "End": "DateTime?"
      }
    }
  ]
}
```

#### 3. **ProfessionalProfiles**
```sql
Id (GUID) PK
UserId (GUID) FK → AspNetUsers (UNIQUE)
Summary (NVARCHAR(MAX))
CreatedAt (DATETIME2)
UpdatedAt (DATETIME2)
```

#### 4. **WorkExperiences**
```sql
Id (GUID) PK
ProfileId (GUID) FK → ProfessionalProfiles
Company (NVARCHAR(200))
Position (NVARCHAR(200))
StartDate (DATE)
EndDate (DATE) NULL
IsCurrent (BIT)
```

#### 5. **Educations**
```sql
Id (GUID) PK
ProfileId (GUID) FK → ProfessionalProfiles
Institution (NVARCHAR(200))
Degree (NVARCHAR(200))
StartDate (DATE)
EndDate (DATE) NULL
```

#### 6. **ProfileSkills**
```sql
Id (GUID) PK
ProfileId (GUID) FK → ProfessionalProfiles
SkillName (NVARCHAR(100))
Level (NVARCHAR(50))
```

### Relaciones

```
ApplicationUser (1) ←──→ (0..1) OnboardingProcess
ApplicationUser (1) ←──→ (0..1) ProfessionalProfile
ProfessionalProfile (1) ←──→ (0..n) WorkExperience
ProfessionalProfile (1) ←──→ (0..n) Education
ProfessionalProfile (1) ←──→ (0..n) ProfileSkill
```

### Índices

- `UserId` en `OnboardingProcesses` (UNIQUE, para búsquedas rápidas)
- `UserId` en `ProfessionalProfiles` (UNIQUE)
- `ProfileId` en tablas relacionadas (para JOINs rápidos)

---

## 🧪 Testing con Swagger

El API incluye Swagger UI para pruebas interactivas:

**URL**: `https://localhost:7295/swagger`

### Flujo de Prueba Manual

1. **Registrar usuario**:
   ```
   POST /api/auth/register
   Body: { "email": "test@test.com", "password": "Test123", "fullName": "Test User" }
   ```

2. **Copiar el token JWT** de la respuesta

3. **Autenticarse en Swagger**:
   - Click en "Authorize" 🔒
   - Pegar: `Bearer {token}`
   - Click en "Authorize"

4. **Guardar progreso**:
   ```
   POST /api/onboarding/save
   Body: { "currentStep": 1, "isCompleted": false, "data": {...} }
   ```

5. **Recuperar progreso**:
   ```
   GET /api/onboarding/resume
   ```

6. **Completar onboarding**:
   ```
   POST /api/onboarding/complete
   ```

---

## 📝 Notas para el Desarrollador Frontend

### ✅ Lo que DEBES hacer:

1. **Almacenar el token JWT** en `localStorage` o `sessionStorage`
2. **Incluir el token en todas las peticiones autenticadas**:
   ```typescript
   headers: { 'Authorization': `Bearer ${token}` }
   ```
3. **Conectar SignalR con el token** para recibir notificaciones
4. **Validar si el token está expirado** antes de hacer peticiones
5. **Redirigir al login** si recibes un `401 Unauthorized`
6. **Usar el wrapper `ApiResponse<T>`** para tipado fuerte
7. **Mostrar errores del array `errors`** cuando `success: false`

### ❌ Lo que NO DEBES hacer:

1. ❌ **NO enviar `userId` en el body** de peticiones (se extrae del token)
2. ❌ **NO exponer el token en la URL** (siempre en headers o query string para SignalR)
3. ❌ **NO almacenar datos sensibles** sin cifrar
4. ❌ **NO confiar en validaciones solo del frontend** (el backend valida todo)
5. ❌ **NO ignorar el `statusCode`** de la respuesta (puede diferir del HTTP status)

### 🔧 Recomendaciones:

- **NgRx/Redux**: Almacena el token y estado de autenticación en el store
- **Interceptors**: Crea un interceptor HTTP para agregar el token automáticamente
- **Auth Guard**: Protege rutas que requieran autenticación
- **Error Interceptor**: Maneja errores 401/500 globalmente
- **Toast/Notifications**: Muestra feedback visual al usuario con las notificaciones SignalR

---

## 🚀 Quick Start para Frontend

### 1. Login y Obtener Token

```typescript
const response = await fetch('https://localhost:7295/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'test@test.com',
    password: 'Test123'
  })
});

const result = await response.json();
if (result.success) {
  localStorage.setItem('token', result.data.token);
  localStorage.setItem('user', JSON.stringify({
    email: result.data.email,
    fullName: result.data.fullName
  }));
}
```

### 2. Conectar SignalR

```typescript
import * as signalR from '@microsoft/signalr';

const token = localStorage.getItem('token');
const connection = new signalR.HubConnectionBuilder()
  .withUrl('https://localhost:7295/hubs/notifications', {
    accessTokenFactory: () => token
  })
  .withAutomaticReconnect()
  .build();

connection.on('ReceiveOnboardingProgress', (message, step) => {
  console.log(`📬 ${message} - Paso ${step}`);
});

connection.on('ReceiveOnboardingCompletion', (message) => {
  console.log(`🎉 ${message}`);
});

await connection.start();
```

### 3. Guardar Progreso

```typescript
const saveProgress = async (step: number, formData: any) => {
  const token = localStorage.getItem('token');
  
  const response = await fetch('https://localhost:7295/api/onboarding/save', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    },
    body: JSON.stringify({
      currentStep: step,
      isCompleted: false,
      data: formData
    })
  });
  
  return await response.json();
};
```

### 4. Recuperar Progreso

```typescript
const getProgress = async () => {
  const token = localStorage.getItem('token');
  
  const response = await fetch('https://localhost:7295/api/onboarding/resume', {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  return await response.json();
};
```

---

## 📞 Soporte y Contacto

Para cualquier duda o problema con el API:

- **Logs**: Los logs del servidor están en la consola del terminal
- **Base de Datos**: Usa SQL Server Management Studio para inspeccionar datos
- **Swagger**: Prueba endpoints en `https://localhost:7295/swagger`

---

## 🎯 Resumen Ejecutivo (TL;DR)

| Aspecto | Detalle |
|---------|---------|
| **Base URL** | `https://localhost:7295/api` |
| **Autenticación** | JWT Bearer Token (60 min de duración) |
| **Endpoints** | `/auth/register`, `/auth/login`, `/onboarding/save`, `/onboarding/resume`, `/onboarding/complete` |
| **SignalR Hub** | `/hubs/notifications` (requiere token) |
| **CORS** | Permitido desde `localhost:4200`, `localhost:3000`, `localhost:5173` |
| **DB** | SQL Server (`VertexDB`) |
| **Formato de Respuesta** | `ApiResponse<T>` con `success`, `statusCode`, `message`, `data`, `errors` |
| **Seguridad** | UserId extraído del token JWT (nunca enviado desde frontend) |
| **Notificaciones** | SignalR envía eventos en tiempo real al guardar progreso y completar onboarding |

---

**Fecha de Creación**: 23 de Enero, 2026  
**Versión del Documento**: 1.0  
**Proyecto**: VERTEX - Sistema de Gestión de Perfiles Profesionales  
**Stack**: .NET 9 + ASP.NET Core + Entity Framework Core + SQL Server + SignalR + JWT
