# 🚀 Próximos Pasos - Proyecto VERTEX

Este documento lista las acciones pendientes para completar el proyecto VERTEX y ponerlo en producción.

---

## ✅ Completado

- [x] Estructura de Clean Architecture implementada
- [x] Entidades de dominio (OnboardingProcess, ProfessionalProfile)
- [x] Interfaces y DTOs en la capa de aplicación
- [x] Repositorio con lógica de Upsert
- [x] DbContext configurado con Entity Framework Core
- [x] Controladores REST (OnboardingController)
- [x] Configuración de servicios (DI, CORS, Swagger)
- [x] Compilación exitosa del proyecto

---

## 📋 Paso 1: Configurar la Base de Datos (INMEDIATO)

### Tareas:
- [ ] Verificar que SQL Server esté instalado y corriendo
- [ ] Ejecutar el script de configuración: `.\setup-database.ps1`
- [ ] O ejecutar manualmente:
  ```bash
  cd src/Vertex.API
  dotnet ef migrations add InitialCreate --project ../Vertex.Infrastructure
  dotnet ef database update
  ```
- [ ] Verificar que la base de datos `VertexDB` se creó correctamente
- [ ] Verificar las tablas: `OnboardingProcesses`, `ProfessionalProfiles`, `AspNetUsers`

---

## 🔐 Paso 2: Implementar Autenticación JWT (CRÍTICO)

### Tareas:
- [ ] Instalar paquete: `Microsoft.AspNetCore.Authentication.JwtBearer`
- [ ] Configurar JWT en `Program.cs`:
  ```csharp
  builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(options => {
          options.TokenValidationParameters = new TokenValidationParameters {
              ValidateIssuer = true,
              ValidateAudience = true,
              ValidateLifetime = true,
              ValidateIssuerSigningKey = true,
              ValidIssuer = configuration["Jwt:Issuer"],
              ValidAudience = configuration["Jwt:Audience"],
              IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(configuration["Jwt:Key"])
              )
          };
      });
  ```
- [ ] Agregar `[Authorize]` attribute a los controladores
- [ ] Crear endpoint de login (`POST /api/Auth/login`)
- [ ] Crear endpoint de registro (`POST /api/Auth/register`)
- [ ] Reemplazar `UserId` hardcodeado por extracción desde Claims:
  ```csharp
  var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
  ```

---

## ✅ Paso 3: Implementar Validaciones (IMPORTANTE)

### Tareas:
- [ ] Instalar `FluentValidation.AspNetCore`
- [ ] Crear validadores para DTOs:
  - `SaveProgressDtoValidator`: Validar CurrentStep, SerializedData
  - Validar que CurrentStep esté entre 1 y N (número máximo de pasos)
  - Validar que SerializedData sea JSON válido
- [ ] Registrar validadores en el contenedor de DI
- [ ] Crear middleware de manejo de errores global
- [ ] Implementar responses estandarizados (Problem Details)

---

## 🧪 Paso 4: Crear Pruebas (CALIDAD)

### Pruebas Unitarias:
- [ ] Crear proyecto: `Vertex.Tests.Unit`
- [ ] Instalar paquetes: `xUnit`, `Moq`, `FluentAssertions`
- [ ] Probar lógica del repositorio:
  - `SaveOrUpdateAsync_WhenUserExists_ShouldUpdate`
  - `SaveOrUpdateAsync_WhenUserNotExists_ShouldCreate`
  - `GetByUserIdAsync_WhenExists_ShouldReturnProcess`
  - `GetByUserIdAsync_WhenNotExists_ShouldReturnNull`
- [ ] Probar controladores (con repositorio mockeado)

### Pruebas de Integración:
- [ ] Crear proyecto: `Vertex.Tests.Integration`
- [ ] Configurar WebApplicationFactory
- [ ] Probar endpoints completos con base de datos in-memory
- [ ] Probar flujo completo: POST /save → GET /resume

---

## 📊 Paso 5: Implementar Logging Estructurado (OBSERVABILIDAD)

### Tareas:
- [ ] Instalar `Serilog.AspNetCore`
- [ ] Configurar Serilog en `Program.cs`:
  ```csharp
  Log.Logger = new LoggerConfiguration()
      .WriteTo.Console()
      .WriteTo.File("logs/vertex-.log", rollingInterval: RollingInterval.Day)
      .CreateLogger();
  ```
- [ ] Agregar logs en puntos críticos:
  - Inicio/fin de operaciones del repositorio
  - Errores en controladores
  - Cambios en el estado del onboarding
- [ ] Considerar integración con Application Insights (Azure)

---

## 🌐 Paso 6: Mejorar la API (USABILIDAD)

### Documentación:
- [ ] Mejorar comentarios XML en controladores para Swagger
- [ ] Agregar ejemplos de requests/responses en Swagger
- [ ] Configurar versioning de API (`/api/v1/Onboarding`)

### Endpoints Adicionales:
- [ ] `DELETE /api/Onboarding/reset` - Reiniciar el proceso
- [ ] `GET /api/Onboarding/progress` - Obtener % de completitud
- [ ] `POST /api/Profile/generate` - Generar ProfessionalProfile al completar

### Optimizaciones:
- [ ] Implementar paginación para listas futuras
- [ ] Agregar Rate Limiting con `AspNetCoreRateLimit`
- [ ] Implementar caché con `IMemoryCache` para consultas frecuentes

---

## 🔒 Paso 7: Seguridad (PRODUCCIÓN)

