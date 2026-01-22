# 📝 VERTEX - Comandos Útiles

Este documento contiene los comandos más comunes para trabajar con el proyecto VERTEX.

---

## 🔨 Compilación y Ejecución

### Restaurar Dependencias
```bash
dotnet restore
```

### Compilar la Solución
```bash
dotnet build
```

### Compilar en Modo Release
```bash
dotnet build --configuration Release
```

### Ejecutar la API
```bash
dotnet run --project src/Vertex.API/Vertex.API.csproj
```

### Ejecutar con Hot Reload (watch)
```bash
cd src/Vertex.API
dotnet watch run
```

---

## 🗄️ Entity Framework Core

### Instalar herramientas de EF (solo primera vez)
```bash
dotnet tool install --global dotnet-ef
```

### Crear una nueva migración
```bash
cd src/Vertex.API
dotnet ef migrations add MigrationName --project ../Vertex.Infrastructure
```

**Ejemplo de la migración inicial con Identity:**
```bash
dotnet ef migrations add InitialMigrationWithIdentity --project ../Vertex.Infrastructure
```

### Aplicar migraciones a la base de datos
```bash
dotnet ef database update
```

### Ver el SQL generado por una migración
```bash
dotnet ef migrations script
```

### Revertir la última migración
```bash
dotnet ef database update PreviousMigrationName
```

### Eliminar la última migración (sin aplicar)
```bash
dotnet ef migrations remove --project ../Vertex.Infrastructure
```

### Eliminar la base de datos
```bash
dotnet ef database drop --force
```

---

## 🧪 Testing

### Ejecutar todas las pruebas
```bash
dotnet test
```

### Ejecutar con reporte de cobertura
```bash
dotnet test /p:CollectCoverage=true
```

---

## 📦 Gestión de Paquetes

### Agregar un paquete NuGet
```bash
dotnet add package NombreDelPaquete
```

### Agregar un paquete a un proyecto específico
```bash
dotnet add src/Vertex.API/Vertex.API.csproj package NombreDelPaquete
```

### Actualizar todos los paquetes
```bash
dotnet list package --outdated
dotnet add package NombreDelPaquete
```

### Listar paquetes instalados
```bash
dotnet list package
```

---

## 🌐 Swagger/OpenAPI

### Acceder a Swagger UI
```
https://localhost:5001/swagger
```

### Descargar el documento OpenAPI (JSON)
```
https://localhost:5001/swagger/v1/swagger.json
```

---

## 🐳 Docker (Futuro)

### Construir imagen
```bash
docker build -t vertex-api .
```

### Ejecutar contenedor
```bash
docker run -p 5000:80 vertex-api
```

---

## 🔍 Debugging

### Ver logs detallados
```bash
dotnet run --project src/Vertex.API/Vertex.API.csproj --verbosity detailed
```

### Limpiar solución
```bash
dotnet clean
```

### Limpiar carpetas bin y obj
```bash
Get-ChildItem -Recurse -Filter "bin" | Remove-Item -Recurse -Force
Get-ChildItem -Recurse -Filter "obj" | Remove-Item -Recurse -Force
```

---

## 📊 Análisis de Código

### Analizar código con SonarQube (si está configurado)
```bash
dotnet sonarscanner begin /k:"vertex"
dotnet build
dotnet sonarscanner end
```

---

## 🚀 Script de Configuración Rápida

### Configurar base de datos (PowerShell)
```powershell
.\setup-database.ps1
```

### O manualmente:
```bash
cd src/Vertex.API
dotnet ef migrations add InitialCreate --project ../Vertex.Infrastructure
dotnet ef database update
```

---

## 📁 Estructura de Carpetas

```
Vertex/
├── src/
│   ├── Vertex.Domain/          # Entidades de negocio
│   ├── Vertex.Application/     # Interfaces y DTOs
│   ├── Vertex.Infrastructure/  # Persistencia (EF Core)
│   └── Vertex.API/             # Controllers y configuración
├── docs/                       # Documentación
├── Vertex.sln                  # Archivo de solución
└── README.md                   # Documentación principal
```

---

## 🔐 Variables de Entorno (Producción)

### Configurar cadena de conexión
```bash
export ConnectionStrings__DefaultConnection="Server=prod-server;Database=VertexDB;User Id=sa;Password=***"
```

### En Windows (PowerShell):
```powershell
$env:ConnectionStrings__DefaultConnection="Server=prod-server;Database=VertexDB;User Id=sa;Password=***"
```

---

## 📝 Notas Importantes

- Siempre ejecuta las migraciones desde `src/Vertex.API`
- El proyecto Infrastructure contiene las migraciones pero API es el startup project
- Usa `dotnet watch run` para desarrollo con hot reload
- Swagger solo está habilitado en modo Development por seguridad

---

## 🆘 Solución de Problemas

### Error: "No DbContext was found"
```bash
# Asegúrate de estar en src/Vertex.API y especificar el proyecto Infrastructure
dotnet ef migrations add MigrationName --project ../Vertex.Infrastructure
```

### Error: "Cannot connect to SQL Server"
- Verifica que SQL Server esté corriendo
- Revisa la cadena de conexión en appsettings.json
- Asegúrate de tener permisos en la base de datos

### Error: "Port already in use"
```bash
# Cambiar el puerto en launchSettings.json o usar:
dotnet run --urls "http://localhost:5555"
```

---

Última actualización: Enero 22, 2026