### Tareas:
- [ ] Configurar HTTPS obligatorio (eliminar HTTP)
- [ ] Implementar HSTS (HTTP Strict Transport Security)
- [ ] Configurar políticas de CORS más restrictivas
- [ ] Agregar validación anti-CSRF para mutaciones
- [ ] Implementar políticas de contraseñas seguras con Identity
- [ ] Configurar secretos con Azure Key Vault o AWS Secrets Manager
- [ ] Habilitar auditoría de cambios (CreatedBy, ModifiedBy)

---

## 🐳 Paso 8: Containerización (DEVOPS)

### Tareas:
- [ ] Crear `Dockerfile` en la raíz:
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
  WORKDIR /app
  EXPOSE 80
  EXPOSE 443

  FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet restore
  RUN dotnet build -c Release -o /app/build

  FROM build AS publish
  RUN dotnet publish -c Release -o /app/publish

  FROM base AS final
  WORKDIR /app
  COPY --from=publish /app/publish .
  ENTRYPOINT ["dotnet", "Vertex.API.dll"]
  ```
- [ ] Crear `docker-compose.yml` para desarrollo local (API + SQL Server)
- [ ] Configurar CI/CD con GitHub Actions o Azure DevOps
- [ ] Desplegar en Azure Container Apps / AWS ECS / Kubernetes

---

## 📈 Paso 9: Monitoreo y Métricas (PRODUCCIÓN)

### Tareas:
- [ ] Configurar Health Checks:
  ```csharp
  builder.Services.AddHealthChecks()
      .AddDbContextCheck<VertexDbContext>();
  app.MapHealthChecks("/health");
  ```
- [ ] Integrar Application Performance Monitoring (APM):
  - Application Insights (Azure)
  - New Relic
  - Datadog
- [ ] Configurar alertas para:
  - Errores críticos (500s)
  - Latencia alta (> 2s)
  - Base de datos down

---

## 🎨 Paso 10: Integración con Frontend (FUNCIONALIDAD)

### Tareas:
- [ ] Generar cliente TypeScript con `NSwag` o `openapi-generator`
- [ ] Configurar axios/fetch en el frontend
- [ ] Implementar manejo de estados del onboarding (React Context / Redux)
- [ ] Crear componentes para cada paso del formulario
- [ ] Implementar persistencia automática (guardar cada X segundos)
- [ ] Mostrar indicador de "Guardando..." durante requests

---

## 📝 Paso 11: Funcionalidades Adicionales (FUTURO)

### Módulo de Perfiles:
- [ ] Endpoint para generar PDF del CV (`GET /api/Profile/{id}/pdf`)
- [ ] Endpoint para compartir perfil público (`GET /api/Profile/{id}/share`)
- [ ] Búsqueda de perfiles por habilidades

### Módulo de Notificaciones:
- [ ] Enviar email al completar onboarding
- [ ] Recordatorios para completar el proceso
- [ ] Integración con SendGrid/Mailgun

### Dashboard de Administración:
- [ ] Estadísticas de onboarding completados
- [ ] Usuarios activos
- [ ] Análisis de tiempo promedio por paso

---

## 🗂️ Paso 12: Migraciones y Datos de Prueba (DESARROLLO)

### Tareas:
- [ ] Crear seeder para datos de prueba (`SeedData.cs`)
- [ ] Generar 100 usuarios de prueba
- [ ] Generar procesos de onboarding en diferentes estados
- [ ] Script para resetear la base de datos de desarrollo

---

## 📊 Checklist de Producción

Antes de ir a producción, verificar:

- [ ] ✅ Base de datos creada y migraciones aplicadas
- [ ] ✅ Autenticación JWT implementada y testeada
- [ ] ✅ Validaciones de entrada funcionando
- [ ] ✅ Pruebas unitarias con cobertura > 80%
- [ ] ✅ Pruebas de integración pasando
- [ ] ✅ HTTPS configurado y obligatorio
- [ ] ✅ CORS configurado para dominios de producción
- [ ] ✅ Logging estructurado funcionando
- [ ] ✅ Secretos en variables de entorno (no en código)
- [ ] ✅ Rate Limiting configurado
- [ ] ✅ Health Checks funcionando
- [ ] ✅ Monitoreo y alertas configurados
- [ ] ✅ Backup automático de base de datos
- [ ] ✅ Plan de rollback definido

---

## 🎯 Priorización Recomendada

### 🔴 ALTA PRIORIDAD (Esta Semana):
1. Configurar base de datos (migraciones)
2. Implementar autenticación JWT
3. Crear pruebas unitarias básicas

### 🟡 MEDIA PRIORIDAD (Próximas 2 Semanas):
4. Implementar validaciones con FluentValidation
5. Configurar logging con Serilog
6. Crear pruebas de integración
7. Containerizar con Docker

### 🟢 BAJA PRIORIDAD (Próximo Mes):
8. Mejorar documentación de Swagger
9. Implementar endpoints adicionales
10. Configurar monitoreo avanzado
11. Desarrollar dashboard de administración

---

## 📞 Recursos y Soporte

- **Documentación Oficial:** [docs.microsoft.com/aspnet/core](https://docs.microsoft.com/aspnet/core)
- **Entity Framework Core:** [docs.microsoft.com/ef/core](https://docs.microsoft.com/ef/core)
- **Clean Architecture:** [blog.cleancoder.com](https://blog.cleancoder.com)
- **Stack Overflow:** [stackoverflow.com/questions/tagged/asp.net-core](https://stackoverflow.com/questions/tagged/asp.net-core)

---

**Fecha de última actualización:** Enero 22, 2026  
**Estado del proyecto:** ✅ Base funcional completada - Listo para fase de desarrollo avanzado
